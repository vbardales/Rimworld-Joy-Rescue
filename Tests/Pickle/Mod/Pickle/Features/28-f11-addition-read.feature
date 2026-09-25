# F11, the other half: the colony that came back from the launch WITHOUT Joy Rescue (27-f11-removal-write.feature
# then Tests/Pickle/RemovalCheck) is loaded with the mod on. It was saved without a single Joy Rescue definition,
# which is what a colony started before the mod was installed looks like. Played on its own, after that chain,
# with wsl-deps.removal.map: it needs the saved game "joyrescue-f11-b" that the chain leaves, and says so if it
# is not there.
@review @joyrescue-sandbox @requires:nelim.joyrescue.orphanwitness
Feature: F11 addition: a colony saved without Joy Rescue loads with it and uses its activities

  @timeout:300 @joyrescue-restart-last
  Scenario: the colony loads, keeps its tolerance and uses the rescued table
    When Joy Rescue: the saved game "joyrescue-f11-b" is loaded
    Then Joy Rescue: the tolerance of "Keeper" for the recreation type "Television" is 0.42
    And Joy Rescue: the building "JoyRescueWitness_Table" is rescued as "SitAdjacent" on the recreation type "Gaming_Cerebral"
    When Joy Rescue: "Keeper" is ready for recreation at any hour
    And game speed is ultrafast
    And Joy Rescue: "Keeper" takes the activity of the building at x=140 z=150
    Then Joy Rescue: "Keeper" comes to the "adjacent cell" of the building at x=140 z=150
    And Joy Rescue: "Keeper" is credited with the recreation type "Gaming_Cerebral" and with no other within 90 seconds
    And no errors were logged
