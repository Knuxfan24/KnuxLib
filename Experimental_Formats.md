The following formats are only partially supported, either due to missing functionality or just being unfinished (files consisting almost entirely of unknown values also fall into this category, even if they can be saved in a binary identical fashion to their original versions).

## Alchemy Engine

- [Assets Container (.gfc/gob)](KnuxLib/Engines/Alchemy/AssetsContainer.cs). Has an unknown chunk of data and no way to import or save a new file.

- [Map Collision (.hke)](KnuxLib/Engines/Alchemy/Collision.cs). There is currently a lot of unknown data in this format that would need to be reverse engineered properly for true support of the format. This format also currently lacks any form of Import function and only has a temporary OBJ export solution.

## Engine Black

- [Archive Data (.data)](KnuxLib/Engines/Black/DataArchive.cs). Currently reads, decompresses and exports data from the data archive found in Shantae and the Pirate's Curse. Writing hardcodes a sequence of 0x204 bytes near the start of the file and only has placeholders for another sequence later on, which leads to the game crashing on launch, replacing the sequence of placeholders with the original values seems to make the file work in game, although this hasn't been tested beyond the WayForward logo.

- [Volume Blob (.vol)](KnuxLib/Engines/Black/VolumeBlob.cs). Currently reads, decompresses (in the case of the 3DS version) and exports data from the vol files found in Shantae and the Pirate's Curse (although the GOG version has a different header which causes problems?).

## Flipnic Engine

- [Binary Archive (.bin)](KnuxLib/Engines/Flipnic/BinaryArchive.cs). Currently reads and exports data, but doesn't have any functionality for importing or saving, as testing it would be a pain in the ass.

## GODS Engine

- [WAD Archive (.wad)](KnuxLib/Engines/Gods/WAD.cs). Has a few unknown values and no way to import or save a new file. The Wii version has even more unknowns and there are also definitely different versions of this format for different Data Design Interactive games.

## Hedgehog Engine

- [2010 Collision (.orc)](KnuxLib/Engines/Hedgehog/Collision_2010.cs). Only reads about half the format but does have an OBJ exporter that exports the basic collision geometry.

- [Rangers Collision (.btmesh)](KnuxLib/Engines/Hedgehog/Collision_Rangers.cs). Only has a reader that misses the bounding volume hierarchy data for the meshes, also has quite a few unknown values.

- [Scene Effect Collision (.fxcol.bin)](KnuxLib/Engines/Hedgehog/SceneEffectCollision.cs). Has extremely basic reading and writing (which produces binary identical files to the originals), but the actual purpose of the data and their actual structures is yet to be researched.

## Nu2 Engine

- [Scenes (.nus/.nux/.gsc)](KnuxLib/Engines/Nu2/Scene.cs) and the chunks that make them up. Most of the GameCube version of this format is handled for reading (with one unknown chunk structure), the Xbox version is missing the Texture Set and Geometry Set chunks and the PlayStation 2 version is missing the Texture Set, Material Set, Geometry Set and SPEC Set chunks. This format also currently lacks any form of Save, Export or Import function.

## Rockman X7 Engine

- [Stage Entity Table (.328f438b/.osd)](KnuxLib/Engines/RockmanX7/StageEntityTable.cs). Entirely functional besides basically every value being an unknown.

- [SLD Spline (.6ae91701/.sld)](KnuxLib/Engines/RockmanX7/SLDSpline.cs). Has very basic reading, but half of the file is an unknown placeholder that clearly interacts with the spline points in some ways. Can also export to a basic OBJ.

## Rockman X8 Engine

- [Stage Entity Table (.31bf570e/.set)](KnuxLib/Engines/RockmanX8/StageEntityTable.cs). Entirely functional besides basically every value being an unknown.

## Sonic Storybook Engine

- [Path Spline (.pth)](KnuxLib/Engines/Storybook/PathSpline.cs). Has reading and writing with a lot of unknown values and basic OBJ exporting.

- [Player Motion Table (.bin)](KnuxLib/Engines/Storybook/PlayerMotionTable.cs). Entirely functional besides every value being an unknown.

- [Stage Entity Table (.bin)](KnuxLib/Engines/Storybook/StageEntityTable.cs). Has a lot of little unknown values and lacks proper object parameters names and types, the HSON template sheet I've created only reads things as either a uint or a float, so if something is actually a different data type it won't be parsed correctly.

## Sonic World Adventure Wii Engine

- [Path Spline (.path.dat)](KnuxLib/Engines/WorldAdventureWii/PathSpline.cs). Only has reading and writing with a lot of unknowns. Completely lacking an Import or Export (other than generic JSON serialisation) function, left out of KnuxTools for this reason.

- [Stage Entity Table (.set)](KnuxLib/Engines/WorldAdventureWii/StageEntityTable.cs). Entirely functional besides a lack of proper object parameters names and types, the HSON template sheet I've created only reads things as either a uint or a float, so if something is actually a different data type it won't be parsed correctly.

## Wayforward Engine

- [Collision (.clb)](KnuxLib/Engines/Wayforward/Collision.cs). Reads and writes fine, but Seven Sirens has a massive chunk of Unknown Data added on to the end that is completely alien to me and needs to be properly reverse engineered. Ducktales Remastered also handles some of the data slightly differently, which isn't properly handled right now. Also has a (potentially temporary?) OBJ exporter and Assimp importer.

- [Level Binary (.lvb)](KnuxLib/Engines/Wayforward/LevelBinary.cs). Really, REALLY unfinished reading that barely gets anywhere. Honestly this format has pushed me to my limit I hate it. Left out of KnuxTools because it has next to no functionality in its current state.

- [Mesh (.wf3d/.gpu)](KnuxLib/Engines/Wayforward/Mesh.cs) and the chunks that make them up. This code currently reads MOST of the data (although a lot of chunks have unknown bits that I am yet to successfully read), but a lot of the data is a mystery. This format also has some (slightly messy) unfinished functionality for Saving, Exporting to OBJ (which is intended to be temporary due to it not supporting things I've yet to reverse engineer) and Assimp Importing.