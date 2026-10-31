# 🪙 Coin Catcher

A fast-paced 2D arcade game built in **Unity 6**. Control the basket at the bottom of the screen, catch falling coins to score points, and avoid explosive bombs before the timer runs out!

---

## 🎮 Play Online
- **Play Link:** *Play link coming soon*

---

## 📸 Screenshots
*Screenshots coming soon*

---

## 🕹️ Controls
- **Move Left:** `Left Arrow` or `A`
- **Move Right:** `Right Arrow` or `D`
- **Full Input Support:** Supports both Unity's New Input System and Legacy Input Manager.

---

## ⏱️ Rules & Gameplay

### 🪙 Catching Coins
- Catch falling gold coins to earn **+1 point** per coin.
- Triggers a responsive squash-and-stretch catch animation with cheerful audio feedback.

### 💣 Avoiding Bombs
- **Hazard:** Dangerous round black bombs with fuses and sparks fall alongside coins.
- **Fairness Rules:**
  - Safe Start: No bombs fall during the first 3 seconds of gameplay.
  - No Consecutive Spawns: Never spawns two bombs in a row.
  - Progressive Spawn Chance: Starts at 25% chance and scales up to 30% as difficulty increases.
- **Explosion Feedback:** Catching a bomb triggers an explosion particle effect, an explosive boom sound effect, and flashes the basket red for 0.35 seconds.
- **HUD Counter:** Real-time bomb counter displayed at top center (`Bombs: 0/3`).
- **Game Over Trigger:** Catching **3 bombs** immediately ends the game with a `"BOOM! You caught 3 bombs!"` Game Over screen.
- **Safe Misses:** Missing a bomb carries no penalty; bombs cleanly despawn below the screen.

### ⚡ Dynamic Difficulty Progression
- Difficulty increases dynamically based on:
  - Collected coins (`+0.03` difficulty per coin)
  - Elapsed time (`+0.01` difficulty per second)
  - Capped at a maximum multiplier of `2.5x` for balanced fairness.
- As difficulty rises, coins and bombs fall faster and spawn at tighter intervals (dropping from 0.9s down to 0.3s).

### 🏆 High Scores & Game Over
- **60-Second Match:** Complete the 60-second timer to score as high as possible.
- **Best Score:** Saved persistently via `PlayerPrefs` and displayed on the Game Over screen with new record indicators.

---

## 🛠️ Tech Stack & Architecture
- **Engine Version:** Unity 6 (`6000.6.7f1`)
- **Physics & Movement:** 2D Kinematic Rigidbody with screen-clamp constraints and frame-rate independent physics.
- **Audio Synthesizer:** Built-in procedural sound generator for coins, clicks, explosions, and chiptune arpeggio music (runs without external WAV files).
- **Automated Tooling:**
  - `Coin Catcher > Build Game Scene`: Generates sprites, prefabs, materials, and complete scene hierarchy automatically.
  - `CoinCatcherValidate`: Automated scene and prefab validation tool for batchmode and regression tests.

---

## 🚀 How to Open and Run the Project

### Prerequisites
- Install **Unity 6** (version `6000.6.7f1` or later) via [Unity Hub](https://unity.com/download).

### Opening in Unity Hub
1. Launch **Unity Hub**.
2. Click **Add** > **Add project from disk**.
3. Select the `AD workshop1` folder.
4. Open the project with Unity 6.

### Running the Game
1. In the Project window, navigate to `Assets/Scenes/GameScene.unity` and double-click to open it.
2. If rebuilding the scene from scratch:
   - Click `Coin Catcher` > `Build Game Scene` in the top menu bar.
3. Press the **Play** button at the top of the Unity Editor.

---

## 📦 Building Standalone and WebGL

### WebGL Build (for itch.io / web)
1. In Unity, go to **File** > **Build Profiles** (or **Build Settings**).
2. Select **WebGL** and click **Switch Platform**.
3. In **Player Settings** > **Publishing Settings**:
   - Set **Compression Format** to `Disabled` or `Gzip`.
   - Enable **Decompression Fallback** if using compression.
4. Click **Build** and choose an output directory.
5. Zip the resulting build directory so `index.html` is at the root of the archive for uploading to itch.io.

### Desktop Standalone (macOS / Windows)
1. Select **macOS** or **Windows** in **Build Settings**.
2. Ensure `Assets/Scenes/GameScene.unity` is checked in the scenes list.
3. Click **Build and Run**.
