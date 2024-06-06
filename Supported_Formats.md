A list of file formats fully supported by KnuxLib, organised by engine. Formats listed here are supported by the KnuxTools project.

## Capcom MT Framework Engine

- [Archive (.arc)](KnuxLib/Engines/CapcomMT/Archive.cs) reading, writing, data extraction and data importing.

> **Note**
> Needs to be tested more in depth.

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

## Crash 6 Engine

Known games:

- Crash Twinsanity (PS2, Xbox)

Supported formats:

- [Data Header Pair (.bd/.bh)](KnuxLib/Engines/Crash6/DataHeaderPair.cs) reading, writing, data extraction and data importing.

Notes:

- Uncertain if this is the engine's name.

## Hasbro Wii Engine

Known games:

- Family Game Night Volume 1 (Wii)

- Family Game Night Volume 2 (Wii)

- Family Game Night Volume 3 (Wii)

Supported formats:

- [Big File Archive (.big)](KnuxLib/Engines/HasbroWii/BigFileArchive.cs) reading, writing, data extraction and data importing.

Notes:

- Uncertain if this is the engine's name.

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

- Sonic Origins (XB1, XBS, PS4, PS5 NSW, PC)

> **Note**
> Sonic Origins also uses Retro Engine components

- Sonic Frontiers (XB1, XBS, PS4, PS5, NSW, PC)

Supported formats:

