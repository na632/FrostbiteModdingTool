using FMT.FileTools;
using FMT.FileTools.Modding;
using FrostbiteSdk;
using FrostySdk;
using FrostySdk.Frosty;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FrostbiteModdingUI.Models
{
    public class ModBundle : ViewModelBase
    {
        private string title;

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }


        private string description;

        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }

        private string author;

        public string Author
        {
            get { return author; }
            set { SetProperty(ref author, value); }
        }


        private string version;

        public string Version
        {
            get { return version; }
            set { SetProperty(ref version, value); }
        }

        private List<IFrostbiteMod> mods;

        public List<IFrostbiteMod> Mods
        {
            get { return mods; }
            set { SetProperty(ref mods, value); }
        }

        public void Save(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);

            using(NativeWriter nw = new NativeWriter(new FileStream(filename, FileMode.CreateNew)))
            {
                nw.WriteLengthPrefixedString(Title);
                nw.WriteLengthPrefixedString(!string.IsNullOrEmpty(Description) ? Description : string.Empty);
                nw.WriteLengthPrefixedString(!string.IsNullOrEmpty(Author) ? Author : string.Empty);
                nw.WriteLengthPrefixedString(!string.IsNullOrEmpty(Version) ? Version : string.Empty);

                nw.Write((short)mods.Count);
                foreach(var m in mods)
                {
                    nw.WriteLengthPrefixedString(m.GetType().ToString());
                    byte[] compBytes = Utils.CompressFile(m.ModBytes, compressionOverride: CompressionType.Default);
                    nw.Write(compBytes.Length);
                    nw.WriteBytes(compBytes);
                }
            }
        }

        public async void SaveAsync(string filename)
        {
            await Task.Run(() => { Save(filename); });
        }

        public void Load(string filename)
        {
            using (NativeReader nr = new NativeReader(new FileStream(filename, FileMode.CreateNew)))
            {
                Title = nr.ReadLengthPrefixedString();
                Description = nr.ReadLengthPrefixedString();
                Author = nr.ReadLengthPrefixedString();
                Version = nr.ReadLengthPrefixedString();

                Mods = new List<IFrostbiteMod>();

                var modCount = nr.ReadShort();
                for(var i = 0; i < modCount; i++)
                {
                    var modType = nr.ReadLengthPrefixedString();
                    var byteCount = nr.ReadInt();
                    var modBytes = nr.ReadBytes(byteCount);
                    var decompBytes = new CasReader(new MemoryStream(modBytes)).Read();
                }
            }
        }

    }
}
