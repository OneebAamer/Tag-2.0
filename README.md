# Tag 2.0 — Setup Guide

This guide will walk you through setting up the **Tag 2.0** project in Unity.

---

## Prerequisites

Before you begin, make sure you have:

- **Unity Editor** — Version **6.4** (or compatible)

---

## Create a New Project

1. Open **Unity Hub**
2. Click **New Project**
3. Select the **3D (Core)** template
4. Name your project (e.g., `tag_2.0_local`)
5. Choose a location to save the project
6. Click **Create**

---

## Import Project Files

After creating your Unity project, you need to copy the Tag 2.0 files into it.

### 1. Open Your Project Folder

1. In **Unity Hub**, locate your project
2. Click the **three dots (⋮)** next to the project name
3. Select **"Show in Explorer"** *(Windows)*  

This will open your project directory.

---

### 2. Copy GitHub Files into Assets

1. Download or clone the Tag 2.0 repository from GitHub
2. Open the downloaded repository folder
3. Copy all contents from the repo
4. Paste them into your Unity project’s `Assets` folder

> ⚠️ If prompted to overwrite files, select **Replace**

---

### 3. Return to Unity

- Go back to Unity
- Wait for assets to import and compile

> ⚠️ Compile errors are expected at this stage — do NOT launch in Safe Mode.

Once complete, you’re ready for the next step.

---

## Install Required Packages

Open the Package Manager:

1. Go to **Window → Package Manager**

Ensure the following packages are installed:

| Package | Location / Installation Method |
|--------|-------------------------------|
| **TextMeshPro** | Usually included by default. If missing: *Window → Package Manager → Unity Registry → TextMeshPro* |
| **TMP Essentials** | *Window → TextMeshPro → Import TMP Essentials* |
| **Input System** | Usually included by default. If missing: *Window → Package Manager → Unity Registry → Input System* |
| **Input System – Rebinding UI** | *Inside Input System package → Samples → Import Rebinding UI* |
| **Netcode for GameObjects** | *Window → Package Manager → Unity Registry → Netcode for GameObjects* |
| **Authentication** | *Window → Package Manager → Unity Registry → Authentication* |
| **Post Processing** | *Window → Package Manager → Unity Registry → Post Processing* |
| **Relay** | *Window → Package Manager → Unity Registry → Install via technical name: `com.unity.services.relay`* |
| **Outline by The Developer** | Custom asset. Import manually via provided package or Asset Store if missing |

---

### Notes

- Make sure the Package Manager is set to **Unity Registry** to find all required packages
- Some packages may already be installed depending on your Unity version
- If any packages are missing, install them before continuing

---

## Configure Input System

To ensure both the menu and in-game controls work:

1. Open **Project Settings**: `Edit → Project Settings…`
2. Select **Player** in the left-hand menu
3. Under **Other Settings → Configuration**, find **Active Input Handling**
4. Select **Both**  

> This allows old input methods (`Input.GetKeyDown`, `Input.GetMouseButton`) to work alongside the new Input System

5. Restart Unity when prompted
6. Verify the Event System in your scene uses `InputSystemUIInputModule`  
   - This ensures menu buttons respond to clicks

---

## Assign Player Movement Inputs

After configuring the Input System, assign movement controls for the player prefab.

### 1. Open the Player Prefab

- In the Hierarchy, select the **Player** prefab

### 2. Assign Input Actions

1. In the Inspector, locate the component handling the input (e.g., `PlayerMove`, `PlayerJump`, etc.)
2. Click the **circle icon** next to the action field to open the selection menu
3. Choose the corresponding action from your Input Actions asset
4. Repeat for all actions:  
   - PlayerMove → Move  
   - PlayerJump → Jump  
   - PlayerSprint → Sprint  
   - PlayerCrouch → Crouch

After this, the player prefab should respond to movement, jump, sprint, and crouch inputs in-game.

---

## Build and Run

Once setup is complete, you can make a build and test multiplayer:

1. Go to **File → Build Settings**
2. Click **Build Profile**
3. **Uncheck** the Sample Scene
4. Click **Build and Run**
5. Select the location where you want the build

The game will launch in a new window:

- Click **Start Host** in the build (not the lobby code)
- In the Editor, run the scene, enter the lobby code, and click **Start Client**

If everything was done correctly, you should be able to join the host session.

---

Congrats! You’re all set and ready to ruin Oneeb’s game.  
If you run into any issues, contact Oneeb.