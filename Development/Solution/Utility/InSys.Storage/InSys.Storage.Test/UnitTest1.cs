using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InSys.Storage.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            var gg = ToURLSlug("BirthdayList.json");



        }

        public string ToURLSlug(string s)
        {

            return Regex.Replace(s, @"[^a-z0-9/.]+", "-", RegexOptions.IgnoreCase)
              .Trim(new char[] { '-' })
              .ToLower();

        }

        [TestMethod]
        public void TestParseNetwork()
        {
            StorageSetting.Init("\\\\\\\\192.168.100.12\\\\Contents");

        }
    }
}
