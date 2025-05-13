using System;

namespace FastElasticsearch.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string type { get; set; }

        public int ignore_above { get; set; }
    }
}
