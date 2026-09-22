# F13's optional integration.  The shared companion drives RIMMSQOL's real settings store;
# it is staged only by wsl-deps.avec-rimmsqol.map and leaves no visibility choice behind.
@review @rimmsqol @requires:MalteSchulze.RIMMSqol @requires:nelim.pickletools.rimmsqol
Feature: RIMMSQOL reveals and hides the Joy Rescue shortcut

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs
    Then mod "MalteSchulze.RIMMSqol" is loaded
    And RIMMSQOL is ready to be driven

  Scenario: RIMMSQOL lists the hidden Joy Rescue shortcut
    Then RIMMSQOL's own list of main buttons offers "JoyRescue_Settings"
    And RIMMSQOL shows the main button "JoyRescue_Settings" as hidden
    And RIMMSQOL holds no choice for the main button "JoyRescue_Settings"
    And the main bar does not draw the button "JoyRescue_Settings"
    When RIMMSQOL's own window is opened on its list of main buttons
    And I take a screenshot "rimmsqol list with joy rescue shortcut"
    And I close all dialogs

  Scenario: RIMMSQOL reveals the shortcut and its activation opens Joy Rescue settings
    When RIMMSQOL reveals the main button "JoyRescue_Settings"
    Then RIMMSQOL's settings file records the main button "JoyRescue_Settings" as visible
    And the main bar draws the button "JoyRescue_Settings"
    When the main bar's button "JoyRescue_Settings" is activated
    Then the Joy Rescue settings dialog is open
    When I take a screenshot "joy rescue settings opened by rimmsqol shortcut"
    And I close all dialogs

  Scenario: hiding then forgetting leaves the normal hidden state intact
    Given RIMMSQOL reveals the main button "JoyRescue_Settings"
    When RIMMSQOL hides the main button "JoyRescue_Settings"
    Then the main bar does not draw the button "JoyRescue_Settings"
    And RIMMSQOL's settings file records the main button "JoyRescue_Settings" as hidden
    When RIMMSQOL forgets its choice for the main button "JoyRescue_Settings"
    Then RIMMSQOL holds no choice for the main button "JoyRescue_Settings"
    And RIMMSQOL's settings file records no choice for the main button "JoyRescue_Settings"
