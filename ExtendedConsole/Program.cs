using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;

namespace ExtendedConsole
{
    public class Program
    {
        public static void Main()
        {
            DVDScreensaver.Start(60, true, false);
        }
    }
}