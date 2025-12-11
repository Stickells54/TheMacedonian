# The Macedonian: Rise of the Usurper

A Mount & Blade II: Bannerlord mod inspired by Basil I of Byzantium that adds deep political intrigue mechanics, allowing players to scheme, conspire, and ultimately seize the throne of their kingdom.

## Overview

In 9th century Byzantium, Basil I rose from nothing to become Emperor through a combination of loyal service, careful scheming, and ruthless action. This mod brings that political gameplay to Bannerlord, allowing you to:

- **Serve as the Right Hand** - Become the ruler's most trusted advisor through loyal military service
- **Build a Conspiracy** - Recruit fellow lords to support your claim to power
- **Eliminate Rivals** - Use various methods to remove obstacles in your path
- **Manage Suspicion** - Balance ambition against the risk of exposure
- **Seize the Throne** - Execute a coup when the time is right
- **Maintain Legitimacy** - Survive as ruler through the dangerous post-coronation period

## Features

### The Right Hand System
- Fight alongside your ruler in battle to earn trust
- Send a companion as "hostage" to prove loyalty
- Gain access to court secrets and assassination opportunities

### Conspiracy System
- Recruit disaffected lords to your cause
- Track **Coup Strength** (percentage of kingdom military power supporting you)
- Manage **Leak Risk** - more conspirators means more danger
- Buy silence from those who learn too much

### Assassination Methods
Four distinct approaches, each with unique mechanics:

| Method | Style | Key Skills | Risk |
|--------|-------|------------|------|
| **Poison** | Subtle | Medicine, Charm | Low detection, high preparation |
| **Hunting Accident** | Opportunistic | Riding, Roguery | Requires target to hunt |
| **Duel Provocation** | Direct | Combat skills | Many witnesses |
| **Midnight Dagger** | Bold | Roguery, Athletics | High risk, high reward |

### Framing System
- Frame rival clans for assassinations
- Blame enemy kingdoms to start wars
- Attribute deaths to bandits

### Suspicion & Legitimacy
- **Suspicion** - The ruler and court's awareness of your scheming
- **Legitimacy** - Your claim to power after taking the throne (0-100)
- Actions have consequences that ripple through your reign

## Installation

1. Download the latest release
2. Extract to `Mount & Blade II Bannerlord\Modules\`
3. Enable "The Macedonian" in the launcher
4. **Recommended**: Install [Mod Configuration Menu (MCM)](https://www.nexusmods.com/mountandblade2bannerlord/mods/612) for settings access

## Requirements

- Mount & Blade II: Bannerlord (v1.2.x+)
- [Mod Configuration Menu v5](https://www.nexusmods.com/mountandblade2bannerlord/mods/612) (optional but recommended)

## Configuration

With MCM installed, access settings via the mod options menu:

- **Difficulty Settings** - Adjust success rates and cooldowns
- **Feature Toggles** - Enable/disable specific systems
- **Notification Settings** - Control what alerts you receive

## Current Status

**Version 0.1.0** - Foundation Release

This release establishes the core framework including:
- ✅ Full data model implementation
- ✅ Probability calculation engine
- ✅ Save/load support
- ✅ MCM settings integration
- ✅ Basic conspiracy recruitment dialogs
- ✅ Suspicion and legitimacy systems

See [CHANGELOG.md](CHANGELOG.md) for details.

### In Development
- Assassination execution menus
- Full dialog trees
- Intrigue status UI panel
- Post-coronation events

## Design Philosophy

The mod aims to create a **parallel progression path** where political maneuvering is as valid as military conquest. Key principles:

1. **Player Agency** - Multiple paths to power, each viable
2. **Meaningful Risk** - Ambitious actions have real consequences
3. **Emergent Stories** - Systems interact to create unique narratives
4. **Historical Inspiration** - Mechanics reflect real political intrigue

## Compatibility

- Should work with most gameplay mods
- May conflict with mods that heavily modify the kingdom/clan system
- Tested with base game + DLCs

## Credits

- Design inspired by the historical Basil I and Byzantine court intrigue
- Built using [Harmony](https://github.com/pardeike/Harmony) for patching
- Settings via [Mod Configuration Menu](https://www.nexusmods.com/mountandblade2bannerlord/mods/612)

## License

MIT License - See LICENSE file for details.

## Contributing

Contributions welcome! Please read the [design document](DESIGN_DOC.md) for feature specifications.
