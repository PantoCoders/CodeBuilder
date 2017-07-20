using Howfar.BuildCode.App_Code;
using Howfar.BuildCode.Models;
using Panto.Map.Extensions.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Howfar.BuildCode.Controllers
{
    public class HomeController : Controller
    {
        static List<Table> StaticDataList = new List<Table>();
        public ActionResult Index()
        {
            return View();
        }

        public void SetData(List<Table> DataList,ConfigInfo ConfigInfo)
        {
            StaticDataList = DataList != null ? DataList.Where(t => t.IsCheck == true).ToList() : StaticDataList;
            List<Table> PKList = new List<Table>();
            if (StaticDataList != null)
            {
                PKList = GetPKList(StaticDataList[0].TableName);
            }
            for (int i = 0; i < StaticDataList.Count; i++)
            {
                StaticDataList[i].IsPK = PKList.Where(t => t.ColumnName == StaticDataList[i].ColumnName).Count() > 0;
                StaticDataList[i].CsharpType = Public.MapCsharpType(StaticDataList[i].TypeName, StaticDataList[i].NotNUll);
            }
        }

        #region · GetPKList
        public List<Table> GetPKList(string TableName)
        {
            string sql = $@"SELECT  col.name ColumnName
                            FROM    sys.key_constraints c
                                    LEFT JOIN sys.indexes i ON c.parent_object_id = i.object_id
                                                                AND c.unique_index_id = i.index_id
                                    LEFT JOIN sys.index_columns ic ON i.object_id = ic.object_id
                                                                        AND i.index_id = ic.index_id
                                                                        AND ic.key_ordinal > 0
                                    LEFT JOIN sys.all_columns col ON ic.column_id = col.column_id
                                                                        AND ic.object_id = col.object_id
                                    LEFT JOIN sys.schemas s ON c.schema_id = s.schema_id
                                    LEFT JOIN sys.objects o ON c.parent_object_id = o.object_id
                                    LEFT JOIN sys.extended_properties ep ON c.object_id = ep.major_id
                                                                            AND ep.minor_id = 0
                                                                            AND ep.class = 1
                                                                            AND ep.name = 'MS_Description'
                            WHERE   s.name = N'dbo'
                                    AND o.type IN ( 'U', 'S', 'V' )
                                    AND o.name = N'{TableName}'";
            return CPQuery.From(sql).ToList<Table>();
        }

        #endregion

        #region · BuildEditHTML
        public ActionResult BuildEditHTML()
        {
            ViewBag.Content = EditHtml();
            Table Entity = new Table();
            Entity.EntityList = StaticDataList;
            return View(Entity);
        }

        public string EditHtml()
        {
            List<string> sb = new List<string>();
            int Index = 0;
            int Count = StaticDataList.Count;
            foreach (var item in StaticDataList)
            {
                if (Index % 2 == 0) { sb.Add("<div class=\"form-group maginWidth\">"); }
                sb.Add($"    <label class=\"col-xs-2 control-label form-left\">{item.Comment}</label>");
                sb.Add("    <div class=\"col-xs-3 form-center\">");
                sb.Add($"        <input type=\"text\" id=\"{item.ColumnName}\" name=\"{item.ColumnName}\" value=\"@Model.Entity.{item.ColumnName}\" />");
                sb.Add("    </div>");
                sb.Add("    <div class=\"col-xs-1 form-right\"></div>");
                if ((Index + 1) == Count || Index % 2 == 1) { sb.Add("</div>"); }
                Index++;
            }
            sb = sb.Where(t => t != string.Empty).ToList();
            return string.Join("\r\n", sb);
        }

        #endregion


        #region · BuildListJS
        public ActionResult BuildListJS()
        {
            ViewBag.ListJSTitleContent = ListJS();
            Table Entity = new Table();
            Entity.EntityList = StaticDataList;
            return View(Entity);
        }

        public string ListJS()
        {
            List<string> sb = new List<string>();
            List<Table> List = StaticDataList.Where(t => t.IsShow == true).ToList();
            int Index = 0, Count = List.Count;
            foreach (var item in List)
            {
                sb.Add("          {");
                sb.Add($"           selectName: '{item.ColumnName}',");
                sb.Add($"           name: '{item.Comment}',");
                sb.Add($"           width: '150px',");
                sb.Add($"           sortable: 'true',");
                sb.Add($"           type: 'link',");
                sb.Add(string.Format("           align: '{0}',", item.TypeName.Contains("int") ? "right" : "left"));
                if (item.TypeName.ToLower().Contains("date"))
                {
                    sb.Add("            fn: function (e) {");
                    sb.Add("                if (e != null && e != '') {");
                    sb.Add($"                   return $.JsonToDateTimeString(e, 'yyyy - MM - dd');");
                    sb.Add("                } else {");
                    sb.Add($"                   return '';");
                    sb.Add("                }");
                    sb.Add("            }");
                }
                sb.Add(string.Format("          }}{0}", (Index + 1) == Count ? "" : ","));
                Index++;
            }

            return string.Join("\r\n", sb).Replace(",\r\n          }", "\r\n          }");
        }

        #endregion

        //public ActionResult test(List<Table> DataList)
        //{
        //    StaticDataList = DataList != null ? DataList : StaticDataList;
        //    ViewBag.Content = BuildEditHtml(StaticDataList);
        //    return View();
        //}

        #region · BuildEntity
        public ActionResult BuildEntity()
        {
            Table Entity = new Table();
            ViewBag.NormalContent = strNormalEntity();
            return View(Entity);
        }
        public string strNormalEntity()
        {
            List<string> sb = new List<string>();
            sb.Add("        #region 标准字段");
            foreach (var item in StaticDataList)
            {
                sb.Add("        /// <summary>");
                sb.Add($"        /// {item.Comment}");
                sb.Add("        /// </summary>");
                sb.Add("        [DataMember] ");
                if (item.IsDataColumn && item.IsPK.Value)
                {
                    sb.Add("        [DataColumn(PrimaryKey = true)] ");
                }
                else if (item.IsDataColumn)
                {
                    sb.Add(string.Format("        [DataColumn(IsNullable = {0})] ", item.NotNUll ? "false" : "true"));
                }
                sb.Add($"        [Description(\"{item.Comment}\")] ");
                sb.Add($"        public {item.CsharpType} {item.ColumnName} {{ get; set; }};");
            }
            sb.Add("        #endregion");

            return string.Join("\r\n", sb);
        }
        #endregion

    }
}