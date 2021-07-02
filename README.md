Join our **community Discord**: https://discord.gg/SSDVCHPbrg

![Among Chess Logo](https://github.com/KyleSmith0905/Among-Chess/blob/master/Images/Logo-3D-Faded.png "Among Chess")

---
# Table of Contents
1. [Disclaimer](https://github.com/KyleSmith0905/Among-Chess/README.md#Disclaimer "Disclaimer Section")
2. [Installation](https://github.com/KyleSmith0905/Among-Chess/README.md#Installation "Installation Section")
3. [Feedback](https://github.com/KyleSmith0905/Among-Chess/README.md#Feedback "Feedback Section")
4. [Credit](https://github.com/KyleSmith0905/Among-Chess/README.md#Credit "Credit Section")
5. [Settings](https://github.com/KyleSmith0905/Among-Chess/README.md#Settings "Settings Section")
    1. [Game Modes](https://github.com/KyleSmith0905/Among-Chess/README.md#Game-Modes "Game Modes Subsection")
    2. [Variation](https://github.com/KyleSmith0905/Among-Chess/README.md#Variation "Variation Subsection")
    3. [Board](https://github.com/KyleSmith0905/Among-Chess/README.md#Board "Variation Subsection")
    4. [Main Time](https://github.com/KyleSmith0905/Among-Chess/README.md#Main-Time "Main Time Subsection")
    5. [Increment Time](https://github.com/KyleSmith0905/Among-Chess/README.md#Increment-Time "Increment Time Subsection")

---
# Disclaimer
<p align="center">This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC.</p>

<p align="center">© Innersloth LLC.</p>

---
# Installation
| Mod Version| Among Us Version| Download|
|:---|:---|:---:|
| 1.0.3| 2021.6.15| [Link](https://github.com/KyleSmith0905/Among-Chess/releases/tag/v1.0.3 "1.0.3")|

1. Download the mod using the download link above, then extract the Among Chess folder.
2. Go to the location of your Among Us folder and copy the folder to an easily accessible location (such as the documents folder).
    1. If you bought Among Us through Steam, open Steam and locate Among Us in your library, select properties then local files.
    2. If you bought Among Us through Epic Games, open file explorer, navigate to Program Files, find Among Us.
3. Move the folder Among Chess folder's contents to the new Among Us folder.

The folder should look like this after step 3 (assuming you are modding the Steam version).
```
.
┣━ Among Us_Data
┣━ BepInEx*
┣━ mono*
┣━ Among Us
┣━ baselib.dll
┣━ doorstop_config*
┣━ GameAssembly.dll
┣━ msvcp140.dll
┣━ UnityCrashHandler32
┣━ UnityPlayer.dll
┣━ vcruntime140.dll
┗━ winhttp.dll*
```

---
# Feedback
If you have any feedback regarding the game, this may include: 
- Bugs
- Suggestions
- Support
- Inquiries

Please contact the current creator by messaging him on Discord (FiNS Flexin#6193) or by joining the [Among Chess Discord server](https://discord.gg/SSDVCHPbrg "Among Chess Community Discord Server"). The mod creator cannot be bothered and is always happy to respond to any feedback.

---
# Credit
| Title| Description| Links|
|:---|:---|:---|
| Harmony| C# patching library| [Github page](https://github.com/pardeike/Harmony "Github Page"), [Documentation](https://harmony.pardeike.net/ "Documentation")|
| BepInEx| Unity patching library| [Github page](https://github.com/BepInEx/BepInEx "Github Page")|
| Reactor| Among Us modding API| [Github page](https://github.com/NuclearPowered/Reactor "Github Page"), [Documentation](https://docs.reactor.gg/ "Documentation"), [Discord](https://discord.com/invite/pKM7pbufP3 "Discord Server")|

---
# Settings

```
.
┣━┯━ Game Modes
┃ └─── Chess
┃
┣━┯━ Variation
┃ └─── Normal
┃
┣━┯━ Board
┃ ├─── Default
┃ └─── Chess960 ┄ (Randomized positions of the major pieces)
┃
┗━┳━ Time Control ┅ [Note 1]
  ┣━┯━ Main Time
  ┃ ├─── Unlimited ┄ (Correspondence)
  ┃ ├─── 0.5 ┄ (Bullet)
  ┃ ├─── 1 ┄ (Bullet)
  ┃ ├─── 2 ┄ (Bullet)
  ┃ ├─── 3 ┄ (Blitz)
  ┃ ├─── 5 ┄ (Blitz)
  ┃ ├─── 10 ┄ (Rapid)
  ┃ ├─── 30 ┄ (Rapid)
  ┃ └─── 60 ┄ (Rapid)
  ┃
  ┗━┯━ Increment Time ┅ [Note 2]
    ├─── 0
    ├─── 0.5
    ├─── 1
    ├─── 2
    ├─── 5
    ├─── 10
    └─── 30
```

[Note 1]: Time control can get slightly desynchronized. The mod will attempt to adjust for it the best it can.\
[Note 2]: The duration it takes to walk between pieces is already considered. Change these settings like you would on a standard chess website.