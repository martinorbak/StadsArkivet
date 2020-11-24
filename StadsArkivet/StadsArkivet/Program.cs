using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace StadsArkivet
{
    class Program
    {
        static void Main(string[] args)
        {
            GetTableTypes();
        }
        public static void GetTableTypes()
        {
            Console.WriteLine("######################     Starting Program    #########################");
            var indicesPath = System.IO.Path.GetFullPath(@"..\..\Database\Indices\tableIndex.xml");
            var TablePath_1 = System.IO.Path.GetFullPath(@"..\..\Database\Tables\table1\table1.xml");
            var TablePath_2 = System.IO.Path.GetFullPath(@"..\..\Database\Tables\table2\table2.xml");
            var TablePath_3 = System.IO.Path.GetFullPath(@"..\..\Database\Tables\table3\table3.xml");

            Console.WriteLine("######################     Getting Indicies   #########################");
            var indicies = GetIndicies(indicesPath);

            Console.WriteLine("######################     Checking Table 1  #########################");
            var checkTbl1 = CheckTable(TablePath_1, "http://www.sa.dk/xmlns/siard/1.0/schema0/table1.xsd", "table1", indicies);

            Console.WriteLine("######################     Checking Table 2  #########################");
            var checkTbl2 = CheckTable(TablePath_2, "http://www.sa.dk/xmlns/siard/1.0/schema0/table2.xsd", "table2", indicies);

            Console.WriteLine("######################     Checking Table 3  #########################");
            var checkTbl3 = CheckTable(TablePath_3, "http://www.sa.dk/xmlns/siard/1.0/schema0/table3.xsd", "table3", indicies);

            Console.WriteLine("######################     Finished all checks #########################");

            Console.WriteLine("######################        Success Status:       #########################");
            Console.WriteLine("######  Table 1="+ checkTbl1+ " -- Table 2=" + checkTbl2 + " -- Table 3=" + checkTbl3 + "  ##########");  
        }


        public static Dictionary<string, Dictionary<string, string>> GetIndicies(string path)
        {
            XDocument IndicesDoc = XDocument.Load(path);            //Load in xml data
            XNamespace ns = "http://www.sa.dk/xmlns/diark/1.0";     //define namespace used in xml
            var tables = IndicesDoc.Descendants(ns + "table");      //Find tables

            //Will save relevant data into a dictionary (table name, dictionary with column names and datatype)
            Dictionary<string, Dictionary<string, string>> tablesDict = new Dictionary<string, Dictionary<string, string>>();

            foreach (var table in tables)
            {
                //save table name and find columns
                var tableName = table.Descendants(ns + "folder").FirstOrDefault().Value;
                var columns = table.Descendants(ns + "columns").Descendants(ns + "column");

                //create dictionary to hold column data then loop columns
                var columnsDict = new Dictionary<string, string>();
                foreach (var column in columns)
                {
                    //find and save column name and data type
                    var columnName = column.Descendants(ns + "columnID").FirstOrDefault().Value;
                    var columnType = column.Descendants(ns + "typeOriginal").FirstOrDefault().Value;
                    columnsDict.Add(columnName, columnType);
                }

                //add whole table info to dictionary
                tablesDict.Add(tableName, columnsDict);
            }
            return tablesDict;
        }


        public static bool CheckTable(string path, string nameSpace, string tableName, Dictionary<string, Dictionary<string, string>> tables)
        {
            var success = true;                                                     //Status variable
            XDocument tableDoc = XDocument.Load(path);                              //Load in xml data
            XNamespace ns = nameSpace;  //define namespace used in xml
            var rows = tableDoc.Descendants(ns + "row");                            //Find rows
            
            //go through each row and elements in the row
            foreach (var row in rows)
            {
                foreach (var element in row.Descendants())
                {
                    var name = element.Name.LocalName;
                    var value = element.Value;                    
                    
                    var fileType = tables.Where(x => x.Key.Equals(tableName) &&                                 //Check table definion that row belongs to
                                                        x.Value.Where(i => i.Key.Equals(name)).Count() > 0)     //Find Column that matches element name
                                                        .FirstOrDefault()                                       //Should never find more than one so take first
                                                        .Value[name];                                           //Take datatype from definition for that field

                    //test if value is an int
                    var intValue = 0;
                    if (Int32.TryParse(value, out intValue))
                    {
                        //if definition expect a string, then it failled
                        if(fileType.Contains("char"))                       
                            success = false;

                    }
                    //check if we are looking for a string
                    else if (fileType.Contains("char"))
                    {
                        //check max allowed lenght of string
                        var strLength = Int32.Parse(String.Join("", fileType.Where(char.IsDigit)));

                        //ensure value does not exceed allowed, else it failled
                        if (value.Length > strLength)
                        {
                            success = false;
                        }
                    }                 
                }
            }
            return success;
        }
    }
}
