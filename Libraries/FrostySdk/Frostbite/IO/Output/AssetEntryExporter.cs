using FMT.FileTools;
using FrostySdk.Managers;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.Frostbite.IO.Output
{
    public class AssetEntryExporter : IDisposable
    {
        private bool disposedValue;

        private IAssetEntry Entry { get; set; }

        public AssetEntryExporter(IAssetEntry entry) { 
        
            Entry = entry;
        }

        public void Export(string filePath) 
        {
            if (Entry == null)
                return;

            if (Entry is EbxAssetEntry)
            {
                var obj = AssetManager.Instance.GetEbx((EbxAssetEntry)Entry).RootObject;

                if (filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    var serialisedObj = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        MaxDepth = 4,
                    });

                    File.WriteAllText(filePath, serialisedObj);
                }
            }


        }

        public string ExportToJson()
        {

            var obj = AssetManager.Instance.GetEbx((EbxAssetEntry)Entry).RootObject;
            var serialisedObj = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MaxDepth = 4,
            });

            return serialisedObj;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Entry = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
