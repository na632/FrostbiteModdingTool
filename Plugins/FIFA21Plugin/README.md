# FIFA 21 Plugin
## For Frostbite Tools developed by paulv2k4

# Plugin Progress - In progress (loading)

# Dependencies
- FrostySDK (Use latest one from Frostbite Tool as reference for now (NUGET later))
- paulv2k4FrostyModdingSupport (Use latest one from Frostbite Tool as reference for now (NUGET later))
- SharpDX (Texture importing / exporting)

# How the file system works

FIFA 21 uses a very similar system to Anthem. A lot of the explanation on how the system works is here https://github.com/xyrin88/anthemtool but its out of date.
It seems EA/DICE updated the system a little in 2020 to suit this new Engine. In my mind, they have just made it more complicated for the sake of things. Possibly 
to stop modding?

Anyhow, here is my explanation on things. 

## Data and Patch folders
FIFA 21 was early access preloaded with just a Data folder. This data folder was (and still is) useless on its own as the corresponding .sb files were empty. Almost all 
the loading is done from the Patch folder. The patch folder can load from the Data folder too. So the data contains quite large CAS files with the patch folder patching 
whatever is needed on top.

## INITFS
Contains an almost zip like encrypted compilation of files are settings for the file system to load into memory. This includes file descripters.

## Layout.toc (Use Patch Folder)
The Layout.toc loads all the corresponding .toc files and .cas files. Each CAS file is given an index here. At the time of writing FIFA 21 has 80 TOC and 21 CAS.

# CAS File 
A CAS file is container of all data, everything ranging from textures to EBX values like gameplay to ini files for careers. This is just compiled together into the a large indexed file called a CAS file.

# TOC File

# SB File
The SB File is a "Super Bundle" which contains many "Bundles". Each "Bundle" contains many items, could it be EBX, RES (textures etc) and Chunk (textures, ini files etc). The SB file contains the name of the "file", the CAS file index, the location of the "file" inside the CAS and the size of the file.

The SB file is started at an offset, these offsets are provided by the TOC file. For each bundle in the TOC file the SB file is read starting at that offset.
A simple example is the first offset is 0 (the start of the SB file). 


## Initial Header
The start of the "Bundle" is a header with 3 important pieces of information all of which are offsets. 

### FOR INFO
- Int is an integer, a number that is whole
- ReadInt (reads 4 bytes together) 
- ReadUInt (reads 4 bytes together but ensures it is a positive value
- Whenever a read occurs the reader moves to the end of the bytes read.
- Endian.Big reads the bytes in the opposite direction

```
uint CatalogOffset = binarySbReader2.ReadUInt(Endian.Big);
uint CatalogAndCASOffset = binarySbReader2.ReadUInt(Endian.Big) + CatalogOffset;
uint casFileForGroupOffset = binarySbReader2.ReadUInt(Endian.Big);
```

## Information Header
You then read ahead 20 bytes to find the Header of all information of the Bundle.

```
size = nr.ReadInt(Endian.Big) + SBFile.SBInformationHeaderLength;
magicStuff = nr.ReadUInt(Endian.Big);
if (magicStuff != 3599661469)
	throw new Exception("Magic/Hash is not right, expecting 3599661469");

totalCount = nr.ReadInt(Endian.Little);
ebxCount = nr.ReadInt(Endian.Little);
resCount = nr.ReadInt(Endian.Little);
chunkCount = nr.ReadInt(Endian.Little);
stringOffset = nr.ReadInt(Endian.Little) + SBFile.SBInformationHeaderLength;
metaOffset = nr.ReadInt(Endian.Little) + SBFile.SBInformationHeaderLength;
metaSize = nr.ReadInt(Endian.Little) + SBFile.SBInformationHeaderLength;

```

## Seaking data and information
This header will tell you the location of the string names, the amount of files in the bundle etc. 

We then go and find all the files. This is where it gets tricky as you need to scan different sections of the SB Bundle to gather information. Most of it is a combination of 2 offsets to find information.


### TODO: Write part about getting names out

## Boolean list of when the next group of items are in a different CAS File
casFileForGroupOffset is used to seek to a location in the bundle and read out the list for the total count in the bundle. At this point the reader reads one byte for a true false value to store.

## Finding items, their cas index, cas size and cas offset
The reader seeks to the CatalogAndCASOffset found earlier. 

 - The reader uses the boolean list from earlier to read 4 bytes of information to discover the CAS Index information for the next group of items
 - The reader then continues reading offset and size information for each item in the bundle until the next boolean is hit in the list and it finds the next CAS Index
 - This continues in a loop until all files in the count are finished


## Creating a ModData folder
The ModData folder is essentially telling the game engine to load from ModData and not from the root of the game. The way Frosty Mod Manager worked was to create links to the root items and then add additional CAS files (and indexes to the Layout.toc) with your mods in them. It is a good clean system but has lots of issues if mods overwrite each other or didnt read in the correct order.











