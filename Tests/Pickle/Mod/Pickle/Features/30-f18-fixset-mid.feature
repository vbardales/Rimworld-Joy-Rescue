# F15 and F18, second process. See 29-f18-fixset-write.feature.
@review @requires:nelim.pickletools.keyedclick
Feature: F15 and F18 mid: the telescope is corrected, tolerances move once, and the set is switched off

  Scenario: after the restart the fix set has applied, and a colony that existed before it keeps its tolerances
    When Joy Rescue: the saved game "joyrescue-f18-a" is loaded
    Then Joy Rescue: an earlier game process wrote the settings
    And Joy Rescue: the common fix set is on in this game
    And Joy Rescue: the fix set corrected the activity "UseTelescope" to the recreation type "Reading"
    And Joy Rescue: the fix set corrected at least 1 activities of the mod "Ludeon.RimWorld"
    And Joy Rescue: every rule of the fix set whose activity is in this game was applied or skipped with a reason
    And Joy Rescue: the three types of the fix set exist with their translated names
    And Joy Rescue: the tolerance of "Keeper" for the recreation type "Reading" is 0.40
    And Joy Rescue: the tolerance of "Keeper" for the recreation type "Telescope" is 0.40
    Given Joy Rescue: "Keeper" has the tolerance 0.10 for the recreation type "Reading"
    And Joy Rescue: "Keeper" has the tolerance 0.90 for the recreation type "Telescope"
    When I save and reload
    And I save and reload
    Then Joy Rescue: the tolerance of "Keeper" for the recreation type "Reading" is 0.10
    And Joy Rescue: the tolerance of "Keeper" for the recreation type "Telescope" is 0.90
    Given a colonist "Newcomer" exists
    And Joy Rescue: "Newcomer" has the tolerance 0.70 for the recreation type "Telescope"
    And Joy Rescue: "Newcomer" has the tolerance 0.05 for the recreation type "Reading"
    When I save and reload
    Then Joy Rescue: the tolerance of "Newcomer" for the recreation type "Reading" is 0.05
    And Joy Rescue: the tolerance of "Newcomer" for the recreation type "Telescope" is 0.70
    When Joy Rescue: the game is saved as "joyrescue-f18-b"
    And Joy Rescue: the common fix set is switched off
    And Joy Rescue: this game is marked as the one that wrote the settings
    Then no errors were logged

  @timeout:300
  Scenario: the corrected telescope credits the corrected type when a colonist uses it
    When Joy Rescue: the saved game "joyrescue-f18-a" is loaded
    And a "Telescope" is built at (140, 150)
    And Joy Rescue: "Keeper" is ready for recreation at any hour
    And game speed is ultrafast
    And Joy Rescue: "Keeper" takes the activity of the building at x=140 z=150
    Then Joy Rescue: "Keeper" comes to the "interaction cell" of the building at x=140 z=150
    And Joy Rescue: "Keeper" is credited with the recreation type "Reading" and with no other within 90 seconds
    And no errors were logged
