using System.Collections.Generic;

namespace FastElasticsearch.Core.Model
{
    public class VectorQuery
    {
        public List<string> Fields { get; set; } = new List<string>();

        public Dictionary<string, object> Match { get; set; } = new Dictionary<string, object>();

        public float[] Data { get; set; }

        public int Top { get; set; } = 10;

        public int Total { get; set; } = 100;
    }
}