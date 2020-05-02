using InSys.Worker.Library.Classes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using z.Data;

namespace InSys.Worker
{
    public partial class MainUI : Form
    {
        public EventLog bgWorker { get; set; }
        public EventLog PassiveLog { get; set; }

        public EventLog movementService { get; set; }
        public EventLog wnService { get; set; }
        public EventLog reportService { get; set; }
        public EventLog msService { get; set; }
        public EventLog tkService { get; set; }
        public EventLog payrollService { get; set; }
        int lastEventWritten = 0;
        public MainUI()
        {
            InitializeComponent();
            string LogName = "BAGWIS";
            string SourceName = "Worker App";
            if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            bgWorker = new EventLog(LogName);
            bgWorker.Source = SourceName;
            bgWorker.Log = LogName;

            PassiveLog = new EventLog(LogName);
            PassiveLog.EnableRaisingEvents = true;
            PassiveLog.EntryWritten += new EntryWrittenEventHandler(OnEntryWritten);

            //SourceName = "ETF Service";
            //if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            //etfService = new EventLog(LogName);
            //etfService.Source = SourceName;
            //etfService.Log = LogName;

            //SourceName = "Movement Service";
            //if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            //movementService = new EventLog(LogName);
            //movementService.Source = SourceName;
            //movementService.Log = LogName;

            //SourceName = "Web Notification Service";
            //if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            //wnService = new EventLog(LogName);
            //wnService.Source = SourceName;
            //wnService.Log = LogName;

            //SourceName = "Report Service";
            //if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            //reportService = new EventLog(LogName);
            //reportService.Source = SourceName;
            //reportService.Log = LogName;

            //SourceName = "Mail Sender Service";
            //if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            //msService = new EventLog(LogName);
            //msService.Source = SourceName;
            //msService.Log = LogName;

            //SourceName = "Timekeeping Service";
            //if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            //tkService = new EventLog(LogName);
            //tkService.Source = SourceName;
            //tkService.Log = LogName;

            //SourceName = "Payroll Service";
            //if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            //payrollService = new EventLog(LogName);
            //payrollService.Source = SourceName;
            //payrollService.Log = LogName;
        }

        private void serverConnectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var srvCon = new ServerConnection();
            srvCon.myLog = bgWorker;
            srvCon.ShowDialog();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void MainUI_Load(object sender, EventArgs e)
        {
            //foreach (EventLogEntry log in PassiveLog.Entries)
            //{
            //    rtbLogs.Text = rtbLogs.Text.Insert(0, $"{"[" + log.Source + " " + log.TimeGenerated.ToString("M/d/yyyy h:mm tt") + "] " + ": " + log.Message}\n");
            //}

            lastEventWritten = PassiveLog.Entries.Count;
            CheckETFServiceStatus();
            CheckReportServiceStatus();
            CheckWebNotificationStatus();
        }
        private void OnEntryWritten(object source, EntryWrittenEventArgs e)
        {
            int e1 = 0;
            var total = PassiveLog.Entries.Count;
            e1 = total - 1;

            for (var x = lastEventWritten; x < total; x++)
            {
                if (rtbLogs.InvokeRequired)
                {
                    rtbLogs.Invoke(new MethodInvoker(() =>
                    {
                        rtbLogs.Text = rtbLogs.Text.Insert(0, $"{"[" + PassiveLog.Entries[x].Source + " " + PassiveLog.Entries[x].TimeGenerated.ToString("M/d/yyyy h:mm tt") + "] " + ": " + PassiveLog.Entries[x].Message}\n");
                    }));
                }
                else
                {
                    rtbLogs.Text = rtbLogs.Text.Insert(0, $"{"[" + PassiveLog.Entries[x].Source + " " + PassiveLog.Entries[x].TimeGenerated.ToString("M/d/yyyy h:mm tt") + "] " + ": " + PassiveLog.Entries[x].Message}\n");
                }
            }
            lastEventWritten = total;
        }
        private ServiceController GetService(string ServiceName)
        {
            //Get All Services and filter based on parameter value
            var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == ServiceName);
            return service;
        }

