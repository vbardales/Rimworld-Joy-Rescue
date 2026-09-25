# F09, second process. See 21-f09-write.feature. Carries @joyrescue-restart-last.
@review @requires:nelim.pickletools.keyedclick
Feature: F09 read: after the restart the values are kept and the window reads well

  @joyrescue-restart-last @timeout:300
  Scenario: values, types and names are kept, and the window is captured for a person to read
    Given the save "test-colony" is loaded
    And I close all dialogs
    Then Joy Rescue: an earlier game process wrote the settings
    And Joy Rescue: the settings hold the non-default values
    And Joy Rescue: there are 2 custom recreation types in the settings
    And Joy Rescue: the custom recreation type number 1 exists in the game as a recreation type
    And Joy Rescue: the custom recreation type number 2 exists in the game as a recreation type
    And Joy Rescue: the custom recreation type number 1 still has the name the game translates for a new type
    And Joy Rescue: the custom recreation type number 2 is named "Witness renamed"
    When I open the Joy Rescue settings dialog
    And Joy Rescue: the window is given 5 frames
    And I take a screenshot "joy rescue settings after the restart"
    Then Joy Rescue: every text of the mod exists in the language the game runs in
    And no errors were logged
