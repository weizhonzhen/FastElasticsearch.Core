namespace FastElasticsearch.Core.Aop
{
    public interface IAop
    {
        void Before(BeforeContext context);

        void After(AfterContext context);
    }
}