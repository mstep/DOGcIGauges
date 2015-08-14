using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIGauges.cs
{
    class DateIBM
    {
        public static DateTime IBMtoMS(int intyr, int intmo, int intday)
        {
            if (0 == intday || 0 == intmo)
                return DateTime.Today;
            else
                return new DateTime(intyr, intmo, intday);
        }


        public static string MStoIBMDate(DateTime dt, int dayshift)
        {
            // send back a date in string mmddyyyy, dayshift can be + or -
            string stday;
            dt = dt.AddDays(dayshift);
            int day = dt.Day;
            if (dt.Day < 1)
                dt = System.DateTime.Now;
            if (dt.Day < 10)
                stday = '0' + day.ToString();
            else
                stday = day.ToString();
            string stmo;
            if (dt.Month < 10)
                stmo = '0' + dt.Month.ToString();
            else
                stmo = dt.Month.ToString();
            string IBMdate = stmo + stday + dt.Year.ToString();
            // send back a date in string mmddyyyy, dayshift can be + or -
            return IBMdate;
        }
        public static int YYYYMMDD(int yr, int mo, int da)
        { // date send by parts return as YYYY MO DAY
            int YYYYMMDD = yr * 10000 + mo * 100 + da;
            return YYYYMMDD;
        }
        public static int YYYYMMDD(int date)
        {   // date in form MMDDYYYY
            int YYYYMMDD = 19800101;
            if (date > 01011980)
            {
                string mo;
                string modayr = date.ToString();
                int intlen = modayr.Length;
                string yr = modayr.Substring(intlen - 4, 4);
                string da = modayr.Substring(intlen - 6, 2);
                if (intlen < 8)
                    mo = modayr.Substring(0, 1);
                else
                    mo = modayr.Substring(0, 2);
                if (Convert.ToInt32(da) > 31)
                    da = "01";
                if (Convert.ToInt32(mo) > 12)
                    mo = mo.Substring(0, 1); ;
                YYYYMMDD = Convert.ToInt32(yr) * 10000 + Convert.ToInt32(mo) * 100 + Convert.ToInt32(da);
            }
            return YYYYMMDD;
        }
        public static int MStoIBMDate(DateTime dt)
        {
            int intmo = MStoIBMDateMM(dt) * 1000000;
            int intday = MStoIBMDateDD(dt) * 10000;
            int intYR = MStoIBMDateYY(dt);
            return intYR + intmo + intday;
        }
        public static int MStoIBMDateMM(DateTime dt)
        {
            int intmo = dt.Month;
            return Convert.ToInt16(intmo);
        }
        public static int MStoIBMDateDD(DateTime dt)
        {
            int intday = dt.Day;
            return Convert.ToInt16(intday);
        }
        public static int MStoIBMDateYY(DateTime dt)
        {
            int intyr = dt.Year;
            return Convert.ToInt16(intyr);
        }
        public static decimal DDMMYY(DateTime dt)
        {
            string stday;
            int day = dt.Day;
            if (dt.Day < 1)
                dt = System.DateTime.Now;
            if (dt.Day < 10)
                stday = '0' + day.ToString();
            else
                stday = day.ToString();
            string stmo;
            if (dt.Month < 10)
                stmo = '0' + dt.Month.ToString();
            else
                stmo = dt.Month.ToString();
            string IBMdateshort = stmo + stday + dt.Year.ToString();
            // send back a date in string mmddyy 
            return Convert.ToDecimal(IBMdateshort);
        }
    }
}
