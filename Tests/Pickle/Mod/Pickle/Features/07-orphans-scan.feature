# F01 of Tests/MANUAL.md: what the scan finds. Played with wsl-deps.orphans.map, which stages the two
# witness mods (Tests/Pickle/OrphanWitness, Tests/Pickle/OwnCodeWitness).
#
# The witnesses are named in the map and defined in XML: three orphans, one for each way of reaching a
# building; a vanilla building a giver already serves; and a building of a mod that ships JoyGiver code
# of its own. No step spells a translated word: the suite runs unchanged in English and in French.
@review @joyrescue-sandbox @requires:nelim.joyrescue.orphanwitness
Feature: the scan tells an orphan from a served building

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs

  Scenario: each orphan is rescued in the mode its shape calls for, on the type it declares
    Then Joy Rescue: the building "JoyRescueWitness_RelayConsole" is rescued as "InteractionCell" on the recreation type "Gaming_Dexterity"
    And Joy Rescue: the building "JoyRescueWitness_Screen" is rescued as "Watch" on the recreation type "Television"
    And Joy Rescue: the building "JoyRescueWitness_Table" is rescued as "SitAdjacent" on the recreation type "Gaming_Cerebral"
    And Joy Rescue: the activity of the building "JoyRescueWitness_RelayConsole" is on
    And Joy Rescue: the activity of the building "JoyRescueWitness_Screen" is on
    And Joy Rescue: the activity of the building "JoyRescueWitness_Table" is on
    And Joy Rescue: the building "JoyRescueWitness_RelayConsole" comes from a mod without recreation code of its own
    And no errors were logged

  Scenario: a building that a giver already serves is left alone
    Then Joy Rescue: the building "ChessTable" is already served and is not rescued
    And Joy Rescue: the building "TubeTelevision" is already served and is not rescued
    And no errors were logged

  Scenario: a mod that ships its own recreation code is reported and left off
    Then Joy Rescue: the building "JoyRescueOwnCode_Kiosk" comes from a mod that ships its own recreation code
    And Joy Rescue: the building "JoyRescueOwnCode_Kiosk" is rescued as "InteractionCell" on the recreation type "Gaming_Cerebral"
    And Joy Rescue: the activity of the building "JoyRescueOwnCode_Kiosk" is off
    And no errors were logged

  Scenario: the counters and the report agree, and the window shows the inventory
    Then Joy Rescue: the scan counters agree with the entries and the report
    When I open the Joy Rescue settings dialog
    Then the Joy Rescue settings dialog is open
    When I take a screenshot "joy rescue settings with the witnesses"
    Then no errors were logged
    When I close all dialogs
