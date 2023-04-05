using NmeaParser.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CollectGpsData
{
    public class DataCollectionService
    {
        private readonly IDependencyProvider _provider;
        private readonly CancellationToken _cancellationToken;

        public DataCollectionService(IDependencyProvider provider, CancellationToken cancellationToken = default)
        {
            _provider = provider;
            _cancellationToken = cancellationToken;
        }

        public async Task RunAsync()
        {
            await _provider.OpenAsync();
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var msg = NmeaMessage.Parse(_provider.DeviceReadLine());
                    if (msg is Rmc rmc)
                    {
                        await _provider.ExecuteNonQueryAsync(
                            "INSERT INTO gps VALUES($timestamp, $value)",
                            new Dictionary<string, object>() {
                                { "$timestamp", rmc.FixTime.ToUnixTimeMilliseconds()},
                                { "$value", rmc.ToString() }
                            }
                        );
                    }
                }
                catch
                {
                    if (!_cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    // nothing needs to be done here since we are closing the connections
                }
            }
        }

    }
}
