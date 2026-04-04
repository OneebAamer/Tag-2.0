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
4. Name your project (e.g. `tag_2.0_local`)
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

Once complete, you’re ready for the next step.

---
## Install Required Packages

Open the Package Manager:

Ensure the following packages are installed:

| Package | Location / Installation Method |
|--------|-------------------------------|
| **TextMeshPro** | Usually included by default. If missing: *Window → Package Manager → Unity Registry → TextMeshPro* |
| **TMP Essentials** | *Window → TextMeshPro → Import TMP Essentials* |
| **Input System** | Usually included by default. If missing: *Window → Package Manager → Unity Registry → Input System* |
| **Input System – Rebinding UI** | *Inside Input System package → Import Rebinding UI* |
| **Netcode for GameObjects** | *Window → Package Manager → Unity Registry → Netcode for GameObjects* |
| **Authentication** | *Window → Package Manager → Unity Registry → Authentication* |
| **Post Processing** | *Window → Package Manager → Unity Registry → Post Processing* |
| **Relay** | *Window → Package Manager → Unity Registry → Install via technical name: `com.unity.services.relay`* |
---

### Notes

- Make sure the Package Manager is set to **Unity Registry** to find all required packages
- Some packages may already be installed depending on your Unity version
- If any packages are missing, install them before continuing

---