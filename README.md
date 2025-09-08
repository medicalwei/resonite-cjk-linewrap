# resonite-cjk-linewrap

This is an attempt to fix [CJK line wrapping in Resonite](https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/3954).

This dll injection includes part of reverse-engineered code for patching out default line splitting behavior in Resonite, therefore I do not have actual copyright to attribute.

## Screenshots
![About YDMS in Japanese rendering](/Screenshots/image1.png?raw=true)
![Settings Text in Japanese rendering](/Screenshots/image2.png?raw=true)

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
1. Place [CJKLinewrap.dll](https://github.com/medicalwei/resonite-cjk-linewrap/releases/latest/download/CJKLinewrap.dll) into your `rml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create this folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Resonite logs.
