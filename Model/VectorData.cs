using System.Collections.Generic;

namespace FastElasticsearch.Core.Model
{
    public class VectorData
    {
        public float[] Data { get; set; }

        public Dictionary<string, object> Field { get; set; } = new Dictionary<string, object>();
    }
}
