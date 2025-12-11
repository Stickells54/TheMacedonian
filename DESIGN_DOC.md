# The Macedonian: Rise of the Usurper - Design Document

## 1. Vision Statement
Inspired by the life of Basil I the Macedonian, this mod introduces **Internal Politics**, **Espionage**, and **Usurpation** mechanics to Mount & Blade II: Bannerlord. It allows a player to rise from a lowly mercenary to a trusted vassal, the King's favorite, and finally the Emperor himself—not through open conquest, but through intrigue, influence, and "accidents."

## 2. Core Gameplay Loop (The Basil Path)
1.  **The Favorite:** Gain high relation with the Ruler to become "The Right Hand."
2.  **The Shadow:** Use Influence and Roguery to gather secrets and blackmail other clans.
3.  **The Plot:** Orchestrate the "removal" of the current ruler (Assassination or Forced Abdication).
4.  **The Coronation:** Seize the throne from within, inheriting the kingdom intact.

---

## 3. Key Features & Mechanics

### A. The "Right Hand" System (The Rise)
Instead of just being a vassal, you can petition to become the **Royal Protector** (Bodyguard/Co-Ruler).
*   **Requirements:** Clan Tier 4+, Relation with Ruler 80+, High Influence cost.
*   **Benefits:**
    *   **Double Influence:** Gain 2x Influence from battles where the Ruler is present.
    *   **Access to the King's Ear:** Can force the King to declare war/peace without a vote (costs Influence).
    *   **Heir Apparent:** If the King dies "naturally," you have a high weight in the election calculation.
*   **Risks / Trade-offs:**
    *   You are publicly seen as responsible for the ruler’s safety and decisions: failed wars, lost fiefs, and court scandals can reduce your **Legitimacy** and Relations with some clans.
    *   The ruler may demand a **hostage** (family member or key companion) at court; losing them in a failed plot hurts more.
    *   Your baseline **Suspicion** at court is higher than a normal vassal’s—everyone knows you are “next in line.”

### B. Political Dominance (The Influence)
Use Influence as a weapon to force your will upon the council.
*   **Strongarm Policy:** Spend massive Influence (e.g., 500) to instantly pass a policy, bypassing the voting percentage chance.
*   **Fabricate Treason:** Spend Influence and Roguery to frame a rival clan leader. If successful, the King expels them or executes them, removing a rival for the throne.
*   **Buy Loyalty:** Spend Gold/Influence to secretly secure a clan's vote for your future succession.

### C. The Assassination System (The Murder)
The core feature. You cannot just declare war; you must remove the obstacle.
*   **The Hunting Accident:** If you are in the same Army as the Ruler, trigger a "Hunting Trip" event.
    *   **Controllable Factors:**
        *   *Weapon Skill:* Your Bow/Crossbow/Throwing skill increases "Accident" success chance.
        *   *The Party:* Bringing only loyal companions (high relation) reduces "Witness" risk.
        *   *The Setup:* Spend Influence beforehand to bribe the King's guides to lead him to a "dangerous spot."
        *   *Time of Day:* Staging it at dusk increases success chance but slightly raises suspicion.
    *   **Outcomes:**
        *   *Success:* Ruler dies. You are innocent.
        *   *Failure:* Ruler is wounded. Suspicion rises.
        *   *Critical Failure:* You are caught. War declared on your clan immediately.
    *   **Usage Limits:**
        *   Each target can only be subjected to a Hunting Accident attempt once per season/year; on failure, their personal **Suspicion** spikes and future assassination chances are reduced.
        *   Event is only available if your **Court Suspicion** is below a threshold and you have built up enough “loyal service” (e.g., X battles fought in the ruler’s army).
