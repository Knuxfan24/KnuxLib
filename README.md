# KnuxLib

A repository for me to push various bits of C# code for random file formats in random games I'm tinkering with, alongside a command line tool for extracting/building supported archives/converting supported formats to and from JSON plain text formats.

## Projects

- KnuxLib - The actual C# library itself. See below for formats currently supported.

- KnuxTest - A template CLI program designed for me to use when writing test code, should be left unedited with commits.

- KnuxTools - A CLI program intended for use by the end user.

## Building

To build any of the projects here, simply clone the repository (either using a tool such as GitHub Desktop or Git CMD) then open the .sln file in a modern version of Visual Studio (anything supporting .NET 8 C# development).

For the average end user, they'll want to right click on the KnuxTools project and choose to `Build`, then find the compiled executable in `KnuxTools/bin/Debug/net8.0`. Alternatively, download the KnuxTools artifact from the most recent GitHub Actions compile.

A developer looking to experiment with or contribute to the project will want to use the KnuxTest project for development, only editing the KnuxTools project to add support for a newly implemented format.

## Used Libraries

This repository also uses various other libraries as part of it in the form of NuGet packages or individual segments of code transplanted into the main project. These projects are:

- [AssimpNet](https://bitbucket.org/Starnick/assimpnet) - Used for importing data from 3D models.

- [AuroraLib.Compression](https://github.com/Venomalia/AuroraLib.Compression) - Used for decompressing/recompressing PRS data in Sonic Storybook Engine ONE Archives and decompressing/recompressing LZ11 data in Sonic World Adventure Wii Engine ONZ Archives.

- [HedgeLib](https://github.com/Radfordhound/HedgeLib/tree/master) and [Marathon](https://github.com/Big-Endian-32/Marathon) - Served as the basis for the ExtendedBinary and BINA functionality.

- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) - Used for seralising data to and from a JSON file.

## Formats

The formats supported by KnuxLib, organised by game engine.

<details><summary><h2>Capcom MT Framework</h2></summary>

<h3>Known games:</h3>

Name|System(s)
----|---------
Mega Man X Legacy Collection|PC, Xbox ONE, PlayStation 4, Switch
Mega Man X Legacy Collection 2|PC, Xbox ONE, PlayStation 4, Switch
Mega Man 11|PC, Xbox ONE, PlayStation 4, Switch
> **Note**
> Absolutely used in more things, but these are the only ones I've personally experimented with.

<h3>Formats:</h3>

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Archive](KnuxLib/Engines/CapcomMT/Archive.cs)|`*.arc`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|[❌](## "Compression tends to produce slightly different results, but the files produced appear to be fully compatible.")|An archive format used by various games on the Capcom MT Framework, currently only supports version 7 and 9 archives.

</details>

<details><summary><h2>Hedgehog Engine</h2></summary>

<h3>Known games:</h3>

Name|System(s)
----|---------
Sonic Unleashed|Xbox 360, PlayStation 3
Sonic Colours|Nintendo Wii
Sonic Generations|Xbox 360, PlayStation 3, PC
Sonic Lost World|Nintendo Wii U, PC
Mario & Sonic at the Rio 2016 Olympic Games[*](## "Hedgehog Engine 2")|Wii U
Sonic Forces[*](## "Hedgehog Engine 2")|PC, Xbox ONE, PlayStation 4, Switch
Sakura Wars[*](## "Hedgehog Engine 2")|PlayStation 4
Olympic Games Tokyo 2020[*](## "Hedgehog Engine 2")|PC, Xbox ONE, PlayStation 4, Switch
Mario & Sonic at the Tokyo 2020 Olympic Games[*](## "Hedgehog Engine 2")|Switch
Puyo Puyo Tetris 2[*](## "Hedgehog Engine 2")|PC, Xbox ONE, PlayStation 4, Switch
Sonic Colours Ultimate[*](## "Also uses a modified version of Godot 3")|PC, Xbox ONE, PlayStation 4, Switch
Sonic Origins[*](## "Hedgehog Engine 2")[*](## "Also uses the Retro Engine RSDK")|PC, Xbox ONE, PlayStation 4, Switch
Sonic Frontiers[*](## "Hedgehog Engine 2")|PC, Xbox ONE, PlayStation 4, Switch

<h3>Formats:</h3>

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Archive Info](KnuxLib/Engines/Hedgehog/ArchiveInfo.cs)|`*.arcinfo`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Sonic Unleashed to determine what archives the game has.
[Density Point Cloud](KnuxLib/Engines/Hedgehog/DensityPointCloud.cs)|`*.densitypointcloud`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Sonic Frontiers to determine where to place objects definied in the Density Settings format.
[Instance Info](KnuxLib/Engines/Hedgehog/InstanceInfo.cs)|`*.terrain-instanceinfo`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used from Sonic Unleashed to Sonic Forces to determine where to place terrain models.
[Map (2010)](KnuxLib/Engines/Hedgehog/Map_2010.cs)|`*.map.bin`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used in Sonic Colours to determine where to place terrain chunks.
[Master Level Table](KnuxLib/Engines/Hedgehog/MasterLevels.cs)|`*.mlevel`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Sonic Frontiers to determine what archives the game has.
[Message Table (2010)](KnuxLib/Engines/Hedgehog/MessageTable_2010.cs)|`*.xtb`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Sonic Colours, Sonic Generations and Mario & Sonic at the London 2012 Olympic Games to store plain text messages.
[Message Table (2013)](KnuxLib/Engines/Hedgehog/MessageTable_2013.cs)|`*.xtb2`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Sonic Lost World to store plain text messages.
[Path Spline](KnuxLib/Engines/Hedgehog/PathSpline.cs)|`*.path` `*.path2.bin`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Sonic Lost World, Sonic Forces and Sonic Frontiers to store splines.
[Point Cloud](KnuxLib/Engines/Hedgehog/PointCloud.cs)|`*.pccol` `*.pcmodel` `*.pcrt` `*.pointcloud`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Sonic Frontiers to determine where to place visual assets.
[Sector Visib](KnuxLib/Engines/Hedgehog/SectorVisibilityCollision_2013.cs)[ility Collision](KnuxLib/Engines/Hedgehog/SectorVisibilityCollision_Wars.cs)|`*.svcol.bin`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|Formats used by Sonic Lost World and Sonic Forces to determine when specific terrain chunks should be shown or hidden.
[Terrain Material](KnuxLib/Engines/Hedgehog/TerrainMaterial.cs)|`*.terrain-material`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Sonic Frontiers to apply textures to a heightmap.

</details>

<details><summary><h2>Nintendo</h2></summary>

<h3>Formats:</h3>

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[U8 Archive](KnuxLib/Engines/Nintendo/U8.cs)|`*.arc`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|[❌](## "Some values are unknown and seemingly useless, so I don't bother to read and thus accurately write them. Sonic '06's modifications to the format also end up having slightly different compression.")|An archive format used by various games Nintendo games (as well as Sonic The Hedgehog (2006) for whatever reason).

</details>

<details><summary><h2>Nu2 Engine</h2></summary>

<h3>Known games:</h3>

Name|System(s)
----|---------
Crash Bandicoot: The Wrath of Cortex|PlayStation 2, Xbox, GameCube
> **Note**
> Absolutely used in more things, but these are the only ones I've personally experimented with.

<h3>Formats:</h3>

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[AI Entity Table](KnuxLib/Engines/Nu2/AiEntityTable.cs)|`*.ai`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Crash Bandicoot: The Wrath of Cortex to place enemy characters.
[Crate Table](KnuxLib/Engines/Nu2/CrateTable.cs)|`*.crt`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export") [🔧](## "Experimental, this format contains multiple unknown values that appear important")|✔️|A format used by Crash Bandicoot: The Wrath of Cortex to place crates.
[Wumpa Fruit Table](KnuxLib/Engines/Nu2/WumpaTable.cs)|`*.wmp`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Crash Bandicoot: The Wrath of Cortex to place Wumpa Fruit.

</details>

<details><summary><h2>OpenSpace</h2></summary>

<h3>Known games:</h3>

Name|System(s)
----|---------
Rayman 2: The Great Escape|Nintendo 64, PC, Dreamcast, PlayStation, PlayStation 2, Nintendo DS, iOS, Nintendo 3DS
> **Note**
> Absolutely used in more things, but these are the only ones I've personally experimented with.

<h3>Formats:</h3>

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Big File Archive](KnuxLib/Engines/OpenSpace/BigFileArchive.cs)|`*.bf` `*.dsc`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|[❌](## "The base game archives have empty folders which I don't replicate.")|An archive format used by Rayman Revolution.

</details>

<details><summary><h2>Project M Engine</h2></summary>

<h3>Known games:</h3>

Name|System(s)
----|---------
Metroid: Other M|Wii
> **Note**
> Likely not this engine's name (if it even had one).

<h3>Formats:</h3>

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Message Table](KnuxLib/Engines/ProjectM/MessageTable.cs)|`*.dat`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Metroid: Other M to store plain text messages.

</details>

<details><summary><h2>Stellar Stone Engine</h2></summary>

<h3>Known games:</h3>

Name|System(s)
----|---------
Big Rigs: Over the Road Racing|PC
> **Note**
> Absolutely used in more things, but these are the only ones I've personally experimented with.

> **Note**
> Likely not this engine's name (if it even had one).

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Material Library](KnuxLib/Engines/StellarStone/MaterialLibrary.cs)|`*.mat`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by the Stellar Stone engine to store material data in a plain text format.
[Mesh Object](KnuxLib/Engines/StellarStone/MeshObject.cs)|`*.sco`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by the Stellar Stone engine to store model data in a plain text format.

</details>

<details><summary><h2>Sonic Storybook Engine</h2></summary>

<h3>Known games:</h3>

Name|System(s)
----|---------
Sonic and the Secret Rings|Wii
Sonic and the Black Knight|Wii

> **Note**
> Likely not this engine's name (if it even had one).

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Light Field](KnuxLib/Engines/SonicStorybook/LightField.cs)|`*.bin`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Sonic and the Black Knight to handle lighting on objects.
[Motion Table](KnuxLib/Engines/SonicStorybook/MotionTable.cs)|`*.bin`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export") [🔧](## "Experimental, this format contains multiple unknown values that appear important")|✔️|A format used by the Storybook Engine to store information for animations.
[ONE Archive](KnuxLib/Engines/SonicStorybook/ONE.cs)|`*.one`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|[❌](## "Compression tends to produce slightly different results, but the files produced appear to be fully compatible.")|An archive format used by the Storybook Engine.
[Stage Entity Table Object Table](KnuxLib/Engines/SonicStorybook/StageEntityTableItems.cs)|`*.bin`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by the Storybook Engine to determine what objects can appear in what stages.

</details>

<details><summary><h2>Sonic World Adventure (SD) Engine</h2></summary>

<h3>Known games:</h3>

Name|System(s)
----|---------
Sonic Unleashed|Wii, PlayStation 2

> **Note**
> Likely not this engine's name (if it even had one).

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Area Points Table](KnuxLib/Engines/SonicWorldAdventure_SD/AreaPoints.cs)|`*.wap`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Sonic Unleashed to determine where to place terrain chunks.
[ONE Archive](KnuxLib/Engines/SonicWorldAdventure_SD/ONE.cs)|`*.one` `*.onz`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|[❌](## "Compression tends to produce slightly different results, but the files produced appear to be fully compatible.")|An archive format used by Sonic Unleashed.

</details>

<details><summary><h2>Space Channel Engine</h2></summary>

<h3>Known games:</h3>

Name|System(s)
----|---------
Sonic Channel 5 Part 2|Dreamcast, PlayStation 2, PlayStation 3, PC, Xbox 360

> **Note**
> Likely not this engine's name (if it even had one).

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Caption Table](KnuxLib/Engines/SpaceChannel/CaptionTable.cs)|`*.bin`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by Space Channel 5 Part 2 to store plain text messages.

</details>

<details><summary><h2>Twinsanity Engine</h2></summary>

<h3>Known games:</h3>

Name|System(s)
----|---------
Crash Twinsanity|PlayStation 2, Xbox

> **Note**
> Likely not this engine's name (if it even had one).

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Data Header Pair](KnuxLib/Engines/Twinsanity/DataHeaderPair.cs)|`*.bd` `*.bh`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|An archive format used by Crash Twinsanity.

</details>

<details><summary><h2>Wayforward Engine</h2></summary>

<h3>Known games:</h3>

Name|System(s)
----|---------
Shantae: Risky's Revenge|Nintendo DSi, iOS, PC, PlayStation 4, Wii U, Switch, Xbox ONE
Ducktales Remastered|PlayStation 3, Wii U, PC, Xbox 360, Android, iOS, Windows Phone
Shantae: Half-Genie Hero|PlayStation 4, PlayStation Vita, Wii U, PC, Xbox ONE, Switch
Shantae and the Seven Sirens|iOS, Switch, PlayStation 4, PC, Xbox ONE
> **Note**
> Likely used in more things, but these are the only ones I've personally experimented with.

Name|Type(s)|Support|[1:1](## "Whether or not KnuxLib can make a binary identical copy of a source file.")|Description
----|----|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Environment Table](KnuxLib/Engines/Wayforward/Environment.cs)|`*.env`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A format used by the Wayforward Engine to place static meshes into a scene.
[Package Archive](KnuxLib/Engines/Wayforward/Package.cs)|`*.pak`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|An archive format used by the Wayforward Engine.

</details>