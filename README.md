# 🪙 Coin Catcher

A fast-paced 2D arcade game built in **Unity 6**. Control the basket at the bottom of the screen and catch as many falling coins as you can before the time runs out!

---

## 🎮 Play Online
- **Play Link (itch.io):** [Play Coin Catcher on itch.io](https://your-username.itch.io/coin-catcher) *(Replace with your itch.io link)*

---

## 📸 Screenshots
*(Add screenshots or gameplay GIFs here)*

| Main Menu | Gameplay | Game Over |
| :---: | :---: | :---: |
| ![Main Menu](Screenshots/main_menu.png) | ![Gameplay](Screenshots/gameplay.png) | ![Game Over](Screenshots/game_over.png) |

---

## 🕹️ Controls
- **Move Left:** `Left Arrow` or `A`
- **Move Right:** `Right Arrow` or `D`
- Supports both Unity's New Input System and Legacy Input Manager.

---

## ⏱️ Rules & Gameplay
- **60-Second Countdown:** You have exactly 60 seconds to score as many points as possible.
- **Catch Coins:** Each caught coin adds to your score and triggers a responsive squash-and-stretch catch animation with audio feedback.
- **Dynamic Difficulty:**
  - Difficulty increases dynamically based on:
    - Collected coins (`+0.03` difficulty per coin)
    - Elapsed time (`+0.01` difficulty per second)
    - Capped at a maximum multiplier of `2.5x` for balanced fairness.
  - As difficulty increases, coins fall faster and spawn at tighter intervals (dropping from 0.9s down to 0.3s).
- **High Scores:** Best score is saved locally via `PlayerPrefs` and displayed on the Game Over screen.

---

## 🛠️ Project Details & Tech Stack
- **Engine Version:** Unity 6 (`6000.6.7f1`)
- **Physics & Rendering:** 2D Kinematic Rigidbody movement, Screen-space UI scaling (1920x1080 reference), unlit custom shaders/materials.
- **Editor Automation:** Features an automated setup tool under `Coin Catcher > Build Game Scene` that generates procedural sprites, particle prefabs, UI, and scene wiring.

---

## 🚀 How to Open and Run the Project

1. **Prerequisites:**
   - Install **Unity 6** (version `6000.6.7f1` or later) via [Unity Hub](https://unity.com/download).
2. **Open in Unity Hub:**
   - Launch Unity Hub.
   - Click **Add** > **Add project from disk**.
   - Select the `AD workshop1` folder.
   - Click to open the project with Unity 6.
3. **Run the Game:**
   - In the Project window, navigate to `Assets/Scenes/GameScene.unity` and double-click to open it.
   - *(Optional setup)* If setting up from scratch:
     1. Go to `Window` > `TextMeshPro` > `Import TMP Essential Resources`.
     2. In the top menu, click `Coin Catcher` > `Build Game Scene`.
   - Click the **Play** button at the top of the Unity Editor.
