# F15 and F18 of Tests/TAXONOMY.md: the common recreation fix set, saved, restarted, and what it does to a colony
# that already exists. Played as a chain of three game processes under one hold of the lock, with
# wsl-deps.settings-restart.map:
#   -Filter 29-f18-fixset-write.feature -Then 30-f18-fixset-mid.feature,31-f18-fixset-final.feature
#
#   write  the window with the controls of the fix set is captured, the set is switched on (it applies at the next
#          startup, so nothing has changed yet), and a colony with a known tolerance is saved BEFORE it applies
#   mid    after the restart the base game's telescope is corrected, the tolerance is carried over once, a save
#          and reload twice moves nothing again, a colonist who arrives later is not given a legacy transfer, and
#          the corrected telescope credits the corrected type when it is used; the set is switched off
#   final  after that restart the original type is back, the three types of the set are still there, and no
#          tolerance was taken back
#
# The one rule every game can exercise is the base game's telescope (Ludeon.RimWorld, UseTelescope, from Telescope
# to Reading). Rules for other mods are exercised by 32-f17-guards.feature on witnesses and by
# 33-f16-real-mods.feature with the real mods.
@review @requires:nelim.pickletools.keyedclick
Feature: F15 and F18 write: the fix set is switched on and a colony saved before it applies

  Scenario: the controls are captured, the set is switched on, a colony with a known tolerance is saved
    Given the save "test-colony" is loaded
    And I close all dialogs
    And a colonist "Keeper" exists
    And Joy Rescue: the settings file of this game is saved aside
    And Joy Rescue: "Keeper" has the tolerance 0.40 for the recreation type "Telescope"
    When I open the Joy Rescue settings dialog
    And Joy Rescue: the window is given 5 frames
    And I take a screenshot "joy rescue settings with the fix set controls"
    And Joy Rescue: the common fix set is switched on
    And Joy Rescue: the window is given 5 frames
    And I take a screenshot "joy rescue settings with the fix set on and waiting for a restart"
    Then Joy Rescue: the common fix set is off in this game
    When I close all dialogs
    And Joy Rescue: the game is saved as "joyrescue-f18-a"
    And Joy Rescue: this game is marked as the one that wrote the settings
    Then no errors were logged
