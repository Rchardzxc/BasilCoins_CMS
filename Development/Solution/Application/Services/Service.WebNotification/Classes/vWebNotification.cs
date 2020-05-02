using System;
using System.Collections.Generic;
using System.Text;

namespace Service.WebNotification.Classes
{
    public class vWebNotification
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int ID_Receiver { get; set; }
        public int ID_Sender { get; set; }
        public string LinkOnClick { get; set; } = null;
        public DateTime DateTimeCreated { get; set; } = DateTime.Now;
        public string EmailAddress { get; set; } = null;
        public bool IsSent { get; set; } = false;
        public bool IsSeen { get; set; } = false;
        public int? ID_WebNotificationTypes { get; set; } = null;
        public int? ReferenceID { get; set; } = null;
        public string Receiver { get; set; }
        public string Sender { get; set; }
    }
}
