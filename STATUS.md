---
mod:          Joy Rescue
packageId:    nelim.joyrescue
repo:         Rimworld-Joy-Rescue
visibility:   public
detached:     no
stage:        done
licence:      original
licence_at:   original work
dependencies: declared
showcase:     complete
tested_on:    2026-08-29
workshop:
remaining:
  - feature: still inside the monorepo. `JoyRescue/` is tracked by the monorepo, has no `.git`
    of its own, and no remote points at it. The public repository
    `https://github.com/vbardales/Rimworld-Joy-Rescue` exists but is empty: not one commit has
    ever reached it.
  - defect: `README.md` is written in French, in a repository meant to be public. Everything
    shipped must be English.
  - unverified: the 2026-08-29 run proves the defs are generated and the load is clean, nothing
    more. The current build is four commits younger than that run, and was never launched.
  - unverified: pawn behaviour on a repaired building was never watched, and the settings window
    was never opened in game.
  - unverified: no off-game test suite. The mode heuristic, the `baseChance` toggle and the
    `workerInt` reset are unchecked outside the game.
session:      local_06dd178f-bf2c-4af0-8dd6-02a211cafcac
updated:      2026-09-12, read by the mod's own session
---

# Joy Rescue — status

Kept at the root, never inside `Mod/`, so Steam never receives it. Maintained by the session
that holds this mod, not by the sweep that first wrote it.

## Where it stands

The work is finished and the showcase is complete: preview, mod icon, and the full-resolution
source under `Art/`. Nothing has been published. The repository name is reserved on GitHub and
public, but empty, so publishing is still entirely ahead.

The mod is an original work. It owes nothing to another mod: not a name, not an asset, not an
idea traceable to one. Malay Themed Expansion is what revealed the problem, having shipped two
recreation buildings no colonist can use, but nothing of it is reused here. Hence
`licence: original`.

## What it is made of

A postfix on `DefGenerator.GenerateImpliedDefs_PreResolve` compares every `ThingDef` carrying a
`joyKind` against every `JoyGiverDef.thingDefs`, and builds the missing `JobDef` and
`JoyGiverDef` for the orphans. That window is the one non-negotiable point of the whole mod:
earlier the `joyKind` is not resolved yet, later `JobGiver_GetJoy` has already sized its
positional `DefMap` on the giver count.

Each of the three modes is a giver and driver pair the base game already uses as-is, so no
behaviour code is written. Choice is made on the shape of the def, never on its name, and every
line is overridable in the settings. Disabling an entry sets `baseChance` to zero rather than
removing the def, which is what keeps the settings applicable to a running game.

## Field vocabulary

`stage`: `port`, `showcase`, `preTest`, `done`, `tested`, `published`. `done` means the work is
finished, not that it is published.

`licence`: `open` an explicit licence, `silent` no licence and a dead source, `alive` no licence
but a living source, `forbidden` a written refusal, `original` owing nothing to anyone.

`remaining`: `feature` for something missing from a first release, `defect` for a known fault
left unfixed, `unverified` for what could not be checked.
