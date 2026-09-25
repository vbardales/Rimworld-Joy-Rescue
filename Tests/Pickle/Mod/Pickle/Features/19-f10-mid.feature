# F10, second process. See 18-f10-write.feature.
@review @requires:nelim.pickletools.keyedclick
Feature: F10 mid: a cancelled reset changes nothing, a confirmed one restores the defaults

  @timeout:300
  Scenario: the reset is cancelled, then confirmed
    Given the save "test-colony" is loaded
    And I close all dialogs
    Then Joy Rescue: an earlier game process wrote the settings
    And Joy Rescue: the settings hold the non-default values
    And Joy Rescue: the custom recreation type number 1 exists in the game as a recreation type
    And Joy Rescue: the building "JoyRescueWitness_Table" is rescued as "SitAdjacent" on the recreation type "JoyRescue_Kind_1"
    And Joy Rescue: the activity of the building "JoyRescueWitness_Screen" is off
    When I open the Joy Rescue settings dialog
    And Joy Rescue: the window is given 5 frames
    And Nelim's Pickle Tools: I click button keyed "JoyRescue.Settings.Reset"
    And Joy Rescue: the window is given 5 frames
    And I take a screenshot "joy rescue reset confirmation"
    And Nelim's Pickle Tools: I click button keyed "GoBack"
    And Joy Rescue: the window is given 5 frames
    Then Joy Rescue: the settings hold the non-default values
    And Joy Rescue: there are 1 custom recreation types in the settings
    And Joy Rescue: the settings hold 1 building reassignments and 1 activity reassignments
    And Joy Rescue: the activity of the building "JoyRescueWitness_Screen" is off
    When Nelim's Pickle Tools: I click button keyed "JoyRescue.Settings.Reset"
    And Joy Rescue: the window is given 5 frames
    And Nelim's Pickle Tools: I click button keyed "Confirm"
    And Joy Rescue: the window is given 5 frames
    Then Joy Rescue: the settings hold their default values and no reassignment
    And Joy Rescue: there are 0 custom recreation types in the settings
    And Joy Rescue: the activity of the building "JoyRescueWitness_Screen" is on
    And Joy Rescue: the building "JoyRescueWitness_Table" is on the recreation type "JoyRescue_Kind_1"
    When I take a screenshot "joy rescue settings after the reset"
    And I close all dialogs
    And Joy Rescue: this game is marked as the one that wrote the settings
    Then no errors were logged
