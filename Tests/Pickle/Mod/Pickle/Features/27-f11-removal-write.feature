# F11 of Tests/MANUAL.md, first half: a colony is saved while a colonist is in the middle of a job that Joy Rescue
# generated. Played as the first launch of
#   -Filter 27-f11-removal-write.feature -Then removal-check.feature -ThenWithout nelim.joyrescue,nelim.joyrescue.pickletests
# with wsl-deps.removal.map; the second launch (Tests/Pickle/RemovalCheck) has no Joy Rescue. Its save is then
# loaded WITH the mod by 28-f11-addition-read.feature, which is the "mod added to an existing colony" half.
#
# The tolerance that must come through is on the type Television, which the colonist does not use here: the job
# they are doing credits Gaming_Cerebral, so the other value stays exactly what was set.
@review @joyrescue-sandbox @requires:nelim.joyrescue.removalcheck
Feature: F11 write: a colonist is mid-job on a generated activity when the colony is saved

  @timeout:300
  Scenario: the colonist is doing the generated job and the colony is saved
    Given the save "test-colony" is loaded
    And I close all dialogs
    And a colonist "Keeper" exists
    And a "JoyRescueWitness_Table" is built at (140, 150)
    And Joy Rescue: "Keeper" has the tolerance 0.42 for the recreation type "Television"
    And Joy Rescue: "Keeper" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Keeper" takes the activity of the building at x=140 z=150
    Then Joy Rescue: "Keeper" comes to the "adjacent cell" of the building at x=140 z=150
    And Joy Rescue: "Keeper" is still doing the job "JoyRescue_JoyRescueWitness_Table"
    When Joy Rescue: the game is saved as "joyrescue-f11-a"
    Then no errors were logged
