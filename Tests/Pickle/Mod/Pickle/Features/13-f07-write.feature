# F07 of Tests/MANUAL.md, first half: creating a recreation type and reassigning an activity or a building are
# changes that need a restart, because they are made while the game builds its definitions. Played as
# `-Filter 13-f07-write.feature -Then 14-f07-read.feature` with wsl-deps.settings-restart.map: two game
# processes under one hold of the lock, so nothing replaces the settings file in between.
#
# The colony saved here carries known tolerances, which the reader finds again after the restart: a type
# added to the definitions must not move what a saved colonist is already tired of.
#
# Three reassignments, arranged so that the precedence is a fact and not a hope:
#   the activity JoyRescueWitness_CoveredGiver -> type 1   (it serves the covered table)
#   the covered table itself                   -> type 2   (more specific: it must win, and leave the activity)
#   the orphan table                           -> type 1
@review @requires:nelim.pickletools.keyedclick
Feature: F07 write: types and reassignments wait for the restart

  @timeout:300
  Scenario: two types are created, three reassignments are made and a colony with known tolerances is saved
    Given the save "test-colony" is loaded
    And I close all dialogs
    And a colonist "Keeper" exists
    And Joy Rescue: the settings file of this game is saved aside
    And Joy Rescue: "Keeper" has the tolerance 0.42 for the recreation type "Gaming_Cerebral"
    And Joy Rescue: "Keeper" has the tolerance 0.13 for the recreation type "Television"
    When I open the Joy Rescue settings dialog
    And Nelim's Pickle Tools: I click button keyed "JoyRescue.Settings.AddKind"
    And Joy Rescue: the window is given 5 frames
    Then Joy Rescue: there are 1 custom recreation types in the settings
    And Joy Rescue: the custom recreation type number 1 is still waiting for a restart
    When Joy Rescue: a custom recreation type is added
    And Joy Rescue: the activity "JoyRescueWitness_CoveredGiver" is reassigned to the custom recreation type number 1
    And Joy Rescue: the building "JoyRescueWitness_CoveredTable" is reassigned to the custom recreation type number 2
    And Joy Rescue: the building "JoyRescueWitness_Table" is reassigned to the custom recreation type number 1
    And I take a screenshot "joy rescue settings with pending types"
    And I close all dialogs
    And Joy Rescue: the game is saved as "joyrescue-f07"
    And Joy Rescue: this game is marked as the one that wrote the settings
    Then no errors were logged
