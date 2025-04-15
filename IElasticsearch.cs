using FastElasticsearch.Core.Model;
using System.Collections.Generic;

namespace FastElasticsearch.Core
{
    public interface IElasticsearch
    {
        EsResponse Add(string index, string _id, Dictionary<string, object> data);

        EsResponse AddList(string index, List<Dictionary<string, object>> data);

        /// <summary>
        ///  query: new { match_all = new { } }
        ///  query: new { wildcard = new { field= value* } }
        ///  query: new { match = new { field= value } }
        /// </summary>
        EsResponse Delete(string index, object query);

        EsResponse Delete(string index, QueryModel query);

        EsResponse Delete(string index);

        EsResponse Delete(string index, List<string> _id);

        /// <summary>
        ///  query: new { match_all = new { } }
        ///  query: new { wildcard = new { field= value* } }
        ///  query: new { match = new { field= value } }
        ///  sort: new []{ new { filed = new { order = desc}}}
        /// </summary>
        EsResponse Page(int pageSize, int pageId, string index, object query, object sort);

        EsResponse Page(int pageSize, int pageId, string index, QueryModel query);

        EsResponse Count(string index);

        /// <summary>
        ///  query: new { match_all = new { } }
        ///  query: new { wildcard = new { field= value* } }
        ///  query: new { match = new { field= value } }
        ///  sort: new []{ new { filed = new { order = desc}}}
        /// </summary>
        EsResponse GetList(string index, object query, object sort, int size = 10);

        EsResponse GetList(string index, QueryModel query, int size = 10);

        /// <summary>
        ///  query : new { term = new { field = value }
        ///  script : new { source = "ctx._source.field = value"}
        /// </summary>
        EsResponse Update(string index, object query, object script);

        /// <summary>
        /// script : new { source = "ctx._source.field = value"
        /// </summary>
        EsResponse Update(string index, UpdateModel query, object script);

        /// <summary>
        /// doc: new { field = value }
        /// </summary>
        EsResponse Update(string index, string _id, object doc);

        EsResponse Update(string index, string _id, UpdateModel query);
    }

    public interface IElasticsearchVector
    {
        EsResponse CreateVector(string vectorIndex, VectorModel model);

        EsResponse DeleteVector(string vectorIndex);

        EsResponse AddVectorData(string vectorIndex, VectorData model);

        EsResponse QueryVector(string vectorIndex, VectorQuery model);
    }
}