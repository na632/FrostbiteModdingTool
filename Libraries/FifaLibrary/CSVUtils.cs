using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaLibrary
{
    public static class CSVUtils
    {
        public static DataTable FromCSV(this DataTable table, string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                // get the first row of csv
                string header = reader.ReadLine();
                var fields = header.Split(',');

                foreach (string column in fields)
                {
                    // add columns to new datatable based on first row of csv
                    table.Columns.Add(column);
                }

                string row = reader.ReadLine();
                // read to end
                while (row != null)
                {
                    // add each row to datatable 
                    var rowDatas = row.Split(',');
                    table.Rows.Add(rowDatas);
                    row = reader.ReadLine();
                }
            }
            return table;
        }

        public static void ToCSV(this DataTable dtDataTable, string path)
        {
            if(File.Exists(path))
                File.Delete(path);  

            dtDataTable.ToCSV(new FileStream(path, FileMode.Create));
        }

        public static void ToCSV(this DataTable dtDataTable, Stream stream)
        {
            StreamWriter sw = new StreamWriter(stream);
            //headers    
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }
    }
}
