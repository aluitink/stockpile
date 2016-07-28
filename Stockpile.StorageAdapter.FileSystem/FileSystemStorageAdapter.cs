using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Stockpile.Sdk.Interfaces;

namespace Stockpile.StorageAdapter.FileSystem
{
    public class FileSystemAdapter : IStorageAdapter, IDisposable
    {
        public string RootPath { get; protected set; }

        private readonly string _connectionString;
        private const string IndexFile = "_index";
        private static long _indexCurrentValue;

        private Stream _indexStream;

        //private static EventWaitHandle _crossProcessWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, "4D1EDE25-7550-4CDA-AC2E-7C85B110B7FE");
        
        private static object _sync = new object();

        public FileSystemAdapter(string connectionString)
        {
            _connectionString = connectionString;
            Initialize();
        }

        public string Create(Stream data)
        {
            if (data == null)
                throw new ArgumentNullException("data", "Stream cannot be null");

            string nextFilePath = null;
            string key = null;
            
            lock (_sync)
            {
                //_crossProcessWaitHandle.WaitOne();
                long previousIndex = _indexCurrentValue;
                try
                {
                    do
                    {
                        key = GetNextKey();
                        var nextFile = KeyToPath(key);
                        nextFilePath = Path.Combine(RootPath, nextFile);
                        if (File.Exists(nextFilePath))
                            WriteValue(_indexCurrentValue);
                    } while (File.Exists(nextFilePath));
                    //Update Index
                    WriteValue(_indexCurrentValue);

                }
                catch (IOException ex)
                {
                    _indexCurrentValue = previousIndex;
                    throw;
                }
                finally
                {
                    //_crossProcessWaitHandle.Set();
                }
            }

            if (!WriteFile(nextFilePath, data).Result)
                throw new Exception("Could not write file.");
            return key;
        }


        public Stream Read(string key)
        {
            var path = KeyToPath(key);
            if(!File.Exists(path))
                throw new FileNotFoundException("Could not find file.", path);
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public bool Update(string key, Stream data)
        {
            if (Delete(key))
            {
                var path = KeyToPath(key);
                return WriteFile(path, data).Result;
            }
            return false;
        }

        public bool Delete(string key)
        {
            var path = KeyToPath(key);
            File.Delete(path);
            return true;
        }

        public bool Exists(string key)
        {
            var path = KeyToPath(key);
            return File.Exists(path);
        }

        public void Dispose()
        {
            if (_indexStream != null)
                _indexStream.Dispose();
        }

        protected void Initialize()
        {
            ParseConnectionString(_connectionString);
            InitializeIndex();
        }

        private void ParseConnectionString(string connectionString)
        {
            string[] parts = connectionString.Split(';');

            foreach (string part in parts)
            {
                string[] pair = part.Split(new[] { '=' }, 2);

                if (pair.Length < 2)
                    continue;

                var key = pair[0];
                var value = pair[1];

                switch (key.ToLowerInvariant())
                {
                    case "data":
                        RootPath = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void InitializeIndex()
        {
            if (!Directory.Exists(RootPath))
                Directory.CreateDirectory(RootPath);

            var indexPath = Path.Combine(RootPath, IndexFile);

            _indexStream = new FileStream(indexPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            try
            {
                ReadValue();
            }
            catch (Exception)
            {
                WriteValue(0);
                ReadValue();
            }
        }

        private void WriteValue(long value)
        {
            lock (_sync)
            {
                byte[] currentValue = BitConverter.GetBytes(value);

                _indexStream.Position = 0;
                _indexStream.Write(currentValue, 0, 8);
            }
        }

        private void ReadValue()
        {
            lock (_sync)
            {
                byte[] currentValue = new byte[8];

                _indexStream.Position = 0;
                var i = _indexStream.Read(currentValue, 0, currentValue.Length);
                if (i <= 0)
                    throw new Exception("could not read from stream.");
                _indexCurrentValue = BitConverter.ToInt32(currentValue, 0);
            }
        }


        private string GetNextKey()
        {
            ReadValue();
            var nextKey = _indexCurrentValue + 1;
            return nextKey.ToString();
        }

        private string KeyToPath(string key)
        {
            long seq;
            if (!Int64.TryParse(key, out seq) || seq < 0 || seq > 999999999999)
            {
                throw (new ArgumentException(String.Format(
                    "File System Store requires key to be an integer between 0 and 999999999999")));
            }

            string path = Path.Combine(RootPath, String.Format("{1:D3}{0}{2:D3}{0}{3:D3}{0}{4:D3}{0}{5}",
                                                              Path.DirectorySeparatorChar,
                                                              seq / 1000000000000,
                                                              (seq / 1000000000) % 1000,
                                                              (seq / 1000000) % 1000,
                                                              (seq / 1000) % 1000,
                                                              key));
            _indexCurrentValue = seq;
            return path;
        }

        private async Task<bool> WriteFile(string filePath, Stream data)
        {
            try
            {
                FileInfo file = new FileInfo(filePath);
                var path = file.Directory.FullName;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                using (FileStream fileStream = new FileStream(file.FullName, FileMode.CreateNew, FileAccess.Write))
                {
                    data.Position = 0;
                    await data.CopyToAsync(fileStream);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
