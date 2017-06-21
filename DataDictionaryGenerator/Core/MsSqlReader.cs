using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using DataDictionaryGenerator.Model;

namespace DataDictionaryGenerator.Core
{
    public class MsSqlReader : IReader
    {
        /// <summary>
        /// 读取数据库表信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IList<TableInfo> Read(string input)
        {
            var tables = GetTables(input);
            var columns = GetColumns(input);
            foreach (var table in tables)
            {
                table.Columns = columns.Where(x => x.ColumnId == table.TableId).ToList();
            }

            return tables;
        }

        /// <summary>
        /// 获取表格信息
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private IList<TableInfo> GetTables(string connectionString)
        {
            var sql = @"SELECT D.name AS Name,D.id AS TableId,F.value AS Comment FROM SYSOBJECTS D 
                        LEFT JOIN SYS.EXTENDED_PROPERTIES F ON D.ID=F.MAJOR_ID AND F.MINOR_ID =0 
                        WHERE xtype IN ('U')
						ORDER BY Name";

            var ds = ExecuteDataset(connectionString, sql);

            if (ds?.Tables == null || ds.Tables.Count <= 0)
                return null;

            var table = ds.Tables[0];

            var list = Table2List<TableInfo>(table);

            return list;
        }

        /// <summary>
        /// 获取表列信息
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private IList<ColumnInfo> GetColumns(string connectionString)
        {
            var sql = @"SELECT [ColumnId] = A.id, 
                --字段序号=A.COLORDER, 
                [Name]=A.NAME, 
                [Identity]=CASE WHEN COLUMNPROPERTY(A.ID,A.NAME,'ISIDENTITY')=1 THEN 1 ELSE 0 END, 
                [IsPrimaryKey]=CASE WHEN EXISTS(SELECT 1 FROM SYSOBJECTS WHERE XTYPE='PK' AND NAME IN (
                  SELECT NAME FROM SYSINDEXES WHERE INDID IN(
                   SELECT INDID FROM SYSINDEXKEYS WHERE ID = A.ID AND COLID=A.COLID 
                   ))) THEN 1 ELSE 0 END, 
                DataType= CASE WHEN	B.NAME LIKE '%char' THEN B.NAME + '(' + CASE WHEN COLUMNPROPERTY(A.ID,A.NAME,'PRECISION') = -1 THEN 'MAX' ELSE CONVERT(NVARCHAR,COLUMNPROPERTY(A.ID,A.NAME,'PRECISION')) END  +')' WHEN B.NAME IN ( 'decimal') THEN B.NAME + '(' + CONVERT(NVARCHAR, COLUMNPROPERTY(A.ID,A.NAME,'PRECISION')) +','+CONVERT(NVARCHAR,COLUMNPROPERTY(A.ID,A.NAME,'SCALE')) +')'ELSE B.NAME END,
                [Length]= COLUMNPROPERTY(A.ID,A.NAME,'PRECISION') , 
                [Precision]=ISNULL(COLUMNPROPERTY(A.ID,A.NAME,'SCALE'),0), 
                [Mandatory]=CASE WHEN A.ISNULLABLE=1 THEN 1 ELSE 0 END, 
                --默认值=CASE WHEN E.TEXT IS NOT NULL THEN SUBSTRING(E.TEXT, 2, LEN(E.TEXT) -2) ELSE '' END,
                Comment=ISNULL(G.[VALUE],'')  FROM SYSCOLUMNS A
                LEFT JOIN SYSTYPES B ON A.XTYPE=B.XUSERTYPE 
                LEFT JOIN SYS.EXTENDED_PROPERTIES G ON A.ID=G.MAJOR_ID AND A.COLID=G.MINOR_ID 
                LEFT JOIN SYSCOMMENTS E ON A.CDEFAULT=E.ID 
                INNER JOIN SYSOBJECTS D ON A.ID=D.ID AND D.XTYPE IN ('U') AND D.NAME<>'DTPROPERTIES' 
                ORDER BY D.XTYPE,D.Name,A.ID,A.COLORDER";

            var ds = ExecuteDataset(connectionString, sql);
            if (ds?.Tables == null || ds.Tables.Count <= 0)
                return null;
            var table = ds.Tables[0];

            var list = Table2List<ColumnInfo>(table);

            return list;
        }



        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        private DataSet ExecuteDataset(string connectionString, string commandText, params SqlParameter[] commandParameters)
        {
            var command = new SqlCommand();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                command.Connection = connection;
                command.CommandText = commandText;
                command.CommandType = CommandType.Text;

                // 赋值参数
                if (commandParameters != null)
                {
                    foreach (var p in commandParameters.Where(p => p != null))
                    {
                        if ((p.Direction == ParameterDirection.InputOutput ||
                             p.Direction == ParameterDirection.Input) &&
                            (p.Value == null))
                        {
                            p.Value = DBNull.Value;
                        }
                        command.Parameters.Add(p);
                    }
                }

                using (var da = new SqlDataAdapter(command))
                {
                    var ds = new DataSet();
                    da.Fill(ds);
                    command.Parameters.Clear();
                    connection.Close();

                    return ds;
                }
            }
        }

        /// <summary>
        /// Table转为指定类型的对象列表
        /// </summary>
        /// <param name="table">数据表</param>
        /// <generic name="T">列表元素数据类型</generic>
        /// <returns>objectType指定类型的列表</returns>
        private IList<T> Table2List<T>(DataTable table) where T : new()
        {
            if (table == null || table.Rows.Count <= 0)
                return new List<T>();

            IList<T> result = new List<T>(table.Rows.Count);


            IList<object[]> propnames = new List<object[]>();
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                string proName = prop.Name;
                if (table.Columns.Contains(proName))
                {
                    object[] temp = new object[] { prop, proName };
                    propnames.Add(temp);
                }
            }

            foreach (DataRow row in table.Rows)
            {
                T t = new T();
                for (int i = 0; i < propnames.Count; i++)
                {
                    PropertyInfo propinfo = propnames[i][0] as PropertyInfo;
                    string name = propnames[i][1] as string;

                    var value = row[name];
                    if (value != null && value != DBNull.Value)
                        propinfo.SetValue(t, Convert.ChangeType(value, propinfo.PropertyType), null);
                }

                result.Add(t);
            }
            return result;
        }
    }
}
