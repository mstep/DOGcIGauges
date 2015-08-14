using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;

namespace CIGauges.cs
{
    class GaugesToI
    {
        public static void write(DataSet ds, Program.Member usr, DataSet dserrors, OdbcConnection connIS)
        {
            Logger oLogger = new Logger();
            Console.WriteLine("Writting Production");
            String strWriteGauge = "insert into " + usr.idatalib + ".PDWD2PF" +   //enWS2PF
                    " (" +  //14 fields
                     "PDDTLC," +//last change date
                     "AUXID," +
                     "PDLEAS," +
                     "PDRPYY," +
                     "PDRPMM," +//5
                     "PDRPDD," +//6
                     "PDCUS," +
                      "PDTBSW," +
                      "WTRINO," + //7,2
                      "PDRMKS," +  //10..50 chr
                      "PDTN1," + //10
                      "PDTG1," +// 7,3 gauge
                      "PDTV1," +  // gross vol
                      "BATT," + //13
                      "BEAT," + //14
                      "PDQTR," +//15 7,2
                      "PDTUBP," +
                      "PDCASP," +
                      "PDCHOK," +
                      "PDSPM" + //20
                       " )" +
               " values(" +  //must insert in order of fields
                     "?," +
                     "?," +
                     "?," +
                     "?," +
                     "?, " +
                     "?," +
                     "?," +
                     "?," +
                     "?," +
                     "?," +  //10
                     "?," +
                      "?," +
                     "?," +
                      "?," +
                     "?," +
                     "?," +
                     "?," +
                      "?," +
                     "?," +
                     "?" +//20
                  ")";
                DataTable DOGGDGauges = ds.Tables["DOGGGauges"];
                //DataTable DOGGDownWellComp = ds.Tables["DOGGWellComp"];
                // join the code to the adjustment codes, may have gas adj at some point
                var GaugeDays = from T1 in DOGGDGauges.AsEnumerable()
                                //join T2 in DOGGDownWellComp.AsEnumerable() on (int)T1["ID"] equals (int)T2["ID"]
                                select new
                                {
                                    lease = T1.Field<string>("LeaseNumber").Trim(),
                                    tank = T1.Field<string>("TankNumber").Trim(),
                                    readingDecimal = T1.Field<Decimal>("DecimalReading"),//7,3
                                    wasDOGG = T1.Field<Boolean>("WasDOGGEntered"),
                                    readDate = T1.Field<DateTime>("ReadingDate"),
                                    entryDate = T1.Field<DateTime>("OriginalEntryDate"),
                                    changeDate = T1.Field<DateTime>("LastDateChanged"),
                                    TubingPres = T1.Field<Int32>("TubingPressure"),
                                    CasingPres = T1.Field<Int32>("CasingPressure"),
                                    ChokeSiz = T1.Field<string>("ChokeSize"),
                                   StrokesPer = T1.Field<double>("StrokesPerMinute"),
                                    Remarks = T1.Field<string>("Remarks"),
                                    TransferDirection = T1.Field<int>("TransferDirection")
                                };
                //must insert in order of fields
                int growcount=0;
                try
                {
                  foreach (var GRow in GaugeDays)
                   {   //format each column of DOGG tickets for Iseries
                    OdbcCommand writeGauge = new OdbcCommand(strWriteGauge, connIS);
                    growcount++;
                    try
                    {
                        int dateentry = DateIBM.MStoIBMDate(DateTime.Now);
                        writeGauge.Parameters.AddWithValue("@PDDTLC",GRow.changeDate);//change date
                        writeGauge.Parameters.AddWithValue("@AUXID",GRow.lease);
                        writeGauge.Parameters.AddWithValue("@PDLEAS", GRow.lease);
                        writeGauge.Parameters.AddWithValue("@PDRPYY", DateIBM.MStoIBMDateYY(GRow.readDate)); // GRow.readDate.Year);
                        writeGauge.Parameters.AddWithValue("@PDRPMM", DateIBM.MStoIBMDateMM(GRow.readDate)); //GRow.readDate.Month);//5
                        writeGauge.Parameters.AddWithValue("@PDRPDD", DateIBM.MStoIBMDateDD(GRow.readDate)); //GRow.readDate.Day);
                        writeGauge.Parameters.AddWithValue("@PDCUS", "40");
                        writeGauge.Parameters.AddWithValue("@PDTBSW", 0.00M);
                        writeGauge.Parameters.AddWithValue("@WTRINO", 0.00M);
                        writeGauge.Parameters.AddWithValue("@PDRMKS", GRow.Remarks); //10
                        writeGauge.Parameters.AddWithValue("@PDTN1", GRow.tank);  // 15 digits
                        writeGauge.Parameters.AddWithValue("@PDTG1", GRow.readingDecimal);//gauge  
                        writeGauge.Parameters.AddWithValue("@PDTV1", 0.00M); // calc vol
                        writeGauge.Parameters.AddWithValue("@BATT", "DOGG"); //
                        writeGauge.Parameters.AddWithValue("@BEAT", " ");
                        int ft = Convert.ToInt32(GRow.readingDecimal / (100));
                        int ftinquarters = ft * 48;
                        int inches = Convert.ToInt32(GRow.readingDecimal) - (ft * 100);
                        int inchesinquarters = inches * 4;
                        int ftandinches = (ft * 100) + inches;
                        int quarters = Convert.ToInt32(((GRow.readingDecimal - ftandinches) * 100) / 25);
                        int ttlqtrs = (ftinquarters + inchesinquarters + quarters);
                        writeGauge.Parameters.AddWithValue("@PDQTR", ttlqtrs);
                        writeGauge.Parameters.AddWithValue("@PDTUBP", GRow.TubingPres);
                        writeGauge.Parameters.AddWithValue("@PDCASP", GRow.CasingPres);
                        Int32 Choke = 0;  // convert choke size to 
                        bool result = Int32.TryParse(GRow.ChokeSiz, out Choke);
                        if (result)
                            writeGauge.Parameters.AddWithValue("@PDCHOK", Choke);
                        else
                            writeGauge.Parameters.AddWithValue("@PDCHOK", 0);
                        decimal Strokes = Convert.ToDecimal(GRow.StrokesPer); // GRow.StrokesPer
                        decimal strokesper =0.0m; // Convert.ToDecimal((GRow.["StrokesPerMinute"].ToString()));
                        strokesper = Math.Round(strokesper, 1, MidpointRounding.AwayFromZero); // Rounds "up"
                        writeGauge.Parameters.AddWithValue("@PDSPM", 0.0m); 
                        writeGauge.ExecuteNonQuery();
                        oLogger.logErrorToLog("Gauge > Iseries:" + GRow.tank.ToString() + " Day:" + GRow.readDate.ToString() + " #:" + growcount.ToString(), usr.PGMNM, usr.logpath);
                        //writeGauge.Dispose();
                    }
                    catch (Exception DBserror)
                    {
                        oLogger.logErrorToLog("Gauges write:" + DBserror.Message, usr.pgmname, usr.logpath) ;//
                    }
                    finally {  writeGauge.Dispose();}
                }
            }
            catch (Exception DBserror)
            {
                oLogger.logErrorToLog("Gauges TO I" + DBserror.Message, usr.pgmname, usr.logpath);
            }
        }
    }
}
