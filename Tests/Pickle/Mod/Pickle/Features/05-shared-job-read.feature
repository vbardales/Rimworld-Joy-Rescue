@joyrescue-shared-reader @requires:nelim.joyrescue.sharedwitness
Feature: F14 shared jobs are isolated after a real restart

  Scenario: the loaded definitions preserve both selected recreation kinds
    Then Joy Rescue shared-job witnesses have independent assigned jobs after restart
    And no errors were logged
