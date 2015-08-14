using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Odbc;

namespace CIGauges.cs
{
    class Logger
    {
        public void logErrorToLog(string strError, string pgmname, string logpath)
        {
            string sLogName = pgmname + DateTime.Now.Year.ToString()
                + DateTime.Now.Month.ToString()
                + DateTime.Now.Day.ToString() + ".txt";
            string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " "
             + DateTime.Now.ToShortTimeString().ToString();
            StreamWriter sw = new StreamWriter(logpath + sLogName, true);
            sw.WriteLine(sLogFormat + "==> " + strError);
            sw.Flush();
            sw.Close();
        }
        public void createLogFile(string strError, string pgmname, string logpath)
        {/// daily log of runs
            string sLogName = pgmname + DateTime.Now.Year.ToString()
                + DateTime.Now.Month.ToString()
                + DateTime.Now.Day.ToString() + ".txt";
            string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " "
               + DateTime.Now.ToShortTimeString().ToString();
            StreamWriter sw = new StreamWriter(logpath + sLogName, true);
            sw.WriteLine(sLogFormat + "==> " + strError);
            sw.Flush();
            sw.Close();
        }
    }
}
