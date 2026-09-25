# F16 of Tests/TAXONOMY.md: the common fix set on real mods. Played as
#   -Filter 33-f16-real-mods-write.feature -Then 34-f16-real-mods-read.feature
# with wsl-deps.real-mods.map, which stages five mods of the Workshop by their ids (a piano, a retro console, a gym,
# a shelf of books, a drum to push), all with the versions the rules of the set were selected from. The fix set
# applies at startup, so this launch only switches it on.
@review @requires:Mlie.NewJoySourcePlayMusic
Feature: F16 write: the fix set is switched on for a game that has real mods

  Scenario: the option is switched on and kept for the next launch
    Given the save "test-colony" is loaded
    And I close all dialogs
    And Joy Rescue: the settings file of this game is saved aside
    When Joy Rescue: the common fix set is switched on
    And Joy Rescue: this game is marked as the one that wrote the settings
    Then Joy Rescue: the common fix set is off in this game
    And no errors were logged
