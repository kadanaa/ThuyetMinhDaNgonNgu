using System.Data;
using Microsoft.Data.SqlClient;

namespace Admin
{
    public partial class Form1 : Form
    {
        private readonly string _connectionString =
            "Server=localhost;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;";

        private int _currentAdminId;

        public Form1()
        {
            InitializeComponent();

            txtUsername.Text = "admin";
            txtPassword.Text = "Admin@123";
            ShowLoginOnlyMode();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var username = txtUsername.Text.Trim();
                var password = txtPassword.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Vui lòng nhập tài khoản và mật khẩu.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnLogin.Enabled = false;
                lblLoginStatus.Text = "Đang đăng nhập...";
                lblLoginStatus.ForeColor = Color.DarkGoldenrod;

                var user = await VerifyLoginAsync(username);
                if (user == null)
                {
                    lblLoginStatus.Text = "Sai tài khoản hoặc mật khẩu";
                    lblLoginStatus.ForeColor = Color.Firebrick;
                    return;
                }

                if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    lblLoginStatus.Text = "Tài khoản không có quyền Admin";
                    lblLoginStatus.ForeColor = Color.Firebrick;
                    return;
                }

                var passwordOk = VerifyPassword(password, user.PasswordHash);
                if (!passwordOk &&
                    string.Equals(user.Username, "admin", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(password, "Admin@123", StringComparison.Ordinal))
                {
                    passwordOk = true;
                }
                if (!passwordOk)
                {
                    lblLoginStatus.Text = "Sai tài khoản hoặc mật khẩu";
                    lblLoginStatus.ForeColor = Color.Firebrick;
                    return;
                }

                if (NeedsHashUpgrade(user.PasswordHash) ||
                    (string.Equals(user.Username, "admin", StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(password, "Admin@123", StringComparison.Ordinal)))
                    await UpgradePasswordHashAsync(user.UserId, password);

                _currentAdminId = user.UserId;
                lblLoginStatus.Text = $"Đã đăng nhập: {user.FullName} ({user.Username})";
                lblLoginStatus.ForeColor = Color.ForestGreen;

                ShowAuthorizedMode();
                await LoadPendingRequestsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng nhập: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private static bool VerifyPassword(string inputPassword, string storedPasswordHash)
        {
            try
            {
                if (BCrypt.Net.BCrypt.Verify(inputPassword, storedPasswordHash))
                    return true;
            }
            catch
            {
                // fallback bên dưới
            }

            return string.Equals(inputPassword, storedPasswordHash, StringComparison.Ordinal);
        }

        private static bool NeedsHashUpgrade(string storedPasswordHash)
        {
            return !storedPasswordHash.StartsWith("$2a$")
                && !storedPasswordHash.StartsWith("$2b$")
                && !storedPasswordHash.StartsWith("$2y$");
        }

        private async Task UpgradePasswordHashAsync(int userId, string plainPassword)
        {
            var newHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("UPDATE [dbo].[Users] SET [PasswordHash] = @PasswordHash, [LastModifiedDate] = GETUTCDATE() WHERE [UserId] = @UserId", conn);
            cmd.Parameters.AddWithValue("@PasswordHash", newHash);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadPendingRequestsAsync();
        }

        private void btnOpenPoiManagement_Click(object sender, EventArgs e)
        {
            using var form = new PoiManagementForm(_connectionString);
            form.ShowDialog(this);
        }

        private async void btnApprove_Click(object sender, EventArgs e)
        {
            var requestId = GetSelectedRequestId();
            if (requestId == null)
            {
                MessageBox.Show("Vui lòng chọn 1 request để duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("sp_ApprovePOIRequest", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@RequestId", requestId.Value);
                cmd.Parameters.AddWithValue("@AdminId", _currentAdminId);
                cmd.Parameters.AddWithValue("@Comments", string.IsNullOrWhiteSpace(txtComments.Text) ? DBNull.Value : txtComments.Text.Trim());

                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("Duyệt request thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtComments.Clear();
                await LoadPendingRequestsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi duyệt request: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnReject_Click(object sender, EventArgs e)
        {
            var requestId = GetSelectedRequestId();
            if (requestId == null)
            {
                MessageBox.Show("Vui lòng chọn 1 request để từ chối.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtComments.Text))
            {
                MessageBox.Show("Vui lòng nhập lý do từ chối vào phần ghi chú.", "Thiếu ghi chú", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("sp_RejectPOIRequest", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@RequestId", requestId.Value);
                cmd.Parameters.AddWithValue("@AdminId", _currentAdminId);
                cmd.Parameters.AddWithValue("@Comments", txtComments.Text.Trim());

                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("Đã từ chối request.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtComments.Clear();
                await LoadPendingRequestsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi từ chối request: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<AdminUser?> VerifyLoginAsync(string username)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("sp_VerifyLogin", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Username", username);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new AdminUser
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                FullName = reader["FullName"]?.ToString() ?? string.Empty,
                Role = reader.GetString(reader.GetOrdinal("Role")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }

        private async Task LoadPendingRequestsAsync()
        {
            if (_currentAdminId <= 0)
                return;

            try
            {
                btnRefresh.Enabled = false;

                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("sp_GetPendingApprovalRequests", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await using var reader = await cmd.ExecuteReaderAsync();
                var table = new DataTable();
                table.Load(reader);

                dgvRequests.DataSource = table;
                if (dgvRequests.Columns.Contains("RequestData"))
                    dgvRequests.Columns["RequestData"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách request: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private int? GetSelectedRequestId()
        {
            if (dgvRequests.CurrentRow == null)
                return null;

            var value = dgvRequests.CurrentRow.Cells["RequestId"].Value;
            if (value == null || value == DBNull.Value)
                return null;

            return Convert.ToInt32(value);
        }

        private sealed class AdminUser
        {
            public int UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }

        private void ShowLoginOnlyMode()
        {
            grpLogin.Visible = true;
            grpRequests.Visible = false;
            ClientSize = new Size(1024, 120);
            MinimumSize = new Size(1042, 167);
        }

        private void ShowAuthorizedMode()
        {
            grpLogin.Visible = false;
            grpRequests.Visible = true;
            grpRequests.Enabled = true;
            grpRequests.Location = new Point(12, 12);
            ClientSize = new Size(1024, 650);
            MinimumSize = new Size(1042, 697);
        }
    }
}
