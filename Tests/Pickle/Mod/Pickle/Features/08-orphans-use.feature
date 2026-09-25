# F02 of Tests/MANUAL.md: a colonist who needs recreation is given each rescued activity and does it.
#
# The activity is handed out by the giver Joy Rescue built, through the same Worker.TryGiveJob the base game
# calls, and started as a job. What is asserted is what happened: the job is the generated one, the driver
# is the mode's, the colonist walks to the cell the mode calls for, and the game then credits the
# recreation type the building declares and no other. `no errors were logged` covers the game's own check
# that the building's type and the job's agree, which it makes on every tick of the job.
@review @joyrescue-sandbox @requires:nelim.joyrescue.orphanwitness
Feature: a colonist uses each rescued building the way its mode says

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs

  @timeout:300
  Scenario: an orphan with an interaction cell is used from that cell
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_RelayConsole" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" takes the activity of the building at x=140 z=150
    Then Joy Rescue: "Ada" comes to the "interaction cell" of the building at x=140 z=150
    And Joy Rescue: "Ada" is credited with the recreation type "Gaming_Dexterity" and with no other within 90 seconds
    And no errors were logged

  @timeout:300
  Scenario: a television-type orphan is watched from one of its watch cells
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_Screen" is built at (140, 150)
    And Joy Rescue setting "requireChairForWatching" is set to "false"
    And Joy Rescue settings are written
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" takes the activity of the building at x=140 z=150
    Then Joy Rescue: "Ada" comes to the "watch cell" of the building at x=140 z=150
    And Joy Rescue: "Ada" is credited with the recreation type "Television" and with no other within 90 seconds
    And no errors were logged

  @timeout:300
  Scenario: a game-table orphan is played from a cell next to it
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_Table" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" takes the activity of the building at x=140 z=150
    Then Joy Rescue: "Ada" comes to the "adjacent cell" of the building at x=140 z=150
    And Joy Rescue: "Ada" is credited with the recreation type "Gaming_Cerebral" and with no other within 90 seconds
    And no errors were logged
