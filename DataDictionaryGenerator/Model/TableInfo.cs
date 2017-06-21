using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDictionaryGenerator.Model
{
    /// <summary>  
    /// 表信息  
    /// </summary>  
    public class TableInfo
    {
        /// <summary>  
        /// 表ID  
        /// </summary>  
        public string TableId { get; set; }

        /// <summary>  
        /// 表名  
        /// </summary>  
        public string Name { get; set; }

        /// <summary>  
        /// 注释  
        /// </summary>  
        public string Comment { get; set; }

        /// <summary>  
        /// 表列集合  
        /// </summary>  
        public IList<ColumnInfo> Columns { get; set; }
    }
}
