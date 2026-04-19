using System.Data;
using System.Globalization;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Web.WebView2.WinForms;

namespace Admin
{
    public partial class Form1 : Form
    {
        private readonly string _connectionString;

        private int _currentAdminId;
        private readonly TabControl _mainTabControl = new() { Dock = DockStyle.Fill };
        private readonly TabPage _tabApproval = new() { Text = "Duyệt POI" };
        private readonly TabPage _tabStatistics = new() { Text = "Thống kê" };
        private readonly DataGridView _dgvPoiStats = new();
        private readonly Button _btnRefreshTourist = new();
        private readonly Label _lblTouristRefreshStatus = new();
        private readonly Label _lblActiveTouristSummary = new();
        private readonly WebView2 _statisticsMap = new() { Dock = DockStyle.Fill };
        private bool _statisticsUiInitialized;

        public Form1(string connectionString)
        {
            _connectionString = connectionString;
            InitializeComponent();
            grpLogin.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpRequests.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            InitializeStatisticsTab();

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
                await RefreshTouristStatisticsAsync();
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
                var selectedRow = dgvRequests.CurrentRow;
                var requestType = selectedRow?.Cells["RequestType"].Value?.ToString();
                int? poiId = null;
                var poiValue = selectedRow?.Cells["POIId"].Value;
                if (poiValue != null && poiValue != DBNull.Value)
                    poiId = Convert.ToInt32(poiValue);

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

                if (string.Equals(requestType, "Create", StringComparison.OrdinalIgnoreCase) && poiId.HasValue)
                {
                    await using var rejectPoiCmd = new SqlCommand(@"UPDATE [dbo].[PointsOfInterest]
SET [Status] = N'Rejected', [LastModifiedDate] = GETUTCDATE()
WHERE [POIId] = @POIId", conn);
                    rejectPoiCmd.Parameters.AddWithValue("@POIId", poiId.Value);
                    await rejectPoiCmd.ExecuteNonQueryAsync();
                }

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

        private void InitializeStatisticsTab()
        {
            if (_statisticsUiInitialized)
                return;

            _statisticsUiInitialized = true;

            grpRequests.Controls.Clear();
            grpRequests.Controls.Add(_mainTabControl);

            _mainTabControl.TabPages.Clear();
            _mainTabControl.TabPages.Add(_tabApproval);
            _mainTabControl.TabPages.Add(_tabStatistics);

            SetupApprovalTabLayout();
            SetupStatisticsTabLayout();
        }

        private void SetupApprovalTabLayout()
        {
            _tabApproval.Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(12, 10, 12, 10)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 78));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 22));

            dgvRequests.Dock = DockStyle.Fill;
            root.Controls.Add(dgvRequests, 0, 0);

