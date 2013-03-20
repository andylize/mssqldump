using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Sdk.Sfc;

namespace mssqldump
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2) { ShowUsage(); return 1; }

            Server sqlServer = new Server(args[0]);
            Database db = default(Database);

            db = sqlServer.Databases[args[1]];

            string filePath = args[2];

            DateTime now = DateTime.Now;
            // set up text file
            string filename = string.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}_{5}.sql", now.Year, now.Month, now.Day, now.Hour, now.Minute, args[1]);
            filename = Path.Combine(filePath, filename);

            Scripter scrp = default(Scripter);

            scrp = new Scripter(sqlServer);

            scrp.Options.ScriptSchema = true;
            scrp.Options.WithDependencies = true;
            scrp.Options.ScriptData = false;


            Urn[] smoObjects = new Urn[2];
            int objectCount = 0;

            // write each table
            foreach (Table tb in db.Tables) 
            {
                if (tb.IsSystemObject == false)
                {
                    Console.WriteLine("Table: {0}", tb.Urn);

                    smoObjects = new Urn[1];
                    smoObjects[0] = tb.Urn;

                    using (StreamWriter w = File.AppendText(filename))
                    {
                        foreach (string s in scrp.EnumScript(new Urn[] { tb.Urn }))
                        {
                            w.WriteLine(s);
                            Console.Write(".");
                            objectCount++;
                        }
                        w.Close();
                    }
                }
                Console.WriteLine();

                Console.Write("-Indexes: ");
                // write each index
                foreach (Index ix in tb.Indexes)
                {
                    if (ix.IsSystemObject == false)
                    {
                        Console.Write(".");
                        objectCount++;
                        
                        using (StreamWriter w = File.AppendText(filename))
                        {
                            StringCollection indexScript = ix.Script();
                            foreach (string s in indexScript)
                            {
                                w.WriteLine(s);
                            }
                            w.Close();
                        }
                    }
                }
                Console.WriteLine();

                Console.Write("-Triggers: ");
                // write each trigger
                foreach (Trigger trig in tb.Triggers)
                {
                    if (trig.IsSystemObject == false)
                    {
                        Console.Write(".");
                        objectCount++; 
                        
                        smoObjects = new Urn[1];
                        smoObjects[0] = trig.Urn;

                        using (StreamWriter w = File.AppendText(filename))
                        {
                            foreach (string s in scrp.EnumScript(new Urn[] { trig.Urn }))
                            {
                                w.WriteLine(s);
                            }
                            w.Close();
                        }
                    }
                }
                Console.WriteLine();//finished triggers

                //next table
                Console.WriteLine();
            }

            // write each view
            Console.Write("Views: ");
            foreach (View vw in db.Views)
            {
                if (vw.IsSystemObject == false)
                {
                    Console.Write(".");
                    objectCount++;

                    smoObjects = new Urn[1];
                    smoObjects[0] = vw.Urn;

                    using (StreamWriter w = File.AppendText(filename))
                    {
                        foreach (string s in scrp.EnumScript(new Urn[] { vw.Urn }))
                        {
                            w.WriteLine(s);
                        }
                        w.Close();
                    }
                }
            }
            Console.WriteLine();

            Console.Write("Stored Procedures: ");
            // write each stored procedure
            foreach (StoredProcedure sp in db.StoredProcedures)
            {
                if (sp.IsSystemObject == false)
                {
                    Console.Write(".");
                    objectCount++;
                    
                    smoObjects = new Urn[1];
                    smoObjects[0] = sp.Urn;

                    using (StreamWriter w = File.AppendText(filename))
                    {
                        foreach (string s in scrp.EnumScript(new Urn[] { sp.Urn }))
                        {
                            w.WriteLine(s);
                        }
                        w.Close();
                    }
                }
            }
            Console.WriteLine();

            // write each user defined funtion
            Console.Write("UserDefinedFunctions: ");
            foreach (UserDefinedFunction udf in db.UserDefinedFunctions)
            {
                if (udf.IsSystemObject == false)
                {
                    smoObjects = new Urn[1];
                    smoObjects[0] = udf.Urn;

                    using (StreamWriter w = File.AppendText(filename))
                    {
                        foreach (string s in scrp.EnumScript(new Urn[] { udf.Urn }))
                        {
                            w.WriteLine(s);
                            Console.Write(".");
                        }
                        w.Close();
                    }
                    objectCount++;
                }
            }
            Console.WriteLine();

            ReportProgress(objectCount);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("File written: {0}", filename);

            return 0;
        }

        private static void ReportProgress(int objectCount)
        {
            Console.WriteLine("Objects written: {0}", objectCount);
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage: mssqldump [server] [database] [outputpath]");
        }

        private static string pad(string value, int length)
        {
            while (value.Length < length)
            {
                value = "0" + value;
            }
            return value;
        }
    }
}
