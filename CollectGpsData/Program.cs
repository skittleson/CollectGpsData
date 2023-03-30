using Microsoft.Data.Sqlite;
using NmeaParser.Messages;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

Console.WriteLine("GPS NMEA Data Parser and Storage");
var cts = new CancellationTokenSource();
using var connection = new SqliteConnection("Data Source=gps.db");
var device = new SerialPort(args[0]);
Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
{
    cts.Cancel();
    device?.Close();
    device?.Dispose();
    connection?.Dispose();
    Environment.Exit(0);
};
await connection.OpenAsync();
await CreateGpsTableAsync();
device.Open();
while (!cts.IsCancellationRequested)
{
    try
    {
        var raw = device.ReadLine();
        var msg = NmeaMessage.Parse(raw);
        if (msg is Rmc)
        {
            var rmc = (Rmc)msg;
            var now = rmc.FixTime.ToOffset(TimeSpan.FromHours(-8));
            Console.WriteLine($"{now}-{rmc.Latitude} {rmc.Longitude}");
            await SaveGpsRmcDataToDatabase(rmc);
        }
    }
    catch(Exception ex) 
    {
        if (!cts.IsCancellationRequested) {
            throw;
        }
       // nothing needs to be done here since we are closing the connections
    }
}

async Task CreateGpsTableAsync()
{
    var command = connection.CreateCommand();
    command.CommandText = @"
CREATE TABLE IF NOT EXISTS gps (
	timestamp INTEGER PRIMARY KEY,
	raw TEXT NOT NULL
);";
    await command.ExecuteNonQueryAsync();
}

async Task SaveGpsRmcDataToDatabase(Rmc rmc)
{
    var command = connection.CreateCommand();
    command.CommandText =
    @"
        INSERT INTO gps
        VALUES ($timestamp, $value)
    ";
    var timestampParameter = command.CreateParameter();
    timestampParameter.ParameterName = "$timestamp";
    timestampParameter.Value = rmc.FixTime.ToUnixTimeMilliseconds();
    command.Parameters.Add(timestampParameter);
    var valueParameter = command.CreateParameter();
    valueParameter.ParameterName = "value";
    valueParameter.Value = rmc.ToString();
    command.Parameters.Add(valueParameter);
    await command.ExecuteNonQueryAsync();
}