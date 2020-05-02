using InSys.Worker.Library.Classes;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using z.Data;

namespace Service.WebNotification.Classes
{
    public class WebNotificationHub : Hub
    {
        public class UserCredentials
        {
            public string ConnectionID { get; set; }
            public int UserID { get; set; }
            public string SessionID { get; set; }

            public UserCredentials(string _ConnectionID, int _UserID, string _SessionID)
            {
                this.ConnectionID = _ConnectionID;
                this.UserID = _UserID;
                this.SessionID = _SessionID;
            }
        }
        public static List<UserCredentials> Users { get; set; } = new List<UserCredentials>();
        public override Task OnDisconnected(bool stopCalled)
        {
            lock (Users)
            {
                if (Users.Count(x => x.ConnectionID == Context.ConnectionId) > 0)
                {
                    var tmp = Users.Where(x => x.ConnectionID == Context.ConnectionId).FirstOrDefault();
                    if (tmp != null) Users.Remove(tmp);
                }
            }
            return base.OnDisconnected(stopCalled);
        }
        #region Client
        public async Task Register(string SessionID, int ID_User)
        {
            var cred = new UserCredentials(Context.ConnectionId, ID_User, SessionID);
            lock (Users)
            {
                if (Users.Count(x => x.ConnectionID == cred.ConnectionID) == 0)
                    Users.Add(cred);
            }

            foreach (var user in Users.Where(x => x.UserID == cred.UserID && x.ConnectionID != cred.ConnectionID && x.SessionID != cred.SessionID))
                await Clients.Client(user.ConnectionID).SendAsync("logout");
        }
        public void SendNotification(DataTable dt, string sqlConStr, IHubContext _hub)
        {
            List<vWebNotification> notificationData = new List<vWebNotification>();

            notificationData = dt.AsEnumerable().Select(x => new vWebNotification()
            {
                Content = x.Field<string>("Content"),
                DateTimeCreated = x.Field<DateTime>("DateTimeCreated"),
                EmailAddress = x.Field<string>("EmailAddress"),
                ID = x.Field<int>("ID"),
                ID_Receiver = x.Field<int>("ID_Receiver"),
                ID_Sender = x.Field<int>("ID_Sender"),
                IsSeen = x.Field<bool>("IsSeen"),
                IsSent = x.Field<bool>("IsSent"),
                LinkOnClick = x.Field<string>("LinkOnClick"),
                Receiver = x.Field<string>("Receiver"),
                Sender = x.Field<string>("Sender"),
                Title = x.Field<string>("Title"),
                ID_WebNotificationTypes = x.Field<int>("ID_WebNotificationTypes"),
                ReferenceID = x.Field<int>("ReferenceID")
            }).ToList();

            foreach (var dr in notificationData)
            {
                int UserID = dr.ID_Receiver;
                var userCredential = Users.Where(x => x.UserID == UserID).ToList();
                foreach (var user in userCredential)
                {
                    _hub.Clients.Client(user.ConnectionID).newNotification(dr.ToJson());
                }
            }
            using (var con = new SqlConnection(sqlConStr))
            {
                try
                {
                    con.Open();
                    using (var cmd = new SqlCommand("Update dbo.tWebNotification SET IsSent = 1 where ID_Receiver IN (" + string.Join(",", notificationData.Select(x => x.ID_Receiver).ToList()) + ")", con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    string message = (ex.InnerException ?? ex).Message;
                    string LogName = "BAGWIS";
                    string SourceName = "Web Notification Service";
                    if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
                    var wnLog = new EventLog(LogName);
                    wnLog.Source = SourceName;
                    wnLog.Log = LogName;
                    Logger.LogError(ex, "HubSendNotification", wnLog);
                }
                finally
                {
                    con.Close();
                }
            }
        }
        #endregion

    }
    public class WebNotificationDependency
    {
        public BaseConfig bCfg { get; set; }
        private SqlConnection con;
        private readonly IHubContext webHub;
        private EventLog wnLog { get; set; }
        public WebNotificationDependency(IHubContext _hub, BaseConfig _bCfg, EventLog _wnLog)
        {
            webHub = _hub;
            bCfg = _bCfg;
            wnLog = _wnLog;
            try
            {
                if(bCfg == null)
                {
                    Logger.LogMessage("BaseConfig is null", "WebNotificationDependency", wnLog, EventLogEntryType.Information);
                }else
                {
                    SqlDependency.Start(bCfg.GetSqlConfig());
                    Logger.LogMessage("Sql Dependency started.", "WebNotificationDependency", wnLog, EventLogEntryType.Information);
                }

            }
            catch (InvalidOperationException ex)
            {
                try
                {
                    using (var sqlcon = new SqlConnection(bCfg.GetSqlConfig()))
                    {
                        sqlcon.Open();
                        using (var sqlcmd = new SqlCommand($"ALTER DATABASE[{bCfg.Database}] SET NEW_BROKER WITH ROLLBACK IMMEDIATE;ALTER DATABASE[{bCfg.Database}] SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;", sqlcon))
                        {
                            sqlcmd.ExecuteNonQuery();
                        }
                        Logger.LogMessage("Created database new broker, broker enabled.", "WebNotificationDependency", wnLog, EventLogEntryType.Information);
                    }
                }catch(Exception ex2)
                {
                    Logger.LogError(ex2, "WebNotificationDependency", wnLog);
                }
                //ALTER DATABASE[Database_name] SET NEW_BROKER WITH ROLLBACK IMMEDIATE
                //ALTER DATABASE[Database_name] SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE
            }
        }
        public void Start()
        {
            try
            {
                InitNotification();
                Logger.LogMessage("Notification started.", "WebNotificationStart", wnLog, EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                if (con != null)
                    if (con.State == ConnectionState.Open)
                        con.Close();

                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ex, "WebNotificationStart", wnLog);
                Dispose();
                Logger.LogMessage("Reconnecting in 30 seconds.", "WebNotificationStart", wnLog, EventLogEntryType.Information);
                Thread.Sleep(30000);
                Start();
            }
        }
        private void InitNotification()
        {
            try
            {
                if (con == null) con = new SqlConnection(bCfg.GetSqlConfig());
                if (con.State == ConnectionState.Closed) con.Open();
                using (var cmd = new SqlCommand("select ID from dbo.tWebNotification where IsSent = 0", con))
                {
                    var dep = new SqlDependency(cmd);
                    dep.OnChange += delegate (object sender, SqlNotificationEventArgs e)
                    {
                        Change(sender, e, con);
                    };
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        using (var ds = new DataSet())
                        {
                            da.Fill(ds);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                con.Close();
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ex, "InitNotification", wnLog);
                Dispose();
            }
        }
        private void Change(object sender, SqlNotificationEventArgs e, SqlConnection con)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                SendNotification(con);
            }
            InitNotification();
        }
        private void SendNotification(SqlConnection con)
        {
            try
            {
                if (con.State == ConnectionState.Closed) con.Open();
                using (var cmd = new SqlCommand("SELECT twn.*, tp.Name Receiver, tp2.Name Sender FROM dbo.tWebNotification twn" +
                    " LEFT JOIN dbo.tUsers tu ON twn.ID_Receiver = tu.ID" +
                    " LEFT JOIN dbo.tEmployee te ON tu.ID_Employee = te.ID" +
                    " LEFT JOIN dbo.tPersona tp ON te.ID_Persona = tp.ID" +
                    " LEFT JOIN dbo.tUsers tu2 ON twn.ID_Sender = tu2.ID" +
                    " LEFT JOIN dbo.tEmployee te2 ON tu.ID_Employee = te2.ID" +
                    " LEFT JOIN dbo.tPersona tp2 ON te.ID_Persona = tp2.ID" +
                    " WHERE twn.IsSent = 0 ORDER BY twn.DateTimeCreated ASC", con))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        using (var dt = new DataTable())
                        {
                            da.Fill(dt);
                            var hub = new WebNotificationHub();
                            if (dt.Rows.Count > 0)
                                hub.SendNotification(dt, bCfg.GetSqlConfig(), webHub);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                //Logger.LogError(ref message, "SendNotificationAsync", "internal", "InSys.Notification");
                Logger.LogError(ex, "SendNotification", wnLog);
                con.Close();
                Dispose();
            }
        }
        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
