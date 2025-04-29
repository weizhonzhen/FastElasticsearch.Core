using System.Collections.Generic;
using System;

namespace FastElasticsearch.Core.Model
{
    public class PageResult
    {
        public PageModel Page = new PageModel();

        public string Id { get; set; }

        public string Index { get; set; }

        public List<Dictionary<string, object>> List { get; set; } = new List<Dictionary<string, object>>();
    }

    public class PageModel
    {
        public int TotalRecord { get; set; }

        public int TotalPage { get; set; }

        public int PageId { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }

    public class ListResult
    {
        public List<Dictionary<string, object>> List { get; set; } = new List<Dictionary<string, object>>();

        public string Id { get; set; }

        public string Index { get; set; }
    }

    public class EsResponse
    {
        public bool IsSuccess { get; set; }

        public Exception Exception { get; set; }

        public PageResult PageResult { get; set; } = new PageResult();

        public ListResult ListResult { get; set; } = new ListResult();

        public int Count {  get; set; }

        public int DeleteCount { get; set; }

        public int UpdateCount { get; set; }
    }
}
