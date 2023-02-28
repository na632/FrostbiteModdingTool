using FMT.FileTools;
using Frostbite.Textures;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.Managers;
using FrostySdk.Resources;
using System;
using System.Collections.Generic;
using System.IO;

namespace Madden21.Textures
{
    public class M21TextureImporter : ITextureImporter
    {
        public void DoImport(string path, EbxAssetEntry assetEntry, ref Texture textureAsset)
        {
            //new TextureImporter().ImportTextureFromFileToTextureAsset_Original(path, assetEntry, AssetManager.Instance, ref textureAsset, out string message);

            var extension = "DDS";
            var spl = path.Split('.');
            extension = spl[spl.Length - 1].ToUpper();

            var compatible_extensions = new List<string>() { "DDS", "PNG" };
            if (!compatible_extensions.Contains(extension))
            {
                throw new NotImplementedException("Incorrect file type used in Texture Importer");
            }

            TextureUtils.ImageFormat imageFormat = TextureUtils.ImageFormat.DDS;
            imageFormat = (TextureUtils.ImageFormat)Enum.Parse(imageFormat.GetType(), extension);


            MemoryStream memoryStream = null;
            TextureUtils.BlobData pOutData = default(TextureUtils.BlobData);
            if (imageFormat == TextureUtils.ImageFormat.DDS)
            {
                memoryStream = new MemoryStream(NativeReader.ReadInStream(new FileStream(path, FileMode.Open, FileAccess.Read)));
            }
            else
            {
                TextureUtils.TextureImportOptions options = default(TextureUtils.TextureImportOptions);
                options.type = textureAsset.Type;
                options.format = TextureUtils.ToShaderFormat(textureAsset.PixelFormat, (textureAsset.Flags & TextureFlags.SrgbGamma) != 0);
                options.generateMipmaps = (textureAsset.MipCount > 1);
                options.mipmapsFilter = 0;
                //options.resizeTexture = true;
                options.resizeTexture = false;
                options.resizeFilter = 0;
                //options.resizeHeight = textureAsset.Height;
                //options.resizeWidth = textureAsset.Width;
                options.resizeHeight = 0;
                options.resizeWidth = 0;
                if (textureAsset.Type == TextureType.TT_2d)
                {
                    byte[] pngarray = NativeReader.ReadInStream(new FileStream(path, FileMode.Open, FileAccess.Read));
                    TextureUtils.ConvertImageToDDS(pngarray, pngarray.Length, imageFormat, options, ref pOutData);
                }
                else
                {
                    throw new NotImplementedException("Unable to process PNG into non-2D texture");
                }
            }

            if (imageFormat != TextureUtils.ImageFormat.DDS)
            {
                memoryStream = new MemoryStream(pOutData.Data);
            }

            //if (!Directory.Exists("Debugging"))
            //	Directory.CreateDirectory("Debugging");

            //if (!Directory.Exists("Debugging\\Other\\"))
            //	Directory.CreateDirectory("Debugging\\Other\\");

            //using (FileStream fileStream = new FileStream("Debugging\\Other\\_TextureImport.dat", FileMode.OpenOrCreate))
            //{
            //	memoryStream.CopyTo(fileStream);
            //	fileStream.Flush();
            //}
            //memoryStream.Position = 0;


            using (NativeReader nativeReader = new NativeReader(memoryStream))
            {
                TextureUtils.DDSHeader dDSHeader = new TextureUtils.DDSHeader();
                if (dDSHeader.Read(nativeReader))
                {

                }

                string pixelFormat = "";
                TextureFlags flags = 0;
                textureAsset.FirstMip = 0;
                //textureAsset.FirstMipOffset = 0;
                //textureAsset.Flags = textureAsset.PixelFormat.Contains("SRGB") ? TextureFlags.SrgbGamma : TextureFlags.Streaming;

                TextureImporter.GetPixelFormat(dDSHeader, textureAsset, out pixelFormat, out flags);


                var ebxAsset = AssetManager.Instance.GetEbx(assetEntry);
                ulong resRid = ((dynamic)ebxAsset.RootObject).Resource;



                byte[] textureArray = new byte[nativeReader.Length - nativeReader.Position];
                nativeReader.Read(textureArray, 0, (int)(nativeReader.Length - nativeReader.Position));
                textureAsset.CalculateMipData((byte)dDSHeader.dwMipMapCount, TextureUtils.GetFormatBlockSize(pixelFormat), TextureUtils.IsCompressedFormat(pixelFormat), (uint)textureArray.Length);
                //textureAsset.Flags = flags;
                //TextureFlags textureFlags = textureAsset.Flags & ~TextureFlags.SrgbGamma;
                //textureAsset.Flags |= textureFlags;
                AssetManager.Instance.ModifyChunk(textureAsset.ChunkId, textureArray, textureAsset);
                using (FileStream fileStream = new FileStream("Debugging\\Other\\_TextureImport.dat", FileMode.OpenOrCreate))
                {
                    fileStream.Write(textureAsset.ToBytes(), 0, textureAsset.ToBytes().Length);
                }


                AssetManager.Instance.ModifyRes(resRid, textureAsset.ToBytes());
                AssetManager.Instance.ModifyEbx(assetEntry.Name, ebxAsset);
                ChunkAssetEntry chunkEntry = AssetManager.Instance.GetChunkEntry(textureAsset.ChunkId);
                ResAssetEntry resEntry = AssetManager.Instance.GetResEntry(resRid);
                resEntry.LinkAsset(chunkEntry);
                assetEntry.LinkAsset(resEntry);
            }

        }

        public void DoImport(string path, AssetEntry assetEntry)
        {
            throw new NotImplementedException();
        }
    }
}