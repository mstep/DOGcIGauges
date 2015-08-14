using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Odbc;
using System.Data;

namespace CIGauges.cs
{
    class Program
    {
        public class Member  //iseries user
        {
            public string logpath  //path to store file
            { get; set; }
            public string datasource  //path 
            { get; set; }
            public string profile //user profile for iseries, entered
            { get; set; }
            public string password //user password for iseries, entered
            { get; set; }
            public string defaultcollection  //user password for iseries, entered
            { get; set; }
            public string LibraryList //user library list
            { get; set; }
            public string xmlpathname//user library list
            { get; set; }
            public string constr //user connection string
            { get; set; }
            public decimal pctcomplete //user connection string
            { get; set; }
            public string logname //set in pgm
            { get; set; }
            public string idatalib //set in pgm
            { get; set; }
            public string outtable//set in pgm
            { get; set; }
            public string sqlserver //set in pgm
            { get; set; }
            public string Ierrors //set in pgm
            { get; set; }
            public string daysback //set in pgm
            { get; set; }
            //public string ODBConnfile //set in pgm
            //{ get; set; }
            public string GaugeTable //set in pgm
            { get; set; }
            public string outdb //set in pgm
            { get; set; }
            public string outTable //set in pgm
            { get; set; }
            public string DOGGConnConfig //set in pgm
            { get; set; }
            public string ODBConn //set in pgm
            { get; set; }
            public string PGMNM //set in pgm
            { get; set; }
            public string ProdConn //set in pgm
            { get; set; }
            public string pgmname //set in pgm
            { get; set; }
            public string ilibfil //set in pgm
            { get; set; }
            public string delDOGG //set in pgm
            { get; set; }
            public Member()
            {
                logpath = ConfigurationManager.AppSettings["logpath"];
                logname = ConfigurationManager.AppSettings["logname"];
                password = ConfigurationManager.AppSettings["password"];
                profile = ConfigurationManager.AppSettings["profile"];
                datasource = ConfigurationManager.AppSettings["iseriesip"];
                sqlserver = ConfigurationManager.AppSettings["sqlserver"];
                idatalib = ConfigurationManager.AppSettings["idatalibrary"];
                outtable = ConfigurationManager.AppSettings["outtable"];
                Ierrors = ConfigurationManager.AppSettings["Iserieserrors"];
                daysback = ConfigurationManager.AppSettings["daysback"];
                GaugeTable = ConfigurationManager.AppSettings["GaugeTable"];
                outdb = ConfigurationManager.AppSettings["outdb"];
                outTable = ConfigurationManager.AppSettings["outTable"];
                DOGGConnConfig = ConfigurationManager.ConnectionStrings["DOGGConn"].ToString();
                ODBConn = ConfigurationManager.ConnectionStrings["ODBConn"].ToString();
                ProdConn = ConfigurationManager.ConnectionStrings["ProductionConnection"].ToString();
                PGMNM = ConfigurationManager.AppSettings["PGMNM"].ToString();
                pgmname = ConfigurationManager.AppSettings["PGMNM"];
                ilibfil = ConfigurationManager.AppSettings["ilibfil"];
                delDOGG = ConfigurationManager.AppSettings["delDOGG"];
            }
        }
        static void Main(string[] args)
        {
              Member usr = new Member();
            OdbcConnection connIS = new OdbcConnection(usr.ODBConn);
            DataSet ds = new DataSet();   
            DataSet dserrors = new DataSet(); 
            try
            {
                 IseriesErrors.TableBuild(dserrors); //.TableBuild(dserrors);
                 Logger oLogger = new Logger();
                 oLogger.createLogFile("Process Started, days look back=" + usr.daysback, usr.pgmname, usr.logpath );
                 connIS.Open();
                 IseriesErrors.AddRowM4(dserrors, "-", "-", DateTime.Today, "Starting:" + usr.pgmname, "1", usr.pgmname, usr.logpath, usr.ilibfil);
                SqlConnection DOGGConn = new SqlConnection(usr.ProdConn );
                DOGGConn.Open();
                int daysback = Convert.ToInt32(usr.daysback);
                TimeSpan daterange = new TimeSpan(daysback, 0, 0, 0, 0);  //days back to load  
                DateTime dtBackDate = DateTime.Today.Subtract(daterange);
                string stBackDate = dtBackDate.ToString("MM/dd/yy") ;
                string strYYYYMM = (dtBackDate.Year * 100 + dtBackDate.Month).ToString();
                DateTime dtfldBackDate = DateTime.Today.Subtract(daterange);  //date format must by '2013-01-01 00:00:00'
                System.DateTime.Today.ToString("MM/dd/yy");
                string strSelectDownTime = "select * from GaugeReadingsTransfer  DT  " +
                     "  where ReadingDate >= '" + stBackDate + "' and TransferDirection = 1 order by leaseNumber, readingdate";
                //string strSelectDownTime = "select * from GaugeReadingsTransfer  DT  " +
                //"  where  TransferDirection = 1";
                SqlDataAdapter adpDownTime = new SqlDataAdapter(strSelectDownTime, DOGGConn);
                adpDownTime.Fill(ds, "DOGGGauges");
                oLogger.logErrorToLog("Gauges Read from DOGG:" + ds.Tables["DOGGGauges"].Rows.Count.ToString(), usr.PGMNM, usr.logpath);
                GaugesToI.write(ds, usr, dserrors, connIS);
                //write dserrors to iseries
                IseriesErrors.Write(dserrors, usr.ilibfil, connIS, usr.pgmname, usr.logpath);
                if (usr.delDOGG == "yes")
                {
                    SqlCommand dltgauges = new SqlCommand("delete from GaugeReadingsTransfer where TransferDirection = 1", DOGGConn);
                    dltgauges.ExecuteNonQuery();
                    oLogger.logErrorToLog("D Gauges Deleted:" + ds.Tables["DOGGGauges"].Rows.Count.ToString(), usr.pgmname, usr.logpath);
                }
                DOGGConn.Close();
                DOGGConn.Dispose();

                connIS.Close();
                connIS.Dispose();

             }
             catch (Exception DBserror)
            {  Logger oLogger = new Logger();
                oLogger.logErrorToLog("DBexcept" + DBserror.Message, usr.pgmname, usr.logpath);
                IseriesErrors.Write(dserrors, usr.ilibfil, connIS, usr.pgmname, usr.logpath);
            }
        }
        }
    }

