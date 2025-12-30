using Elasticsearch.Net;
using FastElasticsearch.Core.Aop;
using FastElasticsearch.Core.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FastElasticsearch.Core
{
    public class Elasticsearch : IElasticsearch
    {
        internal static List<char> filters = new List<char> { '\\', '/', '*', '?', '\"', '<', '>', '|', ' ', '#', '%', '{', '}', ':', '@', '&', '=' };
        internal ElasticLowLevelClient client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
        internal IAop aop = ServiceContext.Engine.Resolve<IAop>();

        public EsResponse Count(string index)
        {
            var data = new EsResponse();
            StringResponse page;
            if (!string.IsNullOrEmpty(index))
                page = client.Search<StringResponse>(GetIndex(index), PostData.Empty);
            else
                page = client.Search<StringResponse>(PostData.Empty);

            data.IsSuccess = page.Success;
            data.Exception = page.OriginalException;

            if (page.Success)
            {
                var list = JsonConvert.DeserializeObject<EsResult>(page.Body);

                data.Count = list.hits.total.value;
            }
            else
                data.Count = 0;

            return data;
        }

        /// <summary>
        ///  query: new { match_all = new { } }
        ///  query: new { wildcard = new { field= value* } }
        ///  query: new { match = new { field= value } }
        /// </summary>
        public EsResponse Delete(string index, object query)
        {
            var data = new EsResponse();
            var param = new DeleteByQueryRequestParameters { Conflicts = Conflicts.Proceed, Refresh = true };

            if (aop != null)
                aop.Before(new BeforeContext { Index = GetIndex(index), Dsl = JsonConvert.SerializeObject(new { query = query }) });

            var result = client.DeleteByQuery<StringResponse>(GetIndex(index), PostData.Serializable(new { query = query }), param);
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;

            if (data.IsSuccess)
            {
                var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.Body);
                data.DeleteCount = int.Parse(dic.GetValue("deleted").ToString());
            }

            if (aop != null)
                aop.After(new AfterContext
                {
                    Index = GetIndex(index),
                    Dsl = JsonConvert.SerializeObject(new { query = query }),
                    Data = result,
                    IsSuccess = data.IsSuccess,
                    Exception = data.Exception
                });

            return data;
        }

        public EsResponse Delete(string index)
        {
            var data = new EsResponse();
            var param = new DeleteByQueryRequestParameters { Conflicts = Conflicts.Proceed, Refresh = true };

            if (aop != null)
                aop.Before(new BeforeContext { Index = GetIndex(index), Dsl = JsonConvert.SerializeObject(new { query = new { match_all = new { } } }) });

            var result = client.DeleteByQuery<StringResponse>(GetIndex(index), PostData.Serializable(new { query = new { match_all = new { } } }), param);
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;

            if (data.IsSuccess)
            {
                var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.Body);
                data.DeleteCount = int.Parse(dic.GetValue("deleted").ToString());
            }

            if (aop != null)
                aop.After(new AfterContext
                {
                    Index = GetIndex(index),
                    Dsl = JsonConvert.SerializeObject(new { query = new { match_all = new { } } }),
                    Data = result,
                    IsSuccess = data.IsSuccess,
                    Exception = data.Exception
                });

            return data;
        }

        public EsResponse Delete(string index, List<string> _id)
        {
            var data = new EsResponse();
            var param = new DeleteByQueryRequestParameters { Conflicts = Conflicts.Proceed, Refresh = true };

            if (aop != null)
                aop.Before(new BeforeContext { Index = GetIndex(index), Dsl = JsonConvert.SerializeObject(new { query = new { terms = new { _id } } }) });

            var result = client.DeleteByQuery<StringResponse>(GetIndex(index), PostData.Serializable(new { query = new { terms = new { _id } } }), param);
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;

            if (data.IsSuccess)
            {
                var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.Body);
                data.DeleteCount = int.Parse(dic.GetValue("deleted").ToString());
            }

            if (aop != null)
                aop.After(new AfterContext
                {
                    Index = GetIndex(index),
                    Dsl = JsonConvert.SerializeObject(new { query = new { terms = new { _id } } }),
                    Data = result,
                    IsSuccess = data.IsSuccess,
                    Exception = data.Exception
                });

            return data;
        }

        internal bool IsChinese(string text)
        {
            return Regex.IsMatch(text ?? string.Empty, @"[\u4e00-\u9fff]");
        }

        public EsResponse Add(string index, string _id, Dictionary<string, object> model)
        {
            var data = new EsResponse();

            if (aop != null)
                aop.Before(new BeforeContext { Index = GetIndex(index) });

            var param = new IndexRequestParameters { Refresh = Refresh.True };
            var result = client.Index<StringResponse>(GetIndex(index), _id, PostData.Serializable(model), param);
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;

            if (aop != null)
                aop.After(new AfterContext
                {
                    Index = GetIndex(index),
                    Data = result,
                    IsSuccess = data.IsSuccess,
                    Exception = data.Exception
                });

            return data;
        }

        public EsResponse Add<T>(string index, T model)
        {
            var data = new EsResponse();
            index = GetIndex(index);
            if (aop != null)
                aop.Before(new BeforeContext { Index = index });

            var param = new IndexRequestParameters { Refresh = Refresh.True };
            var result = client.Index<StringResponse>(index, Guid.NewGuid().ToString(), PostData.Serializable(model), param);
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;

            if (aop != null)
                aop.After(new AfterContext
                {
                    Index = GetIndex(index),
                    Data = result,
                    IsSuccess = data.IsSuccess,
                    Exception = data.Exception
                });

            return data;
        }

        public EsResponse AddList(string index, List<Dictionary<string, object>> list)
        {
            var reponse = new EsResponse();
            var bulkParam = new BulkRequestParameters { Refresh = Refresh.True };
            var param = new List<dynamic>();
            var data = new List<Dictionary<string, object>>();

            if (list == null || list.Count == 0)
                return reponse;

            foreach (var item in list)
            {
                param.Add(new { index = new { _id = Guid.NewGuid().ToString() } });
                dynamic dyn = new ExpandoObject();
                var dic = (IDictionary<string, object>)dyn;
                foreach (var keyValue in item)
                {
                    dic[keyValue.Key] = keyValue.Value;
                }
                param.Add(dic);
            }

            if (aop != null)
                aop.Before(new BeforeContext { Index = GetIndex(index), Dsl = JsonConvert.SerializeObject(param) });

            var result = client.Bulk<StringResponse>(GetIndex(index), PostData.MultiJson(param), bulkParam);
            reponse.IsSuccess = result != null ? result.Success : false;
            reponse.Exception = result?.OriginalException;

            if (aop != null)
                aop.After(new AfterContext
                {
                    Index = GetIndex(index),
                    Dsl = JsonConvert.SerializeObject(param),
                    Data = result,
                    IsSuccess = reponse.IsSuccess,
                    Exception = reponse.Exception
                });

            return reponse;
        }

        public EsResponse AddList<T>(string index, List<T> list)
        {
            var reponse = new EsResponse();
            var bulkParam = new BulkRequestParameters { Refresh = Refresh.True };
            var param = new List<dynamic>();
            var data = new List<Dictionary<string, object>>();

            if (list == null || list.Count == 0)
                return reponse;

            if (aop != null)
                aop.Before(new BeforeContext { Index = GetIndex(index), Dsl = JsonConvert.SerializeObject(param) });

            var result = client.Bulk<StringResponse>(GetIndex(index), PostData.Serializable<List<T>>(list), bulkParam);
            reponse.IsSuccess = result != null ? result.Success : false;
            reponse.Exception = result?.OriginalException;

            if (aop != null)
                aop.After(new AfterContext
                {
                    Index = GetIndex(index),
                    Dsl = JsonConvert.SerializeObject(param),
                    Data = result,
                    IsSuccess = reponse.IsSuccess,
                    Exception = reponse.Exception
                });

            return reponse;
        }

        internal string GetIndex(string index)
        {
            if (string.IsNullOrEmpty(index))
                return string.Empty;
            else
                return new string(index.ToLower().Where(c => !filters.Contains(c)).ToArray());
        }

        internal string GetIndexs(List<string> index)
        {
            if (index == null || index.Count == 0)
                return string.Empty;
            else
            {
                var list = new List<string>();
                index.ForEach(a =>
                {
                    list.Add(new string(a.ToLower().Where(c => !filters.Contains(c)).ToArray()));
                });
                return string.Join(",", list);
            }
        }

        public EsResponse Delete(string index, QueryModel query)
        {
            var data = new EsResponse();
            var param = new Dictionary<string, object>();

            if (query == null)
                return data;

            var match = query.IsPhrase ? "match_phrase" : "match";

            if (query.Match != null && query.Match.Count > 0)
                param.Add(query.Match.Count == 1 ? match : "bool", Match(query.Match));

            if (query.Match != null && query.Match.Count > 0)
                param.Add(query.Match.Count == 1 ? match : "bool", Match(query.Match));

            if (query.Wildcard != null && query.Wildcard.Count > 0)
                param.Add("wildcard", Wildcard(query.Wildcard));

            return Delete(index, param);
        }

        /// <summary>
        ///  query: new { match_all = new { } }m
        ///  query: new { wildcard = new { field= value* } }
        ///  query: new { match = new { field= value } }
        ///  sort: new []{ new { filed = new { order = desc}}}
        /// </summary>
        public EsResponse Page(int pageSize, int pageId, string index, object query, object sort)
        {
            var result = new EsResponse();
            result.PageResult.Page.PageId = pageId;
            result.PageResult.Page.PageSize = pageSize;

            var data = new Dictionary<string, object>();
            StringResponse page;

            if (aop != null)
                aop.Before(new BeforeContext { Index = GetIndex(index), Dsl = JsonConvert.SerializeObject(new { size = pageSize, from = (pageId - 1) * pageSize, query = query, sort = sort }) });

            if (!string.IsNullOrEmpty(GetIndex(index)))
                page = client.Search<StringResponse>(GetIndex(index), PostData.Serializable(new { size = pageSize, from = (pageId - 1) * pageSize, query = query, sort = sort }));
            else
                page = client.Search<StringResponse>(PostData.Serializable(new { size = pageSize, from = (pageId - 1) * pageSize, query = query, sort = sort }));

            if (page?.Success == true)
            {
                result.IsSuccess = true;
                var body = page.Body;
                if (IsChinese(page.Body))
                    body = Uri.UnescapeDataString(page.Body);

                var list = JsonConvert.DeserializeObject<EsResult>(body);

                list.hits.hits.ForEach(a =>
                {
                    a._source.Add("_id", a._id);
                    a._source.Add("_index", a._index);
                    result.PageResult.List.Add(a._source);
                });

                result.PageResult.Page.TotalRecord = list.hits.total.value;
            }
            else
                result.Exception = page?.OriginalException;

            result.PageResult.Page.TotalPage = result.PageResult.Page.TotalRecord / pageSize + 1;

            if ((result.PageResult.Page.TotalRecord % result.PageResult.Page.PageSize) == 0)
                result.PageResult.Page.TotalPage = result.PageResult.Page.TotalRecord / result.PageResult.Page.PageSize;
            else
                result.PageResult.Page.TotalPage = (result.PageResult.Page.TotalRecord / result.PageResult.Page.PageSize) + 1;

            if (result.PageResult.Page.PageId > result.PageResult.Page.TotalPage)
                result.PageResult.Page.PageId = result.PageResult.Page.TotalPage;

            if (aop != null)
                aop.After(new AfterContext
                {
                    Index = GetIndex(index),
                    Dsl = JsonConvert.SerializeObject(new
                    {
                        size = pageSize,
                        from = (pageId - 1) * pageSize,
                        query = query,
                        sort = sort
                    }),
                    Data = result,
                    IsSuccess = result.IsSuccess,
                    Exception = result.Exception
                });

            return result;
        }

        public EsResponse Page(int pageSize, int pageId, List<string> index, object query, object sort)
        {
            return Page(pageSize, pageId, GetIndexs(index), query, sort);
        }

        public EsResponse Page(int pageSize, int pageId, string index, QueryModel query)
        {
            var param = new Dictionary<string, object>();
            var sort = new List<dynamic>();

            if (query == null)
                return new EsResponse();

            var match = query.IsPhrase ? "match_phrase" : "match";

            if (query.Match != null && query.Match.Count > 0)
                param.Add(query.Match.Count == 1 ? match : "bool", Match(query.Match));

            if (query.Wildcard != null && query.Wildcard.Count > 0)
                param.Add("wildcard", Wildcard(query.Wildcard));

            if (query.Sort != null && query.Sort.Count > 0)
                Sort(query.Sort, sort);

            if (param.Count == 0)
                param.Add("match_all", new { });

            return Page(pageSize, pageId, index, param, sort);
        }

        public EsResponse Page(int pageSize, int pageId, List<string> index, QueryModel query)
        {
            return Page(pageSize, pageId, GetIndexs(index), query);
        }

        /// <summary>
        ///  query: new { match_all = new { } }
        ///  query: new { wildcard = new { field= value* } }
        ///  query: new { match = new { field= value } }
        ///  sort: new []{ new { filed = new { order = desc}}}
        /// </summary>
        public EsResponse GetList(string index, object query, object sort, int size = 10)
        {
            var result = new EsResponse();
            StringResponse stringResponse;

            if (aop != null)
                aop.Before(new BeforeContext { Index = GetIndex(index), Dsl = JsonConvert.SerializeObject(new { query = query, size = size, sort = sort }) });

            if (!string.IsNullOrEmpty(index))
                stringResponse = client.Search<StringResponse>(GetIndex(index), PostData.Serializable(new { query = query, size = size, sort = sort }));
            else
                stringResponse = client.Search<StringResponse>(PostData.Serializable(new { query = query, size = size, sort = sort }));
            if (stringResponse.Success)
            {
                result.IsSuccess = true;
                var body = stringResponse.Body;
                if (IsChinese(stringResponse.Body))
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
                    Index = GetIndex(index),
                    Dsl = JsonConvert.SerializeObject(new { query = query, size = size, sort = sort }),
                    Data = result,
                    IsSuccess = result.IsSuccess,
                    Exception = result.Exception
                });

            return result;
        }

        public EsResponse GetList(List<string> index, object query, object sort, int size = 10)
        {
            return GetList(GetIndexs(index), query, sort, size);
        }

        public EsResponse GetList(string index, QueryModel query, int size = 10)
        {
            var param = new Dictionary<string, object>();
            var sort = new List<dynamic>();

            if (query == null)
                return new EsResponse();

            var match = query.IsPhrase ? "match_phrase" : "match";

            if (query.Match != null && query.Match.Count > 0)
                param.Add(query.Match.Count == 1 ? match : "bool", Match(query.Match));

            if (query.Wildcard != null && query.Wildcard.Count > 0)
                param.Add("wildcard", Wildcard(query.Wildcard));

            if (query.Sort != null && query.Sort.Count > 0)
                Sort(query.Sort, sort);

            if (param.Count == 0)
                param.Add("match_all", new { });

            return GetList(index, param, sort, size);
        }

        public EsResponse GetList(List<string> index, QueryModel query, int size = 10)
        {
            return GetList(GetIndexs(index), query, size);
        }

        /// <summary>
        ///  query : new { term = new { field = value }
        ///  script : new { source = "ctx._source.field = value"
        /// </summary>
        public EsResponse Update(string index, object query, object script)
        {
            var data = new EsResponse();
            var param = new UpdateByQueryRequestParameters { Conflicts = Conflicts.Proceed, Refresh = true };

            var result = client.UpdateByQuery<StringResponse>(GetIndex(index), PostData.Serializable(new { query = query, script = script }), param);
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;
            if (data.IsSuccess)
            {
                var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.Body);
                data.UpdateCount = int.Parse(dic.GetValue("updated").ToString());
            }
            return data;
        }

        public EsResponse Update(string index, UpdateModel query, object script)
        {
            var param = new Dictionary<string, object>();

            if (query == null)
                return new EsResponse();

            if (query.Term != null && query.Term.Count == 0)
                return new EsResponse();

            if (query.Term != null && query.Term.Count > 0)
                param.Add(query.Term.Count == 1 ? "term" : "bool", Term(query.Term));

            return Update(index, param, script);
        }

        /// <summary>
        /// doc: new { field = value }
        /// </summary>
        public EsResponse Update(string index, string _id, object doc)
        {
            var data = new EsResponse();
            var param = new UpdateRequestParameters { Refresh = Refresh.True };

            if (aop != null)
                aop.Before(new BeforeContext { Index = GetIndex(index), Dsl = JsonConvert.SerializeObject(new { doc = doc }) });

            var result = client.Update<StringResponse>(GetIndex(index), _id, PostData.Serializable(new { doc = doc }), param);
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;
            if (data.IsSuccess)
                data.UpdateCount = 1;

            if (aop != null)
                aop.After(new AfterContext
                {
                    Index = GetIndex(index),
                    Dsl = JsonConvert.SerializeObject(new { doc = doc }),
                    Data = result,
                    IsSuccess = data.IsSuccess,
                    Exception = data.Exception
                });

            return data;
        }

        public EsResponse Update(string index, string _id, UpdateModel query)
        {
            if (query.Doc.Count == 0)
                return new EsResponse();

            return Update(index, _id, query.Doc);
        }

        private object Wildcard(Dictionary<string, object> param)
        {
            dynamic dyn = new ExpandoObject();
            var dic = (IDictionary<string, object>)dyn;
            foreach (var keyValue in param)
            {
                dic[keyValue.Key] = $"{keyValue.Value}*";
            }

            return dic;
        }

        private void Sort(Dictionary<string, object> param, List<dynamic> sort)
        {
            foreach (var keyValue in param)
            {
                dynamic dyn = new ExpandoObject();
                var dic = (IDictionary<string, object>)dyn;
                dic[keyValue.Key] = new { order = keyValue.Value };
                sort.Add(dic);
            }
        }

        private object Match(Dictionary<string, object> param)
        {
            if (param.Count == 1)
                return param;
            else
            {
                var must = new List<dynamic>();
                foreach (var keyValue in param)
                {
                    dynamic dyn = new ExpandoObject();
                    var dic = (IDictionary<string, object>)dyn;
                    dic[keyValue.Key] = keyValue.Value;
                    must.Add(new { match = dic });
                }

                return new { must = must };
            }
        }

        private object Term(Dictionary<string, object> param)
        {
            if (param.Count == 1)
                return param;
            else
            {
                var term = new List<dynamic>();
                foreach (var keyValue in param)
                {
                    dynamic dyn = new ExpandoObject();
                    var dic = (IDictionary<string, object>)dyn;
                    dic[keyValue.Key] = keyValue.Value;
                    term.Add(new { match = dic });
                }

                return new { term = term };
            }
        }

        public EsResponse GetItem(string index, string _id)
        {
            var data = new EsResponse();
            StringResponse page;
            if (!string.IsNullOrEmpty(index))
                page = client.Get<StringResponse>(GetIndex(index), _id);
            else
                page = client.Search<StringResponse>(PostData.Empty);

            data.IsSuccess = page.Success;
            data.Exception = page.OriginalException;

            if (page.Success)
            {
                var list = JsonConvert.DeserializeObject<Hits>(page.Body);

                data.List.Add(list._source);
                data.Count = 1;
            }
            else
                data.Count = 0;

            return data;
        }

        public EsResponse Create<T>(string index, Dictionary<string, object> settings = null)
        {
            var data = new EsResponse();
            var properties = new Dictionary<string, object>();

            typeof(T).GetProperties().ToList().ForEach(a =>
            {
                var column = a.GetCustomAttribute<ColumnAttribute>();
                if (column != null)
                {
                    if (column.type == "date")
                        properties.Add(a.Name, new
                        {
                            type = column.type,
                            format = "yyyy-MM-dd HH:mm:ss || yyyy-MM-dd'T'HH:mm:ss'+08:00' || yyyy/MM/dd HH:mm:ss || yyyy/MM/dd'T'HH:mm:ss'+08:00'"
                        });
                    else
                        properties.Add(a.Name, new { type = column.type });
                }
            });

            if (aop != null)
                aop.Before(new BeforeContext { Index = GetIndex(index), Dsl = JsonConvert.SerializeObject(new { mappings = new { properties = properties } }) });

            var result = client.Indices.Exists<StringResponse>(GetIndex(index));

            if (result != null && !result.Success)
            {
                if (settings != null)
                    result = client.Indices.Create<StringResponse>(GetIndex(index), PostData.Serializable(new { mappings = new { properties = properties } }));
                else
                    result = client.Indices.Create<StringResponse>(GetIndex(index), PostData.Serializable(new { mappings = new { properties = properties }, settings = settings }));
            }

            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;

            if (aop != null)
                aop.After(new AfterContext
                {
                    Index = GetIndex(index),
                    Dsl = JsonConvert.SerializeObject(new { mappings = new { properties = properties } }),
                    Data = result,
                    IsSuccess = data.IsSuccess,
                    Exception = data.Exception
                });

            return data;
        }

        public bool Exists(string index)
        {
            var result = client.Indices.Exists<StringResponse>(GetIndex(index));
            if (result != null && !result.Success)
                return true;
            else
                return false;
        }

        public EsResponse AnalyzeText(string text)
        {
            var result = new EsResponse();
            var analyzeResponse = client.Indices.AnalyzeForAll<StringResponse>(
                            PostData.Serializable(new { text = new[] { text }, analyzer = Analyzer.ik_smart.ToString() }));
            if (analyzeResponse.Success)
            {
                var body = analyzeResponse.Body;
                if (IsChinese(analyzeResponse.Body))
                    body = Uri.UnescapeDataString(analyzeResponse.Body);

                result.KeyWord = Dic.JsonToDics(Dic.JsonToDic(body).GetValue("tokens").ToString()).Select(a => a.GetValue("token").ToString()).ToList();
            }
            return result;
        }
    }
}


namespace System.Collections.Generic
{
    internal static class Dic
    {
        public static Object GetValue(this Dictionary<string, object> item, string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            key = item.Keys.ToList().Find(a => string.Compare(a, key, true) == 0);

            if (string.IsNullOrEmpty(key))
                return string.Empty;
            else
                return item[key];
        }

        public static Dictionary<string, object> JsonToDic(string jsonValue)
        {
            try
            {
                var item = new Dictionary<string, object>();

                if (string.IsNullOrEmpty(jsonValue))
                    return item;

                var jo = JObject.Parse(jsonValue);

                foreach (var temp in jo)
                {
                    item.Add(temp.Key, temp.Value);
                }
                return item;
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        public static List<Dictionary<string, object>> JsonToDics(string jsonValue)
        {
            try
            {
                var item = new List<Dictionary<string, object>>();

                if (string.IsNullOrEmpty(jsonValue))
                    return item;

                var ja = JArray.Parse(jsonValue);

                foreach (var jo in ja)
                {
                    item.Add(JsonToDic(jo.ToString()));
                }

                return item;
            }
            catch
            {
                return new List<Dictionary<string, object>>();
            }
        }
    }
}