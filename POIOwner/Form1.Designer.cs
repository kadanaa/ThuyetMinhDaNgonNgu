namespace POIOwner
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            grpLogin = new GroupBox();
            lblLoginStatus = new Label();
            btnLogin = new Button();
            txtPassword = new TextBox();
            lblPassword = new Label();
            txtUsername = new TextBox();
            lblUsername = new Label();
            btnRegister = new Button();
            txtRegisterPhone = new TextBox();
            lblRegisterPhone = new Label();
            txtRegisterEmail = new TextBox();
            lblRegisterEmail = new Label();
            txtRegisterFullName = new TextBox();
            lblRegisterFullName = new Label();
            txtRegisterPassword = new TextBox();
            lblRegisterPassword = new Label();
            txtRegisterUsername = new TextBox();
            lblRegisterUsername = new Label();
            grpRequests = new GroupBox();
            btnSendDeleteRequest = new Button();
            btnLogout = new Button();
            cmbMyPois = new ComboBox();
            lblMyPois = new Label();
            txtDeleteReason = new TextBox();
            lblDeleteReason = new Label();
            btnSendCreateRequest = new Button();
            txtWebsite = new TextBox();
            lblWebsite = new Label();
            txtPhone = new TextBox();
            lblPhone = new Label();
            txtAddress = new TextBox();
            lblAddress = new Label();
            txtCategory = new TextBox();
            lblCategory = new Label();
            txtRadius = new TextBox();
            lblRadius = new Label();
            txtLongitude = new TextBox();
            lblLongitude = new Label();
            txtLatitude = new TextBox();
            lblLatitude = new Label();
            txtDescription = new TextBox();
            lblDescription = new Label();
            txtPoiName = new TextBox();
            lblPoiName = new Label();
            dgvMyRequests = new DataGridView();
            lblMyRequests = new Label();
            grpLogin.SuspendLayout();
            grpRequests.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMyRequests).BeginInit();
            SuspendLayout();
            // 
            // grpLogin
            // 
            grpLogin.Controls.Add(lblLoginStatus);
            grpLogin.Controls.Add(btnLogin);
            grpLogin.Controls.Add(txtPassword);
            grpLogin.Controls.Add(lblPassword);
            grpLogin.Controls.Add(txtUsername);
            grpLogin.Controls.Add(lblUsername);
            grpLogin.Controls.Add(btnRegister);
            grpLogin.Controls.Add(txtRegisterPhone);
            grpLogin.Controls.Add(lblRegisterPhone);
            grpLogin.Controls.Add(txtRegisterEmail);
            grpLogin.Controls.Add(lblRegisterEmail);
            grpLogin.Controls.Add(txtRegisterFullName);
            grpLogin.Controls.Add(lblRegisterFullName);
            grpLogin.Controls.Add(txtRegisterPassword);
            grpLogin.Controls.Add(lblRegisterPassword);
            grpLogin.Controls.Add(txtRegisterUsername);
            grpLogin.Controls.Add(lblRegisterUsername);
            grpLogin.Location = new Point(12, 12);
            grpLogin.Name = "grpLogin";
            grpLogin.Size = new Size(1160, 208);
            grpLogin.TabIndex = 0;
            grpLogin.TabStop = false;
            grpLogin.Text = "Đăng nhập POI Owner";
            // 
            // lblLoginStatus
            // 
            lblLoginStatus.AutoSize = true;
            lblLoginStatus.ForeColor = Color.Firebrick;
            lblLoginStatus.Location = new Point(667, 42);
            lblLoginStatus.Name = "lblLoginStatus";
            lblLoginStatus.Size = new Size(117, 20);
            lblLoginStatus.TabIndex = 5;
            lblLoginStatus.Text = "Chưa đăng nhập";
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(519, 34);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(129, 36);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "Đăng nhập";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(318, 38);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(182, 27);
            txtPassword.TabIndex = 3;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(241, 41);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(71, 20);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Mật khẩu";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(88, 38);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(140, 27);
            txtUsername.TabIndex = 1;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(22, 41);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(60, 20);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Tài khoản";
            // 
            // btnRegister
            // 
            btnRegister.Location = new Point(1003, 144);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(137, 36);
            btnRegister.TabIndex = 16;
            btnRegister.Text = "Đăng ký";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            // 
            // txtRegisterPhone
            // 
            txtRegisterPhone.Location = new Point(814, 149);
            txtRegisterPhone.Name = "txtRegisterPhone";
            txtRegisterPhone.Size = new Size(177, 27);
            txtRegisterPhone.TabIndex = 15;
            // 
            // lblRegisterPhone
            // 
            lblRegisterPhone.AutoSize = true;
            lblRegisterPhone.Location = new Point(724, 152);
            lblRegisterPhone.Name = "lblRegisterPhone";
            lblRegisterPhone.Size = new Size(84, 20);
            lblRegisterPhone.TabIndex = 14;
            lblRegisterPhone.Text = "Điện thoại";
            // 
            // txtRegisterEmail
            // 
            txtRegisterEmail.Location = new Point(507, 149);
            txtRegisterEmail.Name = "txtRegisterEmail";
            txtRegisterEmail.Size = new Size(206, 27);
            txtRegisterEmail.TabIndex = 13;
            // 
            // lblRegisterEmail
            // 
            lblRegisterEmail.AutoSize = true;
            lblRegisterEmail.Location = new Point(455, 152);
            lblRegisterEmail.Name = "lblRegisterEmail";
            lblRegisterEmail.Size = new Size(46, 20);
            lblRegisterEmail.TabIndex = 12;
            lblRegisterEmail.Text = "Email";
            // 
            // txtRegisterFullName
            // 
            txtRegisterFullName.Location = new Point(277, 149);
            txtRegisterFullName.Name = "txtRegisterFullName";
            txtRegisterFullName.Size = new Size(167, 27);
            txtRegisterFullName.TabIndex = 11;
            // 
            // lblRegisterFullName
            // 
            lblRegisterFullName.AutoSize = true;
            lblRegisterFullName.Location = new Point(198, 152);
            lblRegisterFullName.Name = "lblRegisterFullName";
            lblRegisterFullName.Size = new Size(73, 20);
            lblRegisterFullName.TabIndex = 10;
            lblRegisterFullName.Text = "Họ và tên";
            // 
            // txtRegisterPassword
            // 
            txtRegisterPassword.Location = new Point(814, 106);
            txtRegisterPassword.Name = "txtRegisterPassword";
            txtRegisterPassword.PasswordChar = '*';
            txtRegisterPassword.Size = new Size(326, 27);
            txtRegisterPassword.TabIndex = 9;
            // 
            // lblRegisterPassword
            // 
            lblRegisterPassword.AutoSize = true;
            lblRegisterPassword.Location = new Point(737, 109);
            lblRegisterPassword.Name = "lblRegisterPassword";
            lblRegisterPassword.Size = new Size(71, 20);
            lblRegisterPassword.TabIndex = 8;
            lblRegisterPassword.Text = "Mật khẩu";
            // 
            // txtRegisterUsername
            // 
            txtRegisterUsername.Location = new Point(277, 106);
            txtRegisterUsername.Name = "txtRegisterUsername";
            txtRegisterUsername.Size = new Size(436, 27);
            txtRegisterUsername.TabIndex = 7;
            // 
            // lblRegisterUsername
            // 
            lblRegisterUsername.AutoSize = true;
            lblRegisterUsername.Location = new Point(162, 109);
            lblRegisterUsername.Name = "lblRegisterUsername";
            lblRegisterUsername.Size = new Size(109, 20);
            lblRegisterUsername.TabIndex = 6;
            lblRegisterUsername.Text = "ĐK - Tài khoản";
            // 
            // grpRequests
            // 
            grpRequests.Controls.Add(btnSendDeleteRequest);
            grpRequests.Controls.Add(btnLogout);
            grpRequests.Controls.Add(cmbMyPois);
            grpRequests.Controls.Add(lblMyPois);
            grpRequests.Controls.Add(txtDeleteReason);
            grpRequests.Controls.Add(lblDeleteReason);
            grpRequests.Controls.Add(btnSendCreateRequest);
            grpRequests.Controls.Add(txtWebsite);
            grpRequests.Controls.Add(lblWebsite);
            grpRequests.Controls.Add(txtPhone);
            grpRequests.Controls.Add(lblPhone);
            grpRequests.Controls.Add(txtAddress);
            grpRequests.Controls.Add(lblAddress);
            grpRequests.Controls.Add(txtCategory);
            grpRequests.Controls.Add(lblCategory);
            grpRequests.Controls.Add(txtRadius);
            grpRequests.Controls.Add(lblRadius);
            grpRequests.Controls.Add(txtLongitude);
            grpRequests.Controls.Add(lblLongitude);
            grpRequests.Controls.Add(txtLatitude);
            grpRequests.Controls.Add(lblLatitude);
            grpRequests.Controls.Add(txtDescription);
            grpRequests.Controls.Add(lblDescription);
            grpRequests.Controls.Add(txtPoiName);
            grpRequests.Controls.Add(lblPoiName);
            grpRequests.Controls.Add(dgvMyRequests);
            grpRequests.Controls.Add(lblMyRequests);
            grpRequests.Enabled = false;
            grpRequests.Location = new Point(12, 114);
            grpRequests.Name = "grpRequests";
            grpRequests.Size = new Size(1160, 574);
            grpRequests.TabIndex = 1;
            grpRequests.TabStop = false;
            grpRequests.Text = "Gửi request cho Admin";
            // 
            // btnSendDeleteRequest
            // 
            btnSendDeleteRequest.BackColor = Color.FromArgb(220, 53, 69);
            btnSendDeleteRequest.ForeColor = Color.White;
            btnSendDeleteRequest.Location = new Point(989, 382);
            btnSendDeleteRequest.Name = "btnSendDeleteRequest";
            btnSendDeleteRequest.Size = new Size(151, 60);
            btnSendDeleteRequest.TabIndex = 25;
            btnSendDeleteRequest.Text = "Gửi request xóa";
            btnSendDeleteRequest.UseVisualStyleBackColor = false;
            btnSendDeleteRequest.Click += btnSendDeleteRequest_Click;
            // 
            // btnLogout
            // 
            btnLogout.BackColor = Color.FromArgb(108, 117, 125);
            btnLogout.ForeColor = Color.White;
            btnLogout.Location = new Point(1009, 548);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(131, 24);
            btnLogout.TabIndex = 26;
            btnLogout.Text = "Đăng xuất";
            btnLogout.UseVisualStyleBackColor = false;
            btnLogout.Click += btnLogout_Click;
            // 
            // cmbMyPois
            // 
            cmbMyPois.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMyPois.FormattingEnabled = true;
            cmbMyPois.Location = new Point(671, 382);
            cmbMyPois.Name = "cmbMyPois";
            cmbMyPois.Size = new Size(302, 28);
            cmbMyPois.TabIndex = 24;
            // 
            // lblMyPois
            // 
            lblMyPois.AutoSize = true;
            lblMyPois.Location = new Point(591, 385);
            lblMyPois.Name = "lblMyPois";
            lblMyPois.Size = new Size(74, 20);
            lblMyPois.TabIndex = 23;
            lblMyPois.Text = "POI của tôi";
            // 
            // txtDeleteReason
            // 
            txtDeleteReason.Location = new Point(671, 416);
            txtDeleteReason.Multiline = true;
            txtDeleteReason.Name = "txtDeleteReason";
            txtDeleteReason.PlaceholderText = "Lý do muốn xóa POI...";
            txtDeleteReason.Size = new Size(302, 60);
            txtDeleteReason.TabIndex = 22;
            // 
            // lblDeleteReason
            // 
            lblDeleteReason.AutoSize = true;
            lblDeleteReason.Location = new Point(621, 419);
            lblDeleteReason.Name = "lblDeleteReason";
            lblDeleteReason.Size = new Size(44, 20);
            lblDeleteReason.TabIndex = 21;
            lblDeleteReason.Text = "Lý do";
            // 
            // btnSendCreateRequest
            // 
            btnSendCreateRequest.BackColor = Color.FromArgb(40, 167, 69);
            btnSendCreateRequest.ForeColor = Color.White;
            btnSendCreateRequest.Location = new Point(989, 482);
            btnSendCreateRequest.Name = "btnSendCreateRequest";
            btnSendCreateRequest.Size = new Size(151, 60);
            btnSendCreateRequest.TabIndex = 20;
            btnSendCreateRequest.Text = "Gửi request thêm";
            btnSendCreateRequest.UseVisualStyleBackColor = false;
            btnSendCreateRequest.Click += btnSendCreateRequest_Click;
            // 
            // txtWebsite
            // 
            txtWebsite.Location = new Point(671, 515);
            txtWebsite.Name = "txtWebsite";
            txtWebsite.Size = new Size(302, 27);
            txtWebsite.TabIndex = 19;
            // 
            // lblWebsite
            // 
            lblWebsite.AutoSize = true;
            lblWebsite.Location = new Point(605, 518);
            lblWebsite.Name = "lblWebsite";
            lblWebsite.Size = new Size(60, 20);
            lblWebsite.TabIndex = 18;
            lblWebsite.Text = "Website";
            // 
            // txtPhone
            // 
            txtPhone.Location = new Point(671, 482);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(302, 27);
            txtPhone.TabIndex = 17;
            // 
            // lblPhone
            // 
            lblPhone.AutoSize = true;
            lblPhone.Location = new Point(581, 485);
            lblPhone.Name = "lblPhone";
            lblPhone.Size = new Size(84, 20);
            lblPhone.TabIndex = 16;
            lblPhone.Text = "Điện thoại";
            // 
            // txtAddress
            // 
            txtAddress.Location = new Point(109, 515);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(464, 27);
            txtAddress.TabIndex = 15;
            // 
            // lblAddress
            // 
            lblAddress.AutoSize = true;
            lblAddress.Location = new Point(48, 518);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(55, 20);
            lblAddress.TabIndex = 14;
            lblAddress.Text = "Địa chỉ";
            // 
            // txtCategory
            // 
            txtCategory.Location = new Point(409, 482);
            txtCategory.Name = "txtCategory";
            txtCategory.Size = new Size(164, 27);
            txtCategory.TabIndex = 13;
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(335, 485);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(68, 20);
            lblCategory.TabIndex = 12;
            lblCategory.Text = "Category";
            // 
            // txtRadius
            // 
            txtRadius.Location = new Point(109, 482);
            txtRadius.Name = "txtRadius";
            txtRadius.Size = new Size(118, 27);
            txtRadius.TabIndex = 11;
            // 
            // lblRadius
            // 
            lblRadius.AutoSize = true;
            lblRadius.Location = new Point(47, 485);
            lblRadius.Name = "lblRadius";
            lblRadius.Size = new Size(53, 20);
            lblRadius.TabIndex = 10;
            lblRadius.Text = "Radius";
            // 
            // txtLongitude
            // 
            txtLongitude.Location = new Point(409, 449);
            txtLongitude.Name = "txtLongitude";
            txtLongitude.Size = new Size(164, 27);
            txtLongitude.TabIndex = 9;
            // 
            // lblLongitude
            // 
            lblLongitude.AutoSize = true;
            lblLongitude.Location = new Point(331, 452);
            lblLongitude.Name = "lblLongitude";
            lblLongitude.Size = new Size(72, 20);
            lblLongitude.TabIndex = 8;
            lblLongitude.Text = "Longitude";
            // 
            // txtLatitude
            // 
            txtLatitude.Location = new Point(109, 449);
            txtLatitude.Name = "txtLatitude";
            txtLatitude.Size = new Size(164, 27);
            txtLatitude.TabIndex = 7;
            // 
            // lblLatitude
            // 
            lblLatitude.AutoSize = true;
            lblLatitude.Location = new Point(39, 452);
            lblLatitude.Name = "lblLatitude";
            lblLatitude.Size = new Size(61, 20);
            lblLatitude.TabIndex = 6;
            lblLatitude.Text = "Latitude";
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(109, 416);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(464, 27);
            txtDescription.TabIndex = 5;
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(49, 419);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(51, 20);
            lblDescription.TabIndex = 4;
            lblDescription.Text = "Mô tả";
            // 
            // txtPoiName
            // 
            txtPoiName.Location = new Point(109, 383);
            txtPoiName.Name = "txtPoiName";
            txtPoiName.Size = new Size(464, 27);
            txtPoiName.TabIndex = 3;
            // 
            // lblPoiName
            // 
            lblPoiName.AutoSize = true;
            lblPoiName.Location = new Point(37, 386);
            lblPoiName.Name = "lblPoiName";
            lblPoiName.Size = new Size(63, 20);
            lblPoiName.TabIndex = 2;
            lblPoiName.Text = "Tên POI";
            // 
            // dgvMyRequests
            // 
            dgvMyRequests.AllowUserToAddRows = false;
            dgvMyRequests.AllowUserToDeleteRows = false;
            dgvMyRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMyRequests.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMyRequests.Location = new Point(19, 61);
            dgvMyRequests.MultiSelect = false;
            dgvMyRequests.Name = "dgvMyRequests";
            dgvMyRequests.ReadOnly = true;
            dgvMyRequests.RowHeadersWidth = 51;
            dgvMyRequests.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMyRequests.Size = new Size(1121, 308);
            dgvMyRequests.TabIndex = 1;
            // 
            // lblMyRequests
            // 
            lblMyRequests.AutoSize = true;
            lblMyRequests.Location = new Point(19, 33);
            lblMyRequests.Name = "lblMyRequests";
            lblMyRequests.Size = new Size(176, 20);
            lblMyRequests.TabIndex = 0;
            lblMyRequests.Text = "Request đã gửi của bạn";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 701);
            Controls.Add(grpRequests);
            Controls.Add(grpLogin);
            MinimumSize = new Size(1202, 748);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "POI Owner - Gửi request cho Admin";
            grpLogin.ResumeLayout(false);
            grpLogin.PerformLayout();
            grpRequests.ResumeLayout(false);
            grpRequests.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMyRequests).EndInit();
            ResumeLayout(false);
        }

        private GroupBox grpLogin;
        private Label lblLoginStatus;
        private Button btnLogin;
        private TextBox txtPassword;
        private Label lblPassword;
        private TextBox txtUsername;
        private Label lblUsername;
        private Button btnRegister;
        private TextBox txtRegisterPhone;
        private Label lblRegisterPhone;
        private TextBox txtRegisterEmail;
        private Label lblRegisterEmail;
        private TextBox txtRegisterFullName;
        private Label lblRegisterFullName;
        private TextBox txtRegisterPassword;
        private Label lblRegisterPassword;
        private TextBox txtRegisterUsername;
        private Label lblRegisterUsername;
        private GroupBox grpRequests;
        private Button btnSendDeleteRequest;
        private Button btnLogout;
        private ComboBox cmbMyPois;
        private Label lblMyPois;
        private TextBox txtDeleteReason;
        private Label lblDeleteReason;
        private Button btnSendCreateRequest;
        private TextBox txtWebsite;
        private Label lblWebsite;
        private TextBox txtPhone;
        private Label lblPhone;
        private TextBox txtAddress;
        private Label lblAddress;
        private TextBox txtCategory;
        private Label lblCategory;
        private TextBox txtRadius;
        private Label lblRadius;
        private TextBox txtLongitude;
        private Label lblLongitude;
        private TextBox txtLatitude;
        private Label lblLatitude;
        private TextBox txtDescription;
        private Label lblDescription;
        private TextBox txtPoiName;
        private Label lblPoiName;
        private DataGridView dgvMyRequests;
        private Label lblMyRequests;

        #endregion
    }
}
