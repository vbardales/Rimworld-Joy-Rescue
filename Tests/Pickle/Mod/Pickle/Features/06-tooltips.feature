# The tooltips of the settings window, looked at and asserted with the shared HoverSteps. They are
# named by KEY, so the scenario runs unchanged in English and in French and shows whichever text the
# game resolved: a raw key or a missing translation is what the capture is for.
#
# The tooltip of an activity row is the activity's own name and nothing more (JoyRescue.Settings.
# GiverTip is "{0}", checked offline by Tests/Test-Xml.ps1). It has no key a scenario can name, so it
# is not hovered here.
#
# HoverSteps had not been played by any suite when this one was written: a failure of the first
# scenario that names the reflection it could not bind is a fact about the tool, not about the mod.
@review @joyrescue-sandbox @requires:nelim.pickletools.hoversteps
Feature: the tooltips of the Joy Rescue settings window read as intended

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs
    And I open the Joy Rescue settings dialog

  Scenario: the option that moves other mods' activities explains itself
    When Nelim's Pickle Tools: I hover over the tooltip keyed "JoyRescue.Taxonomy.Tooltip"
    Then Nelim's Pickle Tools: the tooltip keyed "JoyRescue.Taxonomy.Tooltip" is drawn
    When I take a screenshot "joy rescue taxonomy option tooltip"
    Then no errors were logged
    When I close all dialogs

  Scenario: the View button explains what it arranges
    When Nelim's Pickle Tools: I hover over the tooltip keyed "JoyRescue.Settings.ViewModeTip"
    Then Nelim's Pickle Tools: the tooltip keyed "JoyRescue.Settings.ViewModeTip" is drawn
    When I take a screenshot "joy rescue view button tooltip"
    Then no errors were logged
    When I close all dialogs
