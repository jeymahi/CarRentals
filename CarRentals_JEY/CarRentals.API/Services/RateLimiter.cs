using System.Collections.Concurrent;

namespace CarRentals.API.Services
{
    public class RateLimiter
    {
        private class Hit
        {
            public DateTime WindowStart { get; set; }
            public int Count { get; set; }
        }

        private readonly int _max;
        private readonly TimeSpan _window;
        private readonly ConcurrentDictionary<string, Hit> _hits = new();

        public RateLimiter(int max, TimeSpan window)
        {
            _max = max;
            _window = window;
        }

        public bool Allow(string key)
        {
            var now = DateTime.UtcNow;
            var hit = _hits.GetOrAdd(key, _ => new Hit
            {
                WindowStart = now,
                Count = 0
            });

            lock (hit)
            {
                if (now - hit.WindowStart > _window)
                {
                    hit.WindowStart = now;
                    hit.Count = 1;
                    return true;
                }

                if (hit.Count < _max)
                {
                    hit.Count++;
                    return true;
                }

                return false;
            }
        }

    }
}