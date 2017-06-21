using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDictionaryGenerator.Model
{
    /// <summary>  
    /// 表列信息  
    /// </summary>  
    public class ColumnInfo
    {
        /// <summary>  
        /// 列标识  
        /// </summary>  
        public string ColumnId { get; set; }

        /// <summary>  
        /// 是否主键  
        /// </summary>  
        public bool IsPrimaryKey { get; set; }

        /// <summary>  
        /// 列名  
        /// </summary>  
        public string Name { get; set; }

        /// <summary>  
        /// 注释  
        /// </summary>  
        public string Comment { get; set; }

        /// <summary>  
        /// 数据类型  
        /// </summary>  
        public string DataType { get; set; }

        /// <summary>  
        /// 数据长度  
        /// </summary>  
        public string Length { get; set; }

        /// <summary>  
        /// 是否自增量  
        /// </summary>  
        public bool Identity { get; set; }

        /// <summary>  
        /// 是否可空  
        /// </summary>  
        public bool Mandatory { get; set; }

        /// <summary>  
        /// 精度  
        /// </summary>  
        public string Precision { get; set; }
    }
}
