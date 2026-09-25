# F13 and F19, last launch. See 24-f13-rimmsqol-write.feature.
@review @joyrescue-sandbox @rimmsqol @requires:MalteSchulze.RIMMSqol @requires:nelim.pickletools.rimmsqol
Feature: F13 last: the hide survives the restart and forgetting leaves RIMMSQOL with no choice

  Scenario: hidden as chosen, then forgotten
    Given the save "test-colony" is loaded
    And I close all dialogs
    Then the choices RIMMSQOL kept in the previous launch are in place
    And RIMMSQOL shows the main button "JoyRescue_Settings" as hidden
    And the main bar does not draw the button "JoyRescue_Settings"
    And RIMMSQOL's settings file records the main button "JoyRescue_Settings" as hidden
    When RIMMSQOL forgets its choice for the main button "JoyRescue_Settings"
    Then RIMMSQOL holds no choice for the main button "JoyRescue_Settings"
    And RIMMSQOL's settings file records no choice for the main button "JoyRescue_Settings"
    And Joy Rescue MainButtonDef "JoyRescue_Settings" is hidden and not greyed
