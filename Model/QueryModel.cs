using System.Collections.Generic;

namespace FastElasticsearch.Core.Model
{
    public class QueryModel
    {
        public Dictionary<string, object> Wildcard { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Match { get; set; } = new Dictionary<string, object>();
        public bool IsPhrase { get; set; }
        public Dictionary<string, object> Sort { get; set; } = new Dictionary<string, object>();
    }

    public class UpdateModel
    {
        public Dictionary<string, object> Term { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Doc { get; set; } = new Dictionary<string, object>();
    }
}
