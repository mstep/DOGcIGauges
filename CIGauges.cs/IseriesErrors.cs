using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;
using System.Data;

namespace CIGauges.cs
{
    class IseriesErrors
    {
        public static void Write(DataSet dserrors, string ilibfil, OdbcConnection IseriesConn, string pgmname, string logpath )
        {      // write errors to Iseries pass in data set dserrors
            Console.WriteLine("Writting IseriesErrors ");
            try
            {
                DataTable dtErrors = dserrors.Tables["ErrRow"];
                // ?clear error file on iseries PDERxPF
                Logger oLogger = new Logger();
                // note csDComp matches the PGMNAM below
                string strDelete = "Delete from " + ilibfil + " where PGMNM= " + pgmname;
                OdbcCommand cmdDeleteMSG = new OdbcCommand(strDelete, IseriesConn);
                // each pgm will have different file name
                // complete file layout for PDER4PF  
                // String strcmdInsert = "insert into " + usr.idatalib + "." + usr.Ierrors +
                String strcmdInsert = "insert into  " + ilibfil + 
               " (" +
                " PGMNM, " +  // 1
                " MESSAG, " +
                " MSGDAT, " +
                " PRLEAS, " +
                " PRTKYY, " + //5
                " PRTKMM, " +  //6
                " PRTKDD, " +
                " PRTANK," +
                "DIFDAY," +
                "YMDCHG," +  //10
                "BOPD," +
                "MCFD," +
                "RUNTKT," +
                 "BATT," +
                "PUID" +  //15
                ")" +
                    " values( " +
                    "?," +
                    "?," +
                    "?," +
                    "?," +
                    "?," +  //5
                     "?," +
                     "?," +
                    "?," +
                    "?," +
                    "?," + //10
                      "?," +
                     "?," +
                     "?," +
                     "?," +
                     "?" +  //15
                ")";
                OdbcCommand insx = new OdbcCommand(strcmdInsert, IseriesConn);
                var Error = from Errorrow in dtErrors.AsEnumerable()
                            select new    // layout of file PDERxPF
                            {
                                pgm = Errorrow["PGMNM"].ToString(),
                                Comments = Errorrow["MESSAG"].ToString(),
                                Date = (decimal)Errorrow["DATMSG"],
                                AuxID = Errorrow["PRLEAS"].ToString(),
                                yr = (decimal)Errorrow["PRTKYY"],
                                mo = (decimal)Errorrow["PRTKMM"],
                                da = (decimal)Errorrow["PRTKDD"],
                                Tank = Errorrow["PRTANK"].ToString(),
                                Tickt = (decimal)Errorrow["RUNTKT"],
                            };
                int GaugeNumsCount = 0;
                foreach (var ErrRow in Error)
                {   // iseries insert into PDER4PF
                    GaugeNumsCount = GaugeNumsCount + 1;
                    int intYear = (int)ErrRow.yr;
                    int intMonth = (int)ErrRow.mo;
                    int intDay = (int)ErrRow.da;
                    // build data for insert
                    insx.Parameters.Clear();
                    insx.Parameters.AddWithValue("PGMNAM", "csDComp");//1
                    int len = ErrRow.Comments.Length;
                    if (len > 34)
                        len = 34;
                    string errmsg = "ERR-" + ErrRow.Comments.Substring(0, len);
                    if ((errmsg.Length > 1))
                        insx.Parameters.AddWithValue("MESSAG", errmsg);
                    else
                        insx.Parameters.AddWithValue("MESSAG", " ");  //2
                    insx.Parameters.AddWithValue("MSGDAT", ErrRow.Date);
                    insx.Parameters.AddWithValue("PRLEAS", ErrRow.AuxID);
                    insx.Parameters.AddWithValue("PRTKYY", intYear);
                    insx.Parameters.AddWithValue("PRTKMM", intMonth); //6
                    insx.Parameters.AddWithValue("PRTKDD", intDay);
                    insx.Parameters.AddWithValue("PRTANK", ErrRow.Tank);
                    insx.Parameters.AddWithValue("DIFDAY", 0.0M);
                    insx.Parameters.AddWithValue("YMDCHG", 01012014);//10
                    insx.Parameters.AddWithValue("BOPD", 0.0M);
                    insx.Parameters.AddWithValue("MCFD", 0.0M);
                    insx.Parameters.AddWithValue("RUNTKT", ErrRow.Tickt);
                    insx.Parameters.AddWithValue("BATT", " ");
                    insx.Parameters.AddWithValue("PUID", " ");
                    string tnk = ErrRow.Tank;
                    try
                    {   // insert errors are logged but do not stop process
                        insx.ExecuteNonQuery();
                    }
                    catch (Exception Ex)
                    {
                        oLogger.logErrorToLog("ERROR Write to Iseries ErrRow:" +
                             ErrRow + "." + Ex.Message, pgmname, logpath);
                    }
                }
            }
            catch (Exception Ex)
            {

                throw (Ex);
            }
        }
        public static void TableBuild(DataSet dserrors)
        {   // build table def. for errors
            DataTable dterrors = dserrors.Tables.Add("ErrRow");
            dterrors.Columns.Add("PGMNM", typeof(String));
            dterrors.Columns.Add("MESSAG", typeof(String));
            dterrors.Columns.Add("PRLEAS", typeof(string));
            dterrors.Columns.Add("DATMSG", typeof(Decimal));
            dterrors.Columns.Add("PRTKYY", typeof(Decimal));
            dterrors.Columns.Add("PRTKMM", typeof(Decimal));
            dterrors.Columns.Add("PRTKDD", typeof(Decimal));
            dterrors.Columns.Add("PRTANK", typeof(string));
            dterrors.Columns.Add("PRTCKT", typeof(string));
            dterrors.Columns.Add("DIFDAY", typeof(Decimal));
            dterrors.Columns.Add("YMDCHG", typeof(Decimal));
            dterrors.Columns.Add("BOPD", typeof(Decimal));
            dterrors.Columns.Add("MCFD", typeof(Decimal));
            dterrors.Columns.Add("RUNTKT", typeof(Decimal));
            dterrors.Columns.Add("BATT", typeof(string));
            dterrors.Columns.Add("PUID", typeof(string));
        }
        public static void AddRowM4(DataSet dserrors, string AuxID, string Tank, DateTime proddate, string msg, string ticket, string pgmname, string logpath,string IErrors)
        {   // build row to added to table "ErrRow"  for errors  in pder4pf for DOGG
            Logger oLogger = new Logger();
            try
            {
                DataRow newErrRow = dserrors.Tables["ErrRow"].NewRow();
                newErrRow["PGMNM"] = pgmname;
                newErrRow["MESSAG"] = msg;
                newErrRow["DATMSG"] = DateIBM.MStoIBMDate(proddate);
                newErrRow["PRLEAS"] = AuxID;
                newErrRow["PRTKYY"] = DateIBM.MStoIBMDateYY(proddate);
                newErrRow["PRTKMM"] = DateIBM.MStoIBMDateMM(proddate); ;
                newErrRow["PRTKDD"] = DateIBM.MStoIBMDateDD(proddate);
                newErrRow["PRTANK"] = Tank;
                newErrRow["DIFDAY"] = 0;
                newErrRow["YMDCHG"] = DateIBM.MStoIBMDate(proddate);
                newErrRow["BOPD"] = 0.0M;
                newErrRow["MCFD"] = 0.0M;
                newErrRow["RUNTKT"] = Convert.ToInt32(ticket);
                dserrors.Tables["ErrRow"].Rows.Add(newErrRow);
            }
            catch (Exception Ex)
            {
                oLogger.logErrorToLog("ERROR AddRowM4 add:" + Ex.Message, pgmname, logpath);
                throw (Ex);
            }
        }
    }
}
