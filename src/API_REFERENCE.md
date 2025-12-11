# Bannerlord API Reference for The Macedonian

This document contains verified API calls from the official Bannerlord API documentation (v1.2.7) for use in The Macedonian mod.

## Table of Contents
- [Hero Class](#hero-class)
- [Clan Class](#clan-class)
- [Campaign Events](#campaign-events)
- [Campaign Behavior](#campaign-behavior)
- [Dialog System](#dialog-system)
- [Game Menus](#game-menus)
- [Actions](#actions)
- [Utility Classes](#utility-classes)

---

## Hero Class
**Namespace:** `TaleWorlds.CampaignSystem`

### Key Properties

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `Name` | `TextObject` | get | Hero's full name |
| `FirstName` | `TextObject` | get | Hero's first name |
| `Clan` | `Clan` | get, set | Clan the hero belongs to |
| `Gold` | `int` | get, set | Hero's gold amount |
| `Age` | `float` | get | Hero's current age |
| `Power` | `float` | get | Hero's power level |
| `Level` | `int` | field | Hero's level |
| `IsAlive` | `bool` | get | Whether hero is alive |
| `IsDead` | `bool` | get | Whether hero is dead |
| `IsLord` | `bool` | get | Whether hero is a lord |
| `IsClanLeader` | `bool` | get | Whether hero leads a clan |
| `IsKingdomLeader` | `bool` | get | Whether hero leads a kingdom |
| `IsFactionLeader` | `bool` | get | Whether hero leads a faction |
| `MapFaction` | `IFaction` | get | Faction the hero belongs to |
| `PartyBelongedTo` | `MobileParty` | get | Party the hero is in |
| `CurrentSettlement` | `Settlement` | get | Settlement hero is currently in |
| `HomeSettlement` | `Settlement` | get | Hero's home settlement |
| `Spouse` | `Hero` | get, set | Hero's spouse |
| `Father` | `Hero` | get, set | Hero's father |
| `Mother` | `Hero` | get, set | Hero's mother |
| `Children` | `MBList<Hero>` | get | Hero's children |
| `Siblings` | `IEnumerable<Hero>` | get | Hero's siblings |
| `HitPoints` | `int` | get, set | Current health |
| `MaxHitPoints` | `int` | get | Maximum health |
| `IsWounded` | `bool` | get | Whether hero is wounded |
| `HeroState` | `CharacterStates` | get | Current state (Active, Dead, etc.) |
| `DeathMark` | `KillCharacterAction.KillCharacterActionDetail` | get | How hero is marked for death |
| `CharacterObject` | `CharacterObject` | get | Character template |

### Key Methods

```csharp
// Get hero traits - CONFIRMED WORKING
CharacterTraits GetHeroTraits()

// Get trait level - CONFIRMED WORKING
int GetTraitLevel(TraitObject trait)

// Set trait level
void SetTraitLevel(TraitObject trait, int value)

// Get skill value
int GetSkillValue(SkillObject skill)

// Set skill value
void SetSkillValue(SkillObject skill, int value)

// Get relation with another hero - CONFIRMED WORKING
int GetRelation(Hero otherHero)

// Set personal relation
void SetPersonalRelation(Hero otherHero, int value)

// Get relation with player
float GetRelationWithPlayer()

// Check if enemy
bool IsEnemy(Hero otherHero)

// Check if friend
bool IsFriend(Hero otherHero)

// Change hero gold
void ChangeHeroGold(int changeAmount)

// Change state
void ChangeState(CharacterStates newState)

// Make wounded
void MakeWounded(Hero killerHero = null, KillCharacterAction.KillCharacterActionDetail deathMarkDetail = KillCharacterAction.KillCharacterActionDetail.None)

// Add death mark
void AddDeathMark(Hero killerHero = null, KillCharacterAction.KillCharacterActionDetail deathMarkDetail = KillCharacterAction.KillCharacterActionDetail.None)

// Can hero die check
bool CanDie(KillCharacterAction.KillCharacterActionDetail causeOfDeath)
```

### Static Properties & Methods

```csharp
// Get main hero (player) - CONFIRMED WORKING
static Hero MainHero { get; }

// Get hero in current conversation
static Hero OneToOneConversationHero { get; }

// All living heroes
static MBReadOnlyList<Hero> AllAliveHeroes { get; }

// Dead or disabled heroes
static MBReadOnlyList<Hero> DeadOrDisabledHeroes { get; }

// Find first hero matching predicate
static Hero FindFirst(Func<Hero, bool> predicate)

// Find all heroes matching predicate
static IEnumerable<Hero> FindAll(Func<Hero, bool> predicate)
```

### Hero.CharacterStates Enum

```csharp
enum CharacterStates
{
    NotSpawned,
    Active,
    Fugitive,
    Prisoner,
    Released,
    Dead,
    Disabled,
    Traveling
}
```

---

## Clan Class
**Namespace:** `TaleWorlds.CampaignSystem`

### Key Properties

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `Name` | `TextObject` | get | Clan name |
| `Leader` | `Hero` | get | Clan leader |
| `Kingdom` | `Kingdom` | get, set | Kingdom the clan belongs to |
| `Gold` | `int` | get | Clan treasury |
| `Influence` | `float` | get, set | Clan influence points |
| `TotalStrength` | `float` | get | **CONFIRMED** - Total military strength |
| `Renown` | `float` | get, set | Clan renown |
| `Tier` | `int` | get | Clan tier (1-6) |
| `Heroes` | `MBReadOnlyList<Hero>` | get | All heroes in clan |
| `Lords` | `MBReadOnlyList<Hero>` | get | Lord heroes in clan |
| `Companions` | `MBReadOnlyList<Hero>` | get | Companion heroes |
| `Settlements` | `MBReadOnlyList<Settlement>` | get | Owned settlements |
| `Fiefs` | `MBReadOnlyList<Town>` | get | Owned fiefs (towns/castles) |
| `Villages` | `MBReadOnlyList<Village>` | get | Owned villages |
| `HomeSettlement` | `Settlement` | get | Clan's home settlement |
| `IsNoble` | `bool` | get, set | Whether clan is noble |
| `IsEliminated` | `bool` | get | Whether clan is eliminated |
| `IsMinorFaction` | `bool` | get | Whether minor faction |
| `Stances` | `IEnumerable<StanceLink>` | get | War/peace stances |
| `Culture` | `CultureObject` | get, set | Clan culture |

### Key Methods

```csharp
// Get relation with another clan
int GetRelationWithClan(Clan other)

// Check if at war with faction
bool IsAtWarWith(IFaction other)

// Get stance with faction
StanceLink GetStanceWith(IFaction other)

// Set clan leader
void SetLeader(Hero leader)

// Leave kingdom
void ClanLeaveKingdom(bool giveBackFiefs = false)

// Add renown
void AddRenown(float value, bool shouldNotify = true)

// Update strength calculation
void UpdateStrength()
```

### Static Properties & Methods

```csharp
// Get player's clan - CONFIRMED WORKING
static Clan PlayerClan { get; }

// All clans
static MBReadOnlyList<Clan> All { get; }

// Non-bandit factions
static IEnumerable<Clan> NonBanditFactions { get; }

// Find first clan matching predicate
static Clan FindFirst(Predicate<Clan> predicate)

// Find all clans matching predicate
static IEnumerable<Clan> FindAll(Predicate<Clan> predicate)
```

---

## Campaign Events
**Namespace:** `TaleWorlds.CampaignSystem`

### Tick Events

```csharp
// Daily tick (no parameters)
CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(OnDailyTick));

// Weekly tick (no parameters)
CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, new Action(OnWeeklyTick));

// Hourly tick (no parameters)
CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(OnHourlyTick));

// Daily tick per hero
CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, new Action<Hero>(OnDailyTickHero));

// Daily tick per clan
CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, new Action<Clan>(OnDailyTickClan));
```

### Hero Events

```csharp
// Hero killed - CONFIRMED
CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, 
    new Action<Hero, Hero, KillCharacterAction.KillCharacterActionDetail, bool>(OnHeroKilled));

// Before hero killed
CampaignEvents.BeforeHeroKilledEvent.AddNonSerializedListener(this, 
    new Action<Hero, Hero, KillCharacterAction.KillCharacterActionDetail, bool>(OnBeforeHeroKilled));

// Hero relation changed - CONFIRMED
CampaignEvents.HeroRelationChanged.AddNonSerializedListener(this,
    new Action<Hero, Hero, int, bool, ChangeRelationAction.ChangeRelationDetail, Hero, Hero>(OnHeroRelationChanged));

// Hero created
CampaignEvents.HeroCreated.AddNonSerializedListener(this, new Action<Hero, bool>(OnHeroCreated));

// Hero wounded
CampaignEvents.HeroWounded.AddNonSerializedListener(this, new Action<Hero>(OnHeroWounded));

// Character defeated
CampaignEvents.CharacterDefeated.AddNonSerializedListener(this, new Action<Hero, Hero>(OnCharacterDefeated));
```

### Kingdom/Clan Events

```csharp
// Ruling clan changed
CampaignEvents.RulingClanChanged.AddNonSerializedListener(this, 
    new Action<Kingdom, Clan>(OnRulingClanChanged));

// Clan changed kingdom
CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this,
    new Action<Clan, Kingdom, Kingdom, ChangeKingdomAction.ChangeKingdomActionDetail, bool>(OnClanChangedKingdom));

// Clan destroyed
CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener(this, new Action<Clan>(OnClanDestroyed));

// Kingdom created
CampaignEvents.KingdomCreatedEvent.AddNonSerializedListener(this, new Action<Kingdom>(OnKingdomCreated));

// Kingdom destroyed
CampaignEvents.KingdomDestroyedEvent.AddNonSerializedListener(this, new Action<Kingdom>(OnKingdomDestroyed));

// War declared
CampaignEvents.WarDeclared.AddNonSerializedListener(this,
    new Action<IFaction, IFaction, DeclareWarAction.DeclareWarDetail>(OnWarDeclared));
```

### Session Events

```csharp
// Session launched - use for dialog/menu registration
CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, 
    new Action<CampaignGameStarter>(OnSessionLaunched));

// New game created
CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this,
    new Action<CampaignGameStarter>(OnNewGameCreated));

// Game loaded
CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this,
    new Action<CampaignGameStarter>(OnGameLoaded));
```

### Conversation Events

```csharp
// Conversation ended
CampaignEvents.ConversationEnded.AddNonSerializedListener(this,
    new Action<IEnumerable<CharacterObject>>(OnConversationEnded));

// Agent joined conversation
CampaignEvents.OnAgentJoinedConversationEvent.AddNonSerializedListener(this,
    new Action<IAgent>(OnAgentJoinedConversation));
```

---

## Campaign Behavior
**Namespace:** `TaleWorlds.CampaignSystem`

### Base Class Structure

```csharp
public class MyBehavior : CampaignBehaviorBase
{
    public override void RegisterEvents()
    {
        // Register event listeners here
        CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
    }

    public override void SyncData(IDataStore dataStore)
    {
        // Persist data between saves
        dataStore.SyncData("myVariable", ref _myVariable);
    }
}
```

### Registering Behaviors

```csharp
// In SubModule.cs OnGameStart method
protected override void OnGameStart(Game game, IGameStarter gameStarter)
{
    if (game.GameType is Campaign)
    {
        CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarter;
        campaignStarter.AddBehavior(new MyBehavior());
    }
}
```

---

## Dialog System
**Namespace:** `TaleWorlds.CampaignSystem`

### Adding Dialog Lines

```csharp
// NPC speaks to player - CONFIRMED WORKING
campaignGameStarter.AddDialogLine(
    id: "my_dialog_id",
    inputToken: "start",           // Token to listen for
    outputToken: "my_output",      // Token to send after
    text: "Hello there!",          // What NPC says
    conditionDelegate: () => SomeCondition(),
    consequenceDelegate: () => SomeAction(),
    priority: 100                  // Higher = checked first
);

// Player response - CONFIRMED WORKING
campaignGameStarter.AddPlayerLine(
    id: "player_response_id",
    inputToken: "my_output",       // Matches outputToken above
    outputToken: "close_window",   // "close_window" ends conversation
    text: "I understand.",
    conditionDelegate: () => true,
    consequenceDelegate: () => { },
    priority: 100
);
```

### Common Dialog Tokens

| Token | Description |
|-------|-------------|
| `"start"` | Initial conversation start |
| `"lord_start"` | When talking to a lord |
| `"hero_main_options"` | Main conversation menu |
| `"close_window"` | Ends the conversation |

### Dialog Condition Pattern

```csharp
private bool MyDialogCondition()
{
    Hero conversationHero = Hero.OneToOneConversationHero;
    if (conversationHero == null) return false;
    
    // Your condition logic
    return conversationHero.IsLord && conversationHero.Clan != Clan.PlayerClan;
}
```

---

## Game Menus
**Namespace:** `TaleWorlds.CampaignSystem.GameMenus`

### Adding Game Menus

```csharp
// Add a new menu - CONFIRMED WORKING
campaignGameStarter.AddGameMenu(
    menuId: "my_menu_id",
    menuText: "{=!}My Menu Title",
    initDelegate: (MenuCallbackArgs args) => { /* init code */ },
    overlay: GameOverlays.MenuOverlayType.None,
    menuFlags: GameMenu.MenuFlags.None,
    relatedObject: null
);

// Add option to existing menu - CONFIRMED WORKING
campaignGameStarter.AddGameMenuOption(
    menuId: "town",                // Add to town menu
    optionId: "my_option_id",
    optionText: "{=!}My Option",
    condition: (MenuCallbackArgs args) => 
    {
        args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
        return true;
    },
    consequence: (MenuCallbackArgs args) =>
    {
        GameMenu.SwitchToMenu("my_menu_id");
    },
    isLeave: false,
    index: -1,                     // -1 = end of list
    isRepeatable: false
);
```

### GameMenuOption.LeaveType Enum

```csharp
enum LeaveType
{
    Default,
    Mission,
    Submenu,
    Escape,
    Craft,
    Raid,
    HostileAction,
    Trade,
    Wait,
    Leave,
    Continue,
    Manage,
    Surrender,
    Conversation,
    // ... more
}
```

### Menu Condition Pattern

```csharp
private bool MyMenuCondition(MenuCallbackArgs args)
{
    args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
    
    Settlement current = Settlement.CurrentSettlement;
    if (current == null || !current.IsTown) return false;
    
    // Your condition
    return true;
}
```

---

## Actions
**Namespace:** `TaleWorlds.CampaignSystem.Actions`

### KillCharacterAction - CONFIRMED WORKING

```csharp
// Kill by murder (assassination)
KillCharacterAction.ApplyByMurder(victim, killer, showNotification: true);

// Kill by execution
KillCharacterAction.ApplyByExecution(victim, executer, showNotification: true, isForced: false);

// Kill by battle
KillCharacterAction.ApplyByBattle(victim, killer, showNotification: true);

// Kill by old age
KillCharacterAction.ApplyByOldAge(victim, showNotification: true);

// Kill by wounds
KillCharacterAction.ApplyByWounds(victim, showNotification: true);

// Kill by death mark (custom death)
KillCharacterAction.ApplyByDeathMark(victim, showNotification: false);
```

### KillCharacterAction.KillCharacterActionDetail Enum

```csharp
enum KillCharacterActionDetail
{
    None,
    Murdered,
    DiedInLabor,
    DiedOfOldAge,
    DiedInBattle,
    WoundedInBattle,
    Executed,
    Lost
}
```

### ChangeRelationAction

```csharp
// Change relation between heroes
ChangeRelationAction.ApplyRelationChangeBetweenHeroes(
    hero1: Hero.MainHero,
    hero2: targetHero,
    relationChange: -20,
    showQuickNotification: true
);

// Change player relation with hero
ChangeRelationAction.ApplyPlayerRelation(
    targetHero: targetHero,
    relationChange: 10,
    affectRelationLord: true,
    showQuickNotification: true
);
```

### GainRenownAction

```csharp
GainRenownAction.Apply(Hero.MainHero, renownAmount, doNotNotify: false);
```

### GiveGoldAction

```csharp
GiveGoldAction.ApplyForCharacterToCharacter(
    giverHero: Hero.MainHero,
    receiverHero: targetHero,
    goldAmount: 1000,
    disableNotification: false
);
```

### ChangeClanInfluenceAction

```csharp
ChangeClanInfluenceAction.Apply(Clan.PlayerClan, influenceAmount);
```

---

## Utility Classes

### InformationManager

```csharp
// Display quick information message
InformationManager.DisplayMessage(new InformationMessage(
    "Your message here",
    Colors.Green  // or any Color
));

// Show inquiry (yes/no popup)
InformationManager.ShowInquiry(
    new InquiryData(
        titleText: "Title",
        text: "Question text?",
        isAffirmativeOptionShown: true,
        isNegativeOptionShown: true,
        affirmativeText: "Yes",
        negativeText: "No",
        affirmativeAction: () => { /* on yes */ },
        negativeAction: () => { /* on no */ }
    ),
    pauseGameActiveState: true
);
```

### MBRandom

```csharp
// Random float 0-1
float random = MBRandom.RandomFloat;

// Random int 0 to max (exclusive)
int randomInt = MBRandom.RandomInt(100);

// Random int in range
int randomInRange = MBRandom.RandomInt(10, 50);
```

### Campaign Time

```csharp
// Get current time
CampaignTime now = CampaignTime.Now;

// Create time spans
CampaignTime oneDay = CampaignTime.Days(1);
CampaignTime oneWeek = CampaignTime.Weeks(1);

// Check elapsed time
bool hasElapsed = (CampaignTime.Now - savedTime).ToDays >= 7;
```

### Vec2 (2D Position)

```csharp
// Settlement position
Vec2 position = settlement.Position2D;  // CONFIRMED - not Settlement.Position2D

// Distance between positions
float distance = position.Distance(otherPosition);
```

### TextObject

```csharp
// Simple text
TextObject text = new TextObject("{=!}Simple text");

// Text with variables
TextObject textWithVar = new TextObject("{=!}{HERO_NAME} is plotting.");
textWithVar.SetTextVariable("HERO_NAME", hero.Name);
```

---

## TraitObject Access

```csharp
// Access default traits - CONFIRMED WORKING
TraitObject honor = DefaultTraits.Honor;
TraitObject mercy = DefaultTraits.Mercy;
TraitObject valor = DefaultTraits.Valor;
TraitObject calculating = DefaultTraits.Calculating;
TraitObject generosity = DefaultTraits.Generosity;

// Get trait level for hero
int honorLevel = hero.GetTraitLevel(DefaultTraits.Honor);
// Returns -2 to +2 typically

// Set trait level
hero.SetTraitLevel(DefaultTraits.Honor, 2);
```

---

## Skills Access

```csharp
// Access default skills
SkillObject roguery = DefaultSkills.Roguery;
SkillObject charm = DefaultSkills.Charm;
SkillObject leadership = DefaultSkills.Leadership;
SkillObject tactics = DefaultSkills.Tactics;
SkillObject steward = DefaultSkills.Steward;

// Get skill value
int rogueryLevel = hero.GetSkillValue(DefaultSkills.Roguery);
```

---

## Common Patterns

### Clamp Utility (for .NET 4.7.2)

```csharp
// Math.Clamp doesn't exist in .NET 4.7.2
public static float Clamp(float value, float min, float max)
{
    if (value < min) return min;
    if (value > max) return max;
    return value;
}

public static int Clamp(int value, int min, int max)
{
    if (value < min) return min;
    if (value > max) return max;
    return value;
}
```

### Safe Hero Iteration

```csharp
// Iterate all lords in a kingdom safely
foreach (Clan clan in kingdom.Clans)
{
    foreach (Hero hero in clan.Lords)
    {
        if (hero.IsAlive && !hero.IsPrisoner)
        {
            // Safe to use hero
        }
    }
}
```

### Check if Player is King

```csharp
bool isPlayerKing = Hero.MainHero.Clan?.Kingdom?.RulingClan == Clan.PlayerClan;
```

### Get Kingdom Ruler

```csharp
Hero ruler = kingdom.RulingClan?.Leader;
```

---

## Important Notes

1. **Clan.TotalStrength** - EXISTS and works, returns `float`
2. **Hero.GetHeroTraits()** - Returns `CharacterTraits` object
3. **Hero.GetTraitLevel(TraitObject)** - Returns `int` (-2 to +2 typically)
4. **Hero.GetRelation(Hero)** - Returns `int` relation value
5. **Hero.GetSkillValue(SkillObject)** - Returns `int` skill level
6. **Settlement.Position2D** - Property exists for 2D map position
7. **KillCharacterAction.ApplyByMurder** - Primary method for assassination
8. **CampaignEvents** - All events use `.AddNonSerializedListener()` pattern

---

## Version Compatibility

This reference is based on **Bannerlord v1.2.7 API documentation**.

API surface may vary between game versions. Always test against target game version.
