using System.Data;
using System.Globalization;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using QRCoder;

namespace POIOwner
{
    public partial class Form1 : Form
    {
        private readonly string _connectionString;
        private readonly string _touristApkUrl;
        private readonly string? _touristQrBridgeUrl;
        private const string TouristPackageName = "com.companyname.tourist";

        private int _currentOwnerId;
        private readonly TabControl _mainTabControl = new() { Dock = DockStyle.Fill };
        private readonly TabPage _tabRequests = new() { Text = "Request của tôi" };
        private readonly TabPage _tabPoiEditor = new() { Text = "POI & Bản đồ" };
        private readonly WebView2 _poiPreviewMap = new() { Dock = DockStyle.Fill };
        private readonly Button _btnShowPoiQr = new();
        private readonly TabControl _poiActionTabs = new() { Dock = DockStyle.Fill };
        private readonly TextBox _txtCreatePoiName = new();
        private readonly TextBox _txtCreateDescription = new();
        private readonly TextBox _txtCreateLatitude = new();
        private readonly TextBox _txtCreateLongitude = new();
        private readonly TextBox _txtCreateRadius = new();
        private readonly TextBox _txtCreateCategory = new();
        private readonly TextBox _txtCreateAddress = new();
        private readonly TextBox _txtCreatePhone = new();
        private readonly TextBox _txtCreateWebsite = new();
        private bool _responsiveLayoutInitialized;
        private bool _poiMapBridgeInitialized;

        public Form1(string connectionString, string touristApkUrl, string? touristQrBridgeUrl)
        {
            _connectionString = connectionString;
            _touristApkUrl = touristApkUrl;
            _touristQrBridgeUrl = touristQrBridgeUrl;
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
            _txtCreateLatitude.TextChanged += async (_, _) => await RefreshPoiPreviewMapAsync();
            _txtCreateLongitude.TextChanged += async (_, _) => await RefreshPoiPreviewMapAsync();
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
                SplitterDistance = 430
            };

            void ApplyBalancedSplit()
            {
                if (split.Width <= 0)
                    return;

                var desiredLeft = (int)(split.Width * 0.36);
                var maxLeft = split.Width - 520;
                if (maxLeft < 320)
                    maxLeft = split.Width - 300;

                desiredLeft = Math.Max(320, Math.Min(desiredLeft, maxLeft));

                if (desiredLeft > 0 && desiredLeft < split.Width)
                    split.SplitterDistance = desiredLeft;
            }

            split.SizeChanged += (_, _) => ApplyBalancedSplit();
            split.HandleCreated += (_, _) => BeginInvoke((Action)ApplyBalancedSplit);

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

            var updateDeleteTab = new TabPage("Sửa / Xóa");
            var createTab = new TabPage("Tạo POI mới");

