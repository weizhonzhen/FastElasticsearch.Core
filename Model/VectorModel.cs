using System.Collections.Generic;

namespace FastElasticsearch.Core.Model
{
    public class VectorModel
    {
        public string Name { get; set; }

        public int Dims { get; set; } = 1024;

        public string Type { get; } = "dense_vector";

        public bool Index { get; set; } = true;

        public Similarity Similarity { get; set; } = Similarity.cosine;

        public IndexOption IndexOption { get; set; } = new IndexOption { M = 32, Ef_Construction = 400, Type = OptionType.hnsw };

        public Dictionary<string, object> Field { get; set; } = new Dictionary<string, object>();

        public string Id { get; set; }
    }

    public class IndexOption
    {
        public int M {  get; set; }

        public int Ef_Construction {  get; set; }

        public OptionType Type { get; set; }
    }

    public enum OptionType
    {
        hnsw = 0,
        int8_hnsw = 1,
        flat = 2,
        bbq = 3
    }

    public enum Similarity
    {
        /// <summary>
        /// 余弦相似度
        /// </summary>
        cosine = 1,

        /// <summary>
        /// L1距离
        /// </summary>
        l1_norm = 2,

        /// <summary>
        /// L2距离
        /// </summary>
        l2_norm = 3,

        /// <summary>
        /// 向量点积  高效余弦计算或字节向量相似性
        /// </summary>
        dot_product = 4,

        /// <summary>
        /// 最大内积  推荐系统
        /// </summary>
        //max_inner_product = 5 
    }
}