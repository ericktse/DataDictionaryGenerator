using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DataDictionaryGenerator.Model;

namespace DataDictionaryGenerator.Core
{
    public class PdmReader : IReader
    {
        /// <summary>  
        /// 读取指定Pdm文件的实体集合  
        /// </summary>  
        /// <param name="input">Pdm文件名(全路径名)</param>  
        /// <returns>实体集合</returns>  
        public IList<TableInfo> Read(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            //- 加载文件  
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(input);

            //- 必须增加xml命名空间管理，否则读取会报错.  
            var xmlnsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            xmlnsManager.AddNamespace("a", "attribute");
            xmlnsManager.AddNamespace("c", "collection");
            xmlnsManager.AddNamespace("o", "object");

            IList<TableInfo> tables = new List<TableInfo>();

            //- 读取表节点  
            var xnTablesList = xmlDoc.SelectNodes("//c:Tables", xmlnsManager);
            if (xnTablesList != null)
            {
                foreach (var xnTable in from XmlNode xmlTables in xnTablesList from XmlNode xnTable in xmlTables.ChildNodes where xnTable.Name != "o:Shortcut" select xnTable)
                {
                    tables.Add(GetTable(xnTable));
                }
            }

            return tables;
        }

        /// <summary>
        /// 初始化"o:Table"的节点  
        /// </summary>
        /// <param name="xnTable"></param>
        /// <returns></returns>
        private TableInfo GetTable(XmlNode xnTable)
        {
            var mTable = new TableInfo();
            var xe = (XmlElement)xnTable;
            mTable.TableId = xe.GetAttribute("Id");
            var xnTProperty = xe.ChildNodes;
            foreach (XmlNode xnP in xnTProperty)
            {
                switch (xnP.Name)
                {
                    case "a:Name":
                        mTable.Name = xnP.InnerText;
                        break;
                    case "a:Comment":
                        mTable.Comment = xnP.InnerText;
                        break;
                    case "c:Columns":
                        InitColumns(xnP, mTable);
                        break;
                    case "c:Keys":
                        InitPrimaryKeys(xnP, mTable);
                        break;
                }
            }
            return mTable;
        }

        /// <summary>
        /// 初始化"c:Columns"的节点  
        /// </summary>
        /// <param name="xnColumns"></param>
        /// <param name="table"></param>
        private void InitColumns(XmlNode xnColumns, TableInfo table)
        {
            table.Columns = new List<ColumnInfo>();

            foreach (XmlNode xnColumn in xnColumns)
            {
                table.Columns.Add(GetColumn(xnColumn));
            }
        }

        /// <summary>
        /// 初始化Column  
        /// </summary>
        /// <param name="xnColumn"></param>
        /// <returns></returns>
        private ColumnInfo GetColumn(XmlNode xnColumn)
        {
            var column = new ColumnInfo();
            var xe = (XmlElement)xnColumn;
            column.ColumnId = xe.GetAttribute("Id");
            var xnCProperty = xe.ChildNodes;
            foreach (XmlNode xnP in xnCProperty)
            {
                switch (xnP.Name)
                {
                    case "a:Name":
                        column.Name = xnP.InnerText;
                        break;
                    case "a:Comment":
                        column.Comment = xnP.InnerText;
                        break;
                    case "a:DataType":
                        column.DataType = xnP.InnerText;
                        break;
                    case "a:Length":
                        column.Length = xnP.InnerText;
                        break;
                    case "a:Identity":
                        column.Identity = ObjectToBoolean(xnP.InnerText);
                        break;
                    case "a:Column.Mandatory":
                        column.Mandatory = ObjectToBoolean(xnP.InnerText);
                        break;
                    case "a:Precision":
                        column.Precision = xnP.InnerText;
                        break;
                }
            }
            return column;
        }

        /// <summary>
        /// 初始化c:Keys"的节点  
        /// </summary>
        /// <param name="xnKeys"></param>
        /// <param name="table"></param>
        private void InitPrimaryKeys(XmlNode xnKeys, TableInfo table)
        {
            var xe = (XmlElement)xnKeys;
            var xnKProperty = xe.ChildNodes;
            foreach (XmlNode subNode in xnKProperty)
            {
                foreach (XmlNode innerNode in subNode)
                {
                    switch (innerNode.Name)
                    {
                        case "c:Key.Columns":
                            var primaryKeyRefCode = from XmlNode xn in innerNode.ChildNodes select ((XmlElement)xn).GetAttribute("Ref");
                            var cols = table.Columns.Where(x => primaryKeyRefCode.Contains(x.ColumnId));
                            foreach (var col in cols)
                            {
                                col.IsPrimaryKey = true;
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// string to datetime
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        private DateTime String2DateTime(string dateString)
        {
            //PDM文件中的日期格式采用的是当前日期与1970年1月1日8点之差的秒树来保存.  
            DateTime baseDateTime = new DateTime(1970, 1, 1, 8, 0, 0);

            int theTicker = int.Parse(dateString);
            return baseDateTime.AddSeconds(theTicker);
        }

        /// <summary>
        /// object to bool
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool ObjectToBoolean(object value)
        {
            if (value != null)
            {
                string mStr = value.ToString();
                mStr = mStr.ToLower();
                if (mStr.Equals("y") || mStr.Equals("1") || mStr.Equals("true"))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
