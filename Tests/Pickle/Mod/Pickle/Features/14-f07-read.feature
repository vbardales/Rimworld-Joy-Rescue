# F07, second half: after the restart. See 13-f07-write.feature for the arrangement. The last scenario carries
# @joyrescue-restart-last: it puts the settings file back and deletes the saved game.
@review @requires:nelim.pickletools.keyedclick
Feature: F07 read: after the restart the types exist, the reassignments apply and nothing else moved

  Scenario: the types exist and the reassignments are in effect, the more specific one winning
    Then Joy Rescue: an earlier game process wrote the settings
    And Joy Rescue: there are 2 custom recreation types in the settings
    And Joy Rescue: the custom recreation type number 1 exists in the game as a recreation type
    And Joy Rescue: the custom recreation type number 2 exists in the game as a recreation type
    And Joy Rescue: the activity "JoyRescueWitness_CoveredGiver" serves the recreation type "JoyRescue_Kind_1"
    And Joy Rescue: the building "JoyRescueWitness_CoveredTable" is rescued as "SitAdjacent" on the recreation type "JoyRescue_Kind_2"
    And Joy Rescue: the building "JoyRescueWitness_Table" is rescued as "SitAdjacent" on the recreation type "JoyRescue_Kind_1"
    And no errors were logged

  @timeout:300
  Scenario: a colony saved before the types existed keeps its tolerances and the new types start at zero
    When Joy Rescue: the saved game "joyrescue-f07" is loaded
    Then Joy Rescue: the tolerance of "Keeper" for the recreation type "Gaming_Cerebral" is 0.42
    And Joy Rescue: the tolerance of "Keeper" for the recreation type "Television" is 0.13
    And Joy Rescue: the tolerance of "Keeper" for the recreation type "JoyRescue_Kind_1" is 0
    And Joy Rescue: the tolerance of "Keeper" for the recreation type "JoyRescue_Kind_2" is 0
    And no errors were logged

  @timeout:300
  Scenario: the orphan table reassigned to type 1 credits type 1 when used
    When Joy Rescue: the saved game "joyrescue-f07" is loaded
    And a "JoyRescueWitness_Table" is built at (140, 150)
    And Joy Rescue: "Keeper" is ready for recreation at any hour
    And game speed is ultrafast
    And Joy Rescue: "Keeper" takes the activity of the building at x=140 z=150
    Then Joy Rescue: "Keeper" comes to the "adjacent cell" of the building at x=140 z=150
    And Joy Rescue: "Keeper" is credited with the recreation type "JoyRescue_Kind_1" and with no other within 90 seconds
    And no errors were logged

  @timeout:300 @joyrescue-restart-last
  Scenario: the covered table reassigned by itself credits type 2, and the activity it left credits nothing
    When Joy Rescue: the saved game "joyrescue-f07" is loaded
    And a "JoyRescueWitness_CoveredTable" is built at (140, 150)
    And Joy Rescue: "Keeper" is ready for recreation at any hour
    And game speed is ultrafast
    And Joy Rescue: "Keeper" takes the activity of the building at x=140 z=150
    Then Joy Rescue: "Keeper" comes to the "adjacent cell" of the building at x=140 z=150
    And Joy Rescue: "Keeper" is credited with the recreation type "JoyRescue_Kind_2" and with no other within 90 seconds
    And no errors were logged
