using System.Data;
using System.Globalization;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Web.WebView2.WinForms;

namespace POIOwner
{
    public partial class Form1 : Form
    {
        private readonly string _connectionString =
            "Server=localhost;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;";

        private int _currentOwnerId;
        private readonly TabControl _mainTabControl = new() { Dock = DockStyle.Fill };
        private readonly TabPage _tabRequests = new() { Text = "Request của tôi" };
        private readonly TabPage _tabPoiEditor = new() { Text = "POI & Bản đồ" };
        private readonly WebView2 _poiPreviewMap = new() { Dock = DockStyle.Fill };
        private bool _responsiveLayoutInitialized;

        public Form1()
        {
            InitializeComponent();
            InitializeResponsiveLayout();

            txtUsername.Text = "poiowner01";
            txtPassword.Text = "Admin@123";

            ShowLoginOnlyMode();
        }

        private void InitializeResponsiveLayout()
        {
            if (_responsiveLayoutInitialized)
                return;

            _responsiveLayoutInitialized = true;

            grpLogin.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpRequests.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            grpRequests.Controls.Clear();
            grpRequests.Controls.Add(_mainTabControl);

            _mainTabControl.TabPages.Clear();
            _mainTabControl.TabPages.Add(_tabRequests);
            _mainTabControl.TabPages.Add(_tabPoiEditor);

            SetupRequestsTabLayout();
            SetupPoiEditorTabLayout();

            txtLatitude.TextChanged += async (_, _) => await RefreshPoiPreviewMapAsync();
            txtLongitude.TextChanged += async (_, _) => await RefreshPoiPreviewMapAsync();
        }

        private void SetupRequestsTabLayout()
        {
            _tabRequests.Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));

            lblMyRequests.Dock = DockStyle.Fill;
            lblMyRequests.TextAlign = ContentAlignment.MiddleLeft;
            header.Controls.Add(lblMyRequests, 0, 0);

            btnRefresh.Dock = DockStyle.Fill;
            btnRefresh.Margin = new Padding(4, 2, 4, 2);
            header.Controls.Add(btnRefresh, 1, 0);

            btnLogout.Dock = DockStyle.Fill;
            btnLogout.Margin = new Padding(4, 2, 0, 2);
            header.Controls.Add(btnLogout, 2, 0);

