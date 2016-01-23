using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.PlatformAbstractions;
using Stockpile.Sdk.Interfaces;

namespace Stockpile.Api.App
{
    public class StorageAdapterFactory
    {
        protected static ConcurrentDictionary<string, IStorageAdapter> AdapterCache = new ConcurrentDictionary<string, IStorageAdapter>();

        private static readonly object SyncLock = new object();

        public static IStorageAdapter GetAdapter(ILibraryManager libraryManager, string library, string connectionString)
        {
            lock (SyncLock)
                return AdapterCache.GetOrAdd(library, lib => LoadAdapter(libraryManager, lib, connectionString));
        }

        protected static IStorageAdapter LoadAdapter(ILibraryManager libraryManager, string library, string connectionString)
        {
            var lib = libraryManager.GetLibrary(library);

            var assemblyName = lib.Assemblies.FirstOrDefault();

            if (assemblyName == null)
                throw new ApplicationException("Could not find assembly.");

            var adapterAssembly = Assembly.Load(assemblyName);

            if (adapterAssembly == null)
                throw new ApplicationException("Failed to load assembly.");

            var interfaceType = typeof(IStorageAdapter);

            var types = adapterAssembly.GetTypes()
                .Where(p => interfaceType.IsAssignableFrom(p));

            var adapterType = types.FirstOrDefault();

            if (adapterType == null)
                throw new ApplicationException("Could not locate usable interface.");

            var adapterInterfaceInstance = Activator.CreateInstance(adapterType, connectionString) as IStorageAdapter;

            if (adapterInterfaceInstance == null)
                throw new Exception("Could not create instance of interface.");

            return adapterInterfaceInstance;
        }
    }
}