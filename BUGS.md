# Known issues

Kept at the root, never inside `Mod/`.

## Open

None.

## Resolved

### The reassignment button clips a long pending type name

Seen on the `@review` captures of F07 and F10 (tree `b1dfb84`, English). In the settings list, a building or
activity whose reassignment waits for a restart shows `<type name> (on restart)` on a 140 px button
(`KindColumnWidth`). With the default custom name, `custom recreation 1 (on restart)`, the label is cut on both
sides: `m recreation 1 (on res`. The French notice, `(au redémarrage)`, is longer still. Nothing is lost, the
state is right and the type list above says `on restart` too, but it reads as broken.

Where: `CurrentKindLabel` and `CurrentGiverKindLabel` in `Source/JoyRescueMod.cs`, both building the label from
the `JoyRescue.Settings.KindPending` key.

Options: cut the type name with an ellipsis and keep the notice (very little room left for the name, about
7 characters), or drop the notice from the button and show the pending state by colour and tooltip, or widen the
column. Any of them changes the DLL, so the passes of the third wave replay on the new build.

Found 2026-09-28. Fixed the same day, owner's choice: the name is cut with an ellipsis (whole button width), the button is tinted yellow while pending, the tooltip carries the full name and the restart notice. The third wave replays on the new build.
