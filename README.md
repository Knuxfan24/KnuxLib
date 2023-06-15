# KnuxLib

A repository for me to push various bits of C# code for random file formats in random games I'm tinkering with, alongside basic command line tools for format conversion where applicable.

This repository consists of three projects. The KnuxLib project is the main library, the KnuxTest project is an empty command line application I use to test things while reverse engineering and the KnuxTools project is the command line application intended for use by end users.

This repository also uses various other libraries as part of it, included as either a Nuget Package or a Git Submodule. These projects are:

- [libHSON-csharp](https://github.com/hedge-dev/libHSON-csharp) - Used for reading and writing HSON files.

- [HedgeLib](https://github.com/Radfordhound/HedgeLib/tree/master) - Used for its BINAV2 Reader and Writer.

- [Marathon](https://github.com/Big-Endian-32/Marathon) - Used for its BinaryReader and Writer.

- [prs.net](https://github.com/FraGag/prs.net) - Used for decompressing/recompressing data in Sonic Storybook Engine ONE Archives.

- [PuyoTools](https://github.com/nickworonekin/puyotools) - Used for decompressing/recompressing Sonic World Adventure Wii Engine ONZ Archives.

# Supported:

## Alchemy Engine

Known games:

- Crash Nitro Kart (PS2, GCN, Xbox)

Supported formats:

- [Assets Container (.gfc/gob)](KnuxLib/Engines/Alchemy/AssetsContainer.cs) reading and data extraction.

Notes:

- Definitely used by other Vicarious Visions games, but I have yet to obtain and look at them myself.

## CarZ Engine

Known games:

- Big Rigs: Over the Road Racing (PC)

Supported formats:

- [Material Library (.mat)](KnuxLib/Engines/CarZ/MaterialLibrary.cs) reading, writing, MTL conversion and Assimp based importing.

- [SCO Model (.sco)](KnuxLib/Engines/CarZ/SCO.cs) reading, writing, OBJ conversion and Assimp based importing.

Notes:

- Uncertain if this is the engine's name. Certain files refer to `r3d`, which might be the engine's actual shorthand name.

- The meaning of the SCO extension is unknown. Something Something Object?

- Definitely used by other Stellar Stone games, but I have yet to obtain and look at them myself.

## Gods Engine

Known games:

- Ninjabread Man (PC, PS2, Wii)

Supported formats:

- [WAD Archive (.wad)](KnuxLib/Engines/Gods/WAD.cs) reading and data extraction.

Notes:

- Definitely used by other Data Design Interactive games, but I have yet to obtain and look at them myself.

## Hedgehog Engine

Known games:

- Sonic Unleashed (X360, PS3)

- Sonic Colours (Wii)

- Sonic Generations (X360, PS3, PC)

- Sonic Lost World (Wii U, PC)

- Sonic Colours Ultimate (XB1, PS4, NSW, PC)

> **Note**
> Sonic Colours Ultimate also uses Godot components

> **Note**
> The following games all use the updated Hedgehog Engine 2.

- Mario & Sonic at the Rio 2016 Olympic Games (Wii U)

- Sonic Forces (XB1, PS4, NSW, PC)

- Olympic Games Tokyo 2020 (XB1, PS4, NSW, PC)

- Mario & Sonic at the Tokyo 2020 Olympic Games (NSW)

- Sakura Wars (PS4)

- Puyo Puyo Tetris 2 (XB1, PS4, NSW, PC)

- Sonic Frontiers (XB1, PS4, NSW, PC)

Supported formats:

- [Archive Info (.arcinfo)](KnuxLib/Engines/Hedgehog/ArchiveInfo.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Bullet Skeleton (.skl.pxd)](KnuxLib/Engines/Hedgehog/BulletSkeleton.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Gismo (Rangers) (.gismod/.gismop)](KnuxLib/Engines/Hedgehog/Gismo_Rangers.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Instance Info (.terrain-instanceinfo)](KnuxLib/Engines/Hedgehog/InstanceInfo.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Light Field (Rangers)) (.lf)](KnuxLib/Engines/Hedgehog/LightField_Rangers.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Message Table (Sonic2010/BlueBlur) (.xtb)](KnuxLib/Engines/Hedgehog/MessageTable_2010.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Message Table (Sonic2013) (.xtb2)](KnuxLib/Engines/Hedgehog/MessageTable_2013.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Sonic Lost World has two main message tables, one of them writes in a binary identical fashion to the source file, but the other has mistakes in the BINA Footer. This implementation has also not being tested in game due to HedgeArcPack seemingly corrupting the UI pac files containing the xtb2 files when trying to resave them.

- [Point Cloud (.pccol/.pcmodel/.pcrt)](KnuxLib/Engines/Hedgehog/PointCloud.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Sector Visibility Collision (.bin.svcol)](KnuxLib/Engines/Hedgehog/SectorVisibilityCollision.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> The Sector Visibility Collision format has some extra data in it that is not currently handled, but doesn't seem required?

## Nu2 Engine

Known games:

- Crash Bandicoot: The Wrath of Cortex (PS2, GCN, Xbox)

Supported formats:

- [AI Entity Table (.ai)](KnuxLib/Engines/Nu2/AIEntityTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Wumpa Fruit Table (.wmp)](KnuxLib/Engines/Nu2/WumpaTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Uncertain if this is the engine's name.

- Definitely used by other Travellers Tales games, but I have yet to obtain and look at them myself.

## Project M Engine

Known games:

- Metroid Other M (Wii)

Supported formats:

- [Message Table (.dat)](KnuxLib/Engines/ProjectM/MessageTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Uncertain if this is the engine's name.

## Rockman X7 Engine

Known games:

- Megaman X7 (PC, PS2)

Supported formats:

- [Stage Entity Table (.328f438b/.osd)](KnuxLib/Engines/RockmanX7/StageEntityTable.cs) basic reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Uncertain if this is the engine's name.

## Rockman X8 Engine

Known games:

- Megaman X8 (PC, PS2)

Supported formats:

- [Stage Entity Table (.31bf570e/.set)](KnuxLib/Engines/RockmanX8/StageEntityTable.cs) basic reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Uncertain if this is the engine's name.

## Sonic Storybook Engine

Known games:

- Sonic and the Secret Rings (Wii)

- Sonic and the Black Knight (Wii)

Supported formats:

- [ONE Archive (.one)](KnuxLib/Engines/Storybook/ONE.cs) reading, writing, data extraction and data importing.

- [Stage Entity Table Object Table (.bin)](KnuxLib/Engines/Storybook/StageEntityTableItems.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Texture Directory (.txd)](KnuxLib/Engines/Storybook/TextureDirectory.cs) reading, writing, data extraction and data importing.

Notes:

- Uncertain if this is the engine's name.

## Sonic The Portable Engine

Known games:

- Sonic The Hedgehog 4: Episode 1 (Wii, PC, X360, PS3, iOS, Android)

- Sonic The Hedgehog 4: Episode 2 (PC, X360, PS3, iOS, Android)

Supported formats:

- [AMB Archive (.amb)](KnuxLib/Engines/Portable/AMB.cs) reading, writing, data extraction and data importing.

Notes:

- Uncertain if this is the engine's name.

## Sonic World Adventure Wii Engine

Known games:

- Sonic Unleashed (PS2, Wii)

Supported formats:

- [Area Points Table (.wap)](KnuxLib/Engines/WorldAdventureWii/AreaPoints.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [ONE Archive (.one/.onz)](KnuxLib/Engines/WorldAdventureWii/ONE.cs) reading, writing, data extraction and data importing.

Notes:

- Uncertain if this is the engine's name.

## Westwood Engine

Known games:

- Monopoly (PC)

Supported formats:

- [Message Table (.tru)](KnuxLib/Engines/Westwood/MessageTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Did Westwood use this engine and its formats for other games?

# Experimental Formats

The following formats are only partially supported and have no implementation in the KnuxTools project, either due to missing functionality or just being unfinished.

- [Alchemy Engine Map Collision (.hke)](KnuxLib/Engines/Alchemy/Collision.cs). There is currently a lot of unknown data in this format that would need to be reverse engineered properly for true support of the format. This format also currently lacks any form of Import function and only has a temporary OBJ export solution.

- [Engine Black Archive Data (.data)](KnuxLib/Engines/Black/DataArchive.cs). Currently reads, decompresses and exports data from the data archive found in Shantae and the Pirate's Curse. Writing hardcodes a sequence of 0x204 bytes near the start of the file and only has placeholders for another sequence later on, which leads to the game crashing on launch, replacing the sequence of placeholders with the original values seems to make the file work in game, although this hasn't been tested beyond the WayForward logo.

- [Flipnic Engine Binary Archive (.bin)](KnuxLib/Engines/Flipnic/BinaryArchive.cs). Currently reads and exports data, but doesn't have any functionality for importing or saving, as testing it would be a pain in the ass.

- [Hedgehog Engine 2010 Collision (.orc)](KnuxLib/Engines/Hedgehog/Collision_2010.cs). Only reads about half the format but does have an OBJ exporter that exports the basic collision geometry.

- [Hedgehog Engine Rangers Message Table (.cnvrs-text)](KnuxLib/Engines/Hedgehog/MessageTable_Rangers.cs). Lacks most of a save function, as this format is offset hell and is not going to be fun to write a save setup for, especially if I want to make it accurate to the original files, as I'm reading fonts and layouts in such a way that saving will dupe them a lot.

- [Hedgehog Engine Scene Effect Collision (.bin.fxcol)](KnuxLib/Engines/Hedgehog/SceneEffectCollision.cs). Has extremely basic reading and writing (which produces binary identical files to the originals), but the actual purpose of the data and their actual structures is yet to be researched.

- [Nu2 Engine Scenes (.nus/.nux/.gsc)](KnuxLib/Engines/Nu2/Scene.cs) and the chunks that make them up. Most of the GameCube version of this format is handled for reading (with one unknown chunk structure), the Xbox version is missing the Texture Set and Geometry Set chunks and the PlayStation 2 version is missing the Texture Set, Material Set, Geometry Set and SPEC Set chunks. This format also currently lacks any form of Save, Export or Import function.

- [Storybook Engine Message Table (Secret Rings) (.mtx)](KnuxLib/Engines/Storybook/MessageTable_SecretRings.cs). Has all the required functionality, but the text encoding is currently wrong, leading to reading and writing errors when special characters are involved (e.g. the words `déjá vu` becoming `d駛・vu`).

- [Storybook Engine Path Spline (.pth)](KnuxLib/Engines/Storybook/PathSpline.cs). Has reading and writing with a lot of unknown values and basic OBJ exporting.

- [Storybook Engine Player Motion Table (.bin)](KnuxLib/Engines/Storybook/PlayerMotionTable.cs). Entirely functional besides every value being an unknown.

- [Storybook Engine Stage Entity Table (.bin)](KnuxLib/Engines/Storybook/StageEntityTable.cs). Has a lot of little unknown values and lacks proper object parameters names and types, the HSON template sheet I've created only reads things as either a uint or a float, so if something is actually a different data type it won't be parsed correctly.

- [World Adventure Wii Engine Path Spline (.path.dat)](KnuxLib/Engines/WorldAdventureWii/PathSpline.cs). Only has reading and writing with a lot of unknowns. Completely lacking an Import or Export (other than generic JSON serialisation) function.

- [World Adventure Wii Engine Stage Entity Table (.set)](KnuxLib/Engines/WorldAdventureWii/StageEntityTable.cs). Entirely functional besides a lack of proper object parameters names and types, the HSON template sheet I've created only reads things as either a uint or a float, so if something is actually a different data type it won't be parsed correctly.