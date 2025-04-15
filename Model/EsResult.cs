using System.Collections.Generic;

namespace FastElasticsearch.Core.Model
{
    internal class EsResult
    {
        public int took { get; set; }

        public bool timed_out { get; set; }

        public Shards _shards { get; set; }

        public Hit hits { get; set; }
    }

    internal class Shards
    {
        public int total { get; set; }

        public int successful { get; set; }

        public int skipped { get; set; }

        public int failed { get; set; }
    }

    internal class Hit
    {
        public Total total { get; set; }

        public double? max_score { get; set; }

        public List<Hits> hits { get; set; }= new List<Hits>();
    }

    internal class Total
    {
        public int value { get; set; }

        public string relation { get; set; }
    }

    internal class Hits
    {
        public string _index { get; set; }

        public string _id { get; set; }

        public double? _score { get; set; }

        public Dictionary<string, object> _source { get; set; }
    }
}


