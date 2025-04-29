using System;

namespace FastElasticsearch.Core.Aop
{
    public class AfterContext
    {
        public string Dsl { get; set; }

        public string Index { get; set; }

        public bool IsVector { get; set; }

        public object Data { get; set; }

        public bool IsSuccess { get; set; }

        public Exception Exception { get; set; }
    }
}
