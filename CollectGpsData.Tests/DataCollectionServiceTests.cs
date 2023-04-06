using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CollectGpsData.Tests
{
    public class DataCollectionServiceTests
    {
        private readonly Mock<IDependencyProvider> _mockProvider;
        private readonly CancellationTokenSource _tokenSource;
        private readonly DataCollectionService _service;

        public DataCollectionServiceTests() {
            _mockProvider = new Mock<IDependencyProvider>(MockBehavior.Strict);
            _tokenSource = new CancellationTokenSource();
            _tokenSource.Cancel();
            _service = new DataCollectionService(_mockProvider.Object, _tokenSource.Token);
        }

        [Fact]
        public async Task Can_process_message()
        {
            // Arrange
            var rma = "$GNRMC,060512.00,A,3150.788156,N,11711.922383,E,0.0,,311019,,,A,V*1B";
            _mockProvider.Setup(x => x.OpenAsync())
                .Returns(Task.CompletedTask);
            _mockProvider.Setup(x => x.ExecuteNonQueryAsync(
                It.IsAny<string>(),
                It.IsAny<IDictionary<string,object>>()))
                .Returns(Task.CompletedTask);
            _mockProvider.Setup(x => x.DeviceReadLine())
                .Returns(rma);
            
            // Act
            await _service.RunAsync();
        }
    }
}