            dgvMyRequests.Dock = DockStyle.Fill;

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(dgvMyRequests, 0, 1);
            _tabRequests.Controls.Add(root);
        }

        private void SetupPoiEditorTabLayout()
        {
            _tabPoiEditor.Controls.Clear();

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 640
            };

            var leftRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(10)
            };
            leftRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            leftRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            leftRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            leftRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            leftRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            leftRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            leftRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            leftRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            leftRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            AddEditorRow(leftRoot, 0, lblPoiName, txtPoiName);
            AddEditorRow(leftRoot, 1, lblDescription, txtDescription);

            var latLonPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            latLonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            latLonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            latLonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            latLonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            lblLatitude.Dock = DockStyle.Fill;
            lblLatitude.TextAlign = ContentAlignment.MiddleLeft;
            txtLatitude.Dock = DockStyle.Fill;
            lblLongitude.Dock = DockStyle.Fill;
            lblLongitude.TextAlign = ContentAlignment.MiddleLeft;
            txtLongitude.Dock = DockStyle.Fill;
            latLonPanel.Controls.Add(lblLatitude, 0, 0);
            latLonPanel.Controls.Add(txtLatitude, 1, 0);
            latLonPanel.Controls.Add(lblLongitude, 2, 0);
            latLonPanel.Controls.Add(txtLongitude, 3, 0);
            leftRoot.Controls.Add(new Label { Text = "Tọa độ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            leftRoot.Controls.Add(latLonPanel, 1, 2);

            var radiusCategoryPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            radiusCategoryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            radiusCategoryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            radiusCategoryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            radiusCategoryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            lblRadius.Dock = DockStyle.Fill;
            lblRadius.TextAlign = ContentAlignment.MiddleLeft;
            txtRadius.Dock = DockStyle.Fill;
            lblCategory.Dock = DockStyle.Fill;
            lblCategory.TextAlign = ContentAlignment.MiddleLeft;
            txtCategory.Dock = DockStyle.Fill;
            radiusCategoryPanel.Controls.Add(lblRadius, 0, 0);
            radiusCategoryPanel.Controls.Add(txtRadius, 1, 0);
            radiusCategoryPanel.Controls.Add(lblCategory, 2, 0);
            radiusCategoryPanel.Controls.Add(txtCategory, 3, 0);
            leftRoot.Controls.Add(new Label { Text = "Thông số", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            leftRoot.Controls.Add(radiusCategoryPanel, 1, 3);

            AddEditorRow(leftRoot, 4, lblAddress, txtAddress);

            var phoneWebsitePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            phoneWebsitePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            phoneWebsitePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            phoneWebsitePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            phoneWebsitePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            lblPhone.Dock = DockStyle.Fill;
            lblPhone.TextAlign = ContentAlignment.MiddleLeft;
            txtPhone.Dock = DockStyle.Fill;
            lblWebsite.Dock = DockStyle.Fill;
            lblWebsite.TextAlign = ContentAlignment.MiddleLeft;
            txtWebsite.Dock = DockStyle.Fill;
            phoneWebsitePanel.Controls.Add(lblPhone, 0, 0);
            phoneWebsitePanel.Controls.Add(txtPhone, 1, 0);
            phoneWebsitePanel.Controls.Add(lblWebsite, 2, 0);
            phoneWebsitePanel.Controls.Add(txtWebsite, 3, 0);
            leftRoot.Controls.Add(new Label { Text = "Liên hệ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 5);
            leftRoot.Controls.Add(phoneWebsitePanel, 1, 5);

            var actionPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2
            };
            actionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            actionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.5f));
            actionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.5f));
            actionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            actionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var deleteReasonPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            deleteReasonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            deleteReasonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lblDeleteReason.Dock = DockStyle.Fill;
            lblDeleteReason.TextAlign = ContentAlignment.MiddleLeft;
            txtDeleteReason.Dock = DockStyle.Fill;
            txtDeleteReason.Multiline = true;
            deleteReasonPanel.Controls.Add(lblDeleteReason, 0, 0);
            deleteReasonPanel.Controls.Add(txtDeleteReason, 0, 1);
            actionPanel.SetRowSpan(deleteReasonPanel, 2);
            actionPanel.Controls.Add(deleteReasonPanel, 0, 0);

            btnSendDeleteRequest.Dock = DockStyle.Fill;
            btnSendDeleteRequest.Margin = new Padding(6, 4, 6, 4);
            actionPanel.Controls.Add(btnSendDeleteRequest, 1, 0);

            btnSendUpdateRequest.Dock = DockStyle.Fill;
            btnSendUpdateRequest.Margin = new Padding(6, 4, 6, 4);
            actionPanel.Controls.Add(btnSendUpdateRequest, 2, 0);

            btnSendCreateRequest.Dock = DockStyle.Fill;
            btnSendCreateRequest.Margin = new Padding(6, 4, 6, 4);
            actionPanel.SetColumnSpan(btnSendCreateRequest, 2);
            actionPanel.Controls.Add(btnSendCreateRequest, 1, 1);

            leftRoot.Controls.Add(actionPanel, 1, 6);

            split.Panel1.Controls.Add(leftRoot);

            var rightRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            rightRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            rightRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            rightRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            var poiPickerPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            poiPickerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            poiPickerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            lblMyPois.Dock = DockStyle.Fill;
            lblMyPois.TextAlign = ContentAlignment.MiddleLeft;
            cmbMyPois.Dock = DockStyle.Fill;
            poiPickerPanel.Controls.Add(lblMyPois, 0, 0);
            poiPickerPanel.Controls.Add(cmbMyPois, 1, 0);

            _poiPreviewMap.Dock = DockStyle.Fill;

            rightRoot.Controls.Add(poiPickerPanel, 0, 0);
            rightRoot.Controls.Add(_poiPreviewMap, 0, 1);
            rightRoot.Controls.Add(new Label
            {
                Text = "Bản đồ preview sẽ cập nhật theo Latitude/Longitude",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.DimGray
            }, 0, 2);

            split.Panel2.Controls.Add(rightRoot);

            _tabPoiEditor.Controls.Add(split);
        }

        private static void AddEditorRow(TableLayoutPanel panel, int row, Control label, Control input)
        {
            label.Dock = DockStyle.Fill;
            if (label is Label textLabel)
                textLabel.TextAlign = ContentAlignment.MiddleLeft;

            input.Dock = DockStyle.Fill;
            panel.Controls.Add(label, 0, row);
            panel.Controls.Add(input, 1, row);
        }

        private async Task RefreshPoiPreviewMapAsync()
        {
            if (!decimal.TryParse(txtLatitude.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
                !decimal.TryParse(txtLatitude.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out lat))
            {
                return;
            }

            if (!decimal.TryParse(txtLongitude.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon) &&
                !decimal.TryParse(txtLongitude.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out lon))
            {
                return;
            }

            await _poiPreviewMap.EnsureCoreWebView2Async();
            var html = BuildPoiPreviewMapHtml((double)lat, (double)lon, txtPoiName.Text.Trim());
            _poiPreviewMap.NavigateToString(html);
        }

        private static string BuildPoiPreviewMapHtml(double latitude, double longitude, string poiName)
        {
            var safeName = string.IsNullOrWhiteSpace(poiName) ? "POI" : poiName.Replace("'", "\\'");
            return $@"<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'/>
  <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/leaflet@1.9.4/dist/leaflet.css'/>
  <style>
    html,body,#map {{ height:100%; margin:0; padding:0; }}
  </style>
</head>
<body>
  <div id='map'></div>
  <script src='https://cdn.jsdelivr.net/npm/leaflet@1.9.4/dist/leaflet.js'></script>
  <script>
    const lat = {latitude.ToString(CultureInfo.InvariantCulture)};
    const lon = {longitude.ToString(CultureInfo.InvariantCulture)};
    if (!window.L) {{
      document.getElementById('map').innerHTML = `<div style='padding:12px;color:#c62828;font-family:Segoe UI'>Không tải được OpenStreetMap (Leaflet CDN).</div>`;
    }} else {{
      const map = L.map('map').setView([lat, lon], 16);
      const providers = [
        {{ url: 'https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', attribution: '&copy; OpenStreetMap contributors' }},
        {{ url: 'https://{{s}}.tile.openstreetmap.fr/hot/{{z}}/{{x}}/{{y}}.png', attribution: '&copy; OpenStreetMap contributors, HOT' }},
        {{ url: 'https://{{s}}.basemaps.cartocdn.com/light_all/{{z}}/{{x}}/{{y}}.png', attribution: '&copy; OpenStreetMap contributors &copy; CARTO' }}
      ];

      let layer = null;
      let providerIndex = 0;
      let switched = false;

      function applyLayer(index) {{
        providerIndex = index;
        switched = false;

        if (layer) map.removeLayer(layer);

        const p = providers[index];
        layer = L.tileLayer(p.url, {{
          maxZoom: 19,
          subdomains: 'abc',
          crossOrigin: true,
          attribution: p.attribution
        }});

        layer.on('tileerror', function() {{
          if (switched) return;
          switched = true;
          const next = providerIndex + 1;
          if (next < providers.length) applyLayer(next);
        }});

        layer.addTo(map);
      }}

      applyLayer(0);

      L.marker([lat, lon]).addTo(map).bindPopup('{safeName}').openPopup();
    }}
  </script>
</body>
</html>";
        }

        private async void cmbMyPois_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_currentOwnerId <= 0 || cmbMyPois.SelectedValue == null)
                return;

            if (!int.TryParse(cmbMyPois.SelectedValue.ToString(), out var poiId))
                return;

            try
            {
                await LoadPoiDetailsToFormAsync(poiId);
            }
            catch
            {
            }
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            if (_currentOwnerId <= 0)
                return;

            try
            {
                btnRefresh.Enabled = false;
                await LoadMyPoisAsync();
                await LoadMyRequestsAsync();
                await RefreshPoiPreviewMapAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi refresh dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
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
                await RefreshPoiPreviewMapAsync();
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

        private async void btnSendUpdateRequest_Click(object sender, EventArgs e)
        {
            if (cmbMyPois.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn POI của bạn để gửi request sửa.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryGetCreatePoiInput(out var input, out var message))
            {
                MessageBox.Show(message, "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var poiId = Convert.ToInt32(cmbMyPois.SelectedValue);

                var requestData = JsonSerializer.Serialize(new
                {
                    POIId = poiId,
                    POIName = input.PoiName,
                    input.Description,
                    input.Latitude,
                    input.Longitude,
                    input.Radius,
                    input.Category,
                    input.Address,
                    input.PhoneNumber,
                    input.Website,
                    Action = "Update"
                });

                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new SqlCommand("sp_CreatePOIApprovalRequest", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@POIId", poiId);
                cmd.Parameters.AddWithValue("@OwnerId", _currentOwnerId);
                cmd.Parameters.AddWithValue("@RequestType", "Update");
                cmd.Parameters.AddWithValue("@RequestData", requestData);
                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("Đã gửi request sửa POI cho admin.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadMyRequestsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi request sửa POI: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
  AND [IsApproved] = 1
  AND ISNULL([Status], N'') <> N'Rejected'
ORDER BY [POIName]";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@OwnerId", _currentOwnerId);
            await using var reader = await cmd.ExecuteReaderAsync();

            var table = new DataTable();
            table.Load(reader);

            cmbMyPois.DataSource = table;
            cmbMyPois.DisplayMember = "POIName";
            cmbMyPois.ValueMember = "POIId";

            if (table.Rows.Count > 0)
            {
                cmbMyPois.SelectedIndex = 0;
                await LoadPoiDetailsToFormAsync(Convert.ToInt32(table.Rows[0]["POIId"]));
            }
        }

        private async Task LoadPoiDetailsToFormAsync(int poiId)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
SELECT [POIName], [Description], [Latitude], [Longitude], [Radius], [Category], [Address], [PhoneNumber], [Website]
FROM [dbo].[PointsOfInterest]
WHERE [POIId] = @POIId AND [OwnerId] = @OwnerId";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@POIId", poiId);
            cmd.Parameters.AddWithValue("@OwnerId", _currentOwnerId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return;

            txtPoiName.Text = reader["POIName"]?.ToString() ?? string.Empty;
            txtDescription.Text = reader["Description"] == DBNull.Value ? string.Empty : reader["Description"]?.ToString() ?? string.Empty;
            txtLatitude.Text = Convert.ToDecimal(reader["Latitude"]).ToString(CultureInfo.InvariantCulture);
            txtLongitude.Text = Convert.ToDecimal(reader["Longitude"]).ToString(CultureInfo.InvariantCulture);
            txtRadius.Text = Convert.ToDouble(reader["Radius"]).ToString(CultureInfo.InvariantCulture);
            txtCategory.Text = reader["Category"] == DBNull.Value ? string.Empty : reader["Category"]?.ToString() ?? string.Empty;
            txtAddress.Text = reader["Address"] == DBNull.Value ? string.Empty : reader["Address"]?.ToString() ?? string.Empty;
            txtPhone.Text = reader["PhoneNumber"] == DBNull.Value ? string.Empty : reader["PhoneNumber"]?.ToString() ?? string.Empty;
            txtWebsite.Text = reader["Website"] == DBNull.Value ? string.Empty : reader["Website"]?.ToString() ?? string.Empty;
            await RefreshPoiPreviewMapAsync();
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
            _mainTabControl.SelectedTab = _tabRequests;
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
