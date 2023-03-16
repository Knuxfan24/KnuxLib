# KnuxLib

A repository for me to push various bits of C# code for random file formats in random games I'm tinkering with, alongside basic command line tools for format conversion where applicable.

This repository consists of three projects. The KnuxLib project is the main library, the KnuxTest project is an empty command line application I use to test things while reverse engineering and the KnuxTools project is the command line application intended for use by end users.

This repository also uses various other libraries as part of it, included as either a Nuget Package or a Git Submodule. These projects are:

- [Marathon](https://github.com/Big-Endian-32/Marathon) - Used for its BinaryReader and Writer.

- [HedgeLib](https://github.com/Radfordhound/HedgeLib/tree/master) - Used for its BINAV2 Reader and Writer.

- [prs.net](https://github.com/FraGag/prs.net) - Used for decompressing/recompressing data in Sonic Storybook Engine ONE Archives.

- [PuyoTools](https://github.com/nickworonekin/puyotools) - Used for decompressing/recompressing Sonic World Adventure Wii Engine ONZ Archives.

# Supported:

## Alchemy Engine

Known games:

- Crash Nitro Kart (PS2, GCN, Xbox)

Supported formats:

- [Assets Container](KnuxLib/Engines/Alchemy/AssetsContainer.cs) reading and data extraction.

Notes:

- Definitely used by other Vicarious Visions games, but I have yet to obtain and look at them myself.

## CarZ Engine

Known games:

- Big Rigs: Over the Road Racing (PC)

Supported formats:

- [Material Library](KnuxLib/Engines/CarZ/MaterialLibrary.cs) reading, writing, MTL conversion and Assimp based importing.

- [SCO Model](KnuxLib/Engines/CarZ/SCO.cs) reading, writing, OBJ conversion and Assimp based importing.

Notes:

- Uncertain if this is the engine's name. Certain files refer to `r3d`, which might be the engine's actual shorthand name.

- The meaning of the SCO extension is unknown. Something Something Object?

- Definitely used by other Stellar Stone games, but I have yet to obtain and look at them myself.

## Gods Engine

Known games:

- Ninjabread Man (PC, PS2, Wii)

Supported formats:

- [WAD Archive](KnuxLib/Engines/Gods/WAD.cs) reading and data extraction.

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

- [Archive Info](KnuxLib/Engines/Hedgehog/ArchiveInfo.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Point Cloud](KnuxLib/Engines/Hedgehog/PointCloud.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Gismo V3](KnuxLib/Engines/Hedgehog/GismoV3.cs) reading, writing, JSON serialisation and JSON deserialisation.

## Nu2 Engine

Known games:

- Crash Bandicoot: The Wrath of Cortex (PS2, GCN, Xbox)

Supported formats:

- [AI Entity Table](KnuxLib/Engines/Nu2/AIEntityTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Wumpa Fruit Table](KnuxLib/Engines/Nu2/WumpaTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Uncertain if this is the engine's name.

- Definitely used by other Travellers Tales games, but I have yet to obtain and look at them myself.

## Project M Engine

Known games:

- Metroid Other M (Wii)

Supported formats:

- [Message Table](KnuxLib/Engines/ProjectM/MessageTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Uncertain if this is the engine's name.

## Rockman X7 Engine

Known games:

- Megaman X7 (PC, PS2)

Supported formats:

- [Stage Entity Table](KnuxLib/Engines/RockmanX7/StageEntityTable.cs) basic reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Uncertain if this is the engine's name.

## Sonic Storybook Engine

Known games:

- Sonic and the Secret Rings (Wii)

- Sonic and the Black Knight (Wii)

Supported formats:

- [ONE Archive](KnuxLib/Engines/Storybook/ONE.cs) reading, writing, data extraction and data importing.

- [Stage Entity Table Object Table](KnuxLib/Engines/Storybook/StageEntityTableItems.cs) reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Uncertain if this is the engine's name.

## Sonic The Portable Engine

Known games:

- Sonic The Hedgehog 4: Episode 1 (Wii, PC, X360, PS3, iOS, Android)

- Sonic The Hedgehog 4: Episode 2 (PC, X360, PS3, iOS, Android)

Supported formats:

- [AMB Archive](KnuxLib/Engines/Portable/AMB.cs) reading, writing, data extraction and data importing.

Notes:

- Uncertain if this is the engine's name.

## Sonic World Adventure Wii Engine

Known games:

- Sonic Unleashed (PS2, Wii)

Supported formats:

- [ONE Archive](KnuxLib/Engines/WorldAdventureWii/ONE.cs) reading, writing, data extraction and data importing.

Notes:

- Uncertain if this is the engine's name.

## Westwood Engine

Known games:

- Monopoly (PC)

Supported formats:

- [Message Table](KnuxLib/Engines/Westwood/MessageTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Did Westwood use this engine and its formats for other games?

# Experimental Formats

The following formats are only partially supported and have no implementation in the KnuxTools project, either due to missing functionality or just being unfinished.

- [Alchemy Engine Map Collision](KnuxLib/Engines/Alchemy/Collision.cs). Very messy code that can read and dump the data from a HKE file to various OBJ files. There is currently a lot of unknown data in this format that would need to be reverse engineered properly for true support of the format. This format also currently lacks any form of Save, Export or Import function.

- [Nu2 Engine Scenes](KnuxLib/Engines/Nu2/Scene.cs) and the chunks that make them up. Most of the GameCube version of this format is handled for reading (with one unknown chunk structure), the Xbox version is missing the Texture Set and Geometry Set chunks and the PlayStation 2 version is missing the Texture Set, Material Set, Geometry Set and SPEC Set chunks. This format also currently lacks any form of Save, Export or Import function.

- [Storybook Engine Stage Entity Table](KnuxLib/Engines/Storybook/StageEntityTable.cs). Lacks proper object parameters (each byte in the parameter table is read as a seperate parameter with no context) and has a lot of unknown values.

- [World Adventure Wii Engine Stage Entity Table](KnuxLib/Engines/WorldAdventureWii/StageEntityTable.cs). Entirely functional besides a lack of proper object parameters (each byte in the parameter table is read as a seperate parameter with no context).