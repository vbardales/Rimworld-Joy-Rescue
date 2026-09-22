# F09 and F13 from Tests/MANUAL.md.  These checks use a running game because the offline
# suite cannot show the rendered Dialog_ModSettings or a MainButton worker opening it.
# Dialog_ModSettings pauses ticks, so the local opening steps wait for frames instead.
@review
Feature: Joy Rescue settings and its hidden shortcut work in the running game

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs

  Scenario: Mod options opens the Joy Rescue configuration at its defaults
    When I open the Joy Rescue settings dialog
    Then the Joy Rescue settings dialog is open
    When I take a screenshot "joy rescue settings from mod options"
    Then no errors were logged
    When I close all dialogs

  Scenario: the default MainButtons shortcut is absent rather than greyed
    Then def "JoyRescue_Settings" of type "MainButtonDef" exists
    And Joy Rescue MainButtonDef "JoyRescue_Settings" is hidden and not greyed

  Scenario: activating the shortcut opens the same Joy Rescue settings dialog
    When Joy Rescue activates MainButtonDef "JoyRescue_Settings"
    Then the Joy Rescue settings dialog is open
    When I take a screenshot "joy rescue settings opened by shortcut"
    Then no errors were logged
    When I close all dialogs

  Scenario: a settings change survives closing and reopening the real dialog
    Given Joy Rescue setting "requireChairForWatching" reads "true"
    When Joy Rescue setting "requireChairForWatching" is set to "false"
    And Joy Rescue settings are written
    And I open the Joy Rescue settings dialog
    Then the Joy Rescue settings dialog is open
    And Joy Rescue setting "requireChairForWatching" reads "false"
    When Joy Rescue setting "requireChairForWatching" is set to "true"
    And Joy Rescue settings are written
    And I close all dialogs
