using Microsoft.Extensions.DependencyInjection;
using System;

namespace FastElasticsearch.Core
{
    internal class ServiceEngine : IServiceEngine
    {
        private IServiceProvider serviceProvider;
        public ServiceEngine(IServiceProvider _serviceProvider)
        {
            this.serviceProvider = _serviceProvider;
        }

        public T Resolve<T>()
        {
            return serviceProvider.GetService<T>();
        }
    }

    internal interface IServiceEngine
    {
        T Resolve<T>();
    }

    internal class ServiceContext
    {
        private static IServiceEngine engine;
        public static IServiceEngine Init(IServiceEngine _engine)
        {
            engine = _engine;
            return engine;
        }

        public static IServiceEngine Engine
        {
            get
            {
                return engine;
            }
        }
    }
}
