using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;

namespace Admin
{
    internal sealed class PoiManagementForm : Form
    {
        private readonly string _connectionString;

        private readonly DataGridView _dgvPois = new();
        private readonly TextBox _txtPoiName = new();
        private readonly TextBox _txtDescription = new();
        private readonly TextBox _txtLatitude = new();
        private readonly TextBox _txtLongitude = new();
        private readonly TextBox _txtRadius = new();
        private readonly TextBox _txtCategory = new();
        private readonly TextBox _txtAddress = new();
        private readonly TextBox _txtPhone = new();
        private readonly TextBox _txtWebsite = new();

        private readonly Button _btnAdd = new();
        private readonly Button _btnUpdate = new();
        private readonly Button _btnDelete = new();
        private readonly Button _btnReload = new();

        private int? _selectedPoiId;

        public PoiManagementForm(string connectionString)
        {
            _connectionString = connectionString;
            Text = "Quản lý POI (Thêm / Sửa / Xóa)";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1200;
            Height = 640;
            MinimumSize = new Size(1200, 640);

            BuildUi();
            Shown += async (_, _) => await LoadPoisAsync();
        }

        private void BuildUi()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 12));
            Controls.Add(root);

            _dgvPois.Dock = DockStyle.Fill;
            _dgvPois.ReadOnly = true;
            _dgvPois.MultiSelect = false;
            _dgvPois.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvPois.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvPois.SelectionChanged += DgvPois_SelectionChanged;
            root.Controls.Add(_dgvPois, 0, 0);

            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 5,
                Padding = new Padding(0, 10, 0, 0)
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));

            AddField(fields, 0, "Tên POI", _txtPoiName, "Mô tả", _txtDescription);
            AddField(fields, 1, "Latitude", _txtLatitude, "Longitude", _txtLongitude);
            AddField(fields, 2, "Radius (km)", _txtRadius, "Category", _txtCategory);
            AddField(fields, 3, "Địa chỉ", _txtAddress, "Điện thoại", _txtPhone);
            AddField(fields, 4, "Website", _txtWebsite, string.Empty, new TextBox { Visible = false, Enabled = false });

            root.Controls.Add(fields, 0, 1);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0)
            };

            _btnAdd.Text = "Thêm POI";
            _btnAdd.Width = 160;
            _btnAdd.Height = 40;
            _btnAdd.Click += async (_, _) => await AddPoiAsync();

            _btnUpdate.Text = "Sửa POI";
            _btnUpdate.Width = 160;
            _btnUpdate.Height = 40;
            _btnUpdate.Click += async (_, _) => await UpdatePoiAsync();

            _btnDelete.Text = "Xóa POI";
            _btnDelete.Width = 160;
            _btnDelete.Height = 40;
            _btnDelete.Click += async (_, _) => await DeletePoiAsync();

            _btnReload.Text = "Tải lại";
            _btnReload.Width = 160;
            _btnReload.Height = 40;
            _btnReload.Click += async (_, _) => await LoadPoisAsync();

            actions.Controls.Add(_btnAdd);
            actions.Controls.Add(_btnUpdate);
            actions.Controls.Add(_btnDelete);
            actions.Controls.Add(_btnReload);
            root.Controls.Add(actions, 0, 2);
        }

        private static void AddField(TableLayoutPanel panel, int row, string label1, Control control1, string label2, Control control2)
        {
            panel.Controls.Add(new Label { Text = label1, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) }, 0, row);
            control1.Dock = DockStyle.Fill;
            panel.Controls.Add(control1, 1, row);

            if (!string.IsNullOrWhiteSpace(label2))
                panel.Controls.Add(new Label { Text = label2, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) }, 2, row);

            control2.Dock = DockStyle.Fill;
            panel.Controls.Add(control2, 3, row);
        }

        private async Task LoadPoisAsync()
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"
SELECT [POIId], [POIName], [Description], [Latitude], [Longitude], [Radius], [Category], [Address], [PhoneNumber], [Website], [IsApproved], [Status]
FROM [dbo].[PointsOfInterest]
ORDER BY [POIId] DESC";

                await using var cmd = new SqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                var table = new DataTable();
                table.Load(reader);
                _dgvPois.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải POI: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvPois_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dgvPois.CurrentRow == null)
                return;

            var idObj = _dgvPois.CurrentRow.Cells["POIId"].Value;
            if (idObj == null || idObj == DBNull.Value)
                return;

            _selectedPoiId = Convert.ToInt32(idObj);
            _txtPoiName.Text = _dgvPois.CurrentRow.Cells["POIName"].Value?.ToString() ?? string.Empty;
            _txtDescription.Text = _dgvPois.CurrentRow.Cells["Description"].Value?.ToString() ?? string.Empty;
            _txtLatitude.Text = _dgvPois.CurrentRow.Cells["Latitude"].Value?.ToString() ?? string.Empty;
            _txtLongitude.Text = _dgvPois.CurrentRow.Cells["Longitude"].Value?.ToString() ?? string.Empty;
            _txtRadius.Text = _dgvPois.CurrentRow.Cells["Radius"].Value?.ToString() ?? string.Empty;
            _txtCategory.Text = _dgvPois.CurrentRow.Cells["Category"].Value?.ToString() ?? string.Empty;
            _txtAddress.Text = _dgvPois.CurrentRow.Cells["Address"].Value?.ToString() ?? string.Empty;
            _txtPhone.Text = _dgvPois.CurrentRow.Cells["PhoneNumber"].Value?.ToString() ?? string.Empty;
            _txtWebsite.Text = _dgvPois.CurrentRow.Cells["Website"].Value?.ToString() ?? string.Empty;
        }

        private async Task AddPoiAsync()
        {
            if (!TryGetPoiInput(out var input, out var message))
            {
                MessageBox.Show(message, "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"
INSERT INTO [dbo].[PointsOfInterest]
([POIName], [Description], [Latitude], [Longitude], [Radius], [Category], [Address], [PhoneNumber], [Website], [OwnerId], [IsApproved], [Status], [ApprovedDate], [ApprovedBy])
VALUES
(@POIName, @Description, @Latitude, @Longitude, @Radius, @Category, @Address, @PhoneNumber, @Website, NULL, 1, N'Active', GETUTCDATE(), NULL)";

                await using var cmd = new SqlCommand(sql, conn);
                FillPoiParams(cmd, input);
                await cmd.ExecuteNonQueryAsync();

                await LoadPoisAsync();
                MessageBox.Show("Thêm POI thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm POI: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdatePoiAsync()
        {
            if (_selectedPoiId == null)
            {
                MessageBox.Show("Vui lòng chọn POI cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!TryGetPoiInput(out var input, out var message))
            {
                MessageBox.Show(message, "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = @"
UPDATE [dbo].[PointsOfInterest]
SET [POIName] = @POIName,
    [Description] = @Description,
    [Latitude] = @Latitude,
    [Longitude] = @Longitude,
    [Radius] = @Radius,
    [Category] = @Category,
    [Address] = @Address,
    [PhoneNumber] = @PhoneNumber,
    [Website] = @Website,
    [LastModifiedDate] = GETUTCDATE()
WHERE [POIId] = @POIId";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@POIId", _selectedPoiId.Value);
                FillPoiParams(cmd, input);
                await cmd.ExecuteNonQueryAsync();

                await LoadPoisAsync();
                MessageBox.Show("Cập nhật POI thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật POI: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeletePoiAsync()
        {
            if (_selectedPoiId == null)
            {
                MessageBox.Show("Vui lòng chọn POI cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show("Bạn có chắc muốn xóa POI này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
                return;

            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                const string sql = "DELETE FROM [dbo].[PointsOfInterest] WHERE [POIId] = @POIId";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@POIId", _selectedPoiId.Value);
                await cmd.ExecuteNonQueryAsync();

                _selectedPoiId = null;
                await LoadPoisAsync();
                MessageBox.Show("Xóa POI thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa POI: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void FillPoiParams(SqlCommand cmd, PoiInput input)
        {
            cmd.Parameters.AddWithValue("@POIName", input.PoiName);
            cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(input.Description) ? DBNull.Value : input.Description);
            cmd.Parameters.AddWithValue("@Latitude", input.Latitude);
            cmd.Parameters.AddWithValue("@Longitude", input.Longitude);
            cmd.Parameters.AddWithValue("@Radius", input.Radius);
            cmd.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(input.Category) ? DBNull.Value : input.Category);
            cmd.Parameters.AddWithValue("@Address", string.IsNullOrWhiteSpace(input.Address) ? DBNull.Value : input.Address);
            cmd.Parameters.AddWithValue("@PhoneNumber", string.IsNullOrWhiteSpace(input.Phone) ? DBNull.Value : input.Phone);
            cmd.Parameters.AddWithValue("@Website", string.IsNullOrWhiteSpace(input.Website) ? DBNull.Value : input.Website);
        }

        private bool TryGetPoiInput(out PoiInput input, out string message)
        {
            input = new PoiInput();
            message = string.Empty;

            var name = _txtPoiName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                message = "Tên POI là bắt buộc.";
                return false;
            }

            if (!TryParseDecimal(_txtLatitude.Text, out var lat))
            {
                message = "Latitude không hợp lệ.";
                return false;
            }

            if (!TryParseDecimal(_txtLongitude.Text, out var lon))
            {
                message = "Longitude không hợp lệ.";
                return false;
            }

            if (!TryParseDouble(_txtRadius.Text, out var radius) || radius <= 0)
            {
                message = "Radius phải > 0.";
                return false;
            }

            input = new PoiInput
            {
                PoiName = name,
                Description = _txtDescription.Text.Trim(),
                Latitude = lat,
                Longitude = lon,
                Radius = radius,
                Category = _txtCategory.Text.Trim(),
                Address = _txtAddress.Text.Trim(),
                Phone = _txtPhone.Text.Trim(),
                Website = _txtWebsite.Text.Trim()
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

        private sealed class PoiInput
        {
            public string PoiName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public double Radius { get; set; }
            public string Category { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Website { get; set; } = string.Empty;
        }
    }
}
