# F04 of Tests/MANUAL.md: what the settings window changes while the game runs. An activity switched off
# is not picked any more and is picked again when switched on, for each of the three modes; a job already
# under way is left to finish; a mode forced by the player takes effect on the next selection.
#
# "Picked" is the base game's own recreation choice, JobGiver_GetJoy, asked many times for the same
# colonist: it weighs every giver, so a weight of zero shows as a job that never comes up, and a healthy
# giver shows as one that does. The control (comes up before, never after, comes up again) is what makes
# the absence mean something.
@review @joyrescue-sandbox @requires:nelim.joyrescue.orphanwitness
Feature: switching an activity off and on, and forcing its mode, applies without a restart

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs

  Scenario: the interaction-cell activity is dropped when off and comes back when on
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_RelayConsole" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    Then Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_RelayConsole" comes up
    When Joy Rescue: the activity of the building "JoyRescueWitness_RelayConsole" is switched off
    Then Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_RelayConsole" never comes up
    And Joy Rescue: the job "JoyRescue_JoyRescueWitness_RelayConsole" is offered by a giver at no weight
    When Joy Rescue: the activity of the building "JoyRescueWitness_RelayConsole" is switched on
    Then Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_RelayConsole" comes up
    And Joy Rescue: the job "JoyRescue_JoyRescueWitness_RelayConsole" is offered by a giver at its weight
    And no errors were logged

  Scenario: the watch activity is dropped when off and comes back when on
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_Screen" is built at (140, 150)
    And Joy Rescue setting "requireChairForWatching" is set to "false"
    And Joy Rescue settings are written
    And Joy Rescue: "Ada" is ready for recreation at any hour
    Then Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_Screen" comes up
    When Joy Rescue: the activity of the building "JoyRescueWitness_Screen" is switched off
    Then Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_Screen" never comes up
    And Joy Rescue: the job "JoyRescue_JoyRescueWitness_Screen" is offered by a giver at no weight
    When Joy Rescue: the activity of the building "JoyRescueWitness_Screen" is switched on
    Then Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_Screen" comes up
    And Joy Rescue: the job "JoyRescue_JoyRescueWitness_Screen" is offered by a giver at its weight
    And no errors were logged

  Scenario: the sit-adjacent activity is dropped when off and comes back when on
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_Table" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    Then Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_Table" comes up
    When Joy Rescue: the activity of the building "JoyRescueWitness_Table" is switched off
    Then Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_Table" never comes up
    And Joy Rescue: the job "JoyRescue_JoyRescueWitness_Table" is offered by a giver at no weight
    When Joy Rescue: the activity of the building "JoyRescueWitness_Table" is switched on
    Then Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_Table" comes up
    And Joy Rescue: the job "JoyRescue_JoyRescueWitness_Table" is offered by a giver at its weight
    And no errors were logged

  @timeout:300
  Scenario: a job already under way finishes when its activity is switched off
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_RelayConsole" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" takes the activity of the building at x=140 z=150
    And Joy Rescue: the activity of the building "JoyRescueWitness_RelayConsole" is switched off
    Then Joy Rescue: "Ada" is still doing the job "JoyRescue_JoyRescueWitness_RelayConsole"
    And Joy Rescue: "Ada" comes to the "interaction cell" of the building at x=140 z=150
    And Joy Rescue: "Ada" is credited with the recreation type "Gaming_Dexterity" and with no other within 90 seconds
    And no errors were logged

  @timeout:300
  Scenario: a mode forced by the player is the one used from the next selection, and Auto gives it back
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_RelayConsole" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: the mode of the building "JoyRescueWitness_RelayConsole" is forced to "SitAdjacent"
    Then Joy Rescue: the building "JoyRescueWitness_RelayConsole" is rescued as "SitAdjacent" on the recreation type "Gaming_Dexterity"
    When Joy Rescue: "Ada" takes the activity of the building at x=140 z=150
    Then Joy Rescue: "Ada" comes to the "adjacent cell" of the building at x=140 z=150
    And Joy Rescue: "Ada" is credited with the recreation type "Gaming_Dexterity" and with no other within 90 seconds
    When Joy Rescue: the mode of the building "JoyRescueWitness_RelayConsole" is forced to "Auto"
    Then Joy Rescue: the building "JoyRescueWitness_RelayConsole" is rescued as "InteractionCell" on the recreation type "Gaming_Dexterity"
    And no errors were logged

  @timeout:300
  Scenario: a table forced to watch mode is watched from the cells around it
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_Table" is built at (140, 150)
    And Joy Rescue setting "requireChairForWatching" is set to "false"
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: the mode of the building "JoyRescueWitness_Table" is forced to "Watch"
    Then Joy Rescue: the building "JoyRescueWitness_Table" is rescued as "Watch" on the recreation type "Gaming_Cerebral"
    When Joy Rescue: "Ada" takes the activity of the building at x=140 z=150
    Then Joy Rescue: "Ada" comes to the "watch cell" of the building at x=140 z=150
    And Joy Rescue: "Ada" is credited with the recreation type "Gaming_Cerebral" and with no other within 90 seconds
    And no errors were logged
