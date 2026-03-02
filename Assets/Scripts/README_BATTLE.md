# 2D Turn-Based Battle Prototype (Code-Only)

Fully code-driven mobile portrait turn-based battle. No inspector references or manual linking.

---

## How to Run in Unity

1. **Open the project**
   - Open Unity Hub → Add → select the `unity-2` folder (or open existing project).
   - **Tested with Unity 6000.3.10f1** (Unity 6). Should work on 2020.3 LTS or newer.

2. **Ensure Unity UI package is installed**
   - This project uses **Unity UI (uGUI)** for Canvas, Button, Image, Text. It’s added in `Packages/manifest.json` as `com.unity.ugui`.
   - If you see a `UnityEngine.UI` error: **Window → Package Manager** → Unity Registry → find **Unity UI** → Install.

3. **Open a scene**
   - **File → New Scene** (or use the default empty scene).
   - Save the scene (e.g. **File → Save As** → `Assets/Scenes/Battle.unity`).

4. **Add the bootstrap**
   - In the Hierarchy, right-click → **Create Empty**. Rename it to `BattleRunner` (or any name).
   - With the empty GameObject selected, in the Inspector click **Add Component**.
   - Search for `BattleBootstrap` and add the **Battle Bootstrap** script.

5. **Set portrait (optional but recommended)**
   - In the Game view, open the aspect dropdown (e.g. "Free Aspect") → **+** to add resolution.
   - Set a portrait resolution (e.g. **1080 × 1920**) and select it so the UI fits the layout.

6. **Press Play**
   - Click the **Play** button in the top center of the Editor.
   - A **Start** screen appears with a **Start** button. Tap **Start** to fade into the battle.
   - Battle UI: 2 player units (bottom left/right), 1 enemy (top center), 3 ability buttons at the bottom.
   - Use **Attack**, **Defend**, or **Special** on your turn; the enemy acts automatically on its turn.
   - When the enemy is below 30% HP, the **Capture** button appears—click it or drag a line to try to capture.

No other setup is required. All UI and logic are created at runtime by the bootstrap. No scene switching; start screen and battle are the same scene.

---

## Where to Place Scripts

Put all scripts in **Assets/Scripts/** (flat layout):

- `Unit.cs`
- `Ability.cs`
- `TurnManager.cs`
- `BattleManager.cs`
- `CaptureManager.cs`
- `UIManager.cs`
- `StartScreenManager.cs`
- `SoundManager.cs`
- `BattleBootstrap.cs`

If you created the project from scratch, create the `Assets/Scripts` folder and place these files there. Unity will compile them automatically.

---

## Scene Setup (Summary)

- One **empty** scene.
- One **empty GameObject** with the **BattleBootstrap** component.
- No prefabs, no inspector references, no manual button linking.

---

## Architecture (Runtime Only)

- **BattleBootstrap** – Entry point. Creates units, TurnManager, CaptureManager, BattleManager, UIManager; subscribes turn changes for enemy AI.
- **Unit** – Data: Name, Level, HP/MaxHP, SP/MaxSP, defense buff (next damage −50%).
- **Ability** – Defines Basic Attack (0 SP, 1×), Defend (0 SP, buff), Special (20 SP, 2× damage).
- **TurnManager** – Order: Player 1 → Player 2 → Enemy → repeat; fires `OnTurnChanged`.
- **BattleManager** – Runs abilities (damage, SP, defense buff), win/lose, capture attempt; holds references to both sides.
- **CaptureManager** – MonoBehaviour. Tracks touch/mouse drag; if drag length ≥ threshold, calls battle capture; computes capture chance (30 + missingHP% − level×2, clamped 5–95).
- **UIManager** – MonoBehaviour. Builds Canvas (portrait 1080×1920), HP/SP bars (Image fill), turn label, 3 ability buttons, capture button (shown only when enemy HP < 30%). All UI and button listeners are created in code.

---

## Flow

- Players take turns using Attack / Defend / Special.
- Enemy turn runs automatically after a short delay (basic attack on a random alive player).
- When enemy HP < 30%, Capture button appears; you can also drag a line (length > threshold) to attempt capture.
- Battle ends when the enemy or all players are defeated (or enemy is captured).

---

## Rules Satisfied

- No inspector references; no manual button linking; no public `Button` fields.
- All UI and GameObjects created in code.
- Single empty scene with one GameObject running BattleBootstrap is enough to play.
