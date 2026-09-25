# F11 of Tests/MANUAL.md, the half played WITHOUT the mod: a colony saved while a colonist was in the middle of a
# job that Joy Rescue generated is loaded in a game from which Joy Rescue has been taken out. Played as the
# second launch of
#   -Filter 27-f11-removal-write.feature -Then removal-check.feature -ThenWithout nelim.joyrescue,nelim.joyrescue.pickletests
# with wsl-deps.removal.map. This feature lives in its own test mod because the main one depends on Joy Rescue
# and is taken out with it.
#
# What the manual case expects: the save remains usable, the loss of the generated job is documented, play
# continues. "Usable" is asserted (the colonist is alive, keeps a tolerance, the game runs on), and the loss is
# bounded: every error the load logs concerns the removed mod or the job the colonist was in the middle of. Measured: the
# game cannot run that job and logs about a hundred and fifty errors a second for as long as the colonist is in it, so
# the way out is an order from the player, which is what the scenario does. The colony is then saved with no job of the
# mod in progress, which is what a colony started before the mod was installed looks like. `@allow-errors` lets the
# errors through; the steps say which are allowed.
@review @allow-errors
Feature: F11 removal: a colony saved with Joy Rescue loads without it and plays on

  @timeout:300
  Scenario: the colony loads, plays on, and the colonist stuck in the removed job is freed by an order
    Given Joy Rescue removal: the errors of this launch are being watched for the job of "Keeper"
    Then Joy Rescue removal: the mod "nelim.joyrescue" is not loaded
    When Joy Rescue removal: the saved game "joyrescue-f11-a" is loaded
    Then Joy Rescue removal: "Keeper" is alive and on the map
    And Joy Rescue removal: "Keeper" is not doing a job of the removed mod
    And Joy Rescue removal: the tolerance of "Keeper" for the recreation type "Television" is 0.42
    When Joy Rescue removal: the game runs for 600 ticks
    Then Joy Rescue removal: "Keeper" is alive and on the map
    And Joy Rescue removal: every error logged concerns the removed mod or that job
    When Joy Rescue removal: "Keeper" is drafted and released
    And Joy Rescue removal: the errors logged so far are counted
    And Joy Rescue removal: the game runs for 300 ticks
    Then Joy Rescue removal: no error has been logged since they were counted
    And Joy Rescue removal: every error logged concerns the removed mod or that job
    When Joy Rescue removal: the game is saved as "joyrescue-f11-b"
    And Joy Rescue removal: the saved game "joyrescue-f11-a" is deleted