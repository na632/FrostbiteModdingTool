using Frostbite.Textures;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Frostbite.Textures.TextureUtils;

namespace Frostbite
{
    public class TextureConvertingHelper
    {
        public static byte[] ModifyTexture(DDSHeader existingTextureHeader, byte[] newTexture)
        {
            var ms = new MemoryStream();
            NativeWriter modifiedNewTextureStream = new NativeWriter(ms);
            MemoryStream newTextureStream = new MemoryStream(newTexture);
            DDSHeader newTextureHeader = new DDSHeader();
            newTextureHeader.Read(new NativeReader(newTextureStream));
            Format dxgiFormat = newTextureHeader.ExtendedHeader.dxgiFormat;
            int sizeOfFormat = (dxgiFormat.IsCompressed() ? (dxgiFormat.SizeOfInBits() / 2) : dxgiFormat.SizeOfInBytes());
            existingTextureHeader.dwWidth = newTextureHeader.dwWidth;
            existingTextureHeader.dwHeight = newTextureHeader.dwHeight;
            int existingTextureWidth = existingTextureHeader.dwWidth;
            int existingTextureHeight = existingTextureHeader.dwHeight;
            int numberOfMips = ((existingTextureHeader.dwMipMapCount == 0) ? 1 : existingTextureHeader.dwMipMapCount);
            int num5 = 0;
            for (int i = 0; i < numberOfMips; i++)
            {
                int num6 = (dxgiFormat.IsCompressed() ? (Math.Max(1, (existingTextureWidth + 3) / 4) * sizeOfFormat * existingTextureHeight) : (existingTextureWidth * sizeOfFormat * existingTextureHeight));
                num5 += num6;
                existingTextureWidth >>= 1;
                existingTextureHeight >>= 1;
                if (existingTextureWidth < 1)
                {
                    existingTextureWidth = 1;
                }
                if (existingTextureHeight < 1)
                {
                    existingTextureHeight = 1;
                }
            }
            existingTextureHeader.dwPitchOrLinearSize = num5;
            existingTextureHeader.Write(modifiedNewTextureStream);
            existingTextureWidth = existingTextureHeader.dwWidth;
            existingTextureHeight = existingTextureHeader.dwHeight;
            for (int j = 0; j < numberOfMips; j++)
            {
                int count = (dxgiFormat.IsCompressed() ? (Math.Max(1, (existingTextureWidth + 3) / 4) * sizeOfFormat * existingTextureHeight) : (existingTextureWidth * sizeOfFormat * existingTextureHeight));
                modifiedNewTextureStream.Write(new NativeReader(newTextureStream).ReadBytes(count));
                existingTextureWidth >>= 1;
                existingTextureHeight >>= 1;
                if (existingTextureWidth < 1)
                {
                    existingTextureWidth = 1;
                }
                if (existingTextureHeight < 1)
                {
                    existingTextureHeight = 1;
                }
            }
            
            return ms.ToArray();
        }

        /// <summary>
        /// Will convert and transform an image to try 
        /// </summary>
        /// <param name="resName"></param>
        /// <param name="importFilename"></param>
        /// <returns></returns>
        public bool ImportATextureIntoRES(string resName, string importFilename)
        {
            if (File.Exists("convertedtexture.DDS"))
                File.Delete("convertedtexture.DDS");

            var resEntry = AssetManager.Instance.GetResEntry(resName);
            if (resEntry != null)
            {
                using (var resStream = AssetManager.Instance.GetRes(resEntry))
                {
                    Texture texture = new Texture(resStream, AssetManager.Instance);
                    TextureImporter textureImporter = new TextureImporter();
                    var ebxEntry = AssetManager.Instance.GetEbxEntry(resName);
                    textureImporter.Import(importFilename, ebxEntry, ref texture);

                    return true;
                }
            }
            return false;
        }
    }
}
