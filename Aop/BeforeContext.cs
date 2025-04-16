using FastElasticsearch.Core.Model;

namespace FastElasticsearch.Core.Aop
{
    public class BeforeContext
    {
        public string Dsl {  get; set; }

        public string Index { get; set; }

        public bool IsVector { get; set; }
    }
}