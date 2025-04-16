using FastElasticsearch.Core.Aop;
using System.Collections.Generic;

namespace FastElasticsearch.Core
{
    public class ConfigData
    {
        public List<string> Host { get; set; } = new List<string>();

        public string UserName { get; set; }

        public string PassWord { get; set; }

        public IAop Aop { get; set; }
    }
}
