using System;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.TestHost;
using Stockpile.Api.App;
using Xunit;

namespace Stockpile.Api.Test
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class ApiTest
    {
        [Fact]
        public void Test1()
        {
            using (var testServer = CreateServer())
            {
                var client = testServer.CreateClient();

                Guid id = Guid.NewGuid();
                var result = client.GetAsync(string.Format("api/data/{0}", id)).Result;

            }
        }

        protected TestServer CreateServer()
        {
            WebHostBuilder builder = TestServer.CreateBuilder();
            builder.UseStartup<Startup>();
            return new TestServer(builder);
        }
    }
}
