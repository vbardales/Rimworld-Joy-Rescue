@requires:nelim.joyrescue.sharedwitness
Feature: prepare F14 shared job in chess then Ur order

  Scenario: save the first insertion order for a fresh process
    Given Joy Rescue shared-job witnesses are loaded and share their original job
    When Joy Rescue shared-job assignments are saved in "chess-then-ur" order
    Then no errors were logged
