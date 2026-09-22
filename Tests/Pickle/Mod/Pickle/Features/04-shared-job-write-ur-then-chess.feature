@requires:nelim.joyrescue.sharedwitness
Feature: prepare F14 shared job in Ur then chess order

  Scenario: save the reverse insertion order for a fresh process
    Given Joy Rescue shared-job witnesses are loaded and share their original job
    When Joy Rescue shared-job assignments are saved in "ur-then-chess" order
    Then no errors were logged
