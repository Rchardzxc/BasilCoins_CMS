using System;
using System.Collections.Generic;
using System.Text;

namespace Service.WebNotification.Classes
{
    public class vProcessNotification
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public int Current_Process { get; set; }
        public int Total_Process { get; set; }
        public int ID_Receiver { get; set; }
        public bool IsStillRunning { get; set; }
        public DateTime DateTimeCreated { get; set; } = DateTime.Now;
        public bool IsStop { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
    }
}
