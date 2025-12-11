# The Macedonian: Player's Wiki

*A comprehensive guide to rising from vassal to emperor through intrigue, assassination, and political manipulation.*

---

## Table of Contents

1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [The Intrigue Menu](#the-intrigue-menu)
4. [Assassination System](#assassination-system)
5. [Conspiracy System](#conspiracy-system)
6. [Suspicion Mechanics](#suspicion-mechanics)
7. [Rivalry System](#rivalry-system)
8. [Post-Coronation: Ruling as Basil](#post-coronation-ruling-as-basil)
9. [Calculations & Formulas](#calculations--formulas)
10. [Tips & Strategies](#tips--strategies)
11. [Configuration Options](#configuration-options)

---

## Overview

**The Macedonian** adds deep political intrigue mechanics to Bannerlord, inspired by the historical rise of Basil I the Macedonian. Instead of conquering a kingdom through war, you can seize power from within through:

- **Assassination** - Remove the ruler or rivals through poison, "hunting accidents," or midnight daggers
- **Conspiracy** - Recruit lords to secretly support your coup
- **Political Manipulation** - Use influence, gold, and relationships to bend the kingdom to your will
- **Legitimacy Management** - Once you're ruler, maintain your hold on power

---

## Getting Started

### Requirements to Begin Plotting

1. **Join a Kingdom** - You must be a vassal in a kingdom to access intrigue options
2. **Visit a Settlement** - Intrigue menus are only available in towns and castles
3. **Not Be the Ruler** - Assassination plots target the current ruler (or rivals)

### Accessing the Intrigue Menu

In any **town** or **castle** you own or can enter, look for the option:

> **"Pursue dark intrigues..."**

This opens the main intrigue menu where all plotting takes place.

---

## The Intrigue Menu

When you select "Pursue dark intrigues..." you'll see these options:

| Option | Description |
|--------|-------------|
| **Plan an assassination** | Begin plotting against the ruler |
| **Review your conspiracy** | Check how many lords support you |
| **Gauge suspicions about you** | Check how suspicious the ruler/court is |
| **Identify your greatest rival** | See who threatens your rise to power |
| **Host a grand feast** | (Ruler only) Spend gold to boost legitimacy |
| **Donate to the people** | (Ruler only) Spend gold to boost legitimacy |

---

## Assassination System

### Overview

Assassination is the core mechanic for removing obstacles to your rise. You can eliminate the current ruler to trigger a succession crisis, or target rivals who threaten your position.

### Target Selection

By default, the assassination target is the **current ruler** of your kingdom. This is the most common path to power.

### Methods

Each assassination method has different **success chances** and **detection risks**:

#### 1. Poison

| Stat | Description |
|------|-------------|
| **Base Success** | 35% |
| **Detection Risk** | 20% (if successful) |
| **Best For** | High Medicine skill characters |

**How Success is Calculated:**
- +0.1% per point of your **Medicine** skill (helps create effective poisons)
- +15% if you have alchemy expertise
- -1.5% for every 15 points of target's Medicine skill (they can detect poison)
- -20% if target has bodyguards/food tasters
- +0.05% per point of **Charm** (helps get close to target)

**Flavor:** *"The ruler has died suddenly after falling ill..."*

#### 2. Hunting Accident

| Stat | Description |
|------|-------------|
| **Base Success** | 25% |
| **Detection Risk** | 15% (if successful) |
| **Best For** | High Riding/Roguery characters |

**How Success is Calculated:**
- +0.07% per point of your **Riding** skill (setup the "accident")
- +0.1% per point of your **Roguery** skill (cover-up)
- -0.05% per point of target's **Athletics** (they can escape)
- -5% per point of target's **Valor** trait (they're harder to kill)

**Note:** This method only works if the target hunts regularly.

**Flavor:** *"The ruler suffered a fatal accident during a hunt."*

#### 3. Midnight Dagger

| Stat | Description |
|------|-------------|
| **Base Success** | 40% |
| **Detection Risk** | 10% success / 60% on failure |
| **Best For** | High Roguery/Athletics characters |

**How Success is Calculated:**
- +0.125% per point of your **Roguery** skill (stealth is key)
- +0.07% per point of your **Athletics** skill (agility)
- +0.05% per point of your **One-Handed** skill (the killing blow)
- -30% if target has bodyguards
- -0.07% per point of target's **Scouting** skill (they might detect you)
- **-0.2% per point of Ruler Suspicion** (higher suspicion = more guards)

**Flavor:** *"The ruler was found dead in their chambers."*

### Cooldowns

After any assassination attempt (success or failure), you cannot target the same person again for a configurable period (default: **30 days**). Each failure also increases the target's **awareness**, making future attempts harder.

### Framing

When confirming an assassination, you can choose to:

1. **Proceed and frame an enemy kingdom** - Attempt to redirect blame
2. **Proceed without framing** - Just commit the deed

**Frame Success Chance:**
- Base: 40%
- +0.125% per point of Roguery (plant evidence)
- +0.08% per point of Charm (sell the story)
- +25% if already at war with the framed kingdom

If framing succeeds, **no suspicion** falls on you. If it fails, you still get the suspicion increase.

### Outcomes

| Outcome | What Happens |
|---------|--------------|
| **Success + Not Detected** | Target dies. No one suspects you. |
| **Success + Detected** | Target dies. Suspicion increases. |
| **Success + Frame Works** | Target dies. Blame falls elsewhere. |
| **Failure + Not Detected** | Target lives. No one knows you tried. |
| **Failure + Detected** | Target lives. Major suspicion increase. Relation penalty (-30). Target becomes paranoid. |

---

## Conspiracy System

### Overview

Before seizing power, you'll want allies. The conspiracy system lets you recruit other lords to secretly support your coup.

### How to Recruit Conspirators

When talking to other lords in your kingdom, you can broach the topic of... *new leadership*. Look for dialog options like:

> *"Perhaps it is time for new leadership. Would you support such a change?"*

### Join Chance Calculation

Whether a lord joins your conspiracy depends on:

| Factor | Effect |
|--------|--------|
| **Relation with you** | +0.5% per point |
| **Relation with ruler** | -0.5% per point |
| **High Honor trait** | -15% per trait level |
| **Low Honor trait** | +10% per level below 0 |
| **Calculating trait** | +10% per trait level |
| **Mercy trait** | -5% per trait level (they don't want bloodshed) |
| **Weak clan** | +15% if clan strength < 50% of average |
| **Strong clan** | -10% if clan strength > 150% of average |
| **Your Coup Strength** | +0.25% per % of existing support |

**Base chance:** 25%  
**Range:** 5% to 85% (clamped)

### Coup Strength

Your **Coup Strength** represents how much of the kingdom's military power supports you:

```
Coup Strength = (Conspirator Clans' Total Strength) / (Kingdom Total Strength) × 100
```

View this anytime via **"Review your conspiracy"** in the intrigue menu.

### Consequences of Refusal

If a lord refuses to join:
- They're marked as a **Loyalist**
- Your **Ruler Suspicion** increases by 5 points
- That lord cannot be approached again

### Leak Risk

Every conspirator adds to your **Leak Risk**. Each week, there's a chance (based on Leak Risk / 10) that information leaks:

| Leak Modifier | Effect |
|---------------|--------|
| **Calculating trait** | -2% per level (they're careful) |
| **Honor trait** | +1.5% per level (they might slip up morally) |
| **Naive/low Calculating** | +3% (they're careless) |

---

## Suspicion Mechanics

### Three Types of Suspicion

| Type | What It Represents |
|------|---------------------|
| **Ruler Suspicion** | How much the ruler personally suspects you |
| **Court Suspicion** | General awareness among nobles at court |
| **Kingdom Suspicion** | Rumors spreading among the populace |

### Suspicion Levels

| Level | Range | Meaning |
|-------|-------|---------|
| None | 0-10 | No one suspects you |
| Low | 11-30 | Faint whispers |
| Medium | 31-60 | Growing concerns |
| High | 61-85 | Active investigation |
| Critical | 86-100 | You're a known threat |

### What Increases Suspicion

| Event | Ruler | Court | Kingdom |
|-------|-------|-------|---------|
| Assassination (detected) | +25-40 | +17-28 | +7-12 |
| Assassination (failed + detected) | +37-60 | +30-48 | +18-30 |
| Conspiracy leak (Rumor) | +5 | +3.5 | - |
| Conspiracy leak (Evidence) | +30 | +21 | - |
| Conspiracy leak (Full Exposure) | +50 | +35 | - |
| Lord refuses to conspire | +5 | - | - |

### Suspicion Decay

Suspicion naturally decays over time:
- **Ruler Suspicion:** -0.5 per day
- **Court Suspicion:** -0.25 per day
- **Kingdom Suspicion:** -0.125 per day

### Effects of High Suspicion

- **Midnight Dagger** success reduced by 0.2% per point of Ruler Suspicion
- Future assassination attempts become harder
- Leak consequences become more severe
- (After coronation) Counter-coup risk increases

---

## Rivalry System

### What is a Rival?

Your **Primary Rival** is the lord who most threatens your path to power. They're typically:
- Close to the ruler
- Politically powerful
- Hostile to you
- Well-positioned for succession

### Rivalry Score Calculation

| Factor | Points |
|--------|--------|
| Clan Tier | +10 per tier |
| Influence | +1 per 100 (max +50) |
| Relation with Ruler | +0.5 per point |
| Relation with You | -0.33 per point |
| Relative Military Strength | Up to +20 |
| Spouse of Ruler | +25 |
| Child of Ruler | +30 |
| Calculating trait | +10 per level |

### Viewing Your Rival

Select **"Identify your greatest rival"** from the intrigue menu to see:
- Rival's name and clan
- Their Rivalry Score
- The ruler's favor toward them

### Why Rivals Matter

Rivals compete for the same throne you're after. Consider:
- **Assassinating them** to remove competition
- **Recruiting them** if they're disgruntled with the ruler
- **Framing them** for your own assassinations

---

## Post-Coronation: Ruling as Basil

Once you become ruler, the mod shifts focus to **maintaining power**.

### Legitimacy

Your **Legitimacy** (0-100) represents how accepted your rule is:

| Range | Status |
|-------|--------|
| 75-100 | Stable Rule |
| 50-74 | Uneasy Crown |
| 25-49 | Legitimacy Crisis |
| 0-24 | Tyrant's Grip |

### Initial Legitimacy

How you took power affects starting legitimacy:

| Path | Starting Legitimacy |
|------|---------------------|
| Coup (violent takeover) | 30 + (Coup Strength / 4) - (Prior Suspicion / 3) |
| Legitimate succession | 60 |

### Weekly Legitimacy Changes

| Factor | Change |
|--------|--------|
| Below 50 legitimacy | +0.5 (drift toward center) |
| Above 80 legitimacy | -0.1 (hard to stay at top) |
| Battle victories | +3 per victory |
| Battle defeats | -2 per defeat |
| Hosting a feast | +2 immediate, +2 at week end |
| Donations | +1 per 5,000 denars (max +5) |

### Post-Coronation Actions

Available from the intrigue menu when you're ruler:

| Action | Cost | Effect |
|--------|------|--------|
| **Host a grand feast** | 10,000 denars | +2 Legitimacy, +2 Relation with all lords |
| **Donate to the people** | 5,000 denars | +1 Legitimacy |

### Counter-Coup Risk

If your legitimacy falls below **20%**, each week there's a chance (up to 10% at 0 legitimacy) that discontented lords begin plotting against you!

---

## Calculations & Formulas

### Assassination Success

```
Final Chance = (Base Chance + Skill Modifiers - Target Defenses) × Settings Modifier
Clamped to: 5% - 95%
```

### Detection Chance

| Method | If Successful | If Failed |
|--------|---------------|-----------|
| Poison | 20% | 45% |
| Hunting Accident | 15% | 40% |
| Midnight Dagger | 10% | 60% |

```
Final Detection = Base Detection × Settings Modifier - (Roguery / 10)
```

### Buying Silence (Not Yet Implemented)

When caught, you may be able to bribe your way out:

```
Price = 10,000 + (Suspicion × 100) + (Calculating × 5,000) - (Relation × 50)
```

Honorable lords (Honor > 1) cannot be bribed.

### Conspiracy Join Chance

```
Base (25%) + Relation Factors + Trait Modifiers + Power Factors + Coup Momentum
```

---

## Tips & Strategies

### Early Game

1. **Build Roguery** - Most assassination methods benefit from high Roguery
2. **Befriend the Ruler** - Higher relation gives you more access and opportunities
3. **Recruit Weak Clans First** - They're +15% more likely to join conspiracies
4. **Stay Patient** - Let suspicion decay between attempts

### Mid Game

1. **Target Your Rival First** - Remove competition before going for the throne
2. **Build Coup Strength to 50%+** - This dramatically increases your chances
3. **Watch Your Leak Risk** - Too many conspirators = eventual exposure
4. **Use Framing** - Blame external enemies to avoid suspicion

### Assassination Strategy

1. **Midnight Dagger** has the highest base success rate (40%)
2. **Hunting Accident** is safest if caught (only 15% detection on success)
3. **Poison** scales best with Medicine skill
4. Wait for **Suspicion to decay** before attempting again

### Post-Coronation

1. **Host feasts early** - Build legitimacy buffer
2. **Win battles** - +3 legitimacy per victory
3. **Avoid disasters** - Defeats hurt legitimacy
4. **Stay above 20%** - Below this, counter-coups become possible

---

## Configuration Options

All these can be adjusted in **Mod Configuration Menu (MCM)**:

### Difficulty

| Setting | Range | Default | Effect |
|---------|-------|---------|--------|
| Assassination Success Modifier | 0.5-2.0 | 1.0 | Multiplies success chances |
| Detection Chance Modifier | 0.5-2.0 | 1.0 | Multiplies detection chances |
| Leak Risk Modifier | 0.5-2.0 | 1.0 | Multiplies weekly leak chance |
| Legitimacy Decay Modifier | 0.5-2.0 | 1.0 | Multiplies legitimacy decay |
| Assassination Cooldown | 7-90 days | 30 | Time between attempts on same target |

### Feature Toggles

| Setting | Default | Effect |
|---------|---------|--------|
| Enable Intrigue System | Yes | Master toggle for all features |
| Enable Poison Method | Yes | Allow poison assassinations |
| Enable Hunting Accident | Yes | Allow hunting accidents |
| Enable Midnight Dagger | Yes | Allow midnight dagger |
| Enable Framing | Yes | Allow framing enemies |
| Enable Counter-Coups | Yes | AI can plot against you as ruler |

### Notifications

| Setting | Default | Effect |
|---------|---------|--------|
| Show Suspicion Warnings | Yes | Notify when suspicion increases |
| Show Leak Warnings | Yes | Notify when conspiracy leaks |
| Show Rivalry Updates | Yes | Notify of rival changes |
| Show Legitimacy Updates | Yes | Notify of legitimacy changes |

---

## Troubleshooting

### The intrigue menu doesn't appear

- Make sure you're in a **town or castle**
- You must be a **vassal in a kingdom** (not independent)
- Check MCM to ensure **Enable Intrigue System** is on

### Assassination always fails

- Check your **Roguery, Medicine, Riding** skills
- Reduce **Ruler Suspicion** by waiting for decay
- Target may have **bodyguards** - try different methods
- Check MCM **Assassination Success Modifier**

### Can't recruit any conspirators

- Build better **relations** with lords
- Target lords with **low Honor** trait
- Find lords who **dislike the ruler** (low/negative relation)
- Your **Coup Strength** affects recruitment chances

---

## Version History

- **v0.2.0** - Full implementation: game menus, assassination execution, post-coronation features
- **v0.1.0** - Foundation release: core framework, models, basic dialogs

---

*"The throne is not inherited. It is taken."*  
— The Macedonian
