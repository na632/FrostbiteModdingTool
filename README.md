# Frostbite Modding Tool

# Summary

## Background (I = "Paulv2k4")
The Frostbite Modding Tool (FMT) was born out of necessity when the "Frosty" tool development team decided not to continue after FIFA 20. Mainly due to the lead developer going to work for DICE.

The original `code` for this tool was developed from using ILSpy to dump the badly decompiled "Frosty" `code` into a .NET 4.5 library. 
This code was then cleaned up, fixed and converted into the `FrostySdk` library. However, all the "Frosty" User Interface was unusable!
From this I was able to understand what was done to make "Frosty" load game files and compile mods back.

FMT in its infancy was the first tool available to create some fairly simple mods for FIFA 21 and developed completely separately from FIFA Editor Tool. 
I was unaware that FIFA Editor Tool was being developed (likely due to modding community drama politics) at the time and kept on using this tool to develop mods I wanted for FIFA 21. 

After about three months FIFA Editor Tool (which seems to have the original code for Frosty) surpassed this tool in almost every way and much more feature rich. 
I continued to use/develop this tool out of hobbyness and because of a few issues I had with FIFA Editor Tool's inability to modify Gameplay (my mod of choice) EBX correctly. 

Moving on to recent times FIFA Editor Tool is a much more feature rich and stable modding tool for Frostbite games beyond FIFA 21 and has a much bigger backing (in both money and developer time) than this one. 

This remains a hobby project and now open source for others to use/steal/sell/research as they wish.

## Credits
- Jaycensolo (Jay has been a great help in testing almost everything this Tool does)
- Somers (Made an enormous mod for FIFA 21 and really put this tool to the test)
- [Aranaktu](https://github.com/xAranaktu) - This man is a legend, helped me learn about SDK Generation and Memory Reading.
- [Frosty Tool](https://frostytoolsuite.com/) development team. All the `original code` for `FrostySdk`,`SDK Generator` and `Modding` came from them.
- [FIFA Editor Tool](https://www.fifaeditortool.com/) development team. Used for research into how to properly handle Meshes for FIFA 21. 
