using Elasticsearch.Net;
using FastElasticsearch.Core.Aop;
using FastElasticsearch.Core.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace FastElasticsearch.Core
{
    public class ElasticsearchVector :IElasticsearchVector
    {
        private static string vectorKey = "vectorKey";
        private Elasticsearch elasticsearch = ServiceContext.Engine.Resolve<Elasticsearch>();
        internal ElasticLowLevelClient client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
        internal IAop aop = ServiceContext.Engine.Resolve<IAop>();

        private object VectorScript(VectorModel info, VectorQuery model)
        {
            var script = new object();
            dynamic dyn = new ExpandoObject();
            var dic = (IDictionary<string, object>)dyn;

            switch (info.Similarity)
            {
                case Similarity.cosine:
                    {
                        dic["source"] = $"cosineSimilarity(params.query_vector, '{info.Name}') + 1.0";
                        dic["params"] = new { query_vector = model.Data };
                        break;
                    }
                case Similarity.dot_product:
                    {
                        //dic["source"] = $@"""double value = dotProduct(params.queryVector, '{info.Name}');
                        //return sigmoid(1, Math.E, -value);""";

                        dic["source"] = $"dotProduct(params.query_vector, '{info.Name}')";
                        break;
                    }
                case Similarity.l2_norm:
                    {
                        dic["source"] = $"1/(1 + l2norm(params.query_vector, '{info.Name}'))";
                        dic["params"] = new { query_vector = model.Data };
                        break;
                    }
                case Similarity.l1_norm:
                    {
                        dic["source"] = $"1/(1 + l1norm(params.query_vector, '{info.Name}'))";
                        dic["params"] = new { query_vector = model.Data };
                        break;
                    }
                default:
                    {
                        dic["source"] = $"cosineSimilarity(params.query_vector, '{info.Name}') + 1.0";
                        dic["params"] = new { query_vector = model.Data };
                        break;
                    }
            }

            return dyn;
        }

        private VectorModel GetVectorInfo(string vectorIndex)
        {
            var vectorList = elasticsearch.GetList(vectorKey, new { match_all = new { } }, null, 100).List ?? new List<Dictionary<string, object>>();
            var vectorInfo = vectorList.Find(a => a.ContainsKey(vectorIndex.ToString())) ?? new Dictionary<string, object>();
            return JsonConvert.DeserializeObject<VectorModel>(vectorInfo.GetValue(vectorIndex).ToString()) ?? new VectorModel();
        }

        private object VectorParam(string vectorIndex, VectorQuery model)
        {
            var result = new object();
            var info = GetVectorInfo(vectorIndex);

            dynamic queryDyn = new ExpandoObject();
            var dic = (IDictionary<string, object>)queryDyn;

            if (model.Match.Count > 0)
            {
                dynamic termDyn = new ExpandoObject();
                var termDic = (IDictionary<string, object>)termDyn;
                foreach (var item in model.Match)
                {
                    termDic[item.Key] = new { query = item.Value ,boost = model.MachBoost};
                }

                dic["match"] = termDyn;
            }

            dynamic knnDyn = new ExpandoObject();
            var knnDic = (IDictionary<string, object>)knnDyn;
            knnDic["field"] = info.Name;
            knnDic["query_vector"] = model.Data;
            knnDic["k"] = model.K;
            knnDic["num_candidates"] = model.NumCandidates;
            knnDic["boost"] = model.KnnBoost;

            result = new { knn = knnDyn, query = queryDyn };

            return result;
        }

        public EsResponse AddVectorData(string vectorIndex, VectorData model)
        {
            var data = new EsResponse();
            var info = GetVectorInfo(vectorIndex);

            dynamic dyn = new ExpandoObject();
            var dic = (IDictionary<string, object>)dyn;

            dic[info.Name] = model.Data;
            foreach (var keyValue in model.Field)
            {
                dic[keyValue.Key] = keyValue.Value;
            }

            if (aop != null)
                aop.Before(new BeforeContext { IsVector = true, Index = elasticsearch.GetIndex(vectorIndex), Dsl = JsonConvert.SerializeObject(dic) });

            var result = client.Index<StringResponse>(elasticsearch.GetIndex(vectorIndex), Guid.NewGuid().ToString(), PostData.Serializable(dic));
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;

            if (aop != null)
                aop.After(new AfterContext
                {
                    IsVector = true,
                    Index = elasticsearch.GetIndex(vectorIndex),
                    Dsl = JsonConvert.SerializeObject(dic),
                    Data = result,
                    IsSuccess = data.IsSuccess,
                    Exception = data.Exception
                });

            return data;
        }

        public EsResponse CreateVector(string vectorIndex, VectorModel model)
        {
            var info = GetVectorInfo(vectorIndex);

            if (info.Name == model.Name && info.Name != null)
                return new EsResponse();

            var data = new EsResponse();
            var param = new object();

            dynamic dyn = new ExpandoObject();
            var dic = (IDictionary<string, object>)dyn;
            dic[model.Name] = new
            {
                type = model.Type,
                dims = model.Dims,
                index = model.Index,
                similarity = model.Similarity.ToString(),
                index_options = new
                {
                    type = model.IndexOption.Type.ToString(),
                    ef_construction = model.IndexOption.Ef_Construction,
                    m=model.IndexOption.M
                }
            };

            foreach (var keyValue in model.Field)
            {
                dic[keyValue.Key] = new { type = keyValue.Value };
            }

            param = new { mappings = new { properties = dyn } };

            if (aop != null)
                aop.Before(new BeforeContext { IsVector = true, Index = elasticsearch.GetIndex(vectorIndex), Dsl = JsonConvert.SerializeObject(param) });

            var result = client.Indices.Create<StringResponse>(elasticsearch.GetIndex(vectorIndex), PostData.Serializable(param));
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;

            if (data.IsSuccess)
            {
                model.Id = Guid.NewGuid().ToString();
                elasticsearch.Add(vectorKey, model.Id, new Dictionary<string, object> { { vectorIndex, model } });
            }

            if (aop != null)
                aop.After(new AfterContext
                {
                    IsVector = true,
                    Index = elasticsearch.GetIndex(vectorIndex),
                    Dsl = JsonConvert.SerializeObject(param),
                    Data = result,
                    IsSuccess = data.IsSuccess,
                    Exception = data.Exception
                });

            return data;
        }

        public EsResponse QueryVector(string vectorIndex, VectorQuery model)
        {
            var result = new EsResponse();
            var param = VectorParam(vectorIndex, model);

            if (aop != null)
                aop.Before(new BeforeContext { IsVector = true, Index = elasticsearch.GetIndex(vectorIndex), Dsl = JsonConvert.SerializeObject(param) });

            var stringResponse = client.Search<StringResponse>(elasticsearch.GetIndex(vectorIndex), PostData.Serializable(param));
            if (stringResponse.Success)
            {
                result.IsSuccess = true;
                var body = stringResponse.Body;
                if (elasticsearch.IsChinese(stringResponse.Body))
                    body = Uri.UnescapeDataString(stringResponse.Body);

                var list = JsonConvert.DeserializeObject<EsResult>(body);
                list.hits.hits.ForEach(a =>
                {
                    a._source.Add("_id", a._id);
                    a._source.Add("_index", a._index);
                    result.List.Add(a._source);
                });
            }
            else
                result.Exception = stringResponse.OriginalException;

            if (aop != null)
                aop.After(new AfterContext
                {
                    IsVector = true,
                    Index = elasticsearch.GetIndex(vectorIndex),
                    Dsl = JsonConvert.SerializeObject(param),
                    Data = stringResponse,
                    IsSuccess = result.IsSuccess,
                    Exception = result.Exception
                });

            return result;
        }

        public EsResponse DeleteVector(string vectorIndex)
        {
            var data = new EsResponse();
            if (aop != null)
                aop.Before(new BeforeContext { IsVector = true, Index = elasticsearch.GetIndex(vectorIndex) });

            var result = client.Indices.Delete<StringResponse>(elasticsearch.GetIndex(vectorIndex));
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;

            if (data.IsSuccess)
            {
                var info = GetVectorInfo(vectorIndex);
                elasticsearch.Delete(vectorKey, new List<string> { info.Id });
            }

            if (aop != null)
                aop.After(new AfterContext
                {
                    IsVector = true,
                    Index = elasticsearch.GetIndex(vectorIndex),
                    Data = result,
                    IsSuccess = data.IsSuccess,
                    Exception = data.Exception
                });
            return data;
        }

        public EsResponse QueryVector(List<string> vectorIndex, VectorQuery model)
        {
            var result = new EsResponse();

            vectorIndex.ForEach(a =>
            {
                var info = QueryVector(elasticsearch.GetIndex(a), model);
                if (info.IsSuccess && info.List.Count > 0)
                {
                    result.IsSuccess = true;
                    result.List.AddRange(info.List);
                }
            });

            return result;
        }
    }
}