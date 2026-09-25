@joyrescue-shared-reader @requires:nelim.joyrescue.sharedwitness
Feature: F14 shared jobs are isolated after a real restart

  Scenario: the loaded definitions preserve both selected recreation kinds
    Then Joy Rescue shared-job witnesses have independent assigned jobs after restart
    And no errors were logged

  # The half the definitions cannot show: a colonist who uses each activity is credited with the kind that was
  # assigned to it, and with no other, and the game logs no mismatch between a building and its job. The two
  # activities share the base game's chess job in the game files, and here each has its own after the restart.
  @timeout:300
  Scenario: a colonist playing chess is credited with the kind assigned to chess
    Given the save "test-colony" is loaded
    And a colonist "Ada" exists
    And a "ChessTable" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" takes the activity "JoyRescueWitness_PlayChess" of the building at x=140 z=150
    Then Joy Rescue: "Ada" comes to the "adjacent cell" of the building at x=140 z=150
    And Joy Rescue: "Ada" is credited with the recreation type "Social" and with no other within 90 seconds
    And no errors were logged

  @timeout:300
  Scenario: a colonist playing the game of Ur is credited with the kind assigned to it
    Given the save "test-colony" is loaded
    And a colonist "Ada" exists
    And a "GameOfUrBoard" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" takes the activity "JoyRescueWitness_PlayUr" of the building at x=140 z=150
    Then Joy Rescue: "Ada" comes to the "adjacent cell" of the building at x=140 z=150
    And Joy Rescue: "Ada" is credited with the recreation type "Gaming_Dexterity" and with no other within 90 seconds
    And no errors were logged
