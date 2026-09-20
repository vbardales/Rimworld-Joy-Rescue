# Attribution

Joy Rescue is original work by **Nelim**, distributed under the MIT license in
`LICENSE`. The same notice accompanies the distributed mod in `Mod/LICENSE`.

- **RimWorld**, by Ludeon Studios, provides the game APIs and the recreation givers
  and job drivers used by this mod. Game assemblies are referenced for compilation
  and local testing; they are not included in the distributed mod.
- **Harmony**, by Andreas Pardeike and contributors, provides runtime patching.
  Players install Harmony separately. Its licensing is independent of this mod.
- **Krafs.Rimworld.Ref** and **Krafs.Publicizer** support compilation. Neither is
  shipped as a runtime component of Joy Rescue.
- **Malay Themed Expansion** exposed the missing-recreation problem that motivated
  this project. No code, name or artwork from that mod is incorporated here.
- **Shared Joys** informed the consideration of group recreation capacity; it is
  an optional compatibility context, not a required or bundled dependency.
- The showcase illustration and mascot icon were generated with AI for this mod.
  The source illustration is retained in `Art/Preview-source.png`.

The MIT grant covers this project's own material. It does not relicense RimWorld,
Harmony, other mods, or build dependencies. No affiliation with Ludeon Studios or
the authors of the mentioned mods is implied.

## Compatibility rule data

The common recreation preset references package IDs, definition names and structural
relationships found in installed recreation mods. Its rule table and implementation
are authored for Joy Rescue. No referenced mod code, art or extracted assets are
redistributed. The optional `JoyPreservation.JobDriver_PlayMahjong` visual adapter was
inspected to verify that it retains the vanilla play behavior; its private third-party
implementation is not included. These references neither relicense those mods nor
change their distribution permissions.
