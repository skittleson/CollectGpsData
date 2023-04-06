using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace CollectGpsData
{
    public interface IDependencyProvider : IDisposable
    {
        string DeviceReadLine();
        Task OpenAsync();
        Task ExecuteNonQueryAsync(string commandText);
        Task ExecuteNonQueryAsync(string commandText, IDictionary<string, object> sqliteParameters);
    }
    internal class DependencyProvider : IDependencyProvider
    {
        private readonly SqliteConnection _connection;
        private readonly SerialPort _device;
        private readonly CancellationToken _cancellationToken;

        public DependencyProvider(string port, CancellationToken cancellationToken)
        {
            _connection = new SqliteConnection("Data Source=gps.db");
            _device = new SerialPort(port);
            _cancellationToken = cancellationToken;
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _device?.Dispose();
        }

        public async Task OpenAsync()
        {
            await _connection.OpenAsync(_cancellationToken);
            await ExecuteNonQueryAsync(@"CREATE TABLE IF NOT EXISTS gps (
	            timestamp INTEGER PRIMARY KEY,
	            raw TEXT NOT NULL);");
            _device?.Open();
        }

        public string DeviceReadLine() => _device.ReadLine();

        public async Task ExecuteNonQueryAsync(string commandText)
        {
            var command = _connection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync(_cancellationToken);
        }
        public async Task ExecuteNonQueryAsync(string commandText, IDictionary<string, object> sqliteParameters)
        {
            var command = _connection.CreateCommand();
            command.CommandText = commandText;
            foreach (var kv in sqliteParameters)
            {
                var dbParam = command.CreateParameter();
                dbParam.ParameterName = kv.Key;
                dbParam.Value = kv.Value;
                command.Parameters.Add(dbParam);
            }
            await command.ExecuteNonQueryAsync(_cancellationToken);
        }
    }
}
