# F06 of Tests/MANUAL.md: switching a whole recreation type off. Three kinds of giver share the type
# Gaming_Cerebral here: a repaired one (the witness table), a vanilla one (the chess table, with a stool
# beside it) and a third-party one (the own-code kiosk). While the type is off none of them may come up in
# the real recreation choice; when it is on again all three do, at the weight they had; and an activity
# the player had switched off on its own stays off through both changes.
@review @joyrescue-sandbox @requires:nelim.joyrescue.owncodewitness
Feature: a disabled recreation type stops every giver of that type and restores them all

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs
    And a colonist "Ada" exists
    And a "JoyRescueWitness_Table" is built at (140, 150)
    And a "ChessTable" is built at (150, 150)
    And a "Stool" is built at (150, 151)
    And a "JoyRescueOwnCode_Kiosk" is built at (130, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour

  Scenario: the type off stops the repaired, the vanilla and the third-party giver, and on restores them
    Then Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_Table" comes up
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "Play_Chess" comes up
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescueOwnCode_Use" comes up
    When Joy Rescue: the recreation type "Gaming_Cerebral" is switched off
    Then Joy Rescue: in 600 draws of the real recreation choice for "Ada" no job of the recreation type "Gaming_Cerebral" comes up
    And Joy Rescue: the job "JoyRescue_JoyRescueWitness_Table" is offered by a giver at no weight
    And Joy Rescue: the job "Play_Chess" is offered by a giver at no weight
    And Joy Rescue: the job "JoyRescueOwnCode_Use" is offered by a giver at no weight
    When Joy Rescue: the recreation type "Gaming_Cerebral" is switched on
    Then Joy Rescue: the job "JoyRescue_JoyRescueWitness_Table" is offered by a giver at its weight
    And Joy Rescue: the job "Play_Chess" is offered by a giver at its weight
    And Joy Rescue: the job "JoyRescueOwnCode_Use" is offered by a giver at its weight
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_Table" comes up
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "Play_Chess" comes up
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescueOwnCode_Use" comes up
    And no errors were logged

  Scenario: an activity switched off on its own stays off when its type goes off and on
    When Joy Rescue: the activity of the building "JoyRescueWitness_Table" is switched off
    And Joy Rescue: the recreation type "Gaming_Cerebral" is switched off
    And Joy Rescue: the recreation type "Gaming_Cerebral" is switched on
    Then Joy Rescue: the activity of the building "JoyRescueWitness_Table" is off
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueWitness_Table" never comes up
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "Play_Chess" comes up
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescueOwnCode_Use" comes up
    And no errors were logged
