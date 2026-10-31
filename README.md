# 🪙 Coin Catcher

## 🎮 Play Coin Catcher
Play the game online: **[Play Coin Catcher](https://sachityadav-2007.github.io/AD-workshop1/)**

---

## 📖 About the Game
**Coin Catcher** is a fast-paced 2D arcade game built in **Unity 6**. Take control of the basket at the bottom of the screen, catch falling gold coins to earn points, and dodge dangerous explosive bombs before the 60-second timer runs out!

---

## 🎯 Game Objective & How to Play
- **Catch Coins:** Collect as many falling coins as possible within the 60-second limit to achieve the highest score. Each coin awards **+1 point**.
- **Avoid Bombs:** Black bombs with red stripes and sparks drop among coins. Catching a bomb triggers an explosion burst and flashes your basket red.
- **3 Strikes Rule:** Catching **3 bombs** immediately ends the game with a `"BOOM! You caught 3 bombs!"` Game Over screen!
- **Dynamic Difficulty:** Coins and bombs fall faster and spawn at tighter intervals (from 0.9s down to 0.3s) as time passes and your score increases.
- **High Scores:** Your best score is saved locally via `PlayerPrefs` and displayed on the Game Over screen.

---

## 🕹️ Controls
- **Move Left:** `Left Arrow` or `A`
- **Move Right:** `Right Arrow` or `D`
- **Input System:** Fully responsive with smooth acceleration and screen-boundary clamping across both Unity New Input System and Legacy Input Manager.

---

## 🛠️ Technologies Used
- **Engine:** Unity 6 (`6000.6.7f1`)
- **Language:** C#
- **Platform:** WebGL (HTML5, WebAssembly / WASM) & Desktop Standalone
- **Audio:** Custom procedural synthesizer generating dynamic sound effects and 8-bit chiptune arpeggio music directly in code
- **UI:** Unity TextMesh Pro with responsive Canvas Scaler (1920x1080 reference resolution)
- **CI/CD & Hosting:** GitHub Actions & GitHub Pages

---

## 🚀 Running the Project Locally

### 1. Prerequisites
- Install **Unity 6** (version `6000.6.7f1` or later) via [Unity Hub](https://unity.com/download).

### 2. Opening in Unity
1. Clone or download this repository:
   ```bash
   git clone https://github.com/sachityadav-2007/AD-workshop1.git
   ```
2. Open **Unity Hub**, click **Add** > **Add project from disk**, and select the project folder.
3. Open the project with Unity 6.

### 3. Playing in the Editor
1. In the Project window, navigate to `Assets/Scenes/GameScene.unity` and double-click to open it.
2. Click the **Play** button at the top of the Unity Editor.

### 4. Rebuilding for WebGL
In the top menu bar of Unity, click:
- **`Coin Catcher` > `Build WebGL`**: Generates a fresh WebGL build directly into the `docs/` folder ready for web deployment.
- Or run locally with a test server:
  ```bash
  python3 -m http.server 8080 --directory docs
  ```
  Then visit `http://localhost:8080` in your web browser.
