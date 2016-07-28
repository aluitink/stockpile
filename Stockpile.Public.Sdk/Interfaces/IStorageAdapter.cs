using System.IO;

namespace Stockpile.Public.Sdk.Interfaces
{
    public interface IStorageAdapter
    {
        string Create(Stream data);
        Stream Read(string key);
        bool Update(string key, Stream data);
        bool Delete(string key);
        bool Exists(string key);
    }
}