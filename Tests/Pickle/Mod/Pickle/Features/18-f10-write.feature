# F10 of Tests/MANUAL.md: the reset button. Played as
#   -Filter 18-f10-write.feature -Then 19-f10-mid.feature,20-f10-final.feature
# with wsl-deps.settings-restart.map.
#
#   write  every kind of setting is given a value, and the window is captured
#   mid    after the restart the deferred changes are in effect; the reset is cancelled (nothing changes), then
#          confirmed (the defaults are back at once, the changes that need a restart are still in effect)
#   final  after the restart the deferred changes are gone too
@review @requires:nelim.pickletools.keyedclick
Feature: F10 write: every kind of setting is given a value

  @timeout:300
  Scenario: options, a type, both reassignments, a disabled type and a disabled activity are saved
    Given the save "test-colony" is loaded
    And I close all dialogs
    And Joy Rescue: the settings file of this game is saved aside
    When Joy Rescue: the settings are given non-default values
    And Joy Rescue: a custom recreation type is added
    And Joy Rescue: the building "JoyRescueWitness_Table" is reassigned to the custom recreation type number 1
    And Joy Rescue: the activity "JoyRescueWitness_CoveredGiver" is reassigned to the custom recreation type number 1
    And Joy Rescue: the recreation type "Television" is switched off
    And Joy Rescue: the activity of the building "JoyRescueWitness_Screen" is switched off
    And I open the Joy Rescue settings dialog
    And Joy Rescue: the window is given 5 frames
    And I take a screenshot "joy rescue settings populated before the reset"
    And I close all dialogs
    And Joy Rescue: this game is marked as the one that wrote the settings
    Then no errors were logged
