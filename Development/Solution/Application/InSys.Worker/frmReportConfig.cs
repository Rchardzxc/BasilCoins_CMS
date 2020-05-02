using InSys.Worker.Library.Classes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using z.Data;

namespace InSys.Worker
{
    public partial class frmReportConfig : Form
    {
        private ReportGeneratorConfig rptConfig { get; set; } = new ReportGeneratorConfig();
        public frmReportConfig()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Core.FileDialog fileDialog = app.get_FileDialog(Microsoft.Office.Core.MsoFileDialogType.msoFileDialogFolderPicker);
            if(Directory.Exists(rptConfig.ReportPath)) fileDialog.InitialFileName = rptConfig.ReportPath;
            else fileDialog.InitialFileName = "C:";

            int nres = fileDialog.Show();
            if (nres == -1)
            {
                Microsoft.Office.Core.FileDialogSelectedItems selectedItems = fileDialog.SelectedItems;

                string[] selectedFolders = selectedItems.Cast<string>().ToArray();

                if (selectedFolders.Length > 0)
                {
                    txtReportPath.Text = selectedFolders[0];
                }
            }
        }

        private void txtPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.KeyCode.ToInt32() >= 48 && e.KeyCode.ToInt32() <= 57) && !(e.KeyCode.ToInt32() >= 96 && e.KeyCode.ToInt32() <= 105) && !(e.KeyCode.ToInt32() >= 37 && e.KeyCode.ToInt32() <= 40) && !(e.KeyCode.ToInt32() == 8)) e.SuppressKeyPress = true;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            save();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void frmReportConfig_Load(object sender, EventArgs e)
        {
            var rptSection = ConfigurationManager.GetSection("Report") as NameValueCollection;
            rptConfig.Port = rptSection["Port"].ToString();
            rptConfig.ReportPath = rptSection["ReportPath"].ToString();
            rptConfig.WebClient = rptSection["WebClient"].ToString();
            rptConfig.KioskServer = rptSection["KioskServer"].ToString();
            rptConfig.KioskDatabase = rptSection["KioskDatabase"].ToString();
            rptConfig.KioskUserID = rptSection["KioskUserID"].ToString();
            rptConfig.KioskPassword = rptSection["KioskPassword"].ToString();
            rptConfig.KioskPort = rptSection["KioskPort"].ToString();
            txtPort.Text = rptConfig.Port;
            txtReportPath.Text = rptConfig.ReportPath;
            txtWebClient.Text = rptConfig.WebClient;
            txtKioskServer.Text = rptConfig.KioskServer;
            txtKioskDatabase.Text = rptConfig.KioskDatabase;
            txtKioskUsername.Text = rptConfig.KioskUserID;
            txtKioskPassword.Text = rptConfig.KioskPassword;
            txtKioskPort.Text = rptConfig.KioskPort;
        }

        private void btnKioskSave_Click(object sender, EventArgs e)
        {
            save();
        }

        private void btnKioskCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void save()
        {
            rptConfig.Port = txtPort.Text;
            rptConfig.ReportPath = txtReportPath.Text;
            rptConfig.WebClient = txtWebClient.Text;
            rptConfig.KioskServer = txtKioskServer.Text;
            rptConfig.KioskDatabase = txtKioskDatabase.Text;
            rptConfig.KioskUserID = txtKioskUsername.Text;
            rptConfig.KioskPassword = txtKioskPassword.Text;
            rptConfig.KioskPort = txtKioskPort.Text;

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            xmlDoc.SelectSingleNode("//Report/add[@key='Port']").Attributes["value"].Value = rptConfig.Port.ToString();
            xmlDoc.SelectSingleNode("//Report/add[@key='ReportPath']").Attributes["value"].Value = rptConfig.ReportPath.ToString();
            xmlDoc.SelectSingleNode("//Report/add[@key='WebClient']").Attributes["value"].Value = rptConfig.WebClient.ToString();

            xmlDoc.SelectSingleNode("//Report/add[@key='KioskServer']").Attributes["value"].Value = rptConfig.KioskServer.ToString();
            xmlDoc.SelectSingleNode("//Report/add[@key='KioskDatabase']").Attributes["value"].Value = rptConfig.KioskDatabase.ToString();
            xmlDoc.SelectSingleNode("//Report/add[@key='KioskUserID']").Attributes["value"].Value = rptConfig.KioskUserID.ToString();
            xmlDoc.SelectSingleNode("//Report/add[@key='KioskPassword']").Attributes["value"].Value = rptConfig.KioskPassword.ToString();
            xmlDoc.SelectSingleNode("//Report/add[@key='KioskPort']").Attributes["value"].Value = rptConfig.KioskPort.ToString();

            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            ConfigurationManager.RefreshSection("Report");

            MessageBox.Show("Saved successfully!", "Report Generator Config", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
