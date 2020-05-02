using InSys.Worker.Library.Classes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace InSys.Worker
{
    public partial class frmWebNotificationConfig : Form
    {
        private WebNotificationConfig wnConfig { get; set; } = new WebNotificationConfig();
        public frmWebNotificationConfig()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            wnConfig.Port = txtPort.Text;
            wnConfig.WebClient = txtWebClient.Text;

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            xmlDoc.SelectSingleNode("//WebNotification/add[@key='Port']").Attributes["value"].Value = wnConfig.Port.ToString();
            xmlDoc.SelectSingleNode("//WebNotification/add[@key='WebClient']").Attributes["value"].Value = wnConfig.WebClient.ToString();

            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            ConfigurationManager.RefreshSection("WebNotification");

            MessageBox.Show("Saved successfully!", "Web Notification Config", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmWebNotificationConfig_Load(object sender, EventArgs e)
        {
            var rptSection = ConfigurationManager.GetSection("WebNotification") as NameValueCollection;
            wnConfig.Port = rptSection["Port"].ToString();
            wnConfig.WebClient = rptSection["WebClient"].ToString();
            txtPort.Text = wnConfig.Port;
            txtWebClient.Text = wnConfig.WebClient;
        }
    }
}