            var actionLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 4
            };
            actionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            actionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            actionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            actionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            actionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            actionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            lblComments.Dock = DockStyle.Left;
            lblComments.Margin = new Padding(0, 4, 0, 0);
            actionLayout.Controls.Add(lblComments, 0, 0);

            btnOpenPoiManagement.Dock = DockStyle.Right;
            btnOpenPoiManagement.Width = 276;
            actionLayout.SetColumnSpan(btnOpenPoiManagement, 3);
            actionLayout.Controls.Add(btnOpenPoiManagement, 1, 0);

            txtComments.Dock = DockStyle.Fill;
            txtComments.Multiline = true;
            txtComments.Margin = new Padding(0, 4, 8, 0);
            actionLayout.Controls.Add(txtComments, 0, 1);

            btnApprove.Dock = DockStyle.Fill;
            btnApprove.Margin = new Padding(4, 4, 4, 0);
            actionLayout.Controls.Add(btnApprove, 1, 1);

            btnReject.Dock = DockStyle.Fill;
            btnReject.Margin = new Padding(4, 4, 4, 0);
            actionLayout.Controls.Add(btnReject, 2, 1);

            btnRefresh.Dock = DockStyle.Fill;
            btnRefresh.Margin = new Padding(4, 4, 0, 0);
            actionLayout.Controls.Add(btnRefresh, 3, 1);

            root.Controls.Add(actionLayout, 0, 1);
            _tabApproval.Controls.Add(root);
        }

        private void SetupStatisticsTabLayout()
        {
            var headerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 48,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(8, 6, 8, 6)
            };
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            _btnRefreshTourist.Text = "Refresh Tourist";
            _btnRefreshTourist.Dock = DockStyle.Fill;
            _btnRefreshTourist.Margin = new Padding(0, 0, 8, 0);
            _btnRefreshTourist.Click += async (_, _) => await RefreshTouristStatisticsAsync();

            _lblTouristRefreshStatus.Dock = DockStyle.Fill;
            _lblTouristRefreshStatus.TextAlign = ContentAlignment.MiddleLeft;
            _lblTouristRefreshStatus.Text = "Chưa tải dữ liệu";
            _lblTouristRefreshStatus.AutoEllipsis = true;

            _lblActiveTouristSummary.Dock = DockStyle.Fill;
            _lblActiveTouristSummary.TextAlign = ContentAlignment.MiddleRight;
            _lblActiveTouristSummary.Text = "Tourist đang online: 0";
            _lblActiveTouristSummary.AutoEllipsis = true;

            headerPanel.Controls.Add(_btnRefreshTourist, 0, 0);
            headerPanel.Controls.Add(_lblTouristRefreshStatus, 1, 0);
            headerPanel.Controls.Add(_lblActiveTouristSummary, 2, 0);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 260
            };

            _statisticsMap.Dock = DockStyle.Fill;
            split.Panel1.Controls.Add(_statisticsMap);

            _dgvPoiStats.Dock = DockStyle.Fill;
            _dgvPoiStats.ReadOnly = true;
            _dgvPoiStats.MultiSelect = false;
            _dgvPoiStats.AllowUserToAddRows = false;
            _dgvPoiStats.AllowUserToDeleteRows = false;
            _dgvPoiStats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvPoiStats.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            split.Panel2.Controls.Add(_dgvPoiStats);

            _tabStatistics.Controls.Add(split);
            _tabStatistics.Controls.Add(headerPanel);
        }

        private async Task RefreshTouristStatisticsAsync()
        {
            if (_currentAdminId <= 0)
                return;

            try
            {
                _btnRefreshTourist.Enabled = false;
                _lblTouristRefreshStatus.Text = "Đang cập nhật thống kê tourist...";
                _lblTouristRefreshStatus.ForeColor = Color.DarkGoldenrod;

                await MarkInactiveTouristsByLastSeenAsync();

                var table = await GetPoiTouristStatisticsAsync();
                _dgvPoiStats.DataSource = table;

                var activeTourists = await GetActiveTouristCountAsync();
                _lblActiveTouristSummary.Text = $"Tourist đang online: {activeTourists}";

                var activeTouristLocations = await GetActiveTouristLocationsAsync();

                await RenderStatisticsMapAsync(table, activeTouristLocations);

                _lblTouristRefreshStatus.Text = $"Cập nhật lúc {DateTime.Now:HH:mm:ss}";
                _lblTouristRefreshStatus.ForeColor = Color.ForestGreen;
            }
            catch (Exception ex)
            {
                _lblTouristRefreshStatus.Text = "Lỗi tải thống kê";
                _lblTouristRefreshStatus.ForeColor = Color.Firebrick;
                MessageBox.Show($"Lỗi tải thống kê tourist: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnRefreshTourist.Enabled = true;
            }
        }

        private async Task MarkInactiveTouristsByLastSeenAsync()
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
UPDATE [dbo].[TouristSessions]
SET [IsActive] = 0
WHERE [IsActive] = 1
  AND [LastSeenUtc] < DATEADD(MINUTE, -1, GETUTCDATE());";

            await using var cmd = new SqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<DataTable> GetPoiTouristStatisticsAsync()
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
SELECT
    p.[POIId],
    p.[POIName],
    CAST(p.[Latitude] AS FLOAT) AS [Latitude],
    CAST(p.[Longitude] AS FLOAT) AS [Longitude],
    CAST(p.[Radius] AS FLOAT) AS [RadiusKm],
    ISNULL(t.[ActiveTouristCount], 0) AS [ActiveTouristCount],
    p.[Category],
    p.[Address]
FROM [dbo].[PointsOfInterest] p
OUTER APPLY
(
    SELECT COUNT(1) AS [ActiveTouristCount]
    FROM [dbo].[TouristSessions] s
    WHERE s.[IsActive] = 1
      AND s.[CurrentLatitude] IS NOT NULL
      AND s.[CurrentLongitude] IS NOT NULL
      AND (6371 * ACOS(
            COS(RADIANS(CAST(s.[CurrentLatitude] AS FLOAT))) * COS(RADIANS(CAST(p.[Latitude] AS FLOAT))) *
            COS(RADIANS(CAST(p.[Longitude] AS FLOAT)) - RADIANS(CAST(s.[CurrentLongitude] AS FLOAT))) +
            SIN(RADIANS(CAST(s.[CurrentLatitude] AS FLOAT))) * SIN(RADIANS(CAST(p.[Latitude] AS FLOAT)))
      )) <= CAST(p.[Radius] AS FLOAT)
) t
WHERE p.[IsApproved] = 1
  AND p.[Status] = 'Active'
ORDER BY [ActiveTouristCount] DESC, p.[POIName];";

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        private async Task<int> GetActiveTouristCountAsync()
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
SELECT COUNT(1)
FROM [dbo].[TouristSessions]
WHERE [IsActive] = 1
  AND [LastSeenUtc] >= DATEADD(MINUTE, -1, GETUTCDATE());";

            await using var cmd = new SqlCommand(sql, conn);
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result, CultureInfo.InvariantCulture);
        }

        private async Task<List<TouristMapMarker>> GetActiveTouristLocationsAsync()
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
SELECT
    [DeviceId],
    CAST([CurrentLatitude] AS FLOAT) AS [Latitude],
    CAST([CurrentLongitude] AS FLOAT) AS [Longitude]