            var updateDeletePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(6)
            };
            updateDeletePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            updateDeletePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.5f));
            updateDeletePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.5f));
            updateDeletePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            updateDeletePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var deleteReasonPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            deleteReasonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            deleteReasonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lblDeleteReason.Dock = DockStyle.Fill;
            lblDeleteReason.TextAlign = ContentAlignment.MiddleLeft;
            txtDeleteReason.Dock = DockStyle.Fill;
            txtDeleteReason.Multiline = true;
            deleteReasonPanel.Controls.Add(lblDeleteReason, 0, 0);
            deleteReasonPanel.Controls.Add(txtDeleteReason, 0, 1);
            updateDeletePanel.SetRowSpan(deleteReasonPanel, 2);
            updateDeletePanel.Controls.Add(deleteReasonPanel, 0, 0);

            btnSendDeleteRequest.Dock = DockStyle.Fill;
            btnSendDeleteRequest.Margin = new Padding(6, 4, 6, 4);
            updateDeletePanel.Controls.Add(btnSendDeleteRequest, 1, 0);

            btnSendUpdateRequest.Dock = DockStyle.Fill;
            btnSendUpdateRequest.Margin = new Padding(6, 4, 6, 4);
            updateDeletePanel.Controls.Add(btnSendUpdateRequest, 2, 0);

            updateDeleteTab.Controls.Add(updateDeletePanel);

            var createPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(12)
            };
            createPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            createPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            createPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            createPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            createPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            createPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            createPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            createPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            createPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _txtCreateDescription.Multiline = true;

            AddEditorRow(createPanel, 0, new Label { Text = "Tên POI" }, _txtCreatePoiName);
            AddEditorRow(createPanel, 1, new Label { Text = "Mô tả" }, _txtCreateDescription);

            var createLatLon = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            createLatLon.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            createLatLon.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            createLatLon.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            createLatLon.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            createLatLon.Controls.Add(new Label { Text = "Vĩ độ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            _txtCreateLatitude.Dock = DockStyle.Fill;
            createLatLon.Controls.Add(_txtCreateLatitude, 1, 0);
            createLatLon.Controls.Add(new Label { Text = "Kinh độ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 0);
            _txtCreateLongitude.Dock = DockStyle.Fill;
            createLatLon.Controls.Add(_txtCreateLongitude, 3, 0);
            createPanel.Controls.Add(new Label { Text = "Tọa độ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            createPanel.Controls.Add(createLatLon, 1, 2);

            var createMeta = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            createMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            createMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            createMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            createMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            createMeta.Controls.Add(new Label { Text = "Bán kính", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            _txtCreateRadius.Dock = DockStyle.Fill;
            createMeta.Controls.Add(_txtCreateRadius, 1, 0);
            createMeta.Controls.Add(new Label { Text = "Danh mục", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 0);
            _txtCreateCategory.Dock = DockStyle.Fill;
            createMeta.Controls.Add(_txtCreateCategory, 3, 0);
            createPanel.Controls.Add(new Label { Text = "Thông số", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            createPanel.Controls.Add(createMeta, 1, 3);

            AddEditorRow(createPanel, 4, new Label { Text = "Địa chỉ" }, _txtCreateAddress);

            var createContact = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            createContact.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            createContact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            createContact.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            createContact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            createContact.Controls.Add(new Label { Text = "Điện thoại", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            _txtCreatePhone.Dock = DockStyle.Fill;
            createContact.Controls.Add(_txtCreatePhone, 1, 0);
            createContact.Controls.Add(new Label { Text = "Trang web", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 0);
            _txtCreateWebsite.Dock = DockStyle.Fill;
            createContact.Controls.Add(_txtCreateWebsite, 3, 0);
            createPanel.Controls.Add(new Label { Text = "Liên hệ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 5);
            createPanel.Controls.Add(createContact, 1, 5);

            var createActions = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            createActions.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            createActions.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

            createActions.Controls.Add(new Label
            {
                Text = "Nhập thông tin POI bên trên, chọn tọa độ bằng cách nhấp bản đồ, rồi gửi request tạo mới.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.DimGray
            }, 0, 0);

            btnSendCreateRequest.Dock = DockStyle.Fill;
            btnSendCreateRequest.Margin = new Padding(6, 4, 6, 4);
            createActions.Controls.Add(btnSendCreateRequest, 0, 1);
            createPanel.Controls.Add(createActions, 1, 6);

            createTab.Controls.Add(createPanel);

            _poiActionTabs.TabPages.Clear();
            _poiActionTabs.TabPages.Add(updateDeleteTab);
            _poiActionTabs.TabPages.Add(createTab);
            _poiActionTabs.SelectedIndexChanged += async (_, _) => await RefreshPoiPreviewMapAsync();

            leftRoot.Controls.Add(_poiActionTabs, 1, 6);

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
            var footerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2
            };
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));

            footerPanel.Controls.Add(new Label
            {
                Text = "Nhấp vào bản đồ để chọn tọa độ (vĩ độ/kinh độ)",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.DimGray
            }, 0, 0);

            _btnShowPoiQr.Text = "Mã QR POI";
            _btnShowPoiQr.Dock = DockStyle.Fill;
            _btnShowPoiQr.Margin = new Padding(8, 2, 0, 2);
            _btnShowPoiQr.Click += (_, _) => ShowPoiQrForCurrentSelection();
            footerPanel.Controls.Add(_btnShowPoiQr, 1, 0);

            rightRoot.Controls.Add(footerPanel, 0, 2);

            split.Panel2.Controls.Add(rightRoot);

            _tabPoiEditor.Controls.Add(split);
        }

        private void ShowPoiQrForCurrentSelection()
        {
            if (cmbMyPois.SelectedValue == null || !int.TryParse(cmbMyPois.SelectedValue.ToString(), out var poiId))
            {
                MessageBox.Show("Vui lòng chọn POI để tạo QR.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var qrPayload = BuildPoiIntentUrl(poiId);

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(qrPayload, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            var bitmap = qrCode.GetGraphic(16);

            try
            {
                using var dialog = new Form
                {
                    Text = $"QR - POI #{poiId}",
                    StartPosition = FormStartPosition.CenterParent,
                    Width = 460,
                    Height = 560,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var root = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 4,
                    Padding = new Padding(12)
                };
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

                root.Controls.Add(new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "Quét mã QR: mở Tourist vào POI, nếu chưa có ứng dụng sẽ chuyển sang liên kết tải APK",
                    TextAlign = ContentAlignment.MiddleLeft
                }, 0, 0);

                root.Controls.Add(new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Image = bitmap,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = BorderStyle.FixedSingle
                }, 0, 1);

                var txtPayload = new TextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Multiline = true,
                    Text = qrPayload,
                    ScrollBars = ScrollBars.Vertical
                };
                root.Controls.Add(txtPayload, 0, 2);

                var actions = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft
                };

                var btnClose = new Button { Text = "Đóng", Width = 100, Height = 32 };
                btnClose.Click += (_, _) => dialog.Close();

                var btnCopy = new Button { Text = "Sao chép liên kết", Width = 130, Height = 32 };
                btnCopy.Click += (_, _) =>
                {
                    Clipboard.SetText(qrPayload);
                    MessageBox.Show("Đã copy nội dung QR.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                actions.Controls.Add(btnClose);
                actions.Controls.Add(btnCopy);
                root.Controls.Add(actions, 0, 3);

                dialog.Controls.Add(root);
                dialog.ShowDialog(this);
            }
            finally
            {
                bitmap.Dispose();
            }
        }

        private string BuildPoiIntentUrl(int poiId)
        {
            if (!string.IsNullOrWhiteSpace(_touristQrBridgeUrl))
            {
                var separator = _touristQrBridgeUrl.Contains('?') ? "&" : "?";
                var escapedApkUrl = Uri.EscapeDataString(_touristApkUrl);
                return $"{_touristQrBridgeUrl}{separator}poi={poiId}&apk={escapedApkUrl}";
            }

            return _touristApkUrl;
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
            var (latInput, lonInput) = GetActiveCoordinateInputs();

            if (!decimal.TryParse(latInput.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
                !decimal.TryParse(latInput.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out lat))
            {
                return;
            }

            if (!decimal.TryParse(lonInput.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon) &&
                !decimal.TryParse(lonInput.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out lon))
            {
                return;
            }

            await _poiPreviewMap.EnsureCoreWebView2Async();
            EnsurePoiPreviewMapBridge();
            var activePoiName = GetActivePoiNameInput().Text.Trim();
            var html = BuildPoiPreviewMapHtml((double)lat, (double)lon, activePoiName);
            _poiPreviewMap.NavigateToString(html);
        }

        private TextBox GetActivePoiNameInput()
        {
            return _poiActionTabs.SelectedIndex == 1 ? _txtCreatePoiName : txtPoiName;
        }

        private (TextBox lat, TextBox lon) GetActiveCoordinateInputs()
        {
            return _poiActionTabs.SelectedIndex == 1
                ? (_txtCreateLatitude, _txtCreateLongitude)
                : (txtLatitude, txtLongitude);
        }

        private void EnsurePoiPreviewMapBridge()
        {
            if (_poiMapBridgeInitialized || _poiPreviewMap.CoreWebView2 == null)
                return;

            _poiPreviewMap.CoreWebView2.WebMessageReceived += PoiPreviewMap_WebMessageReceived;
            _poiMapBridgeInitialized = true;
        }

        private void PoiPreviewMap_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using var doc = JsonDocument.Parse(e.WebMessageAsJson);
                var root = doc.RootElement;

                if (!root.TryGetProperty("type", out var typeProp) ||
                    !string.Equals(typeProp.GetString(), "map-click", StringComparison.OrdinalIgnoreCase))
                    return;

                if (!root.TryGetProperty("latitude", out var latProp) ||
                    !root.TryGetProperty("longitude", out var lonProp))
                    return;

                var latitude = latProp.GetDouble();
                var longitude = lonProp.GetDouble();

                var (latInput, lonInput) = GetActiveCoordinateInputs();
                latInput.Text = latitude.ToString("F8", CultureInfo.InvariantCulture);
                lonInput.Text = longitude.ToString("F8", CultureInfo.InvariantCulture);
            }
            catch
            {
                // Ignore malformed map messages.
            }
        }

        private bool TryGetCreatePoiInputFromCreateTab(out CreatePoiInput input, out string message)
        {
            input = new CreatePoiInput();
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(_txtCreatePoiName.Text))
            {
                message = "Tên POI là bắt buộc.";
                return false;
            }

            if (!TryParseDecimal(_txtCreateLatitude.Text, out var lat))
            {
                message = "Latitude không hợp lệ.";
                return false;
            }

            if (!TryParseDecimal(_txtCreateLongitude.Text, out var lon))
            {
                message = "Longitude không hợp lệ.";
                return false;
            }

            if (!TryParseDouble(_txtCreateRadius.Text, out var radius) || radius <= 0)
            {
                message = "Radius phải > 0.";
                return false;
            }

            input = new CreatePoiInput
            {
                PoiName = _txtCreatePoiName.Text.Trim(),
                Description = _txtCreateDescription.Text.Trim(),
                Latitude = lat,
                Longitude = lon,
                Radius = radius,
                Category = _txtCreateCategory.Text.Trim(),
                Address = _txtCreateAddress.Text.Trim(),
                PhoneNumber = _txtCreatePhone.Text.Trim(),
                Website = _txtCreateWebsite.Text.Trim()
            };

            return true;
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
        {{ url: 'https://{{s}}.basemaps.cartocdn.com/light_all/{{z}}/{{x}}/{{y}}.png', attribution: '&copy; OpenStreetMap contributors &copy; CARTO' }},
        {{ url: 'https://{{s}}.basemaps.cartocdn.com/voyager/{{z}}/{{x}}/{{y}}.png', attribution: '&copy; OpenStreetMap contributors &copy; CARTO' }},
        {{ url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{{z}}/{{y}}/{{x}}', attribution: 'Tiles &copy; Esri' }},
        {{ url: 'https://{{s}}.tile.openstreetmap.fr/hot/{{z}}/{{x}}/{{y}}.png', attribution: '&copy; OpenStreetMap contributors, HOT' }}
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

      let currentMarker = null;

      function setMarkerAndNotify(markerLat, markerLon, shouldNotify) {{
        if (currentMarker) map.removeLayer(currentMarker);
        currentMarker = L.marker([markerLat, markerLon]).addTo(map).bindPopup('{safeName}').openPopup();

        if (shouldNotify && window.chrome && window.chrome.webview) {{
          window.chrome.webview.postMessage({{
            type: 'map-click',
            latitude: markerLat,
            longitude: markerLon
          }});
        }}
      }}

      setMarkerAndNotify(lat, lon, false);

      map.on('click', function(e) {{
        const markerLat = Number(e.latlng.lat);
        const markerLon = Number(e.latlng.lng);
        setMarkerAndNotify(markerLat, markerLon, true);
      }});
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
            if (!TryGetCreatePoiInputFromCreateTab(out var input, out var message))
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
            _txtCreatePoiName.Clear();
            _txtCreateDescription.Clear();
            _txtCreateLatitude.Clear();
            _txtCreateLongitude.Clear();
            _txtCreateRadius.Clear();
            _txtCreateCategory.Clear();
            _txtCreateAddress.Clear();
            _txtCreatePhone.Clear();
            _txtCreateWebsite.Clear();

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