*   **The Poisoned Chalice:** If in the same Settlement (Feast/Wait).
    *   Requires "Poisoner" associate (Companion with high Roguery).
    *   **Buying Silence:** If the plot is discovered, you have a brief window to intervene.
        *   *Bribe:* Pay a massive sum (e.g., 100,000 Gold) to the guards/witnesses to "lose" the evidence.
        *   *Consequence:* You avoid immediate execution/war, but your Companion is executed/exiled, and you suffer a major Relation penalty with the King's clan (they suspect, but can't prove it).
        *   *Framing Option:* On success, you can optionally plant evidence pointing to:
            *   A rival clan inside your kingdom (raises internal tensions, lowers Security more, but removes a competitor), **or**
            *   A clan/kingdom you are already at war with (boosts internal unity and relation with loyalist clans, but may lengthen/embitter the war).
*   **The Midnight Dagger:** If in the same Settlement (Keep).
    *   **Requirements:**
        *   *The Insider:* Bribe a servant (Cost scales with Settlement Security) to unlock the royal chambers.
        *   *The Blade:* Assign a Companion with high Roguery/Athletics to sneak in and "stab him in his sleep."
    *   **Outcomes:**
        *   *Success:* Target (King or Rival) found dead. No witnesses.
        *   *Failure:* Companion caught. You must disavow them (Companion executed, you lose Honor) or intervene (War).
        *   *Frame Job:* (Critical Success) Plant evidence implicating either:
            *   Your **Primary Rival** clan (they are purged/banished, but kingdom Security takes a sharp hit), or
            *   An external enemy kingdom (loyalist clans gain Relation/Loyalty for your “decisive stance”, but that war’s intensity and duration increase).
*   **The Praetorian Coup:** If you are the **Royal Protector**, you can march into the Keep and demand abdication.
    *   Triggers a battle in the Throne Room (You + Companions vs. King + Guards).
    *   Winner takes the Kingdom.

### D. The Conspiracy (Turning Lords)
A hidden "Coup Strength" meter tracks your support.
*   **Sow Dissent:** Dialog option with other lords. "The King is weak/mad."
*   **The Pact:** Secure a secret agreement. "When I strike, you stand down."
*   **Coup Strength Calculation:**
    *   Formula: `(Total Power of Clans in Pact) / (Total Kingdom Power)`.
    *   **Tracking:** A new indicator in the **Kingdom > Clans** tab or a dedicated **"Plots"** tab in the Clan screen showing a bar: *Loyalist Support vs. Usurper Support*.
*   **Leak Risk:**
    *   Each conspirator clan adds both **power** and **leak chance**; paranoid/honest personality traits modify leak risk.
    *   At high leak risk, special events can trigger where a conspirator flips, evidence surfaces, or the ruler becomes more paranoid (raising **Court Suspicion** and making further plotting harder).
*   **Biggest Rival Identification:**
    *   Each clan gets a **Rivalry Score** vs. the player based on:
        *   Their Power relative to yours and their proximity in the succession line.
        *   Negative Relations with you.
        *   Competing claims (they are also close to the ruler, hold many fiefs near the capital, or share culture with the ruling clan).
    *   The clan with the highest Rivalry Score is flagged as **"Primary Rival"** and surfaced clearly in the UI.
    *   **Assassinating a Rival:**
        *   You can target your Primary Rival with the same assassination methods (Hunting Accident, Poisoned Chalice, Midnight Dagger).
        *   If you **frame an external enemy** for their death and your kingdom is at war with that enemy:
            *   +Loyalty / +Relation with ruler’s clan and most loyalist clans (you are seen as a stabilizing force).
            *   Small but kingdom-wide **Security penalty** (people are unsettled by elite deaths, even if “explained”).
        *   If you **frame another internal clan**, you gain Coup Strength (one less competitor) but take a larger **Security hit** and may push some neutral clans toward Loyalist alignment (they fear further purges).
*   **Threshold:** When you have >50% of the Kingdom's strength in your Pact, you can launch a **Bloodless Coup**.

### E. Legitimacy & Suspicion
After (and during) your rise, two soft stats track how stable your claim is:
*   **Legitimacy (0–100):**
    *   Increases from: same culture as ruling clan, long loyal service as Right Hand, high relations with notables and major clans, majority support in the election/coup.
    *   Effects: higher tax efficiency, fewer rebellions, easier policy changes, clans more willing to back your wars.
