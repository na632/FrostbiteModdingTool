// Decompiled with JetBrains decompiler
// Type: Fifa_FB__Explorer.Tools
// Assembly: Fifa_FB_ Explorer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44FAAEA9-9D3F-4DE0-98FD-24D726AEDC8A
// Assembly location: C:\Program Files (x86)\3dGameDevBlog\CG File Explorer 18\Fifa_FB_ Explorer.exe

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace v2k4FIFAModdingCL.CGFE
{
  public class Tools
  {
    public static Color[] c_ShoesColor = new Color[16]
    {
      Color.FromArgb((int) byte.MaxValue, 20, 20, 20),
      Color.FromArgb((int) byte.MaxValue, 50, 34, 105),
      Color.FromArgb((int) byte.MaxValue, 1, 32, 87),
      Color.FromArgb((int) byte.MaxValue, 8, 77, 158),
      Color.FromArgb((int) byte.MaxValue, 1, 159, 224),
      Color.FromArgb((int) byte.MaxValue, 0, 177, 17),
      Color.FromArgb((int) byte.MaxValue, 120, 200, 25),
      Color.FromArgb((int) byte.MaxValue, 250, 245, 10),
      Color.FromArgb((int) byte.MaxValue, 250, 240, 0),
      Color.FromArgb((int) byte.MaxValue, 236, 86, 1),
      Color.FromArgb((int) byte.MaxValue, 227, 1, 103),
      Color.FromArgb((int) byte.MaxValue, 177, 11, 35),
      Color.FromArgb((int) byte.MaxValue, 112, 37, 42),
      Color.FromArgb((int) byte.MaxValue, 146, 114, 63),
      Color.FromArgb((int) byte.MaxValue, 160, 160, 160),
      Color.FromArgb((int) byte.MaxValue, 235, 235, 235)
    };
    private static UTF8Encoding ue = new UTF8Encoding();
    public static string[] suntimes = new string[37]
    {
      "14:00",
      "14:15",
      "14:30",
      "14:45",
      "15:00",
      "15:15",
      "15:30",
      "15:45",
      "16:00",
      "16:15",
      "16:30",
      "16:45",
      "17:00",
      "17:15",
      "17:30",
      "17:45",
      "18:00",
      "18:15",
      "18:30",
      "18:45",
      "19:00",
      "19:15",
      "19:30",
      "19:45",
      "20:00",
      "20:15",
      "20:30",
      "20:45",
      "21:00",
      "21:15",
      "21:30",
      "21:45",
      "22:00",
      "22:15",
      "22:30",
      "22:45",
      "23:00"
    };
    public static string[] Compdatanames = new string[10]
    {
      "dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/compdata/compobj.txt",
      "dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/compdata/settings.txt",
      "dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/compdata/standings.txt",
      "dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/compdata/advancement.txt",
      "dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/compdata/schedule.txt",
      "dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/compdata/weather.txt",
      "dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/compdata/tasks.txt",
      "dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/compdata/initteams.txt",
      "dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/compdata/compids.txt",
      "dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/internationals.txt"
    };
    public static Dictionary<uint, string> ResTypes = new Dictionary<uint, string>()
    {
      {
        277989573U,
        "morphtargets"
      },
      {
        284222881U,
        "shaderprogramdb"
      },
      {
        614472140U,
        "material"
      },
      {
        759670271U,
        "gfx"
      },
      {
        817145171U,
        "occludermesh"
      },
      {
        832408784U,
        "ragdoll"
      },
      {
        921957056U,
        "shaderdb"
      },
      {
        1214542715U,
        "hkdestruction"
      },
      {
        1236358868U,
        "mesh"
      },
      {
        1369688147U,
        "ant"
      },
      {
        1506253200U,
        "facefx"
      },
      {
        1506732887U,
        "shaderdatabase"
      },
      {
        1541398270U,
        "lightingsystem"
      },
      {
        1548309670U,
        "itexture"
      },
      {
        1585851909U,
        "talktable"
      },
      {
        1807144914U,
        "streamingstub"
      },
      {
        1809719482U,
        "Texture"
      },
      {
        1892010814U,
        "enlighten"
      },
      {
        1987325384U,
        "delayloadbundles"
      },
      {
        2062533702U,
        "staticenlighten"
      },
      {
        2432974693U,
        "hknondestruction"
      },
      {
        2507944625U,
        "alttexture"
      },
      {
        2722002395U,
        "layercombinations"
      },
      {
        2951524386U,
        "luac"
      },
      {
        3335336582U,
        "static"
      },
      {
        3336302087U,
        "mohwspecific"
      },
      {
        3497062097U,
        "animtrackdata"
      },
      {
        3780554611U,
        "probeset"
      },
      {
        3815705945U,
        "clothasset"
      },
      {
        3944908039U,
        "headmoprh"
      },
      {
        4022798120U,
        "zs"
      }
    };
    public static Dictionary<int, string> TextureTypes = new Dictionary<int, string>()
    {
      {
        0,
        "TT_2d"
      },
      {
        1,
        "TT_Cube"
      },
      {
        2,
        "TT_3d"
      },
      {
        3,
        "TT_2dArray"
      },
      {
        4,
        "TT_1dArray"
      },
      {
        5,
        "TT_1d"
      },
      {
        6,
        "TT_CubeArray"
      }
    };
    public static Dictionary<int, string> ImageTypes = new Dictionary<int, string>()
    {
      {
        0,
        "Invalid"
      },
      {
        1,
        "R4G4_UNORM"
      },
      {
        2,
        "R4G4B4A4_UNORM"
      },
      {
        3,
        "R5G6B5_UNORM"
      },
      {
        4,
        "B5G6R5_UNORM"
      },
      {
        5,
        "R5G5B5A1_UNORM"
      },
      {
        6,
        "R8_UNORM"
      },
      {
        7,
        "R8_SNORM"
      },
      {
        8,
        "R8_SRGB"
      },
      {
        9,
        "R8_UINT"
      },
      {
        10,
        "R8_SINT"
      },
      {
        11,
        "R8G8_UNORM"
      },
      {
        12,
        "R8G8_SNORM"
      },
      {
        13,
        "R8G8_SRGB"
      },
      {
        14,
        "R8G8_UINT"
      },
      {
        15,
        "R8G8_SINT"
      },
      {
        16,
        "R8G8B8_UNORM"
      },
      {
        17,
        "R8G8B8_SRGB"
      },
      {
        18,
        "R8G8B8A8_UNORM"
      },
      {
        19,
        "R8G8B8A8_SNORM"
      },
      {
        20,
        "R8G8B8A8_UNORM_SRGB"
      },
      {
        21,
        "R8G8B8A8_UINT"
      },
      {
        22,
        "R8G8B8A8_SINT"
      },
      {
        23,
        "B8G8R8A8_UNORM"
      },
      {
        24,
        "B8G8R8A8_SRGB"
      },
      {
        25,
        "R10G11B11_FLOAT"
      },
      {
        26,
        "R11G11B10_FLOAT"
      },
      {
        27,
        "R10G10B10A2_UNORM"
      },
      {
        28,
        "R10G10B10A2_UINT"
      },
      {
        29,
        "R9G9B9E5_FLOAT"
      },
      {
        30,
        "R16_FLOAT"
      },
      {
        31,
        "R16_UNORM"
      },
      {
        32,
        "R16_SNORM"
      },
      {
        33,
        "R16_UINT"
      },
      {
        34,
        "R16_SINT"
      },
      {
        35,
        "R16G16_FLOAT"
      },
      {
        36,
        "R16G16_UNORM"
      },
      {
        37,
        "R16G16_SNORM"
      },
      {
        38,
        "R16G16_UINT"
      },
      {
        39,
        "R16G16_SINT"
      },
      {
        40,
        "R16G16B16A16_FLOAT"
      },
      {
        41,
        "R16G16B16A16_UNORM"
      },
      {
        42,
        "R16G16B16A16_SNORM"
      },
      {
        43,
        "R16G16B16A16_UINT"
      },
      {
        44,
        "R16G16B16A16_SINT"
      },
      {
        45,
        "R32_FLOAT"
      },
      {
        46,
        "R32_UINT"
      },
      {
        47,
        "R32_SINT"
      },
      {
        48,
        "R32G32_FLOAT"
      },
      {
        49,
        "R32G32_UINT"
      },
      {
        50,
        "R32G32_SINT"
      },
      {
        51,
        "R32G32B32A32_FLOAT"
      },
      {
        52,
        "R32G32B32A32_UINT"
      },
      {
        53,
        "R32G32B32A32_SINT"
      },
      {
        54,
        "BC1_UNORM"
      },
      {
        55,
        "BC1_UNORM_SRGB"
      },
      {
        56,
        "BC1A_UNORM"
      },
      {
        57,
        "BC1A_UNORM_SRGB"
      },
      {
        58,
        "BC2_UNORM"
      },
      {
        59,
        "BC2_UNORM_SRGB"
      },
      {
        60,
        "BC3_UNORM"
      },
      {
        61,
        "BC3_UNORM_SRGB"
      },
      {
        62,
        "BC4_UNORM"
      },
      {
        63,
        "BC5_UNORM"
      },
      {
        64,
        "BC6U_FLOAT"
      },
      {
        65,
        "BC6S_FLOAT"
      },
      {
        66,
        "BC7_UNORM"
      },
      {
        67,
        "BC7_UNORM_SRGB"
      },
      {
        68,
        "ETC1_UNORM"
      },
      {
        69,
        "ETC1_UNORM_SRGB"
      },
      {
        70,
        "ETC2RGB_UNORM"
      },
      {
        71,
        "ETC2RGB_UNORM_SRGB"
      },
      {
        72,
        "ETC2RGBA_UNORM"
      },
      {
        73,
        "ETC2RGBA_SRGB"
      },
      {
        74,
        "ETC2RGBA1_UNORM"
      },
      {
        75,
        "ETC2RGBA1_SRGB"
      },
      {
        76,
        "EAC_R11_UNORM"
      },
      {
        77,
        "EAC_R11_SNORM"
      },
      {
        78,
        "EAC_RG11_UNORM"
      },
      {
        79,
        "EAC_RG11_SNORM"
      },
      {
        80,
        "PVRTC1_4BPP_RGBA_UNORM"
      },
      {
        81,
        "PVRTC1_4BPP_RGBA_SRGB"
      },
      {
        82,
        "PVRTC1_4BPP_RGB_UNORM"
      },
      {
        83,
        "PVRTC1_4BPP_RGB_SRGB"
      },
      {
        84,
        "PVRTC1_2BPP_RGBA_UNORM"
      },
      {
        85,
        "PVRTC1_2BPP_RGBA_SRGB"
      },
      {
        86,
        "PVRTC1_2BPP_RGB_UNORM"
      },
      {
        87,
        "PVRTC1_2BPP_RGB_SRGB"
      },
      {
        88,
        "PVRTC2_4BPP_UNORM"
      },
      {
        89,
        "PVRTC2_4BPP_SRGB"
      },
      {
        90,
        "PVRTC2_2BPP_UNORM"
      },
      {
        91,
        "PVRTC2_2BPP_SRGB"
      },
      {
        92,
        "ASTC_4x4_UNORM"
      },
      {
        93,
        "ASTC_4x4_SRGB"
      },
      {
        94,
        "ASTC_5x4_UNORM"
      },
      {
        95,
        "ASTC_5x4_SRGB"
      },
      {
        96,
        "ASTC_5x5_UNORM"
      },
      {
        97,
        "ASTC_5x5_SRGB"
      },
      {
        98,
        "ASTC_6x5_UNORM"
      },
      {
        99,
        "ASTC_6x5_SRGB"
      },
      {
        100,
        "ASTC_6x6_UNORM"
      },
      {
        101,
        "ASTC_6x6_SRGB"
      },
      {
        102,
        "ASTC_8x5_UNORM"
      },
      {
        103,
        "ASTC_8x5_SRGB"
      },
      {
        104,
        "ASTC_8x6_UNORM"
      },
      {
        105,
        "ASTC_8x6_SRGB"
      },
      {
        106,
        "ASTC_8x8_UNORM"
      },
      {
        107,
        "ASTC_8x8_SRGB"
      },
      {
        108,
        "ASTC_10x5_UNORM"
      },
      {
        109,
        "ASTC_10x5_SRGB"
      },
      {
        110,
        "ASTC_10x6_UNORM"
      },
      {
        111,
        "ASTC_10x6_SRGB"
      },
      {
        112,
        "ASTC_10x8_UNORM"
      },
      {
        113,
        "ASTC_10x8_SRGB"
      },
      {
        114,
        "ASTC_10x10_UNORM"
      },
      {
        115,
        "ASTC_10x10_SRGB"
      },
      {
        116,
        "ASTC_12x10_UNORM"
      },
      {
        117,
        "ASTC_12x10_SRGB"
      },
      {
        118,
        "ASTC_12x12_UNORM"
      },
      {
        119,
        "ASTC_12x12_SRGB"
      },
      {
        120,
        "D24_UNORM_S8_UINT"
      },
      {
        121,
        "D24_FLOAT_S8_UINT"
      },
      {
        122,
        "D32_FLOAT_S8_UINT"
      },
      {
        123,
        "D16_UNORM"
      },
      {
        124,
        "D24_UNORM"
      },
      {
        125,
        "D32_FLOAT"
      }
    };
    private static uint[] c_LanguageHashtable = new uint[256]
    {
      0U,
      1996959894U,
      3993919788U,
      2567524794U,
      124634137U,
      1886057615U,
      3915621685U,
      2657392035U,
      249268274U,
      2044508324U,
      3772115230U,
      2547177864U,
      162941995U,
      2125561021U,
      3887607047U,
      2428444049U,
      498536548U,
      1789927666U,
      4089016648U,
      2227061214U,
      450548861U,
      1843258603U,
      4107580753U,
      2211677639U,
      325883990U,
      1684777152U,
      4251122042U,
      2321926636U,
      335633487U,
      1661365465U,
      4195302755U,
      2366115317U,
      997073096U,
      1281953886U,
      3579855332U,
      2724688242U,
      1006888145U,
      1258607687U,
      3524101629U,
      2768942443U,
      901097722U,
      1119000684U,
      3686517206U,
      2898065728U,
      853044451U,
      1172266101U,
      3705015759U,
      2882616665U,
      651767980U,
      1373503546U,
      3369554304U,
      3218104598U,
      565507253U,
      1454621731U,
      3485111705U,
      3099436303U,
      671266974U,
      1594198024U,
      3322730930U,
      2970347812U,
      795835527U,
      1483230225U,
      3244367275U,
      3060149565U,
      1994146192U,
      31158534U,
      2563907772U,
      4023717930U,
      1907459465U,
      112637215U,
      2680153253U,
      3904427059U,
      2013776290U,
      251722036U,
      2517215374U,
      3775830040U,
      2137656763U,
      141376813U,
      2439277719U,
      3865271297U,
      1802195444U,
      476864866U,
      2238001368U,
      4066508878U,
      1812370925U,
      453092731U,
      2181625025U,
      4111451223U,
      1706088902U,
      314042704U,
      2344532202U,
      4240017532U,
      1658658271U,
      366619977U,
      2362670323U,
      4224994405U,
      1303535960U,
      984961486U,
      2747007092U,
      3569037538U,
      1256170817U,
      1037604311U,
      2765210733U,
      3554079995U,
      1131014506U,
      879679996U,
      2909243462U,
      3663771856U,
      1141124467U,
      855842277U,
      2852801631U,
      3708648649U,
      1342533948U,
      654459306U,
      3188396048U,
      3373015174U,
      1466479909U,
      544179635U,
      3110523913U,
      3462522015U,
      1591671054U,
      702138776U,
      2966460450U,
      3352799412U,
      1504918807U,
      783551873U,
      3082640443U,
      3233442989U,
      3988292384U,
      2596254646U,
      62317068U,
      1957810842U,
      3939845945U,
      2647816111U,
      81470997U,
      1943803523U,
      3814918930U,
      2489596804U,
      225274430U,
      2053790376U,
      3826175755U,
      2466906013U,
      167816743U,
      2097651377U,
      4027552580U,
      2265490386U,
      503444072U,
      1762050814U,
      4150417245U,
      2154129355U,
      426522225U,
      1852507879U,
      4275313526U,
      2312317920U,
      282753626U,
      1742555852U,
      4189708143U,
      2394877945U,
      397917763U,
      1622183637U,
      3604390888U,
      2714866558U,
      953729732U,
      1340076626U,
      3518719985U,
      2797360999U,
      1068828381U,
      1219638859U,
      3624741850U,
      2936675148U,
      906185462U,
      1090812512U,
      3747672003U,
      2825379669U,
      829329135U,
      1181335161U,
      3412177804U,
      3160834842U,
      628085408U,
      1382605366U,
      3423369109U,
      3138078467U,
      570562233U,
      1426400815U,
      3317316542U,
      2998733608U,
      733239954U,
      1555261956U,
      3268935591U,
      3050360625U,
      752459403U,
      1541320221U,
      2607071920U,
      3965973030U,
      1969922972U,
      40735498U,
      2617837225U,
      3943577151U,
      1913087877U,
      83908371U,
      2512341634U,
      3803740692U,
      2075208622U,
      213261112U,
      2463272603U,
      3855990285U,
      2094854071U,
      198958881U,
      2262029012U,
      4057260610U,
      1759359992U,
      534414190U,
      2176718541U,
      4139329115U,
      1873836001U,
      414664567U,
      2282248934U,
      4279200368U,
      1711684554U,
      285281116U,
      2405801727U,
      4167216745U,
      1634467795U,
      376229701U,
      2685067896U,
      3608007406U,
      1308918612U,
      956543938U,
      2808555105U,
      3495958263U,
      1231636301U,
      1047427035U,
      2932959818U,
      3654703836U,
      1088359270U,
      936918000U,
      2847714899U,
      3736837829U,
      1202900863U,
      817233897U,
      3183342108U,
      3401237130U,
      1404277552U,
      615818150U,
      3134207493U,
      3453421203U,
      1423857449U,
      601450431U,
      3009837614U,
      3294710456U,
      1567103746U,
      711928724U,
      3020668471U,
      3272380065U,
      1510334235U,
      755167117U
    };

    public static Stream GenerateStreamFromString(string s)
    {
      MemoryStream memoryStream = new MemoryStream();
      StreamWriter streamWriter = new StreamWriter((Stream) memoryStream);
      streamWriter.Write(s);
      streamWriter.Flush();
      memoryStream.Position = 0L;
      return (Stream) memoryStream;
    }

    public static int TotalLines(string filePath)
    {
      int num1;
      using (StreamReader streamReader = new StreamReader(Tools.GenerateStreamFromString(filePath)))
      {
        int num2 = 0;
        while (streamReader.ReadLine() != null)
          ++num2;
        num1 = num2;
      }
      return num1;
    }

    public static float ConverthalfToFloat(short float16Bit)
    {
      int num1 = ((int) float16Bit & 32768) == 0 ? 1 : -1;
      int num2 = ((int) float16Bit & 31744) >> 10;
      int num3 = ((int) float16Bit & 1023) + 1024;
      float num4;
      if (num2 == 0 && num3 == 0)
        num4 = 0.0f;
      else if (num2 == 31)
      {
        num4 = float.NaN;
      }
      else
      {
        float num5 = (float) Math.Pow(2.0, (double) (num2 - 15));
        num4 = (float) (num1 * num3) / 1024f * num5;
      }
      return num4;
    }

    public static uint ComputeLanguageHash(string name)
    {
      byte[] bytes = Tools.ue.GetBytes(name);
      return Tools.EAHash(bytes, bytes.Length);
    }

    private static uint EAHash(byte[] str, int length)
    {
      uint num1 = 0;
      for (int index = 0; index < length; ++index)
      {
        uint num2 = ((uint) str[index] & 223U ^ num1) & (uint) byte.MaxValue;
        num1 = num1 >> 8 ^ Tools.c_LanguageHashtable[(int) (uint) (UIntPtr) num2];
      }
      return num1 ^ 2147483648U;
    }

    public static int MakeLong(short lowPart, short highPart)
    {
      return (int) (ushort) lowPart | (int) highPart << 16;
    }

    public static int ConvertFromDate(DateTime date)
    {
      DateTime dateTime = new DateTime(1582, 10, 14, 0, 0, 0);
      return (date - dateTime).Days;
    }

    public static DateTime ConvertToDate(int gregorian)
    {
      DateTime dateTime = new DateTime(1582, 10, 14, 12, 0, 0);
      return gregorian < 0 ? dateTime : dateTime.AddDays((double) gregorian);
    }

    public static string ConvertBytesToString(byte[] bytes)
    {
      return Tools.ue.GetString(bytes);
    }

    public static byte[] ConvertStringToBytes(string str)
    {
      return Tools.ue.GetBytes(str);
    }

    public static Color GetGenericColor(int colorId)
    {
      return colorId < 0 || colorId > 15 ? Color.FromArgb(0, 0, 0, 0) : Tools.c_ShoesColor[colorId];
    }

    public static int ComputeCrcDb11(byte[] bytes)
    {
      int num1 = -1;
      for (uint index = 0; (ulong) index < (ulong) bytes.Length; ++index)
      {
        int num2 = 7;
        num1 ^= (int) bytes[(int) index] << 24;
        do
        {
          if (num1 >= 0)
            num1 *= 2;
          else
            num1 = num1 * 2 ^ 79764919;
          --num2;
        }
        while (num2 >= 0);
      }
      return num1;
    }

    //public static byte[] GetCASData(CAT Cat, bool uncompressed = false, bool get = true)
    //{
    //  using (FileStream fileStream = new FileStream(Path.GetDirectoryName(Cat.Catpath) + "\\cas_" + Cat.casnum.ToString("D2") + ".cas", FileMode.Open, FileAccess.Read))
    //  {
    //    using (BinaryReader binaryReader = new BinaryReader((Stream) fileStream))
    //    {
    //      binaryReader.BaseStream.Position = Cat.offset;
    //      return uncompressed ? Tools.getUncompresseddata(binaryReader.ReadBytes((int) Cat.size)) : binaryReader.ReadBytes((int) Cat.size);
    //    }
    //  }
    //}

    //public static byte[] getUncompresseddata(byte[] CompBytes)
    //{
    //  MemoryStream memoryStream1 = new MemoryStream();
    //  byte[] numArray1 = new byte[10];
    //  using (MemoryStream memoryStream2 = new MemoryStream(CompBytes))
    //  {
    //    using (BinaryReader binaryReader = new BinaryReader((Stream) memoryStream2))
    //    {
    //      while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
    //      {
    //        byte num1 = binaryReader.ReadByte();
    //        byte[] numArray2 = new byte[4];
    //        for (int index = 2; index >= 0; --index)
    //          numArray2[index] = binaryReader.ReadByte();
    //        long int32_1 = (long) BitConverter.ToInt32(numArray2, 0);
    //        byte num2 = binaryReader.ReadByte();
    //        int num3 = (int) binaryReader.ReadByte();
    //        int num4 = num3 - (num3 & 240);
    //        byte[] numArray3 = binaryReader.ReadBytes(2);
    //        byte[] numArray4 = new byte[4]
    //        {
    //          (byte) 0,
    //          (byte) 0,
    //          (byte) num4,
    //          (byte) 0
    //        };
    //        numArray4[1] = numArray3[0];
    //        numArray4[0] = numArray3[1];
    //        long int32_2 = (long) BitConverter.ToInt32(numArray4, 0);
    //        byte[] src = binaryReader.ReadBytes((int) int32_2);
    //        byte[] numArray5 = new byte[int32_1];
    //        switch (num2)
    //        {
    //          case 15:
    //            switch (src[0])
    //            {
    //              case 38:
    //                long num5 = (long) Compression.ZSTD_decompress(numArray5, (ulong) (uint) int32_1, src, (ulong) int32_2);
    //                break;
    //              case 40:
    //                switch (num1)
    //                {
    //                  case 0:
    //                    using (Decompressor decompressor = new Decompressor())
    //                    {
    //                      numArray5 = decompressor.Unwrap(src, int.MaxValue);
    //                      break;
    //                    }
    //                  case 1:
    //                    using (Decompressor decompressor = new Decompressor(new DecompressionOptions(Form1.ebxdict)))
    //                    {
    //                      numArray5 = decompressor.Unwrap(src, int.MaxValue);
    //                      break;
    //                    }
    //                }
    //                break;
    //            }
    //            break;
    //          case 21:
    //            Compression.OodleLZ_Decompress(src, int32_2, numArray5, int32_1, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 0L, 3L);
    //            break;
    //          default:
    //            numArray5 = src;
    //            break;
    //        }
    //        memoryStream1.Write(numArray5, 0, numArray5.Length);
    //      }
    //      numArray1 = memoryStream1.ToArray();
    //    }
    //  }
    //  return numArray1;
    //}

    public static int getInt8(int x)
    {
      return (double) x <= Math.Pow(2.0, 7.0) - 1.0 ? x : (int) ((double) x - Math.Pow(2.0, 8.0));
    }

    public static short SwapEndian(short x)
    {
      byte num = (byte) ((uint) x & (uint) byte.MaxValue);
      return (short) ((int) (byte) (((int) x & 65280) >> 8) + (int) num * 256);
    }

    public static ushort SwapEndian(ushort x)
    {
      byte num = (byte) ((uint) x & (uint) byte.MaxValue);
      return (ushort) ((uint) (byte) (((int) x & 65280) >> 8) + (uint) num * 256U);
    }

    public static int SwapEndian(int x)
    {
      int num1 = x & (int) byte.MaxValue;
      x >>= 8;
      int num2 = num1 * 256 + (x & (int) byte.MaxValue);
      x >>= 8;
      int num3 = num2 * 256 + (x & (int) byte.MaxValue);
      x >>= 8;
      return num3 * 256 + (x & (int) byte.MaxValue);
    }

    public static uint SwapEndian(uint x)
    {
      uint num1 = x & (uint) byte.MaxValue;
      x >>= 8;
      uint num2 = (uint) ((int) num1 * 256 + ((int) x & (int) byte.MaxValue));
      x >>= 8;
      uint num3 = (uint) ((int) num2 * 256 + ((int) x & (int) byte.MaxValue));
      x >>= 8;
      return (uint) ((int) num3 * 256 + ((int) x & (int) byte.MaxValue));
    }

    public static long SwapEndian(long x)
    {
      long num1 = x & (long) byte.MaxValue;
      x >>= 8;
      long num2 = num1 * 256L + (x & (long) byte.MaxValue);
      x >>= 8;
      long num3 = num2 * 256L + (x & (long) byte.MaxValue);
      x >>= 8;
      long num4 = num3 * 256L + (x & (long) byte.MaxValue);
      x >>= 8;
      long num5 = num4 * 256L + (x & (long) byte.MaxValue);
      x >>= 8;
      long num6 = num5 * 256L + (x & (long) byte.MaxValue);
      x >>= 8;
      long num7 = num6 * 256L + (x & (long) byte.MaxValue);
      x >>= 8;
      return num7 * 256L + (x & (long) byte.MaxValue);
    }

    public static ulong SwapEndian(ulong x)
    {
      ulong num1 = x & (ulong) byte.MaxValue;
      x >>= 8;
      ulong num2 = (ulong) ((long) num1 * 256L + ((long) x & (long) byte.MaxValue));
      x >>= 8;
      ulong num3 = (ulong) ((long) num2 * 256L + ((long) x & (long) byte.MaxValue));
      x >>= 8;
      ulong num4 = (ulong) ((long) num3 * 256L + ((long) x & (long) byte.MaxValue));
      x >>= 8;
      ulong num5 = (ulong) ((long) num4 * 256L + ((long) x & (long) byte.MaxValue));
      x >>= 8;
      ulong num6 = (ulong) ((long) num5 * 256L + ((long) x & (long) byte.MaxValue));
      x >>= 8;
      ulong num7 = (ulong) ((long) num6 * 256L + ((long) x & (long) byte.MaxValue));
      x >>= 8;
      return (ulong) ((long) num7 * 256L + ((long) x & (long) byte.MaxValue));
    }

    public static string ReadNullString(BinaryReader br)
    {
      string empty = string.Empty;
      byte num;
      while ((num = br.ReadByte()) > (byte) 0)
        empty += ((char) num).ToString();
      return empty;
    }

    public static string ReadNullString(Stream s)
    {
      string str = "";
      byte num;
      while ((num = (byte) s.ReadByte()) > (byte) 0 && s.Position < s.Length)
        str += ((char) num).ToString();
      return str;
    }

    public static ulong LEB128(BinaryReader br)
    {
      ulong num1 = 0;
      try
      {
        byte num2 = 0;
        byte num3;
        do
        {
          num3 = br.ReadByte();
          num1 |= (ulong) ((int) num3 & (int) sbyte.MaxValue) << (int) num2;
          num2 += (byte) 7;
        }
        while (num3 >= (byte) 128);
      }
      catch
      {
      }
      return num1;
    }

    public static int RoundUp4(int v)
    {
      return v + 3 & -4;
    }

    public static int RoundUp(int v, int align)
    {
      return v + (align - 1) & ~(align - 1);
    }

    public static long RoundUp(long v, int align)
    {
      return v + (long) (align - 1) & ~(long) (align - 1);
    }

    public static byte[] createLEB128(ulong value)
    {
      MemoryStream memoryStream = new MemoryStream();
      using (BinaryWriter binaryWriter = new BinaryWriter((Stream) memoryStream))
      {
        ulong num;
        for (num = value; num >= 128UL; num >>= 7)
          binaryWriter.Write((byte) (num | 128UL));
        binaryWriter.Write((byte) num);
      }
      return memoryStream.ToArray();
    }

    public static int ComputeBitUsed(uint range)
    {
      if (range == 0U)
        return 1;
      for (int index = 32; index > 0; --index)
      {
        uint num = (uint) (1 << index - 1);
        if ((range & num) > 0U)
          return index;
      }
      return 0;
    }

    public static string ReadNullPaddedString(BinaryReader r, int length)
    {
      byte[] bytes = r.ReadBytes(length);
      int count = 0;
      while (count < length && bytes[count] > (byte) 0)
        ++count;
      return count == 0 ? string.Empty : Tools.ue.GetString(bytes, 0, count);
    }

    public static void WriteNullPaddedString(BinaryWriter w, string str, int length)
    {
      if (str == null)
        str = string.Empty;
      byte[] bytes = Tools.ue.GetBytes(str);
      if (bytes.Length > length)
      {
        w.Write(bytes, 0, length);
      }
      else
      {
        w.Write(bytes);
        for (int length1 = bytes.Length; length1 < length; ++length1)
          w.Write(0);
      }
    }

    public static string ReadString(BinaryReader r, int offset)
    {
      long position = r.BaseStream.Position;
      r.BaseStream.Position = (long) offset;
      int count = (int) r.ReadInt16();
      string str = Tools.ue.GetString(r.ReadBytes(count));
      r.BaseStream.Position = position;
      return str;
    }

    public static int ReadInt(Stream s)
    {
      byte[] buffer = new byte[4];
      s.Read(buffer, 0, 4);
      return BitConverter.ToInt32(buffer, 0);
    }

    public static uint ReadUInt(Stream s)
    {
      byte[] buffer = new byte[4];
      s.Read(buffer, 0, 4);
      return BitConverter.ToUInt32(buffer, 0);
    }

    public static short ReadShort(Stream s)
    {
      byte[] buffer = new byte[2];
      s.Read(buffer, 0, 2);
      return BitConverter.ToInt16(buffer, 0);
    }

    public static ushort ReadUShort(Stream s)
    {
      byte[] buffer = new byte[2];
      s.Read(buffer, 0, 2);
      return BitConverter.ToUInt16(buffer, 0);
    }

    public static long ReadLong(Stream s)
    {
      byte[] buffer = new byte[8];
      s.Read(buffer, 0, 8);
      return BitConverter.ToInt64(buffer, 0);
    }

    public static ulong ReadULong(Stream s)
    {
      byte[] buffer = new byte[8];
      s.Read(buffer, 0, 8);
      return BitConverter.ToUInt64(buffer, 0);
    }

    public static float ReadFloat(Stream s)
    {
      byte[] buffer = new byte[4];
      s.Read(buffer, 0, 4);
      return BitConverter.ToSingle(buffer, 0);
    }

    public static List<string> DirSearchtoc(string sDir)
    {
      List<string> stringList = new List<string>();
      try
      {
        foreach (string file in Directory.GetFiles(sDir))
        {
          if (Path.GetExtension(file) == ".toc")
            stringList.Add(file);
        }
        foreach (string directory in Directory.GetDirectories(sDir))
          stringList.AddRange((IEnumerable<string>) Tools.DirSearchtoc(directory));
      }
      catch
      {
      }
      return stringList;
    }

    public static string ByteArrayToString(byte[] ba)
    {
      StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
      for (int index = 0; index < ba.Length; ++index)
      {
        byte num = ba[index];
        stringBuilder.AppendFormat("{0:x2}", (object) num);
      }
      return stringBuilder.ToString();
    }

    public static byte[] StringToByteArray(string hex)
    {
      int length = hex.Length;
      byte[] numArray = new byte[length / 2];
      for (int startIndex = 0; startIndex < length; startIndex += 2)
        numArray[startIndex / 2] = Convert.ToByte(hex.Substring(startIndex, 2), 16);
      return numArray;
    }

    public static byte[] StringToByteArray(string str, int length)
    {
      return Encoding.ASCII.GetBytes(str.PadRight(length, char.MinValue));
    }

    public static System.Guid ReadGuid(BinaryReader reader)
    {
      return new System.Guid(reader.ReadBytes(16));
    }

    public static string GetTextureType(int type)
    {
      return !Tools.TextureTypes.ContainsKey(type) ? "" : Tools.TextureTypes[type];
    }

    public static string GetImageType(int type)
    {
      return !Tools.ImageTypes.ContainsKey(type) ? "" : Tools.ImageTypes[type];
    }

    //public static byte[] chunkmaker(
    //  byte[] bytes,
    //  int buffersize,
    //  bool texture,
    //  byte type,
    //  int fullmipmap1,
    //  int fullmipmap2,
    //  out int mipmap1,
    //  out int mipmap2,
    //  byte ver = 0)
    //{
    //  mipmap1 = 0;
    //  mipmap2 = 0;
    //  MemoryStream memoryStream1 = new MemoryStream();
    //  using (BinaryWriter binaryWriter = new BinaryWriter((Stream) memoryStream1))
    //  {
    //    using (MemoryStream memoryStream2 = new MemoryStream(bytes))
    //    {
    //      using (BinaryReader binaryReader = new BinaryReader((Stream) memoryStream2))
    //      {
    //        byte[] buffer = new byte[0];
    //        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length - (long) buffersize)
    //        {
    //          byte[] src = binaryReader.ReadBytes(buffersize);
    //          binaryWriter.Write((byte) 0);
    //          byte[] bytes1 = BitConverter.GetBytes(buffersize);
    //          binaryWriter.Write(bytes1[2]);
    //          binaryWriter.Write(bytes1[1]);
    //          binaryWriter.Write(bytes1[0]);
    //          binaryWriter.Write(type);
    //          switch (type)
    //          {
    //            case 0:
    //              buffer = src;
    //              break;
    //            case 15:
    //              switch (ver)
    //              {
    //                case 38:
    //                  buffer = Tools.Wrap(src);
    //                  break;
    //                case 40:
    //                  using (CompressionOptions options = new CompressionOptions(20))
    //                  {
    //                    using (Compressor compressor = new Compressor(options))
    //                    {
    //                      buffer = compressor.Wrap(src);
    //                      break;
    //                    }
    //                  }
    //              }
    //              break;
    //            case 21:
    //              buffer = Tools.Wrapoodle(src);
    //              break;
    //          }
    //          byte[] bytes2 = BitConverter.GetBytes(buffer.Length);
    //          int num = 112 + (int) bytes2[2];
    //          binaryWriter.Write((byte) num);
    //          binaryWriter.Write(bytes2[1]);
    //          binaryWriter.Write(bytes2[0]);
    //          binaryWriter.Write(buffer);
    //          if (texture)
    //          {
    //            long position1 = binaryReader.BaseStream.Position;
    //            long position2 = memoryStream1.Position;
    //            if (position1 == (long) fullmipmap1)
    //            {
    //              mipmap1 = (int) position2;
    //              mipmap2 = (int) position2;
    //            }
    //            if (position1 == (long) fullmipmap2 + (long) fullmipmap1)
    //              mipmap2 = (int) position2;
    //          }
    //        }
    //        long num1 = binaryReader.BaseStream.Length - binaryReader.BaseStream.Position;
    //        byte[] src1 = binaryReader.ReadBytes((int) num1);
    //        binaryWriter.Write((byte) 0);
    //        byte[] bytes3 = BitConverter.GetBytes((int) num1);
    //        binaryWriter.Write(bytes3[2]);
    //        binaryWriter.Write(bytes3[1]);
    //        binaryWriter.Write(bytes3[0]);
    //        binaryWriter.Write(type);
    //        switch (type)
    //        {
    //          case 0:
    //            binaryWriter.Write((byte) 0);
    //            buffer = src1;
    //            break;
    //          case 15:
    //            switch (ver)
    //            {
    //              case 38:
    //                buffer = Tools.Wrap(src1);
    //                break;
    //              case 40:
    //                using (CompressionOptions options = new CompressionOptions(20))
    //                {
    //                  using (Compressor compressor = new Compressor(options))
    //                  {
    //                    buffer = compressor.Wrap(src1);
    //                    break;
    //                  }
    //                }
    //            }
    //            break;
    //          case 21:
    //            buffer = Tools.Wrapoodle(src1);
    //            break;
    //        }
    //        byte[] bytes4 = BitConverter.GetBytes(buffer.Length);
    //        int num2 = 112 + (int) bytes4[2];
    //        binaryWriter.Write((byte) num2);
    //        binaryWriter.Write(bytes4[1]);
    //        binaryWriter.Write(bytes4[0]);
    //        binaryWriter.Write(buffer);
    //      }
    //    }
    //  }
    //  return memoryStream1.ToArray();
    //}

    //public static byte[] Wrapoodle(byte[] src)
    //{
    //  byte[] dest = new byte[(ulong) src.Length * 2UL];
    //  ulong length = (ulong) Compression.OodleLZ_Compress(8, src, (long) src.Length, dest, (long) dest.Length, 0L, 0L);
    //  if ((long) dest.Length == (long) length)
    //    return dest;
    //  byte[] numArray = new byte[length];
    //  Array.Copy((Array) dest, (Array) numArray, (int) length);
    //  return numArray;
    //}

    //public static byte[] Wrap(byte[] src)
    //{
    //  if (src.Length == 0)
    //    return new byte[0];
    //  ulong dstCapacity = Compression.ZSTD_compressBound((ulong) src.Length);
    //  byte[] dst = new byte[dstCapacity];
    //  ulong length = Compression.ZSTD_compress(dst, dstCapacity, src, (ulong) src.Length, 20);
    //  if ((long) dstCapacity == (long) length)
    //    return dst;
    //  byte[] numArray = new byte[length];
    //  Array.Copy((Array) dst, (Array) numArray, (int) length);
    //  return numArray;
    //}

    //public static byte[] Wrap(byte[] src, byte[] dict)
    //{
    //  using (CompressionOptions options = new CompressionOptions(dict, 6))
    //  {
    //    using (Compressor compressor = new Compressor(options))
    //      return compressor.Wrap(src);
    //  }
    //}

    public static byte[] createsha1(byte[] buffer)
    {
      byte[] numArray = new byte[20];
      using (SHA1CryptoServiceProvider cryptoServiceProvider = new SHA1CryptoServiceProvider())
        numArray = cryptoServiceProvider.ComputeHash(buffer);
      return numArray;
    }

    public static int HashFNV1(string StrToHash, int hashseed = 5381, int hashprime = 33)
    {
      int num1 = hashseed;
      for (int index = 0; index < StrToHash.Length; ++index)
      {
        byte num2 = (byte) StrToHash[index];
        num1 = num1 * hashprime ^ (int) num2;
      }
      return num1;
    }

    public static byte[] Easfdecyption(byte[] filebytes)
    {
        var initfskey = "xa37dd45ffe100bfffcc9753aabac325f07cb3fa231144fe2e33ae4783feead2b8a73ff021fac326df0ef9753ab9cdf6573ddff0312fab0b0ff39779eaff312a4f5de65892ffee33a44569bebf21f66d22e54a22347efd375981188743afd99baacc342d88a99321235798725fedcbf43252669dade32415fee89da543bf23d4ex";
         
        Aes aes = (Aes) new AesManaged();
        aes.Key = Encoding.ASCII.GetBytes(initfskey);
        //aes.IV = numArray;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes.CreateDecryptor().TransformFinalBlock(filebytes, 0, filebytes.Length);
    }

    public static void AlignStream(Stream s, int align)
    {
      while ((ulong) s.Position % (ulong) align > 0UL)
        s.Seek(1L, SeekOrigin.Current);
    }

    public static string ByteArrayToHexString(byte[] data, int start = 0, int len = 0)
    {
      if (data == null)
        data = new byte[0];
      StringBuilder stringBuilder = new StringBuilder();
      if (start == 0)
      {
        foreach (byte num in data)
          stringBuilder.Append(num.ToString("X2"));
      }
      else
      {
        if (start <= 0 || start + len > data.Length)
          return "";
        for (int index = start; index < start + len; ++index)
          stringBuilder.Append(data[index].ToString("X2"));
      }
      return stringBuilder.ToString();
    }

    public static byte[] builddds(
      byte[] rawdata,
      string Type,
      int M_firstmipmapsize,
      int m_Height,
      int m_Width,
      int m_depth,
      int m_NMipMaps)
    {
      StringBuilder stringBuilder = new StringBuilder();
      switch (Type)
      {
        case "BC1_UNORM":
          stringBuilder.Append("444453207C000000");
          stringBuilder.Append("07100A00");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Height)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Width)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(M_firstmipmapsize)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_depth)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_NMipMaps)));
          stringBuilder.Append("00000000000000000000000000000000000000000000000000000000000000000000000000000000000000002000000004000000");
          stringBuilder.Append("44585431");
          stringBuilder.Append("00000000000000000000000000000000000000000810400000000000000000000000000000000000");
          break;
        case "BC1_UNORM_SRGB":
          stringBuilder.Append("444453207C000000");
          stringBuilder.Append("07100A00");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Height)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Width)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(M_firstmipmapsize)));
          stringBuilder.Append("00000000");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_NMipMaps)));
          stringBuilder.Append("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000020000000040000004458313000000000000000000000000000000000000000000810400000000000000000000000000000000000");
          stringBuilder.Append("4800000003000000000000000100000000000000");
          break;
        case "BC3_UNORM":
          stringBuilder.Append("444453207C000000");
          stringBuilder.Append("07100A00");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Height)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Width)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(M_firstmipmapsize)));
          stringBuilder.Append("01000000");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_NMipMaps)));
          stringBuilder.Append("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000020000000040000004458543500000000000000000000000000000000000000000810400000000000000000000000000000000000");
          break;
        case "BC3_UNORM_SRGB":
          stringBuilder.Append("444453207C000000");
          stringBuilder.Append("07100A00");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Height)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Width)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(M_firstmipmapsize)));
          stringBuilder.Append("01000000");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_NMipMaps)));
          stringBuilder.Append("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000020000000040000004458313000000000000000000000000000000000000000000810400000000000000000000000000000000000");
          stringBuilder.Append("4E00000003000000000000000100000000000000");
          break;
        case "BC5_UNORM":
          stringBuilder.Append("444453207C000000");
          stringBuilder.Append("07100A00");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Height)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Width)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(M_firstmipmapsize)));
          stringBuilder.Append("01000000");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_NMipMaps)));
          stringBuilder.Append("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000020000000040000004243355500000000000000000000000000000000000000000810400000000000000000000000000000000000");
          break;
        case "BC7_UNORM":
          stringBuilder.Append("444453207C000000");
          stringBuilder.Append("07100A00");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Height)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Width)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(M_firstmipmapsize)));
          stringBuilder.Append("01000000");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_NMipMaps)));
          stringBuilder.Append("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000020000000040000004458313000000000000000000000000000000000000000000810400000000000000000000000000000000000");
          stringBuilder.Append("6200000003000000000000000100000000000000");
          break;
        case "R8G8B8A8_UNORM":
          stringBuilder.Append("444453207C000000");
          stringBuilder.Append("0F100200");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Height)));
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_Width)));
          int num = (m_Width * 32 + 7) / 8;
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(num)));
          stringBuilder.Append("01000000");
          stringBuilder.Append(Tools.ByteArrayToString(BitConverter.GetBytes(m_NMipMaps)));
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("20000000");
          stringBuilder.Append("41000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("20000000");
          stringBuilder.Append("FF000000");
          stringBuilder.Append("00FF0000");
          stringBuilder.Append("0000FF00");
          stringBuilder.Append("000000FF");
          stringBuilder.Append("00100000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          stringBuilder.Append("00000000");
          break;
      }
      byte[] byteArray = Tools.StringToByteArray(stringBuilder.ToString());
      byte[] numArray = new byte[byteArray.Length + rawdata.Length];
      Buffer.BlockCopy((Array) byteArray, 0, (Array) numArray, 0, byteArray.Length);
      Buffer.BlockCopy((Array) rawdata, 0, (Array) numArray, byteArray.Length, rawdata.Length);
      return numArray;
    }

    public enum PixelFormat
    {
      RGBA,
      RGB,
      BC1_UNORM,
      BC1_UNORM_SRGB,
      DXT2,
      DXT3,
      DXT4,
      BC3_UNORM,
      BC3_UNORM_SRGB,
      THREEDC,
      ATI1N,
      LUMINANCE,
      LUMINANCE_ALPHA,
      RXGB,
      A16B16G16R16,
      R16F,
      G16R16F,
      A16B16G16R16F,
      R32F,
      G32R32F,
      A32B32G32R32F,
      DX10,
      UNKNOWN,
    }
  }
}
