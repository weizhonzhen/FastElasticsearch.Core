```
services.AddFastElasticsearch(aop => new EsAop());
or
services.AddFastElasticsearch(a =>
{
    a.Host = new List<string> { "http://127.0.0.1:9200" };
    a.Aop = new EsAop();
    a.PassWord = "123456";
    a.UserName = "elastic";
});

```

```
//json 
 "Elasticsearch": {
   "Host": [ "http://127.0.0.1:9200" ],
   "UserName": "elastic",
   "PassWord": "123456"
 }
```
