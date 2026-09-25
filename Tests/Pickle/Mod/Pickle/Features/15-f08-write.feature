# F08 of Tests/MANUAL.md: deleting a type, and what deleting it leaves behind. Played as
#   -Filter 15-f08-write.feature -Then 16-f08-mid.feature,17-f08-final.feature
# with wsl-deps.settings-restart.map: three game processes under one hold of the lock.
#
#   write  a type is created and given a building, then deleted BEFORE it exists (the pending state): nothing
#          may be left pointing at it. Two more types are created (numbers 2 and 3, since numbers are never
#          reused) and each is given something to carry. The colony is saved.
#   mid    the pending types now exist. Type 2 is deleted while it exists, and given a tolerance first, so that
#          the deletion can be seen to move nothing. The colony is saved again.
#   final  the deleted type is gone, nothing points at it, and the tolerance that belonged to the type after it
#          is still its own. The game keeps its tolerances by POSITION in the list of types: a type removed
#          from the middle of the list moves every type after it, and this is where that would show.
@review @requires:nelim.pickletools.keyedclick
Feature: F08 write: a type deleted before it exists, then two more types created

  Scenario: the pending type deleted by its button leaves no reassignment behind, two other types wait
    Given the save "test-colony" is loaded
    And I close all dialogs
    And a colonist "Keeper" exists
    And Joy Rescue: the settings file of this game is saved aside
    And Joy Rescue: "Keeper" has the tolerance 0.42 for the recreation type "Gaming_Cerebral"
    When I open the Joy Rescue settings dialog
    And Nelim's Pickle Tools: I click button keyed "JoyRescue.Settings.AddKind"
    And Joy Rescue: the window is given 5 frames
    Then Joy Rescue: there are 1 custom recreation types in the settings
    When Joy Rescue: the building "JoyRescueWitness_Table" is reassigned to the custom recreation type number 1
    Then Joy Rescue: the settings hold 1 building reassignments and 0 activity reassignments
    When Nelim's Pickle Tools: I click button keyed "JoyRescue.Settings.RemoveKind"
    And Joy Rescue: the window is given 5 frames
    Then Joy Rescue: there are 0 custom recreation types in the settings
    And Joy Rescue: no reassignment in the settings points at the custom recreation type "JoyRescue_Kind_1"
    And Joy Rescue: the settings hold 0 building reassignments and 0 activity reassignments
    When Joy Rescue: a custom recreation type is added
    And Joy Rescue: a custom recreation type is added
    And Joy Rescue: the activity "JoyRescueWitness_CoveredGiver" is reassigned to the custom recreation type number 1
    And Joy Rescue: the building "JoyRescueWitness_Table" is reassigned to the custom recreation type number 2
    And I close all dialogs
    And Joy Rescue: the game is saved as "joyrescue-f08-a"
    And Joy Rescue: this game is marked as the one that wrote the settings
    Then no errors were logged
