# F15 and F18, last process. See 29-f18-fixset-write.feature. Carries @joyrescue-restart-last.
@review @requires:nelim.pickletools.keyedclick
Feature: F15 and F18 final: switched off, the original types are back and nothing is taken back

  @joyrescue-restart-last @timeout:300
  Scenario: the original type is back, the three types stay, and no tolerance rolled back
    When Joy Rescue: the saved game "joyrescue-f18-b" is loaded
    Then Joy Rescue: the common fix set is off in this game
    And Joy Rescue: the activity "UseTelescope" serves the recreation type "Telescope"
    And Joy Rescue: the three types of the fix set exist with their translated names
    And Joy Rescue: the tolerance of "Keeper" for the recreation type "Reading" is 0.10
    And Joy Rescue: the tolerance of "Keeper" for the recreation type "Telescope" is 0.90
    And Joy Rescue: the tolerance of "Newcomer" for the recreation type "Reading" is 0.05
    And no errors were logged
