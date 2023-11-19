A list of file formats fully supported by KnuxLib, organised by engine. Formats listed here are supported by the KnuxTools project.

## Alchemy Engine

Known games:

- Crash Nitro Kart (PS2, GCN, Xbox)

Supported formats:

- [Assets Container (.gfc/gob)](KnuxLib/Engines/Alchemy/AssetsContainer.cs) reading and data extraction.

> **Note**
> Has an unknown chunk of data.

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

> **Note**
> Has a few unknown values.

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

> **Note**
> Has an unknown value.

- [Bullet Skeleton (.skl.pxd)](KnuxLib/Engines/Hedgehog/BulletSkeleton.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Gismo (Rangers) (.gismod/.gismop)](KnuxLib/Engines/Hedgehog/Gismo_Rangers.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Instance Info (.terrain-instanceinfo)](KnuxLib/Engines/Hedgehog/InstanceInfo.cs) reading, writing, JSON serialisation and JSON deserialisation. Also included is a function to convert a folder of terrain-instanceinfo files into a Sonic Frontiers pcmodel Point Cloud file.

- [Light Field (Rangers) (.lf)](KnuxLib/Engines/Hedgehog/LightField_Rangers.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Master Level Table (.mlevel)](KnuxLib/Engines/Hedgehog/MasterLevels.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has an unknown value.

> **Note**
> Doesn't write in a binary identical format due to a design choice in HedgeLib#, game doesn't seem to mind though?

- [Message Table (Sonic2010/BlueBlur/William) (.xtb)](KnuxLib/Engines/Hedgehog/MessageTable_2010.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Message Table (Sonic2013) (.xtb2)](KnuxLib/Engines/Hedgehog/MessageTable_2013.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has a few unknown values.

> **Note**
> Sonic Lost World has two main message tables, one of them writes in a binary identical fashion to the source file, but the other has mistakes in the BINA Footer. This implementation has also not being tested in game due to HedgeArcPack seemingly corrupting the UI pac files containing the xtb2 files when trying to resave them.

- [Point Cloud (.pccol/.pcmodel/.pcrt)](KnuxLib/Engines/Hedgehog/PointCloud.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has an unknown value.

- [Sector Visibility Collision (.bin.svcol)](KnuxLib/Engines/Hedgehog/SectorVisibilityCollision.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has an unknown value.

> **Note**
> The Sector Visibility Collision format has some extra data in it that is not currently handled, but doesn't seem required?

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

> **Note**
> Almost entirely unknowns.

Notes:

- Uncertain if this is the engine's name.

## Rockman X8 Engine

Known games:

- Megaman X8 (PC, PS2)

Supported formats:

- [Stage Entity Table (.31bf570e/.set)](KnuxLib/Engines/RockmanX8/StageEntityTable.cs) basic reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Almost entirely unknowns.

Notes:

- Uncertain if this is the engine's name.

## Sonic Storybook Engine

Known games:

- Sonic and the Secret Rings (Wii)

- Sonic and the Black Knight (Wii)

Supported formats:

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

## Wayforward Engine

Known games:

- Shantae Risky's Revenge (DSi, iOS, PC, PS4, Wii U, NSW, XB1, PS5)

- Shantae Half-Genie Hero (PS4, PSV, Wii U, PC, XB1, NSW, PS5)

- Shantae and the Seven Sirens (iOS, NSW, PS4, PC, XB1, PS5)

Supported formats:

- [Environment (.env)](KnuxLib/Engines/Wayforward/Environment.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Has a few unknown values.

- [Layer List (.lgb)](KnuxLib/Engines/Wayforward/Layers.cs) reading, writing, JSON serialisation and JSON deserialisation.

> **Note**
> Unsure on the length of the strings within this file, 40 characters seems to work, but I'm not confident that all 40 can be used. Needs testing.

- [List Table (.ltb)](KnuxLib/Engines/Wayforward/ListTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

- [Package Archive (.pak)](KnuxLib/Engines/Wayforward/Package.cs) reading, writing, data extraction and data importing.

> **Note**
> The Package Archive code produces incorrect results for some files in Half-Genie Hero due to an incorrect assumption that needs revising. Risky's Revenge resaving has not been tested.

- Definitely used by other Wayforward games, but I have yet to obtain and look at them myself.

## Westwood Engine

Known games:

- Monopoly (PC)

Supported formats:

- [Message Table (.tre/.tru)](KnuxLib/Engines/Westwood/MessageTable.cs) reading, writing, JSON serialisation and JSON deserialisation.

Notes:

- Did Westwood use this engine and its formats for other games?