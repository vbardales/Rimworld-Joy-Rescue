# F10, last process. See 18-f10-write.feature. Carries @joyrescue-restart-last.
@review @requires:nelim.pickletools.keyedclick
Feature: F10 final: after the restart the changes that needed one are gone too

  @joyrescue-restart-last
  Scenario: the defaults hold and the reassignments no longer apply
    Then Joy Rescue: an earlier game process wrote the settings
    And Joy Rescue: the settings hold their default values and no reassignment
    And Joy Rescue: there are 0 custom recreation types in the settings
    And Joy Rescue: the recreation type "JoyRescue_Kind_1" does not exist
    And Joy Rescue: the building "JoyRescueWitness_Table" is rescued as "SitAdjacent" on the recreation type "Gaming_Cerebral"
    And Joy Rescue: the activity "JoyRescueWitness_CoveredGiver" serves the recreation type "Gaming_Cerebral"
    And Joy Rescue: the activity of the building "JoyRescueWitness_Screen" is on
    And no errors were logged
