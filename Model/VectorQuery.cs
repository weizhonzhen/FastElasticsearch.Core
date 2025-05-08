using System.Collections.Generic;

namespace FastElasticsearch.Core.Model
{
    public class VectorQuery
    {
        public List<string> Fields { get; set; } = new List<string>();

        public Dictionary<string, object> Match { get; set; } = new Dictionary<string, object>();

        public double MachBoost { get; set; } = 0.6;

        public float[] Data { get; set; }

        public int K { get; set; } = 10;

        public int NumCandidates { get; set; } = 100;

        public double KnnBoost { get; set; } = 0.4;
    }
}