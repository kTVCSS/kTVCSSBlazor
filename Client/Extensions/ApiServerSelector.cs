using System;

namespace kTVCSSBlazor.Client.Extensions
{
    public class ApiServerSelector
    {
        private readonly string[] _servers;
        private readonly object _lock = new();

        public ApiServerSelector(string[] servers)
        {
            _servers = servers;
        }

        public string GetNextServer()
        {
            lock (_lock)
            {
                var server = _servers[new Random().Next(_servers.Length)];
                Console.WriteLine(server);
                return server;
            }
        }
    }
}
