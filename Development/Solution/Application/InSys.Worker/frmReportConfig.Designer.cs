namespace InSys.Worker
{
    partial class frmReportConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label9 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtReportPath = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtWebClient = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnKioskCancel = new System.Windows.Forms.Button();
            this.btnKioskSave = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtKioskServer = new System.Windows.Forms.TextBox();
            this.txtKioskDatabase = new System.Windows.Forms.TextBox();
            this.txtKioskUsername = new System.Windows.Forms.TextBox();
            this.txtKioskPassword = new System.Windows.Forms.TextBox();
            this.txtKioskPort = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(-4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(309, 32);
            this.label9.TabIndex = 46;
            this.label9.Text = "Report Generator Configuration";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(248, 87);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(39, 22);
            this.button1.TabIndex = 45;
            this.button1.Text = "Path";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(212, 162);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 44;
            this.btnCancel.Text = "C&ancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(131, 162);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 43;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txtReportPath
            // 
            this.txtReportPath.Location = new System.Drawing.Point(93, 88);
            this.txtReportPath.Name = "txtReportPath";
            this.txtReportPath.Size = new System.Drawing.Size(156, 20);
            this.txtReportPath.TabIndex = 42;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(93, 38);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(194, 20);
            this.txtPort.TabIndex = 41;
            this.txtPort.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPort_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 40;
            this.label2.Text = "Report Path";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(66, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 39;
            this.label1.Text = "Port";
            // 
            // txtWebClient
            // 
            this.txtWebClient.Location = new System.Drawing.Point(93, 64);
            this.txtWebClient.Name = "txtWebClient";
            this.txtWebClient.Size = new System.Drawing.Size(194, 20);
            this.txtWebClient.TabIndex = 48;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 47;
            this.label3.Text = "Web Client";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(309, 217);
            this.tabControl1.TabIndex = 49;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnKioskCancel);
            this.tabPage2.Controls.Add(this.btnKioskSave);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.txtKioskServer);
            this.tabPage2.Controls.Add(this.txtKioskDatabase);
            this.tabPage2.Controls.Add(this.txtKioskUsername);
            this.tabPage2.Controls.Add(this.txtKioskPassword);
            this.tabPage2.Controls.Add(this.txtKioskPort);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(301, 191);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "SQL Configuration";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnKioskCancel
            // 
            this.btnKioskCancel.Location = new System.Drawing.Point(212, 162);
            this.btnKioskCancel.Name = "btnKioskCancel";
            this.btnKioskCancel.Size = new System.Drawing.Size(75, 23);
            this.btnKioskCancel.TabIndex = 59;
            this.btnKioskCancel.Text = "C&ancel";
            this.btnKioskCancel.UseVisualStyleBackColor = true;
            this.btnKioskCancel.Click += new System.EventHandler(this.btnKioskCancel_Click);
            // 
            // btnKioskSave
            // 
            this.btnKioskSave.Location = new System.Drawing.Point(131, 162);
            this.btnKioskSave.Name = "btnKioskSave";
            this.btnKioskSave.Size = new System.Drawing.Size(75, 23);
            this.btnKioskSave.TabIndex = 58;
            this.btnKioskSave.Text = "&Save";
            this.btnKioskSave.UseVisualStyleBackColor = true;
            this.btnKioskSave.Click += new System.EventHandler(this.btnKioskSave_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 41);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 13);
            this.label5.TabIndex = 48;
            this.label5.Text = "Server Name";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 66);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(84, 13);
            this.label6.TabIndex = 49;
            this.label6.Text = "Database Name";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(37, 90);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.TabIndex = 50;
            this.label7.Text = "Username";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(39, 114);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 13);
            this.label8.TabIndex = 51;
            this.label8.Text = "Password";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(66, 138);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(26, 13);
            this.label10.TabIndex = 52;
            this.label10.Text = "Port";
            // 
            // txtKioskServer
            // 
            this.txtKioskServer.Location = new System.Drawing.Point(93, 38);
            this.txtKioskServer.Name = "txtKioskServer";
            this.txtKioskServer.Size = new System.Drawing.Size(194, 20);
            this.txtKioskServer.TabIndex = 53;
            // 
            // txtKioskDatabase
            // 
            this.txtKioskDatabase.Location = new System.Drawing.Point(93, 63);
            this.txtKioskDatabase.Name = "txtKioskDatabase";
            this.txtKioskDatabase.Size = new System.Drawing.Size(194, 20);
            this.txtKioskDatabase.TabIndex = 54;
            // 
            // txtKioskUsername
            // 
            this.txtKioskUsername.Location = new System.Drawing.Point(93, 87);
            this.txtKioskUsername.Name = "txtKioskUsername";
            this.txtKioskUsername.Size = new System.Drawing.Size(194, 20);
            this.txtKioskUsername.TabIndex = 55;
            // 
            // txtKioskPassword
            // 
            this.txtKioskPassword.Location = new System.Drawing.Point(93, 111);
            this.txtKioskPassword.Name = "txtKioskPassword";
            this.txtKioskPassword.Size = new System.Drawing.Size(194, 20);
            this.txtKioskPassword.TabIndex = 56;
            this.txtKioskPassword.UseSystemPasswordChar = true;
            // 
            // txtKioskPort
            // 
            this.txtKioskPort.Location = new System.Drawing.Point(93, 135);
            this.txtKioskPort.Name = "txtKioskPort";
            this.txtKioskPort.Size = new System.Drawing.Size(194, 20);
            this.txtKioskPort.TabIndex = 57;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(-3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(308, 32);
            this.label4.TabIndex = 47;
            this.label4.Text = "Kiosk SQL Connection Configuration";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label9);
            this.tabPage1.Controls.Add(this.txtWebClient);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.txtPort);
            this.tabPage1.Controls.Add(this.button1);
            this.tabPage1.Controls.Add(this.txtReportPath);
            this.tabPage1.Controls.Add(this.btnCancel);
            this.tabPage1.Controls.Add(this.btnSave);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(301, 191);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Report Configuration";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // frmReportConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 217);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmReportConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Report Generator Config";
            this.Load += new System.EventHandler(this.frmReportConfig_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtReportPath;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtWebClient;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtKioskServer;
        private System.Windows.Forms.TextBox txtKioskDatabase;
        private System.Windows.Forms.TextBox txtKioskUsername;
        private System.Windows.Forms.TextBox txtKioskPassword;
        private System.Windows.Forms.TextBox txtKioskPort;
        private System.Windows.Forms.Button btnKioskCancel;
        private System.Windows.Forms.Button btnKioskSave;
    }
}