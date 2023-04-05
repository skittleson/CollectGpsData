using CollectGpsData;
using System;
using System.Threading;

Console.WriteLine("GPS NMEA Data Parser and Storage");
var cts = new CancellationTokenSource();
var provider = new DependencyProvider(args[0], cts.Token);
var service = new DataCollectionService(provider, cts.Token);
Console.CancelKeyPress += (sender, e) =>
{
    cts.Cancel();
    provider?.Dispose();
    Environment.Exit(0);
};
await service.RunAsync();