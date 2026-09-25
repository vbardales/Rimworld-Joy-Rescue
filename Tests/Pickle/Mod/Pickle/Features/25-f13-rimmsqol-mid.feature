# F13 and F19, second launch. See 24-f13-rimmsqol-write.feature.
@review @joyrescue-sandbox @rimmsqol @requires:MalteSchulze.RIMMSqol @requires:nelim.pickletools.rimmsqol
Feature: F13 mid: the reveal survives the restart, both routes open the same settings, then it is hidden

  Scenario: still revealed, the shortcut opens the settings, an edit is kept on both routes, then it is hidden
    Given the save "test-colony" is loaded
    And I close all dialogs
    Then the choices RIMMSQOL kept in the previous launch are in place
    And RIMMSQOL shows the main button "JoyRescue_Settings" as visible
    And the main bar draws the button "JoyRescue_Settings"
    When the main bar's button "JoyRescue_Settings" is activated
    Then the Joy Rescue settings dialog is open
    When Joy Rescue setting "requireChairForWatching" is set to "false"
    And I close all dialogs
    And I open the Joy Rescue settings dialog
    Then the Joy Rescue settings dialog is open
    And Joy Rescue setting "requireChairForWatching" reads "false"
    When I close all dialogs
    And RIMMSQOL hides the main button "JoyRescue_Settings"
    Then the main bar does not draw the button "JoyRescue_Settings"
    And RIMMSQOL's choices are kept for the next launch
