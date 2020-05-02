using InSys.Worker.Library.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using z.Data;

namespace InSys.Worker
{
    public partial class ServerConnection : Form
    {
        private Configuration confgmgr { get; set; } = ConfigurationManager.OpenExeConfiguration(Path.Combine(Application.StartupPath, "InSys.Worker.exe"));
        private BaseConfig srvCon { get; set; }
        public EventLog myLog { get; set; }
        public ServerConnection()
        {
            InitializeComponent();
        }

        private void ServerConnection_Load(object sender, EventArgs e)
        {
            try
            {
                srvCon = new BaseConfig().GetConfig();
                ServerName.Text = srvCon.Server;
                DatabaseName.Text = srvCon.Database;
                Username.Text = srvCon.UserID;
                Password.Text = srvCon.Password;
                Port.Text = srvCon.Port;
                txtDomainName.Text = srvCon.DomainName;
                txtUsername.Text = srvCon.DUsername;
                txtPassword.Text = srvCon.DPassword;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "ServerConnection_Load", myLog);
            }
            
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            save();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSaveSql_Click(object sender, EventArgs e)
        {
            save();
        }

        private void btnCancelSql_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void save()
        {
            try
            {
                srvCon.Server = ServerName.Text;
                srvCon.Database = DatabaseName.Text;
                srvCon.UserID = Username.Text;
                srvCon.Password = Password.Text;
                srvCon.Port = Port.Text;
                srvCon.DUsername = txtUsername.Text;
                srvCon.DPassword = txtPassword.Text;
                srvCon.DomainName = txtDomainName.Text;

                foreach (var prop in srvCon.GetType().GetProperties())
                {
                    if (confgmgr.AppSettings.Settings[prop.Name] == null) confgmgr.AppSettings.Settings.Add(prop.Name, srvCon.GetValue(prop.Name));
                    else confgmgr.AppSettings.Settings[prop.Name].Value = srvCon.GetValue(prop.Name);
                }
                confgmgr.Save();

                Logger.LogMessage("Configurations updated.", "Save", myLog, EventLogEntryType.Information);
                MessageBox.Show("Saved successfully!", "Server Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "btnSave_Click", myLog);
            }
        }
    }
}
