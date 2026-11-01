using DeferredPartial.CrossCutting.Contracts;
using DeferredPartial.CrossCutting.Models;
using System;
using System.Runtime.Caching;

namespace DeferredPartial.Services.Stores
{
    /// <summary>
    /// Responsável por armazenar temporariamente os dados da Partial View.
    /// </summary>
    public class DeferredPartialStore : IDeferredPartialStore
    {
        private readonly ObjectCache _cache;
        private readonly object _sync = new object();

        private readonly TimeSpan _expiration =
            TimeSpan.FromMinutes(5);

        public DeferredPartialStore()
        {
            _cache = MemoryCache.Default;
        }

        public string Store(DeferredPartialEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            var id = Guid.NewGuid().ToString();

            var key = "deferred:" + id;

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration =
                    DateTimeOffset.UtcNow.Add(_expiration)
            };

            _cache.Set(key, entry, policy);

            return id;
        }

        public DeferredPartialEntry Take(
            string id,
            string owner)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            lock (_sync)
            {
                var key = "deferred:" + id;

                var entry = _cache.Get(key)
                    as DeferredPartialEntry;

                if (entry == null)
                    return null;

                if (!string.Equals(
                    entry.Owner,
                    owner,
                    StringComparison.Ordinal))
                {
                    return null;
                }

                _cache.Remove(key);

                return entry;
            }
        }
    }
}
