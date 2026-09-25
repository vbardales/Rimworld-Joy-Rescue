# F08, last process. See 15-f08-write.feature. Carries @joyrescue-restart-last: it puts the settings file
# back and deletes both saved games.
@review @requires:nelim.pickletools.keyedclick
Feature: F08 final: the deleted type is gone and no tolerance changed owner

  @joyrescue-restart-last @timeout:300
  Scenario: after the restart nothing points at the deleted type and the surviving type keeps its own tolerance
    When Joy Rescue: the saved game "joyrescue-f08-b" is loaded
    Then Joy Rescue: the recreation type "JoyRescue_Kind_2" does not exist
    And Joy Rescue: there are 1 custom recreation types in the settings
    And Joy Rescue: the custom recreation type number 1 exists in the game as a recreation type
    And Joy Rescue: the building "JoyRescueWitness_Table" is rescued as "SitAdjacent" on the recreation type "JoyRescue_Kind_3"
    And Joy Rescue: the tolerance of "Keeper" for the recreation type "Gaming_Cerebral" is 0.42
    And Joy Rescue: the tolerance of "Keeper" for the recreation type "JoyRescue_Kind_3" is 0.20
    And no errors were logged
