using System;
using System.Collections.Generic;
using DataDictionaryGenerator.Model;

namespace DataDictionaryGenerator.Core
{
    public interface IReader
    {
        IList<TableInfo> Read(string input);
    }
}
