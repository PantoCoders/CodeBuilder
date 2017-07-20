using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Howfar.BuildCode.App_Code
{
    public class Public
    {
        public static string MapCsharpType(string dbtype, bool NotNull)
        {
            string strNull = NotNull ? "" : "?";
            if (string.IsNullOrEmpty(dbtype)) return dbtype;
            dbtype = dbtype.ToLower();
            string csharpType = "object";
            switch (dbtype)
            {
                case "bigint": csharpType = "long"; break;
                case "binary": csharpType = "byte[]"; break;
                case "bit": csharpType = "bool" + strNull; break;
                case "char": csharpType = "string"; break;
                case "date": csharpType = "DateTime" + strNull; break;
                case "DateTime": csharpType = "DateTime" + strNull; break;
                case "datetime2": csharpType = "DateTime" + strNull; break;
                case "datetimeoffset": csharpType = "DateTimeOffset"; break;
                case "decimal": csharpType = "decimal"+strNull; break;
                case "float": csharpType = "double"+strNull; break;
                case "image": csharpType = "byte[]"; break;
                case "int": csharpType = "int" + strNull; break;
                case "money": csharpType = "decimal"+strNull; break;
                case "nchar": csharpType = "string"; break;
                case "ntext": csharpType = "string"; break;
                case "numeric": csharpType = "decimal"+strNull; break;
                case "nvarchar": csharpType = "string"; break;
                case "real": csharpType = "Single"; break;
                case "smalldatetime": csharpType = "DateTime" + strNull; break;
                case "smallint": csharpType = "short"; break;
                case "smallmoney": csharpType = "decimal"+strNull; break;
                case "sql_variant": csharpType = "object"; break;
                case "sysname": csharpType = "object"; break;
                case "text": csharpType = "string"; break;
                case "time": csharpType = "TimeSpan"; break;
                case "timestamp": csharpType = "byte[]"; break;
                case "tinyint": csharpType = "byte"; break;
                case "uniqueidentifier": csharpType = "Guid" + strNull; break;
                case "varbinary": csharpType = "byte[]"; break;
                case "varchar": csharpType = "string"; break;
                case "xml": csharpType = "string"; break;
                default: csharpType = "object"; break;
            }
            return csharpType;
        }
    }
}