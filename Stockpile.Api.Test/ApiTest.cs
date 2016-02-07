using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.TestHost;
using Moq;
using Stockpile.Api.App;
using Stockpile.Sdk;
using Xunit;

namespace Stockpile.Api.Test
{
    public class ApiTest
    {
        //[Fact] Pending resolution of issue https://github.com/aspnet/KestrelHttpServer/issues/520
        public void CRUD_LinuxServer()
        {
            var stockpileClient = new StockpileClient("http://stockpile.aluitink.io");

            Random r = new Random();
            byte[] expectedBytes = new byte[1024 + 1024];

            r.NextBytes(expectedBytes);

            var stockId = stockpileClient.CreateAsync(new MemoryStream(expectedBytes)).Result;

            Assert.NotEqual(Guid.Empty, stockId);

            using (var stream = stockpileClient.RetrieveAsync(stockId).Result)
            {
                Assert.NotNull(stream);

                var actualBytes = StreamToBytesAsync(stream).Result;

                Assert.NotNull(actualBytes);

                Assert.True(CheckData(expectedBytes, actualBytes));
            }

            r.NextBytes(expectedBytes);

            stockpileClient.UpdateAsync(stockId, new MemoryStream(expectedBytes)).Wait();

            using (var stream = stockpileClient.RetrieveAsync(stockId).Result)
            {
                Assert.NotNull(stream);

                var actualBytes = StreamToBytesAsync(stream).Result;

                Assert.NotNull(actualBytes);

                Assert.True(CheckData(expectedBytes, actualBytes));
            }

            stockpileClient.DeleteAsync(stockId).Wait();
            Assert.ThrowsAny<Exception>(() => stockpileClient.RetrieveAsync(stockId).Result);
        }

        [Fact]
        public void CRUD_Basic()
        {
            using (var testServer = CreateServer())
            {
                var httpClient = testServer.CreateClient();
                Mock<StockpileClient> clientMock = new Mock<StockpileClient>(testServer.BaseAddress.ToString(), httpClient);
                var stockpileClient = clientMock.Object;

                Random r = new Random();
                byte[] expectedBytes = new byte[1024 + 1024];

                r.NextBytes(expectedBytes);

                var stockId = stockpileClient.CreateAsync(new MemoryStream(expectedBytes)).Result;

                Assert.NotEqual(Guid.Empty, stockId);

                using (var stream = stockpileClient.RetrieveAsync(stockId).Result)
                {
                    Assert.NotNull(stream);

                    var actualBytes = StreamToBytesAsync(stream).Result;

                    Assert.NotNull(actualBytes);

                    Assert.True(CheckData(expectedBytes, actualBytes));
                }

                r.NextBytes(expectedBytes);

                stockpileClient.UpdateAsync(stockId, new MemoryStream(expectedBytes)).Wait();

                using (var stream = stockpileClient.RetrieveAsync(stockId).Result)
                {
                    Assert.NotNull(stream);

                    var actualBytes = StreamToBytesAsync(stream).Result;

                    Assert.NotNull(actualBytes);

                    Assert.True(CheckData(expectedBytes, actualBytes));
                }

                stockpileClient.DeleteAsync(stockId).Wait();
                Assert.ThrowsAny<Exception>(() => stockpileClient.RetrieveAsync(stockId).Result);
            }
        }

        [Fact]
        public void CRUD_WithAuth()
        {
            var stockKey = "LookAtThisKey";
            using (var testServer = CreateServer())
            {
                var httpClient = testServer.CreateClient();
                Mock<StockpileClient> clientMock = new Mock<StockpileClient>(testServer.BaseAddress.ToString(), httpClient);
                var stockpileClient = clientMock.Object;

                Random r = new Random();
                byte[] expectedBytes = new byte[1024 + 1024];

                r.NextBytes(expectedBytes);

                var stockId = stockpileClient.CreateAsync(new MemoryStream(expectedBytes), stockKey).Result;

                Assert.NotEqual(Guid.Empty, stockId);

                Assert.ThrowsAny<AggregateException>(() => stockpileClient.RetrieveAsync(stockId).Result);

                using (var stream = stockpileClient.RetrieveAsync(stockId, stockKey).Result)
                {
                    Assert.NotNull(stream);

                    var actualBytes = StreamToBytesAsync(stream).Result;

                    Assert.NotNull(actualBytes);

                    Assert.True(CheckData(expectedBytes, actualBytes));
                }

                r.NextBytes(expectedBytes);

                stockpileClient.UpdateAsync(stockId, new MemoryStream(expectedBytes), stockKey).Wait();

                Assert.ThrowsAny<AggregateException>(() => stockpileClient.RetrieveAsync(stockId).Result);

                using (var stream = stockpileClient.RetrieveAsync(stockId, stockKey).Result)
                {
                    Assert.NotNull(stream);

                    var actualBytes = StreamToBytesAsync(stream).Result;

                    Assert.NotNull(actualBytes);

                    Assert.True(CheckData(expectedBytes, actualBytes));
                }

                stockpileClient.DeleteAsync(stockId, stockKey).Wait();
                Assert.ThrowsAny<AggregateException>(() => stockpileClient.RetrieveAsync(stockId, stockKey).Result);
            }
        }

        protected TestServer CreateServer()
        {
            WebHostBuilder builder = TestServer.CreateBuilder();
            builder.UseEnvironment("Development");
            builder.UseStartup<Startup>();
            return new TestServer(builder);
        }
        
        private bool CheckData(byte[] expected, byte[] actual)
        {
            return expected.SequenceEqual(actual);
        }

        private async Task<byte[]> StreamToBytesAsync(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
        }
    }
}
