# F13 and F19 of Tests/MANUAL.md and Tests/TAXONOMY.md, the restart half: a choice RIMMSQOL makes about the Joy
# Rescue shortcut survives a restart, both ways, and neither the mod nor RIMMSQOL is needed for the primary
# route. Played as a chain of three launches under one hold of the lock, with wsl-deps.avec-rimmsqol.map:
#   -Filter 24-f13-rimmsqol-write.feature -Then 25-f13-rimmsqol-mid.feature,26-f13-rimmsqol-last.feature
#
#   write  the shortcut is hidden by default (neither drawn nor greyed), RIMMSQOL reveals it, the choice is kept
#   mid    after the restart it is still revealed with nothing said in this process; both routes open the same
#          settings and keep an edit; RIMMSQOL hides it again, the choice is kept
#   last   after the restart it is hidden as chosen; forgetting leaves RIMMSQOL with no choice at all
#
# The restart hand-off is the shared RimmsqolSteps one: a scenario that fails before its last step keeps
# nothing, and the teardown of the shared steps puts RIMMSQOL back.
@review @joyrescue-sandbox @rimmsqol @requires:MalteSchulze.RIMMSqol @requires:nelim.pickletools.rimmsqol
Feature: F13 write: RIMMSQOL reveals the Joy Rescue shortcut and the choice is kept

  Scenario: hidden by default, revealed by RIMMSQOL, kept for the next launch
    Given the save "test-colony" is loaded
    And I close all dialogs
    Then mod "MalteSchulze.RIMMSqol" is loaded
    And RIMMSQOL is ready to be driven
    And Joy Rescue MainButtonDef "JoyRescue_Settings" is hidden and not greyed
    And RIMMSQOL holds no choice for the main button "JoyRescue_Settings"
    And the main bar does not draw the button "JoyRescue_Settings"
    When RIMMSQOL reveals the main button "JoyRescue_Settings"
    Then the main bar draws the button "JoyRescue_Settings"
    And RIMMSQOL's settings file records the main button "JoyRescue_Settings" as visible
    And RIMMSQOL's choices are kept for the next launch
