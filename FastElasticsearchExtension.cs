using Elasticsearch.Net;
using FastElasticsearch.Core;
using FastElasticsearch.Core.Aop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastElasticsearchExtension
    {
        private static string key = "Elasticsearch";
        public static IServiceCollection AddFastElasticsearch(this IServiceCollection serviceCollection, Action<ConfigData> action)
        {
            var config = new ConfigData();
            action(config);

            if (config == null || config.Host == null)
                throw new Exception(@"services.AddFastElasticsearch(a => {  })");

            var node = new List<Node>();
            config.Host.ForEach(a => { node.Add(new Node(new Uri(a))); });
            var pool = new StaticConnectionPool(node);
            var conn = new ConnectionConfiguration(pool).EnableHttpCompression().ServerCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true);
            conn.BasicAuthentication(config.UserName, config.PassWord);
            var client = new ElasticLowLevelClient(conn);
            serviceCollection.AddSingleton(client);

            serviceCollection.AddScoped<IElasticsearch, FastElasticsearch.Core.Elasticsearch>();
            serviceCollection.AddScoped<FastElasticsearch.Core.Elasticsearch>();
            serviceCollection.AddSingleton<IElasticsearchVector, FastElasticsearch.Core.ElasticsearchVector>();
            serviceCollection.AddSingleton<FastElasticsearch.Core.ElasticsearchVector>();

            if (config.Aop != null)
                serviceCollection.AddSingleton<IAop>(config.Aop);

            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }

        public static IServiceCollection AddFastElasticsearch(this IServiceCollection serviceCollection, string dbFile = "db.json", IAop aop = null)
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile(dbFile, optional: true, reloadOnChange: true);
            var config = new ServiceCollection().AddOptions().Configure<ConfigData>(build.Build().GetSection(key)).BuildServiceProvider().GetService<IOptions<ConfigData>>().Value;

            var node = new List<Node>();
            config.Host.ForEach(a => { node.Add(new Node(new Uri(a))); });
            var pool = new StaticConnectionPool(node);
            var conn = new ConnectionConfiguration(pool).EnableHttpCompression().ServerCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true);
            conn.BasicAuthentication(config.UserName, config.PassWord);
            var client = new ElasticLowLevelClient(conn);
            serviceCollection.AddSingleton(client);

            serviceCollection.AddScoped<IElasticsearch, FastElasticsearch.Core.Elasticsearch>();
            serviceCollection.AddScoped<FastElasticsearch.Core.Elasticsearch>();
            serviceCollection.AddSingleton<IElasticsearchVector, FastElasticsearch.Core.ElasticsearchVector>();
            serviceCollection.AddSingleton<FastElasticsearch.Core.ElasticsearchVector>();

            if (aop != null)
                serviceCollection.AddSingleton<IAop>(aop);

            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }
    }
}