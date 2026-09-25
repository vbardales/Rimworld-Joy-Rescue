# F09 of Tests/MANUAL.md: settings survive a restart and read well, in the language the game runs in. Played as
#   -Filter 21-f09-write.feature -Then 22-f09-read.feature
# with wsl-deps.settings-restart.map, once in English and once in French (two tickets): a language is never
# switched during a run, so each language is its own pair of processes.
#
# The initial name of a type is the translation of a key, resolved once when the type is created and kept as
# text afterwards. So a type nobody renamed must still carry the name the game translates in the language it
# was created in, and a type that was renamed must carry exactly what was typed.
#
# No step spells a translated word: the suite runs unchanged in English and in French.
@review @requires:nelim.pickletools.keyedclick
Feature: F09 write: non-default values and two types are saved with the button

  Scenario: values are set, two types are created and one is renamed, and the Save button writes them
    Given the save "test-colony" is loaded
    And I close all dialogs
    And Joy Rescue: the settings file of this game is saved aside
    When Joy Rescue: the settings are given non-default values
    And I open the Joy Rescue settings dialog
    And Joy Rescue: the window is given 5 frames
    And Nelim's Pickle Tools: I click button keyed "JoyRescue.Settings.AddKind"
    And Joy Rescue: the window is given 5 frames
    And Joy Rescue: a custom recreation type is added
    And Joy Rescue: the custom recreation type number 2 is renamed "Witness renamed"
    And Nelim's Pickle Tools: I click button keyed "JoyRescue.Settings.SaveNow"
    And Joy Rescue: the window is given 5 frames
    And I take a screenshot "joy rescue settings written by the button"
    Then Joy Rescue: there are 2 custom recreation types in the settings
    And Joy Rescue: the custom recreation type number 1 still has the name the game translates for a new type
    And Joy Rescue: the custom recreation type number 2 is named "Witness renamed"
    When I close all dialogs
    And Joy Rescue: this game is marked as the one that wrote the settings
    Then no errors were logged