*   **Suspicion:**
    *   Tracked per faction: `RulerSuspicion`, `CourtSuspicion`, and a general `KingdomSuspicion` against you.
    *   Increases from failed plots, frequent “accidents,” sudden rival deaths, and visible power grabs.
    *   High Suspicion reduces future assassination/conspiracy success and increases the chance the same systems are used **against you** once you are ruler.

---

## 4. Technical Implementation Plan

### Phase 1: The Foundation (Data & Behaviors)
*   **`MacedonianBehavior.cs`**: The core CampaignBehavior.
    *   Track `_conspiracyStrength` per clan.
    *   Track `_isRightHand` status.
    *   Track per-clan **RivalryScore**, per-faction **Suspicion**, and kingdom-level **Legitimacy**.
    *   Save/Load data using `SyncData`.
*   **`UsurpationModel.cs`**: Calculate chances of assassination success based on Roguery, Relation, and RNG.

### Phase 2: The Interactions (Dialogs & Menus)
*   **Game Menus:** Add "Plot against Ruler" option in Town Keep menus.
*   **Dialogs:** Add "What do you think of our King?" dialog trees to recruit conspirators.
*   **Influence Actions:** Patch `KingdomPolicyDecision` to allow "Force Pass" buttons.

### Phase 3: The Action (Assassination)
*   **`KillCharacterAction`**: Use Harmony to trigger death events that look natural (or violent).
*   **`ChangeRulingClanAction`**: The API call to transfer the crown to the player.
*   **Cooldowns & Gating:** Implement per-target cooldowns for assassination attempts and gate availability behind Suspicion thresholds and prior loyal service metrics.

---

## 5. User Interface (UI)
*   **The Intrigue Panel:** A new Gauntlet UI screen (accessible via hotkey or Kingdom menu).
    *   Shows current "Coup Strength."
    *   Shows **Legitimacy** and summarized **Suspicion** levels (Low / Medium / High) with tooltips.
    *   List of Loyalists vs. Conspirators.
    *   Assassination Plot readiness.
    *   For major actions (Kill King, Kill Primary Rival, Frame External Enemy, Bloodless Coup), show a small preview of expected changes: `Coup Strength`, `Legitimacy`, `Security`, and key Relations, so the player can plan their next move.
    *   **Primary Rival Highlight:** Shows the clan banner, leader portrait, and Rivalry Score of your biggest rival with a short tooltip explaining *why* they oppose you ("Close to the throne", "Controls key heartland fiefs", "Old feud", etc.).

---

## 6. Suggested Roadmap
1.  **v0.1:** Skeleton mod. Add the "Right Hand" title (text only) and a basic "Assassinate" button that just kills the king (debug mode).
2.  **v0.2:** Implement the Roguery calculations and "Hunting Accident" event.
3.  **v0.3:** Add the Conspiracy dialogs to turn other lords.
4.  **v0.4:** The UI Panel and Influence Strongarming.
5.  **v1.0:** Full release with "Praetorian Coup" throne room battles and basic post-coronation systems (Legitimacy, Suspicion feedback loops, early counter-plots against the new ruler).

---

## 7. Post-Coronation Phase (Ruling as Basil)
Becoming ruler does not end the mod; it shifts its focus.
*   **The Macedonian Reforms:** Unlock a small set of powerful but risky policies (e.g., centralization, military reforms) that trade short-term **Security/Legitimacy** for long-term economic or military buffs.
*   **Counter-Plots:** Remnants of loyalists, foreign agents, and ambitious vassals can now use similar assassination and conspiracy systems **against you**.
    *   Invest in counter-intrigue: bodyguards, food-tasters, spy networks to reduce your assassination risk.
    *   Fail to manage Suspicion and Legitimacy, and you may face your own Basil-style usurper.
