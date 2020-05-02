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
    public partial class frmETFConfig : Form
    {
        private ETFConfig etfConfig { get; set; } = new ETFConfig();
        public frmETFConfig()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Core.FileDialog fileDialog = app.get_FileDialog(Microsoft.Office.Core.MsoFileDialogType.msoFileDialogFolderPicker);
            if(Directory.Exists(etfConfig.ExcelTemplatePath)) fileDialog.InitialFileName = etfConfig.ExcelTemplatePath;
            else fileDialog.InitialFileName = "C:";

            int nres = fileDialog.Show();
            if (nres == -1)
            {
                Microsoft.Office.Core.FileDialogSelectedItems selectedItems = fileDialog.SelectedItems;

                string[] selectedFolders = selectedItems.Cast<string>().ToArray();

                if (selectedFolders.Length > 0)
                {
                    xlsSourcePath.Text = selectedFolders[0];
                }
            }
        }

        private void CheckInterval_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.KeyCode.ToInt32() >= 48 && e.KeyCode.ToInt32() <= 57) && !(e.KeyCode.ToInt32() >= 96 && e.KeyCode.ToInt32() <= 105) && !(e.KeyCode.ToInt32() >= 37 && e.KeyCode.ToInt32() <= 40) && !(e.KeyCode.ToInt32() == 8)) e.SuppressKeyPress = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            etfConfig.CheckInterval = CheckInterval.Text;
            etfConfig.ExcelTemplatePath = xlsSourcePath.Text;

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            xmlDoc.SelectSingleNode("//ETF/add[@key='CheckInterval']").Attributes["value"].Value = etfConfig.CheckInterval.ToString();
            xmlDoc.SelectSingleNode("//ETF/add[@key='ExcelTemplatePath']").Attributes["value"].Value = etfConfig.ExcelTemplatePath.ToString();
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            ConfigurationManager.RefreshSection("ETF");

            MessageBox.Show("Saved successfully!", "ETF Uploader Config", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ETFConfig_Load(object sender, EventArgs e)
        {
            var etfSection = ConfigurationManager.GetSection("ETF") as NameValueCollection;
            etfConfig.CheckInterval = etfSection["CheckInterval"].ToString();
            etfConfig.ExcelTemplatePath = etfSection["ExcelTemplatePath"].ToString();
            xlsSourcePath.Text = etfConfig.ExcelTemplatePath;
            CheckInterval.Text = etfConfig.CheckInterval;
        }
    }
}
