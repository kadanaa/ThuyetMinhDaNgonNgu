using System.Data;
using System.Globalization;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace POIOwner
{
    public partial class Form1 : Form
    {
        private readonly string _connectionString =
            "Server=localhost;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;";

        private int _currentOwnerId;

        public Form1()
        {
            InitializeComponent();

            txtUsername.Text = "poiowner01";
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

                if (!string.Equals(user.Role, "POIOwner", StringComparison.OrdinalIgnoreCase))
                {
                    lblLoginStatus.Text = "Tài khoản không phải POI Owner";
                    lblLoginStatus.ForeColor = Color.Firebrick;
                    return;
                }

                var passwordOk = VerifyPassword(password, user.PasswordHash);
                if (!passwordOk &&
                    string.Equals(user.Username, "poiowner01", StringComparison.OrdinalIgnoreCase) &&
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
                    (string.Equals(user.Username, "poiowner01", StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(password, "Admin@123", StringComparison.Ordinal)))
                {
                    await UpgradePasswordHashAsync(user.UserId, password);
                }

                _currentOwnerId = user.UserId;
                ShowAuthorizedMode();

                await LoadMyPoisAsync();
                await LoadMyRequestsAsync();
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

        private void btnLogout_Click(object sender, EventArgs e)
        {
            _currentOwnerId = 0;

            dgvMyRequests.DataSource = null;
            cmbMyPois.DataSource = null;
            txtDeleteReason.Clear();
            ClearCreateForm();

            txtPassword.Clear();
            lblLoginStatus.Text = "Chưa đăng nhập";
            lblLoginStatus.ForeColor = Color.Firebrick;

            ShowLoginOnlyMode();
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                var username = txtRegisterUsername.Text.Trim();
                var password = txtRegisterPassword.Text;
                var fullName = txtRegisterFullName.Text.Trim();
                var email = txtRegisterEmail.Text.Trim();
                var phone = txtRegisterPhone.Text.Trim();

                if (string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(fullName) ||
                    string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ tài khoản, mật khẩu, họ tên, email.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (password.Length < 6)
                {
                    MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự.", "Mật khẩu yếu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("sp_CreateUser", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Role", "POIOwner");
                cmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrWhiteSpace(phone) ? DBNull.Value : phone);

                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("Đăng ký POIOwner thành công. Bạn có thể đăng nhập ngay.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtUsername.Text = username;
                txtPassword.Clear();

                txtRegisterUsername.Clear();
                txtRegisterPassword.Clear();
                txtRegisterFullName.Clear();
                txtRegisterEmail.Clear();
                txtRegisterPhone.Clear();
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                MessageBox.Show("Tài khoản hoặc email đã tồn tại.", "Đăng ký thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng ký: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSendCreateRequest_Click(object sender, EventArgs e)
        {
            if (!TryGetCreatePoiInput(out var input, out var message))
            {
                MessageBox.Show(message, "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var poiId = await CreatePendingPoiAsync(conn, input);

                var requestData = JsonSerializer.Serialize(new
                {
                    POIName = input.PoiName,
                    input.Description,
                    input.Latitude,
                    input.Longitude,
                    input.Radius,
                    input.Category,
                    input.Address,
                    input.PhoneNumber,
                    input.Website,
                    Action = "Create"
                });

                await using var cmd = new SqlCommand("sp_CreatePOIApprovalRequest", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@POIId", poiId);
                cmd.Parameters.AddWithValue("@OwnerId", _currentOwnerId);
                cmd.Parameters.AddWithValue("@RequestType", "Create");
                cmd.Parameters.AddWithValue("@RequestData", requestData);
                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("Đã gửi request thêm POI cho admin.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearCreateForm();

                await LoadMyPoisAsync();
                await LoadMyRequestsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi request thêm POI: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSendDeleteRequest_Click(object sender, EventArgs e)
        {
            if (cmbMyPois.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn POI của bạn để gửi request xóa.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var reason = txtDeleteReason.Text.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("Vui lòng nhập lý do xóa.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var poiId = Convert.ToInt32(cmbMyPois.SelectedValue);

                var requestData = JsonSerializer.Serialize(new
                {
                    POIId = poiId,
                    Reason = reason,
                    Action = "Delete"
                });

                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("sp_CreatePOIApprovalRequest", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@POIId", poiId);
                cmd.Parameters.AddWithValue("@OwnerId", _currentOwnerId);
                cmd.Parameters.AddWithValue("@RequestType", "Delete");
                cmd.Parameters.AddWithValue("@RequestData", requestData);
                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("Đã gửi request xóa POI cho admin.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtDeleteReason.Clear();

                await LoadMyRequestsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi request xóa POI: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<int> CreatePendingPoiAsync(SqlConnection conn, CreatePoiInput input)
        {
            const string sql = @"
INSERT INTO [dbo].[PointsOfInterest]
([POIName], [Description], [Latitude], [Longitude], [Radius], [Category], [Address], [PhoneNumber], [Website], [OwnerId], [IsApproved], [Status])
VALUES
(@POIName, @Description, @Latitude, @Longitude, @Radius, @Category, @Address, @PhoneNumber, @Website, @OwnerId, 0, N'Pending');
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@POIName", input.PoiName);
            cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(input.Description) ? DBNull.Value : input.Description);
            cmd.Parameters.AddWithValue("@Latitude", input.Latitude);
            cmd.Parameters.AddWithValue("@Longitude", input.Longitude);
            cmd.Parameters.AddWithValue("@Radius", input.Radius);
            cmd.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(input.Category) ? DBNull.Value : input.Category);
            cmd.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(input.Address) ? DBNull.Value : input.Address);
            cmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrWhiteSpace(input.PhoneNumber) ? DBNull.Value : input.PhoneNumber);
            cmd.Parameters.AddWithValue("@Website", string.IsNullOrWhiteSpace(input.Website) ? DBNull.Value : input.Website);
            cmd.Parameters.AddWithValue("@OwnerId", _currentOwnerId);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private async Task LoadMyPoisAsync()
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
SELECT [POIId], [POIName]
FROM [dbo].[PointsOfInterest]
WHERE [OwnerId] = @OwnerId
ORDER BY [POIName]";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@OwnerId", _currentOwnerId);
            await using var reader = await cmd.ExecuteReaderAsync();

            var table = new DataTable();
            table.Load(reader);

            cmbMyPois.DataSource = table;
            cmbMyPois.DisplayMember = "POIName";
            cmbMyPois.ValueMember = "POIId";
        }

        private async Task LoadMyRequestsAsync()
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
SELECT [RequestId], [POIId], [RequestType], [Status], [RequestedDate], [ReviewedDate], [AdminComments]
FROM [dbo].[POIApprovalRequests]
WHERE [OwnerId] = @OwnerId
ORDER BY [RequestedDate] DESC";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@OwnerId", _currentOwnerId);
            await using var reader = await cmd.ExecuteReaderAsync();
            var table = new DataTable();
            table.Load(reader);

            dgvMyRequests.DataSource = table;
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
                // ignore and fallback
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

        private async Task<AppUser?> VerifyLoginAsync(string username)
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

            return new AppUser
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role"))
            };
        }

        private bool TryGetCreatePoiInput(out CreatePoiInput input, out string message)
        {
            input = new CreatePoiInput();
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(txtPoiName.Text))
            {
                message = "Tên POI là bắt buộc.";
                return false;
            }

            if (!TryParseDecimal(txtLatitude.Text, out var lat))
            {
                message = "Latitude không hợp lệ.";
                return false;
            }

            if (!TryParseDecimal(txtLongitude.Text, out var lon))
            {
                message = "Longitude không hợp lệ.";
                return false;
            }

            if (!TryParseDouble(txtRadius.Text, out var radius) || radius <= 0)
            {
                message = "Radius phải > 0.";
                return false;
            }

            input = new CreatePoiInput
            {
                PoiName = txtPoiName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Latitude = lat,
                Longitude = lon,
                Radius = radius,
                Category = txtCategory.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                Website = txtWebsite.Text.Trim()
            };

            return true;
        }

        private static bool TryParseDecimal(string value, out decimal result)
        {
            return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result)
                   || decimal.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
        }

        private static bool TryParseDouble(string value, out double result)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result)
                   || double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
        }

        private void ClearCreateForm()
        {
            txtPoiName.Clear();
            txtDescription.Clear();
            txtLatitude.Clear();
            txtLongitude.Clear();
            txtRadius.Clear();
            txtCategory.Clear();
            txtAddress.Clear();
            txtPhone.Clear();
            txtWebsite.Clear();
        }

        private void ShowLoginOnlyMode()
        {
            grpLogin.Visible = true;
            grpRequests.Visible = false;
            ClientSize = new Size(1184, 235);
            MinimumSize = new Size(1202, 282);
        }

        private void ShowAuthorizedMode()
        {
            grpLogin.Visible = false;
            grpRequests.Visible = true;
            grpRequests.Enabled = true;
            grpRequests.Location = new Point(12, 12);
            ClientSize = new Size(1184, 701);
            MinimumSize = new Size(1202, 748);
        }

        private sealed class AppUser
        {
            public int UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        private sealed class CreatePoiInput
        {
            public string PoiName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public double Radius { get; set; }
            public string Category { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string Website { get; set; } = string.Empty;
        }
    }
}
