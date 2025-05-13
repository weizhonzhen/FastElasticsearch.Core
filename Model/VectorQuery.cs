using System.Collections.Generic;

namespace FastElasticsearch.Core.Model
{
    public class VectorQuery
    {
        public List<string> Fields { get; set; } = new List<string>();

        public Dictionary<string, object> Match { get; set; } = new Dictionary<string, object>();

        public double MachBoost { get; set; } = 0.4;

        public float[] Data { get; set; }

        public int K { get; set; } = 10;

        public int NumCandidates { get; set; } = 100;

        public double KnnBoost { get; set; } = 0.6;

        public Analyzer Analyzer { get; set; } = Analyzer.ik_smart;

        //public Rank Rank { get; set; } = new Rank();
    }

    public enum Analyzer
    {
        standard = 0,
        ik_smart = 1,
        ik_max_word = 2
    }

    //public class Rank
    //{
    //    public int WindowSize { get; set; } = 10;
    //    public int RankConstant { get; set; } = 50;
    //}
}