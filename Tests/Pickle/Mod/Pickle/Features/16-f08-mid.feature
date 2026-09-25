# F08, second process. See 15-f08-write.feature.
@review @requires:nelim.pickletools.keyedclick
Feature: F08 mid: a type that exists is deleted, and moves nothing

  @timeout:300
  Scenario: the pending types exist after the restart, and the first is deleted
    When Joy Rescue: the saved game "joyrescue-f08-a" is loaded
    Then Joy Rescue: an earlier game process wrote the settings
    And Joy Rescue: there are 2 custom recreation types in the settings
    And Joy Rescue: the custom recreation type number 1 exists in the game as a recreation type
    And Joy Rescue: the custom recreation type number 2 exists in the game as a recreation type
    And Joy Rescue: the activity "JoyRescueWitness_CoveredGiver" serves the recreation type "JoyRescue_Kind_2"
    And Joy Rescue: the building "JoyRescueWitness_Table" is rescued as "SitAdjacent" on the recreation type "JoyRescue_Kind_3"
    And Joy Rescue: the tolerance of "Keeper" for the recreation type "Gaming_Cerebral" is 0.42
    Given Joy Rescue: "Keeper" has the tolerance 0.30 for the recreation type "JoyRescue_Kind_2"
    And Joy Rescue: "Keeper" has the tolerance 0.20 for the recreation type "JoyRescue_Kind_3"
    When Joy Rescue: the custom recreation type number 1 is deleted
    Then Joy Rescue: there are 1 custom recreation types in the settings
    And Joy Rescue: no reassignment in the settings points at the custom recreation type "JoyRescue_Kind_2"
    And Joy Rescue: the settings hold 1 building reassignments and 0 activity reassignments
    When Joy Rescue: the game is saved as "joyrescue-f08-b"
    And Joy Rescue: this game is marked as the one that wrote the settings
    Then no errors were logged
