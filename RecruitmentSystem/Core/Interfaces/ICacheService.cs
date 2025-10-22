using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key);
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<bool> RemoveAsync(string key);
        Task<long> IncrementAsync(string key, long value = 1);
        Task<bool> ExistsAsync(string key);
    }
}