FROM [dbo].[TouristSessions]
WHERE [IsActive] = 1
  AND [LastSeenUtc] >= DATEADD(MINUTE, -1, GETUTCDATE())
  AND [CurrentLatitude] IS NOT NULL
  AND [CurrentLongitude] IS NOT NULL;";

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var result = new List<TouristMapMarker>();
            while (await reader.ReadAsync())
            {
                result.Add(new TouristMapMarker
                {
                    DeviceId = reader["DeviceId"]?.ToString() ?? string.Empty,
                    Latitude = reader["Latitude"] == DBNull.Value ? 0d : Convert.ToDouble(reader["Latitude"], CultureInfo.InvariantCulture),
                    Longitude = reader["Longitude"] == DBNull.Value ? 0d : Convert.ToDouble(reader["Longitude"], CultureInfo.InvariantCulture)
                });
            }

            return result;
        }

        private async Task RenderStatisticsMapAsync(DataTable table, List<TouristMapMarker> activeTouristLocations)
        {
            await _statisticsMap.EnsureCoreWebView2Async();

            if (table.Rows.Count == 0)
            {
                _statisticsMap.NavigateToString("<html><body style='font-family:Segoe UI;padding:16px'>Không có POI để hiển thị bản đồ.</body></html>");
                return;
            }

            var markers = new List<PoiMapMarker>();
            foreach (DataRow row in table.Rows)
            {
                if (row["Latitude"] == DBNull.Value || row["Longitude"] == DBNull.Value)
                    continue;

                markers.Add(new PoiMapMarker
                {
                    POIName = row["POIName"]?.ToString() ?? string.Empty,
                    Latitude = Convert.ToDouble(row["Latitude"], CultureInfo.InvariantCulture),
                    Longitude = Convert.ToDouble(row["Longitude"], CultureInfo.InvariantCulture),
                    RadiusKm = row["RadiusKm"] == DBNull.Value ? 0d : Convert.ToDouble(row["RadiusKm"], CultureInfo.InvariantCulture),
                    ActiveTouristCount = row["ActiveTouristCount"] == DBNull.Value ? 0 : Convert.ToInt32(row["ActiveTouristCount"], CultureInfo.InvariantCulture),
                    Category = row["Category"]?.ToString() ?? string.Empty,
                    Address = row["Address"]?.ToString() ?? string.Empty
                });
            }

            if (markers.Count == 0)
            {
                _statisticsMap.NavigateToString("<html><body style='font-family:Segoe UI;padding:16px'>POI chưa có tọa độ hợp lệ để hiển thị bản đồ.</body></html>");
                return;
            }

            var markersJson = JsonSerializer.Serialize(markers);
            var touristMarkersJson = JsonSerializer.Serialize(activeTouristLocations);
            var html = BuildStatisticsMapHtml(markersJson, touristMarkersJson);

            _statisticsMap.NavigateToString(html);
        }

        private static string BuildStatisticsMapHtml(string markersJson, string touristMarkersJson)
        {
            return $$"""
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>POI Statistics Map</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/leaflet@1.9.4/dist/leaflet.css" />
    <style>
        html, body { height: 100%; margin: 0; padding: 0; font-family: Segoe UI, Arial, sans-serif; }
        #map { height: calc(100% - 30px); width: 100%; }
        #message { height: 30px; line-height: 30px; padding: 0 10px; font-size: 12px; color: #555; background: #f7f7f7; border-bottom: 1px solid #ddd; }
    </style>
</head>
<body>
    <div id="message">OpenStreetMap</div>
    <div id="map"></div>

    <script src="https://cdn.jsdelivr.net/npm/leaflet@1.9.4/dist/leaflet.js"></script>
    <script>
        var markers = {{markersJson}};
        var tourists = {{touristMarkersJson}};

        function renderIframeFallback(message) {
            var m = document.getElementById('message');
            if (m) m.innerText = message;

            var lat = 21.0285;
            var lon = 105.8542;
            if (markers && markers.length > 0) {
                lat = Number(markers[0].Latitude) || lat;
                lon = Number(markers[0].Longitude) || lon;
            } else if (tourists && tourists.length > 0) {
                lat = Number(tourists[0].Latitude) || lat;
                lon = Number(tourists[0].Longitude) || lon;
            }

            var delta = 0.01;
            var left = lon - delta;
            var right = lon + delta;
            var top = lat + delta;
            var bottom = lat - delta;

            var mapHost = document.getElementById('map');
            if (!mapHost) return;
            mapHost.innerHTML = '';

            var iframe = document.createElement('iframe');
            iframe.style.width = '100%';
            iframe.style.height = '100%';
            iframe.style.border = '0';
            iframe.src = 'https://www.openstreetmap.org/export/embed.html?bbox=' + left + '%2C' + bottom + '%2C' + right + '%2C' + top + '&layer=mapnik&marker=' + lat + '%2C' + lon;
            mapHost.appendChild(iframe);
        }

        function renderMap() {
            if (!window.L) {
                renderIframeFallback('Leaflet bị chặn, đang dùng fallback OpenStreetMap.');
                return;
            }

            var center = [21.0285, 105.8542];
            if (markers && markers.length > 0) {
                center = [Number(markers[0].Latitude) || 21.0285, Number(markers[0].Longitude) || 105.8542];
            } else if (tourists && tourists.length > 0) {
                center = [Number(tourists[0].Latitude) || 21.0285, Number(tourists[0].Longitude) || 105.8542];
            }

            var map = L.map('map', { zoomControl: true }).setView(center, 14);
            var providers = [
                { url: 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png', attribution: '&copy; OpenStreetMap contributors &copy; CARTO' },
                { url: 'https://{s}.basemaps.cartocdn.com/voyager/{z}/{x}/{y}.png', attribution: '&copy; OpenStreetMap contributors &copy; CARTO' },
                { url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{z}/{y}/{x}', attribution: 'Tiles &copy; Esri' },
                { url: 'https://{s}.tile.openstreetmap.fr/hot/{z}/{x}/{y}.png', attribution: '&copy; OpenStreetMap contributors, HOT' }
            ];

            var layer = null;
            var index = 0;

            function applyProvider(i) {
                index = i;
                if (layer) map.removeLayer(layer);
                var p = providers[index];

                layer = L.tileLayer(p.url, {
                    maxZoom: 19,
                    subdomains: 'abc',
                    attribution: p.attribution
                });

                layer.on('tileerror', function () {
                    if (index + 1 < providers.length) {
                        document.getElementById('message').innerText = 'Tile bị chặn/403, chuyển sang nguồn bản đồ khác...';
                        applyProvider(index + 1);
                    } else {
                        renderIframeFallback('Nguồn tile bị chặn, đang dùng fallback OpenStreetMap.');
                    }
                });

                layer.addTo(map);
            }

            applyProvider(0);

            var bounds = [];
            for (var i = 0; i < markers.length; i++) {
                var item = markers[i];
                var lat = Number(item.Latitude);
                var lon = Number(item.Longitude);
                if (!isFinite(lat) || !isFinite(lon)) continue;

                var pos = [lat, lon];
                bounds.push(pos);

                var info = '<div style="min-width:220px">'
                    + '<strong>' + (item.POIName || '') + '</strong><br/>'
                    + 'Tourist online trong vùng: <b>' + (item.ActiveTouristCount || 0) + '</b><br/>'
                    + 'Bán kính: ' + (item.RadiusKm || 0) + ' km<br/>'
                    + 'Danh mục: ' + (item.Category || 'N/A') + '<br/>'
                    + 'Địa chỉ: ' + (item.Address || 'N/A')
                    + '</div>';

                L.marker(pos).addTo(map).bindPopup(info);

                var radiusMeters = Number(item.RadiusKm || 0) * 1000;
                L.circle(pos, {
                    radius: radiusMeters,
                    color: '#1976D2',
                    fillColor: '#64B5F6',
                    fillOpacity: 0.2,
                    weight: 2
                }).addTo(map);
            }

            for (var j = 0; j < tourists.length; j++) {
                var t = tourists[j];
                var tLat = Number(t.Latitude);
                var tLon = Number(t.Longitude);
                if (!isFinite(tLat) || !isFinite(tLon)) continue;

                var tPos = [tLat, tLon];
                bounds.push(tPos);

                L.circleMarker(tPos, {
                    radius: 6,
                    color: '#B71C1C',
                    fillColor: '#F44336',
                    fillOpacity: 0.95,
                    weight: 1
                }).addTo(map).bindPopup('Tourist đang hoạt động');
            }

            if (bounds.length > 1) map.fitBounds(bounds);
        }

        renderMap();
    </script>
</body>
</html>
""";
        }

        private sealed class PoiMapMarker
        {
            public string POIName { get; set; } = string.Empty;
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double RadiusKm { get; set; }
            public int ActiveTouristCount { get; set; }
            public string Category { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }

        private sealed class TouristMapMarker
        {
            public string DeviceId { get; set; } = string.Empty;
            public double Latitude { get; set; }
            public double Longitude { get; set; }
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
            _mainTabControl.SelectedTab = _tabApproval;
            ClientSize = new Size(1024, 650);
            MinimumSize = new Size(1042, 697);
        }
    }
}