        #region ETF SERVICE
        private void CheckETFServiceStatus()
        {
            var service = GetService("ETFService");
            if (service != null)
            {
                etfStatus.Enabled = true;
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    etfInstall.Text = "Uninstall";
                    etfInstall.Enabled = true;

                    etfStatus.Text = "Start";
                }
                else
                {
                    etfInstall.Text = "Uninstall";
                    etfInstall.Enabled = false;

                    etfStatus.Text = "Stop";
                }
            }
            else
            {
                etfStatus.Text = "Start";
                etfStatus.Enabled = false;

                etfInstall.Text = "Install";
                etfInstall.Enabled = true;

            }
        }

        #region EVENT
        private void etfStatus_Click(object sender, EventArgs e)
        {
            try
            {
                var service = GetService("ETFService");

                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                else
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message.ToString();
                Logger.LogError(ex, "ETF-StartService", bgWorker);
                MessageBox.Show(message, "Start Service", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            CheckETFServiceStatus();
        }
        private void etfConfig_Click(object sender, EventArgs e)
        {
            var frm = new frmETFConfig();
            frm.ShowDialog();
        }
        private void etfInstall_Click(object sender, EventArgs e)
        {
            try
            {
                var service = GetService("ETFService");
                if (service != null)
                {
                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Path.Combine(Path.Combine(Application.StartupPath, "Services\\ETF"), "Service.ETF.exe") });
                    Logger.LogMessage("Service uninstalled.", "InstallService", bgWorker, EventLogEntryType.Information);
                    MessageBox.Show("Service uninstalled.", "Install Service", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    ManagedInstallerClass.InstallHelper(new string[] { Path.Combine(Path.Combine(Application.StartupPath, "Services\\ETF"), "Service.ETF.exe") });
                    Logger.LogMessage("Service installed.", "InstallService", bgWorker, EventLogEntryType.Information);
                    MessageBox.Show("Service installed.", "Install Service", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message.ToString();
                Logger.LogError(ex, "ETF-InstallService", bgWorker);
                MessageBox.Show(message, "Install Service", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            CheckETFServiceStatus();
        }
        #endregion
        #endregion

        #region REPORT GENERATOR SERVICE
        private void CheckReportServiceStatus()
        {
            var service = GetService("ReportGeneratorService");
            if (service != null)
            {
                rptStatus.Enabled = true;
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    rptInstall.Text = "Uninstall";
                    rptInstall.Enabled = true;

                    rptStatus.Text = "Start";
                }
                else
                {
                    rptInstall.Text = "Uninstall";
                    rptInstall.Enabled = false;

                    rptStatus.Text = "Stop";
                }
            }
            else
            {
                rptStatus.Text = "Start";
                rptStatus.Enabled = false;

                rptInstall.Text = "Install";
                rptInstall.Enabled = true;

            }
        }

        #region EVENT
        private void rptStatus_Click(object sender, EventArgs e)
        {
            try
            {
                var service = GetService("ReportGeneratorService");

                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                else
                {
                    var rptConfig = new ReportGeneratorConfig().GetConfig(Application.StartupPath, bgWorker);
                    if (CheckPort.IsAvailable(rptConfig.Port.ToInt32()))
                    {
                        FirewallException.AddFirewallException(rptConfig.Port, "ReportGeneratorService");
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                    }
                    else
                    {
                        Logger.LogError("Port not available.", "Report-StartService", bgWorker);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message.ToString();
                Logger.LogError(ex, "Report-StartService", bgWorker);
                MessageBox.Show(message, "Start Service", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            CheckReportServiceStatus();
        }
        private void rptConfig_Click(object sender, EventArgs e)
        {
            var frm = new frmReportConfig();
            frm.ShowDialog();
        }
        private void rptInstall_Click(object sender, EventArgs e)
        {
            try
            {
                var service = GetService("ReportGeneratorService");
                if (service != null)
                {
                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Path.Combine(Path.Combine(Application.StartupPath, "Services\\ReportGenerator"), "Service.ReportGenerator.exe") });
                    Logger.LogMessage("Service uninstalled.", "InstallService", bgWorker, EventLogEntryType.Information);
                    MessageBox.Show("Service uninstalled.", "Install Service", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    ManagedInstallerClass.InstallHelper(new string[] { Path.Combine(Path.Combine(Application.StartupPath, "Services\\ReportGenerator"), "Service.ReportGenerator.exe") });
                    Logger.LogMessage("Service installed.", "InstallService", bgWorker, EventLogEntryType.Information);
                    MessageBox.Show("Service installed.", "Install Service", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message.ToString();
                Logger.LogError(ex, "WebNotificationService-InstallService", bgWorker);
                MessageBox.Show(message, "Install Service", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            CheckReportServiceStatus();
        }
        #endregion

        #endregion

        #region WEB NOTIFICATION SERVICE
        private void CheckWebNotificationStatus()
        {
            var service = GetService("WebNotificationService");
            if (service != null)
            {
                wnStatus.Enabled = true;
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    wnInstall.Text = "Uninstall";
                    wnInstall.Enabled = true;

                    wnStatus.Text = "Start";
                }
                else
                {
                    wnInstall.Text = "Uninstall";
                    wnInstall.Enabled = false;

                    wnStatus.Text = "Stop";
                }
            }
            else
            {
                wnStatus.Text = "Start";
                wnStatus.Enabled = false;

                wnInstall.Text = "Install";
                wnInstall.Enabled = true;

            }
        }
        #region EVENT
        private void wnInstall_Click(object sender, EventArgs e)
        {
            try
            {
                var service = GetService("WebNotificationService");
                if (service != null)
                {
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = true;
                    startInfo.WorkingDirectory = @"C:\Windows\System32";
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Verb = "runas";
                    startInfo.Arguments = @"/k sc delete WebNotificationService";
                    process.StartInfo = startInfo;
                    process.Start();
                    Logger.LogMessage("Service uninstalled.", "InstallService", bgWorker, EventLogEntryType.Information);
                    MessageBox.Show("Service uninstalled.", "Install Service", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = true;
                    startInfo.WorkingDirectory = @"C:\Windows\System32";
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Verb = "runas";
                    startInfo.Arguments = "/k sc create WebNotificationService start=\"auto\" DisplayName=\"Web Notification Service\" BinPath=\"" + Path.Combine(Path.Combine(Application.StartupPath, "Services\\WebNotification"), "Service.WebNotification.exe") + "\"";
                    process.StartInfo = startInfo;
                    process.Start();

                    startInfo.Arguments = "/k sc description WebNotificationService \"This is a service for InSys.Suite that notifies user's in real time.\" ";
                    process.StartInfo = startInfo;
                    process.Start();

                    Logger.LogMessage("Service installed.", "InstallService", bgWorker, EventLogEntryType.Information);
                    MessageBox.Show("Service installed.", "Install Service", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message.ToString();
                Logger.LogError(ex, "WebNotificationService-InstallService", bgWorker);
                MessageBox.Show(message, "Install Service", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            CheckWebNotificationStatus();
        }

        private void wnStatus_Click(object sender, EventArgs e)
        {
            try
            {
                var service = GetService("WebNotificationService");

                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                else
                {
                    var wnConfig = new WebNotificationConfig().GetConfig(Application.StartupPath, bgWorker);
                    if (CheckPort.IsAvailable(wnConfig.Port.ToInt32()))
                    {
                        FirewallException.AddFirewallException(wnConfig.Port, "WebNotificationService");
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                    }
                    else
                    {
                        Logger.LogError("Port not available.", "WebNotification-StartService", bgWorker);
                    }

                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message.ToString();
                Logger.LogError(ex, "WebNotification-StartService", bgWorker);
                MessageBox.Show(message, "Start Service", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            CheckWebNotificationStatus();
        }

        private void wnConfig_Click(object sender, EventArgs e)
        {
            var frm = new frmWebNotificationConfig();
            frm.ShowDialog();
        }
        #endregion
        #endregion
    }
}
