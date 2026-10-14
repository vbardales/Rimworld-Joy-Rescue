# F12 of Tests/MANUAL.md: how the mod list presents Joy Rescue. The list is drawn from the ModMetaData the game
# read out of About.xml, so the steps assert that. There is no capture: the page itself is the game's own
# drawing, and the Preview is checked at mountPreview (owner decision, 2026-10-14). What a click
# on the link does is the game's own (it hands the address to the operating system's browser): the mod's part
# is that the address is right and that the link sits where the game turns it into a link.
@joyrescue-sandbox
Feature: the mod list presents Joy Rescue with its title, its game version, its dependency and its source link

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs

  Scenario: the entry says what About.xml says
    Then Joy Rescue: the mod list entry is titled "Joy Rescue"
    And Joy Rescue: the mod list entry is compatible with the game version this suite runs on
    And Joy Rescue: the mod list entry requires the mod "brrainz.harmony"
    And Joy Rescue: the mod list entry links to "https://github.com/vbardales/Rimworld-Joy-Rescue" in its metadata and ends its description with that link
