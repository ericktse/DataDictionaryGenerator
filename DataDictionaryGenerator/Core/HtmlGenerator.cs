using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DataDictionaryGenerator.Model;

namespace DataDictionaryGenerator.Core
{
    public class HtmlGenerator
    {
        private static string CATALOGUETEMPLATE = "Template.CatalogueTemplate.html";
        private static string TABLETEMPLATE = "Template.TableTemplate.html";
        public static string CATALOGUEFILENAME = "数据表目录";
        public static string TABLEDICNAME = "表结构";

        public static void GeneralHtml(string name, IList<TableInfo> data)
        {
            //- 创建目录
            string path = Path.GetFullPath(name);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }

            //- 生成目录html
            var catalogueData = GeneralCatalogueData(data);
            var catalogueFile = Path.Combine(path, $"{CATALOGUEFILENAME}.html");

            GeneralCatalogue(catalogueFile, catalogueData);

            //- 生成表html
            // 创建目录
            path = Path.GetFullPath($"{name}\\{TABLEDICNAME}");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);

            //- 生成目录html
            var tableDatas = GeneralTableData(data);
            foreach (var tabledata in tableDatas)
            {
                var tableFile = Path.Combine(path, $"{tabledata["title"]}.html");
                GeneralTable(tableFile, tabledata);
            }

        }

        public static void DeleteHtml(string name)
        {
            if (Directory.Exists(name))
            {
                Directory.Delete(name, true);
            }
        }

        private static Hashtable GeneralCatalogueData(IList<TableInfo> data)
        {
            Hashtable hash = new Hashtable();
            hash.Add("title", CATALOGUEFILENAME);
            hash.Add("tabletitle", CATALOGUEFILENAME);

            int i = 1;
            List<Hashtable> list = new List<Hashtable>();
            foreach (TableInfo table in data)
            {
                Hashtable subhash = new Hashtable();
                subhash.Add("tdnum", i);
                subhash.Add("tdhref", $@"{TABLEDICNAME}\{table.Name.Replace("/", "▪").Replace("\\", "▪")}.html");
                subhash.Add("tdname", table.Name);
                subhash.Add("tdremark", string.IsNullOrEmpty(table.Comment) ? "&nbsp;" : table.Comment);

                i++;
                list.Add(subhash);
            }
            hash.Add("tabledetail", list);
            return hash;
        }

        private static void GeneralCatalogue(string fileName, Hashtable param)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{typeof(HtmlGenerator).Namespace}.{CATALOGUETEMPLATE}";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string text = reader.ReadToEnd(); ;
                    string html = HtmlConvertor.ParseHtml(text, param);

                    File.WriteAllText(fileName, html);
                }
            }
        }

        private static IList<Hashtable> GeneralTableData(IList<TableInfo> data)
        {
            IList<Hashtable> hashList = new List<Hashtable>();

            foreach (var tableinfo in data)
            {
                Hashtable hash = new Hashtable();
                hash.Add("title", tableinfo.Name);
                hash.Add("tabletitle", tableinfo.Name);
                hash.Add("tablename", tableinfo.Name);
                hash.Add("tableremark", tableinfo.Comment);
                hash.Add("tablehref", $"../{CATALOGUEFILENAME}.html");

                int i = 1;
                List<Hashtable> list = new List<Hashtable>();
                foreach (ColumnInfo column in tableinfo.Columns)
                {
                    Hashtable subhash = new Hashtable();
                    subhash.Add("tdnum", i);
                    subhash.Add("tdname", column.Name);
                    subhash.Add("tdtype", column.DataType);
                    subhash.Add("tdprimarykey", column.IsPrimaryKey ? "√" : "&nbsp;");
                    subhash.Add("tdnull", !column.Mandatory ? "√" : "&nbsp;");
                    subhash.Add("tdremark", string.IsNullOrEmpty(column.Comment) ? "&nbsp;" : column.Comment);

                    i++;
                    list.Add(subhash);
                }
                hash.Add("tabledetail", list);
                hashList.Add(hash);
            }

            return hashList;
        }

        private static void GeneralTable(string fileName, Hashtable param)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{typeof(HtmlGenerator).Namespace}.{TABLETEMPLATE}";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string text = reader.ReadToEnd(); ;
                    string html = HtmlConvertor.ParseHtml(text, param);

                    File.WriteAllText(fileName, html);
                }
            }
        }
    }
}
