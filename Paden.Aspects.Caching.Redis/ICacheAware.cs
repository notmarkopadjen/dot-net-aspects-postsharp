namespace Paden.Aspects.Caching.Redis
{
    public interface ICacheAware
    {
        bool CacheEnabled { get; set; }
    }
}
