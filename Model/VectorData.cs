using System.Collections.Generic;

namespace FastElasticsearch.Core.Model
{
    public class VectorData
    {
        public float[] Data { get; set; }

        public Dictionary<string, object> Filed { get; set; } = new Dictionary<string, object>();
    }
}
