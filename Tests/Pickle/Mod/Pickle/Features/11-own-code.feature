# F05 of Tests/MANUAL.md: a building of a mod that serves it from code. The witness (Tests/Pickle/
# OwnCodeWitness) ships a JoyGiver class and a giver that lists no thingDefs, so seen from XML alone its
# kiosk is an orphan. Joy Rescue must leave it off by default, do what the player says when they enable
# it, and follow the global option only where the player has not decided.
#
# The last scenario is the observation the manual case asks for, "whether duplicate activities compete":
# with the rescue on, the building is served by the mod's own giver and by the generated one, and both
# come up in the real recreation choice. That is the reason the default is off, and it is recorded here as
# a fact, not asserted to be a defect.
@review @joyrescue-sandbox @requires:nelim.joyrescue.owncodewitness
Feature: a mod that ships its own recreation code is not rescued unless the player decides so

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs

  Scenario: by default only the mod's own activity is offered
    Given a colonist "Ada" exists
    And a "JoyRescueOwnCode_Kiosk" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    Then Joy Rescue: the activity of the building "JoyRescueOwnCode_Kiosk" is off
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescueOwnCode_Use" comes up
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueOwnCode_Kiosk" never comes up
    And no errors were logged

  Scenario: enabling the rescue of that building explicitly is honoured, and both activities compete
    Given a colonist "Ada" exists
    And a "JoyRescueOwnCode_Kiosk" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    When Joy Rescue: the activity of the building "JoyRescueOwnCode_Kiosk" is switched on
    Then Joy Rescue: the activity of the building "JoyRescueOwnCode_Kiosk" is on
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueOwnCode_Kiosk" comes up
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescueOwnCode_Use" comes up
    And no errors were logged

  Scenario: the global option turns the rescue on for every such building, and an explicit decision beats it
    Given a colonist "Ada" exists
    And a "JoyRescueOwnCode_Kiosk" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    When Joy Rescue setting "rescueModsWithOwnCode" is set to "true"
    And Joy Rescue settings are written
    Then Joy Rescue: the activity of the building "JoyRescueOwnCode_Kiosk" is on
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueOwnCode_Kiosk" comes up
    When Joy Rescue: the activity of the building "JoyRescueOwnCode_Kiosk" is switched off
    Then Joy Rescue: the activity of the building "JoyRescueOwnCode_Kiosk" is off
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescue_JoyRescueOwnCode_Kiosk" never comes up
    And Joy Rescue: in 600 draws of the real recreation choice for "Ada" the job "JoyRescueOwnCode_Use" comes up
    And no errors were logged
