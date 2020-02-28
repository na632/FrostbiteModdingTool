using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;

namespace v2k4FIFAModding
{
    public static class v2k4Util
    {
        public static IEnumerable<DataRow> ToEnumerable(this DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
                yield return row;
        }
    }
}
