# F17 of Tests/TAXONOMY.md: the fix set refuses what it cannot correct safely, and a choice made by hand wins.
# Played with wsl-deps.rules.map, in one game process.
#
# The rules of the fix set name real mods and cannot name a witness, so the witnesses get rules of their own,
# handed to the same CommonTaxonomy.Apply that startup calls. What is under test is the guards, on definitions
# loaded from XML and, for the last case, from a mod's own assembly. Every activity is on Gaming_Cerebral with the
# audited vanilla giver and driver, so what differs is what a guard looks at.
@review @joyrescue-sandbox @requires:nelim.joyrescue.rulewitness
Feature: the fix set corrects the clean case and refuses each unsafe one, with a reason

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs

  Scenario: each guard, on a witness
    Given Joy Rescue setting "commonTaxonomy" is set to "true"
    And Joy Rescue: the activity "JoyRescueRule_ManualGiver" is assigned by hand to the recreation type "Social"
    When Joy Rescue: the fix set is applied with the rules of the witnesses
    Then Joy Rescue: the fix set corrected the activity "JoyRescueRule_CleanGiver" to the recreation type "Reading"
    And Joy Rescue: the fix set corrected the activity "JoyRescueRule_AgreeGiverA" to the recreation type "Reading"
    And Joy Rescue: the fix set corrected the activity "JoyRescueRule_AgreeGiverB" to the recreation type "Reading"
    And Joy Rescue: the fix set skipped the activity "JoyRescueRule_ShareGiverA" because of "shared job or equipment"
    And Joy Rescue: the fix set left the activity "JoyRescueRule_ShareGiverA" on the recreation type "Gaming_Cerebral"
    And Joy Rescue: the fix set left the activity "JoyRescueRule_ShareGiverB" on the recreation type "Gaming_Cerebral"
    And Joy Rescue: the fix set skipped the activity "JoyRescueRule_ManualGiver" because of "explicit player assignment"
    And Joy Rescue: the fix set left the activity "JoyRescueRule_ManualGiver" on the recreation type "Gaming_Cerebral"
    And Joy Rescue: the fix set skipped the activity "JoyRescueRule_StaleJobGiver" because of "job changed"
    And Joy Rescue: the fix set left the activity "JoyRescueRule_StaleJobGiver" on the recreation type "Gaming_Cerebral"
    And Joy Rescue: the fix set skipped the activity "JoyRescueRule_StaleSetGiver" because of "equipment set changed"
    And Joy Rescue: the fix set left the activity "JoyRescueRule_StaleSetGiver" on the recreation type "Gaming_Cerebral"
    And Joy Rescue: the fix set skipped the activity "JoyRescueOwnCode_Giver" because of "custom or unsupported driver"
    And Joy Rescue: the fix set left the activity "JoyRescueOwnCode_Giver" on the recreation type "Gaming_Cerebral"
    And no errors were logged
