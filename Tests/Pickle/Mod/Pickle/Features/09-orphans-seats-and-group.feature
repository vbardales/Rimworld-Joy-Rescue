# F03 of Tests/MANUAL.md: seats, beds and a group, on the television-type witness.
#
# The witness screen has four watch cells, one in each direction, two cells from it: (x, z+2), (x+2, z),
# (x, z-2), (x-2, z). That makes the seat and the group cases arrangeable facts. The screen stands at
# (140, 150), so the cell north of it is (140, 152).
#
# Not covered here, and why: "a group mod if installed". No mod that reserves recreation buildings for
# a group is part of the test set, and a stand-in written for the test would show only that the stand-in
# agrees with itself. The reservation the game itself makes for a group, up to the job's own limit and
# never beyond the cells there are, is what the last scenario checks.
@review @joyrescue-sandbox @requires:nelim.joyrescue.orphanwitness
Feature: watching wants a seat when the setting says so, and never over-reserves

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs

  Scenario: with a seat required and no seat, nothing is offered until one is built
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_Screen" is built at (140, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    Then Joy Rescue setting "requireChairForWatching" reads "true"
    And Joy Rescue: "Ada" is offered no activity by the building at x=140 z=150
    Given a "Stool" is built at (140, 152)
    Then Joy Rescue: "Ada" is offered the activity of the building at x=140 z=150
    And no errors were logged

  @timeout:300
  Scenario: with a seat required and one built, the colonist is sent to it
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_Screen" is built at (140, 150)
    And a "Stool" is built at (140, 152)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" takes the activity of the building at x=140 z=150
    Then Joy Rescue: the job of "Ada" was given the cell x=140 z=152
    And Joy Rescue: the job of "Ada" was given a seat
    And Joy Rescue: "Ada" comes to the "watch cell" of the building at x=140 z=150
    And Joy Rescue: "Ada" is credited with the recreation type "Television" and with no other within 90 seconds
    And no errors were logged

  @timeout:300
  Scenario: with no seat required, the colonist watches standing up
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_Screen" is built at (140, 150)
    And Joy Rescue setting "requireChairForWatching" is set to "false"
    And Joy Rescue settings are written
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" takes the activity of the building at x=140 z=150
    Then Joy Rescue: the job of "Ada" was given no seat
    And Joy Rescue: "Ada" comes to the "watch cell" of the building at x=140 z=150
    And no errors were logged

  Scenario: the seat is taken away, then the requirement, and the offer follows each change at once
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_Screen" is built at (140, 150)
    And a "Stool" is built at (140, 152)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    Then Joy Rescue: "Ada" is offered the activity of the building at x=140 z=150
    When Joy Rescue: the building at x=140 z=152 is removed
    Then Joy Rescue: "Ada" is offered no activity by the building at x=140 z=150
    When Joy Rescue setting "requireChairForWatching" is set to "false"
    And Joy Rescue settings are written
    Then Joy Rescue: "Ada" is offered the activity of the building at x=140 z=150
    And no errors were logged

  @timeout:300
  Scenario: a colonist lying in a bed inside the watch cells watches from it
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_Screen" is built at (140, 150)
    And a "Bed" is built at (140, 151)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" lies down in the bed at x=140 z=151
    Then Joy Rescue: "Ada" in bed is offered the activity of the building at x=140 z=150, in the bed
    And no errors were logged

  @timeout:300
  Scenario: a colonist lying in a bed outside the watch cells is offered nothing
    Given a colonist "Ada" exists
    And a "JoyRescueWitness_Screen" is built at (140, 150)
    And a "Bed" is built at (150, 150)
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" lies down in the bed at x=150 z=150
    Then Joy Rescue: "Ada" in bed is offered no activity by the building at x=140 z=150
    And no errors were logged

  @timeout:300
  Scenario: four colonists fill the four watch cells and a fifth is not sent anywhere
    Given a colonist "Ada" exists
    And a colonist "Bo" exists
    And a colonist "Cy" exists
    And a colonist "Di" exists
    And a colonist "Eve" exists
    And a "JoyRescueWitness_Screen" is built at (140, 150)
    And Joy Rescue setting "requireChairForWatching" is set to "false"
    And Joy Rescue settings are written
    And Joy Rescue: "Ada" is ready for recreation at any hour
    And Joy Rescue: "Bo" is ready for recreation at any hour
    And Joy Rescue: "Cy" is ready for recreation at any hour
    And Joy Rescue: "Di" is ready for recreation at any hour
    And Joy Rescue: "Eve" is ready for recreation at any hour
    And game speed is ultrafast
    When Joy Rescue: "Ada" takes the activity of the building at x=140 z=150
    And Joy Rescue: "Bo" takes the activity of the building at x=140 z=150
    And Joy Rescue: "Cy" takes the activity of the building at x=140 z=150
    And Joy Rescue: "Di" takes the activity of the building at x=140 z=150
    Then Joy Rescue: "Eve" is offered no activity by the building at x=140 z=150
    And Joy Rescue: "Ada" comes to the "watch cell" of the building at x=140 z=150
    And Joy Rescue: "Di" comes to the "watch cell" of the building at x=140 z=150
    And no errors were logged
