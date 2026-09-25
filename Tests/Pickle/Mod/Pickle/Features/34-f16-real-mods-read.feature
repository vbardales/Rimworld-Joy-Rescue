# F16, second process. See 33-f16-real-mods-write.feature. Carries @joyrescue-restart-last on its last scenario.
#
# What each scenario asserts, and what it does not:
#   - every rule whose activity is in the game is applied or refused with a reason, and the ones that were chosen
#     for this pass are applied: if a mod changed since the rules were selected, this is where it shows, with the
#     reason the set gave
#   - a colonist who uses a corrected activity does it for real, and is credited with the corrected type; the game
#     logs no mismatch between the building's type and the job's
# Whether a rule is right about what a mod's activity is (a piano is high culture) is the choice of the table, not
# something a run can show.
@review @requires:Mlie.NewJoySourcePlayMusic
Feature: F16 read: the corrections the fix set made on real mods, and their use

  Scenario: every rule whose mod is loaded is accounted for, and the chosen ones were applied
    Then Joy Rescue: an earlier game process wrote the settings
    And Joy Rescue: the common fix set is on in this game
    And Joy Rescue: every rule of the fix set whose activity is in this game was applied or skipped with a reason
    And Joy Rescue: the fix set corrected the activity "PlayPiano" to the recreation type "HighCulture"
    And Joy Rescue: the fix set corrected the activity "PlayRimtendoES" to the recreation type "JoyRescue_Kind_taxonomy_video"
    And Joy Rescue: the fix set corrected the activity "UFLI_UseDumbbellrack" to the recreation type "Gaming_Dexterity"
    And Joy Rescue: the fix set corrected the activity "UFLI_UseTelescope" to the recreation type "Reading"
    And Joy Rescue: the fix set corrected the activity "Fox_Book_read" to the recreation type "Reading"
    And Joy Rescue: the fix set corrected the activity "Pushing_a_drumcan" to the recreation type "Gaming_Dexterity"
    And Joy Rescue: the fix set corrected the activity "UseTelescope" to the recreation type "Reading"
    And Joy Rescue: the three types of the fix set exist with their translated names
    And no errors were logged

  @timeout:300
  Scenario: a piano corrected to high culture credits high culture when played
    Given the save "test-colony" is loaded
    And a colonist "Ada" exists
    And a "M4_SPiano" is built at (140, 150)
    And a "Stool" is built at (140, 151)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" takes the activity "PlayPiano" of the building at x=140 z=150
    Then Joy Rescue: "Ada" is credited with the recreation type "HighCulture" and with no other within 120 seconds
    And no errors were logged

  @timeout:300 @joyrescue-restart-last
  Scenario: a dumbbell rack corrected to dexterity credits dexterity when used
    Given the save "test-colony" is loaded
    And a colonist "Ada" exists
    And a "UFLI_Dumbbellrack" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" takes the activity "UFLI_UseDumbbellrack" of the building at x=140 z=150
    Then Joy Rescue: "Ada" is credited with the recreation type "Gaming_Dexterity" and with no other within 120 seconds
    And no errors were logged
