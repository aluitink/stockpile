using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Internal;
using Stockpile.Public.Sdk.Interfaces;

namespace Stockpile.Public.Api.App
{
    public class StorageAdapterFactory
    {
        protected static ConcurrentDictionary<string, IStorageAdapter> AdapterCache = new ConcurrentDictionary<string, IStorageAdapter>();

        private static readonly object SyncLock = new object();

        public static IStorageAdapter GetAdapter(string library, string connectionString)
        {
            lock (SyncLock)
                return AdapterCache.GetOrAdd(library, lib => LoadAdapter(lib, connectionString));
        }

        protected static IStorageAdapter LoadAdapter(string library, string connectionString)
        {
            var libs = DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(library);
            
            var lib = libs.FirstOrDefault();
            var assemblyName = lib.Name;

            if (assemblyName == null)
                throw new Exception("Could not find assembly.");

            var adapterAssembly = Assembly.Load(new AssemblyName(assemblyName));

            if (adapterAssembly == null)
                throw new Exception("Failed to load assembly.");

            var interfaceType = typeof(IStorageAdapter);

            var types = adapterAssembly.GetTypes()
                .Where(p => interfaceType.IsAssignableFrom(p));

            var adapterType = types.FirstOrDefault();

            if (adapterType == null)
                throw new Exception("Could not locate usable interface.");

            var adapterInterfaceInstance = Activator.CreateInstance(adapterType, connectionString) as IStorageAdapter;

            if (adapterInterfaceInstance == null)
                throw new Exception("Could not create instance of interface.");

            return adapterInterfaceInstance;
        }
    }
}