- [Archive Info (.arcinfo)](KnuxLib/Engines/Hedgehog/ArchiveInfo.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has an unknown value.

- [Bullet Skeleton (.skl.pxd)](KnuxLib/Engines/Hedgehog/BulletSkeleton.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Density Point Cloud (.densitypointcloud)](KnuxLib/Engines/Hedgehog/DensityPointCloud.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has a few unknown values.

- [Gismo (Rangers) (.gismod/.gismop)](KnuxLib/Engines/Hedgehog/Gismo_Rangers.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Instance Info (.terrain-instanceinfo)](KnuxLib/Engines/Hedgehog/InstanceInfo.cs) reading, writing, JSON serialisation and JSON deserialisation. Also included is a function to convert a folder of terrain-instanceinfo files into a Sonic Frontiers pcmodel Point Cloud file.

- [Light Field (Rangers) (.lf)](KnuxLib/Engines/Hedgehog/LightField_Rangers.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Map (2010) (.map.bin)](KnuxLib/Engines/Hedgehog/Map_2010.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has an unknown value and has a slightly inaccurate BINA Footer on written files.

- [Master Level Table (.mlevel)](KnuxLib/Engines/Hedgehog/MasterLevels.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has an unknown value.

> **Note**
> Doesn't write in a binary identical format due to a design choice in HedgeLib#, game doesn't seem to mind though?

- [Message Table (Sonic2010/BlueBlur/William) (.xtb)](KnuxLib/Engines/Hedgehog/MessageTable_2010.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Message Table (Sonic2013) (.xtb2)](KnuxLib/Engines/Hedgehog/MessageTable_2013.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has a few unknown values.

- [Path Spline (.path)](KnuxLib/Engines/Hedgehog/PathSpline.cs) reading, writing, OBJ exporting and OBJ importing.

> **Note**
> Has a few unknown values and an unknown chunk of data in the form of each path's k-d tree.

- [Point Cloud (.pccol/.pcmodel/.pcrt/.pointcloud)](KnuxLib/Engines/Hedgehog/PointCloud.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has an unknown value.

- [Sector Visibility Collision (.svcol.bin)](KnuxLib/Engines/Hedgehog/SectorVisibilityCollision_Wars.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has an unknown value.

> **Note**
> Has some extra data in it that is not currently handled, but doesn't seem required?

- [Terrain Material(.terrain-material)](KnuxLib/Engines/Hedgehog/TerrainMaterial.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has a few unknown values.

> **Note**
> Doesn't write in a binary identical format due to a design choice in HedgeLib#, game doesn't seem to mind though?

## NiGHTS2 Engine

Known games:

- NiGHTS: Journey of Dreams (Wii)

Supported formats:

- [ONE Archive (.one)](KnuxLib/Engines/NiGHTS2/ONE.cs) reading, writing, data extraction and data importing.

> **Note**
> Has a few unknown or skipped values.

> **Note**
> The ONE Archives aren't written accurately and need more testing in game (the hub loaded, but I haven't tested beyond that).

Notes:

- Uncertain if this is the engine's name.

## Nu2 Engine

Known games:

- Crash Bandicoot: The Wrath of Cortex (PS2, GCN, Xbox)

Supported formats:

- [AI Entity Table (.ai)](KnuxLib/Engines/Nu2/AIEntityTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Wumpa Fruit Table (.wmp)](KnuxLib/Engines/Nu2/WumpaTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Uncertain if this is the engine's name.

- Definitely used by other Travellers Tales games, but I have yet to obtain and look at them myself.

## OpenSpace Engine

Known games:

- Rayman 2: The Great Escape (N64, PC, DC, PS1, PS2, DS, iOS, 3DS)

Supported formats:

- [Big File Archive (.bf/.dsc)](KnuxLib/Engines/OpenSpace/BigFileArchive.cs) reading, writing, data extraction and data importing.

Notes:

- Definitely used by other Ubisoft games, but I have yet to obtain and look at them myself.

## Project M Engine

Known games:

- Metroid Other M (Wii)

Supported formats:

- [Message Table (.dat)](KnuxLib/Engines/ProjectM/MessageTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Uncertain if this is the engine's name.

## Sonic Storybook Engine

Known games:

- Sonic and the Secret Rings (Wii)

- Sonic and the Black Knight (Wii)

Supported formats:

- [Light Field (.bin)](KnuxLib/Engines/Storybook/LightField.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has a few unknown values.

- [Message Table (Secret Rings) (.mtx)](KnuxLib/Engines/Storybook/ONE.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has an unknown value.

> **Note**
> The Event Message Tables do not write correctly for some reason.

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

> **Note**
> Has an unknown value.

- [ONE Archive (.one/.onz)](KnuxLib/Engines/WorldAdventureWii/ONE.cs) reading, writing, data extraction and data importing.

Notes:

- Uncertain if this is the engine's name.

## Space Channel Engine

Known games:

- Space Channel 5 Part 2 (DC, PS2, PC, PS3, X360)

Supported formats:

- [Caption Table (.bin)](KnuxLib/Engines/SpaceChannel/CaptionTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> A single Japanese file doesn't write correctly.

Notes:

- Uncertain if this is the engine's name.

## Wayforward Engine

Known games:

- Shantae Risky's Revenge (DSi, iOS, PC, PS4, Wii U, NSW, XB1, PS5)

- Ducktales Remastered (PS3, Wii U, PC, X360, Android, iOS, WP)

- Shantae Half-Genie Hero (PS4, PSV, Wii U, PC, XB1, NSW, PS5)

- Shantae and the Seven Sirens (iOS, NSW, PS4, PC, XB1, PS5)

Supported formats:

- [Environment (.env)](KnuxLib/Engines/Wayforward/Environment.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has a few unknown values.

- [Layer List (.lgb)](KnuxLib/Engines/Wayforward/Layers.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> String lengths are definitely wrong if the garbage data presented by the Ducktales Remastered format is anything to go by.

- [List Table (.ltb)](KnuxLib/Engines/Wayforward/ListTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Package Archive (.pak)](KnuxLib/Engines/Wayforward/Package.cs) reading, writing, data extraction and data importing.

- Definitely used by other Wayforward games, but I have yet to obtain and look at them myself.

## Westwood Engine

Known games:

- Monopoly (PC)

Supported formats:

- [Message Table (.tre/.tru)](KnuxLib/Engines/Westwood/MessageTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Did Westwood use this engine and its formats for other games?

## Yacht Club Engine

Known games:

- Shovel Knight (3DS, Wii U, PC, PS3, PS4, PSV, XB1, AFTV, NSW)

Supported formats:

- [String Translation List (.stl)](KnuxLib/Engines/YachtClub/StringTranslationList.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Japanese files don't write correctly (and might also not be reading correctly?).