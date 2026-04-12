namespace Admin
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
            grpRequests = new GroupBox();
            btnRefresh = new Button();
            btnOpenPoiManagement = new Button();
            btnReject = new Button();
            btnApprove = new Button();
            txtComments = new TextBox();
            lblComments = new Label();
            dgvRequests = new DataGridView();
            grpLogin.SuspendLayout();
            grpRequests.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRequests).BeginInit();
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
            grpLogin.Location = new Point(12, 12);
            grpLogin.Name = "grpLogin";
            grpLogin.Size = new Size(1000, 96);
            grpLogin.TabIndex = 0;
            grpLogin.TabStop = false;
            grpLogin.Text = "1) Đăng nhập Admin";
            // 
            // lblLoginStatus
            // 
            lblLoginStatus.AutoSize = true;
            lblLoginStatus.ForeColor = Color.FromArgb(192, 0, 0);
            lblLoginStatus.Location = new Point(663, 42);
            lblLoginStatus.Name = "lblLoginStatus";
            lblLoginStatus.Size = new Size(150, 20);
            lblLoginStatus.TabIndex = 5;
            lblLoginStatus.Text = "Chưa đăng nhập";
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(514, 34);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(129, 36);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "Đăng nhập";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(315, 38);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(180, 27);
            txtPassword.TabIndex = 3;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(238, 41);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(71, 20);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Mật khẩu";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(85, 38);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(140, 27);
            txtUsername.TabIndex = 1;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(19, 41);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(60, 20);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Tài khoản";
            // 
            // grpRequests
            // 
            grpRequests.Controls.Add(btnRefresh);
            grpRequests.Controls.Add(btnOpenPoiManagement);
            grpRequests.Controls.Add(btnReject);
            grpRequests.Controls.Add(btnApprove);
            grpRequests.Controls.Add(txtComments);
            grpRequests.Controls.Add(lblComments);
            grpRequests.Controls.Add(dgvRequests);
            grpRequests.Enabled = false;
            grpRequests.Location = new Point(12, 121);
            grpRequests.Name = "grpRequests";
            grpRequests.Size = new Size(1000, 517);
            grpRequests.TabIndex = 1;
            grpRequests.TabStop = false;
            grpRequests.Text = "2) Duyệt request thêm/cập nhật POI từ POIOwner";
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(849, 451);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(135, 48);
            btnRefresh.TabIndex = 5;
            btnRefresh.Text = "Làm mới";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnOpenPoiManagement
            // 
            btnOpenPoiManagement.Location = new Point(708, 422);
            btnOpenPoiManagement.Name = "btnOpenPoiManagement";
            btnOpenPoiManagement.Size = new Size(276, 23);
            btnOpenPoiManagement.TabIndex = 6;
            btnOpenPoiManagement.Text = "Quản lý POI (Thêm / Sửa / Xóa)";
            btnOpenPoiManagement.UseVisualStyleBackColor = true;
            btnOpenPoiManagement.Click += btnOpenPoiManagement_Click;
            // 
            // btnReject
            // 
            btnReject.BackColor = Color.FromArgb(220, 53, 69);
            btnReject.ForeColor = Color.White;
            btnReject.Location = new Point(708, 451);
            btnReject.Name = "btnReject";
            btnReject.Size = new Size(135, 48);
            btnReject.TabIndex = 4;
            btnReject.Text = "Từ chối";
            btnReject.UseVisualStyleBackColor = false;
            btnReject.Click += btnReject_Click;
            // 
            // btnApprove
            // 
            btnApprove.BackColor = Color.FromArgb(40, 167, 69);
            btnApprove.ForeColor = Color.White;
            btnApprove.Location = new Point(567, 451);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new Size(135, 48);
            btnApprove.TabIndex = 3;
            btnApprove.Text = "Duyệt";
            btnApprove.UseVisualStyleBackColor = false;
            btnApprove.Click += btnApprove_Click;
            // 
            // txtComments
            // 
            txtComments.Location = new Point(19, 451);
            txtComments.Multiline = true;
            txtComments.Name = "txtComments";
            txtComments.PlaceholderText = "Nhập ghi chú duyệt/từ chối...";
            txtComments.Size = new Size(530, 48);
            txtComments.TabIndex = 2;
            // 
            // lblComments
            // 
            lblComments.AutoSize = true;
            lblComments.Location = new Point(19, 422);
            lblComments.Name = "lblComments";
            lblComments.Size = new Size(120, 20);
            lblComments.TabIndex = 1;
            lblComments.Text = "Ghi chú Admin:";
            // 
            // dgvRequests
            // 
            dgvRequests.AllowUserToAddRows = false;
            dgvRequests.AllowUserToDeleteRows = false;
            dgvRequests.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRequests.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRequests.Location = new Point(19, 33);
            dgvRequests.MultiSelect = false;
            dgvRequests.Name = "dgvRequests";
            dgvRequests.ReadOnly = true;
            dgvRequests.RowHeadersWidth = 51;
            dgvRequests.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRequests.Size = new Size(965, 378);
            dgvRequests.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1024, 650);
            Controls.Add(grpRequests);
            Controls.Add(grpLogin);
            MinimumSize = new Size(1042, 697);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Admin - Quản lý duyệt POI";
            grpLogin.ResumeLayout(false);
            grpLogin.PerformLayout();
            grpRequests.ResumeLayout(false);
            grpRequests.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRequests).EndInit();
            ResumeLayout(false);
        }

        private GroupBox grpLogin;
        private Label lblLoginStatus;
        private Button btnLogin;
        private TextBox txtPassword;
        private Label lblPassword;
        private TextBox txtUsername;
        private Label lblUsername;
        private GroupBox grpRequests;
        private Button btnRefresh;
        private Button btnOpenPoiManagement;
        private Button btnReject;
        private Button btnApprove;
        private TextBox txtComments;
        private Label lblComments;
        private DataGridView dgvRequests;

        #endregion
    }
}
