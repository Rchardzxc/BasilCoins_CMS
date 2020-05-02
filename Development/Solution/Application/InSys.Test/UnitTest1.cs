using System;
using System.Diagnostics;
using System.Timers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InSys.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Timer aTimer = new Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 5000;
            aTimer.Enabled = true;
            aTimer.Start();
        }
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Debug.WriteLine("Hello World!");
        }
    }
}
