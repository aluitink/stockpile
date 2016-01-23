using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Stockpile.Sdk;

namespace Stockpile.Benchmark
{
    public class Program
    {
        private StockpileClient _client;
        public void Main(string[] args)
        {
            if (args.Length == 0)
                throw new ApplicationException("Need Base Address");

            Thread.Sleep(5000);

            _client = new StockpileClient(string.Format("{0}", args[0]));
            
            var t1 = Task.Factory.StartNew(() =>
            {
                CrudItAsync(10000).Wait();
            });


            var t2 = Task.Factory.StartNew(() =>
            {
                CrudItAsync(10000).Wait();
            });

            var t3 = Task.Factory.StartNew(() =>
            {
                CrudItAsync(10000).Wait();
            });

            var t4 = Task.Factory.StartNew(() =>
            {
                CrudItAsync(10000).Wait();
            });

            var t5 = Task.Factory.StartNew(() =>
            {
                CrudItAsync(10000).Wait();
            });

            var t6 = Task.Factory.StartNew(() =>
            {
                CrudItAsync(10000).Wait();
            });

            var t7 = Task.Factory.StartNew(() =>
            {
                CrudItAsync(10000).Wait();
            });

            var t8 = Task.Factory.StartNew(() =>
            {
                CrudItAsync(10000).Wait();
            });

            Task.WaitAll(t1, t2, t3, t4, t5, t6, t7, t8);
        }

        public async Task CrudItAsync(int count)
        {
            Random r = new Random();
            byte[] data = new byte[1024 + 1024];

            try
            {
                for (int i = 0; i < count; i++)
                {
                    
                    try
                    {
                        r.NextBytes(data);
                        var guid = await CreateAsync(data);

                        Console.WriteLine("Created: {0}", guid);

                        var rData = await RetrieveAsync(guid);

                        Console.WriteLine("Retreived: {0}, Length: {1}", guid, rData.Length);

                        if (!CheckData(data, rData))
                            Console.WriteLine("Data Does Not Match");

                        r.NextBytes(data);

                        await UpdateAsync(guid, data);

                        Console.WriteLine("Updated: {0}", guid);

                        rData = await RetrieveAsync(guid);

                        Console.WriteLine("Retreived: {0}, Length: {1}", guid, rData.Length);

                        if (!CheckData(data, rData))
                            Console.WriteLine("Data Does Not Match");

                        //await DeleteAsync(guid);

                        //Console.WriteLine("Deleted: {0}", guid);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public bool CheckData(byte[] expected, byte[] actual)
        {
            return expected.SequenceEqual(actual);
        }

        public async Task<Guid> CreateAsync(byte[] data)
        {
            return await _client.CreateAsync(new MemoryStream(data));
        }

        public async Task UpdateAsync(Guid id, byte[] data)
        {
            await _client.UpdateAsync(id, new MemoryStream(data));
        }

        public async Task<byte[]> RetrieveAsync(Guid id)
        {
            var stream = await _client.RetrieveAsync(id);
            return await StreamToBytesAsync(stream);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _client.DeleteAsync(id);
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
