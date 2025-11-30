# Mesh-Free Transform Handles for Unity
Lightweight immediate mode rendered handles without GameObjects or Meshes

A high-performance runtime transform handle system for Unity, inspired by Unity's editor transform tools. Manipulate GameObjects at runtime with visual handles for translation, rotation, and scale - all without creating a single mesh or GameObject.

![DemoCubeRotations](https://github.com/user-attachments/assets/49f689b6-0f94-434a-a608-2b7d8a08e8a7)

## ✨ Basic Features

- 🎯 **Translation Handles** - Move objects along X, Y, Z axes with arrow handles
- 🔄 **Rotation Handles** - Rotate objects with circular handles
- 📏 **Scale Handles** - Scale objects with box handles and uniform scaling
- 🌍 **Local/Global Space** - Toggle between local and world space (X key)
- ⌨️ **Editor-Style Controls** - W for translation, E for rotation, R for scale
- 🚀 **Performant** - GL-based rendering, no GameObjects needed

## 🌟 Special Features
- 🎛️ **Mixed Space Handle Profiles** - Use local X with global Y/Z on the same object
- 🚫 **Selective Axes** - Disable specific handles per object (e.g., no Y rotation)
- 🏷️ **Per-Object Settings** - Each object can have unique handle behavior
- 💾 **ScriptableObject Based** - Create handle presets as project assets

## 📦 Installation

### Option 1: Package Manager (Recommended)
1. Open the Unity Package Manager (`Window → Package Manager`)
2. Click the **+** button → **Add package from git URL**
3. Enter: `https://github.com/BjoernGit/TransformHandle.git?path=/Packages/com.bjoerngit.transformhandle#v1.2.1`
4. Click **Add** – Unity will download and import automatically
5. Done! No dependencies required

### Option 2: Manual Installation via manifest.json
Add the following to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.bjoerngit.transformhandle": "https://github.com/BjoernGit/TransformHandle.git?path=/Packages/com.bjoerngit.transformhandle#v1.2.1"
  }
}

```

### Option 3: Manual Installation
1. Clone or download this repository
2. Copy the `Assets/Scripts/MeshFreeHandles` folder into your Unity project
3. That's it - the system initializes automatically

Import Sample Scene (Optional)

1. In Package Manager, select Transform Handle
2. Switch to the Samples tab
3. Click Import next to "Sample Scene"
4. Find the scene in `Assets/Samples/Transform Handle/1.0.0/Sample Scene/`

## 🎮 Input System Configuration

This project uses Unity's **legacy (old) Input System** for maximum compatibility.

When using this package in a project that is set up with the **new Input System only**, input events may not work as expected.  
In this case, Unity needs to be configured to support **both input systems**.

#### How to fix

1. Open **Edit → Project Settings**
2. Navigate to **Player**
3. Set **Active Input Handling** to **Both**
4. Restart Unity when prompted

This allows the package to work correctly while keeping compatibility with projects that already rely on the new Input System.

<img width="604" height="265" alt="image" src="https://github.com/user-attachments/assets/41c830b8-000a-43ab-b78e-9e56a74a97ca" />


## 🚀 Quick Start

There is an included `TransformHandleKeyManager` and a `SelectionManager` to directly use this package.
However you will mostlikely need to implement the behaviour in your own selection system. For this you can use the following hints:

Implement `MeshFreeHandles`:
```csharp
using MeshFreeHandles;
```

Chose implementation that fits your application best:

```csharp
void Update()
{
   if (Input.GetMouseButtonDown(0))
   {
       if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
       {
            // Extension method - easiest way
            hit.transform.ShowHandles();
       }
   }
}
```

Or: 

```csharp
void Update()
{
   if (Input.GetMouseButtonDown(0))
   {
       if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
       {
            // Or using the singleton directly
            TransformHandleManager.Instance.SetTarget(hit.transform);
       }
   }
}

```

And include the basic handle switches in your Update:

```csharp
    // Keyboard controls
    if (Input.GetKeyDown(KeyCode.W)) TransformHandleManager.Instance.SetTranslationMode();
    if (Input.GetKeyDown(KeyCode.E)) TransformHandleManager.Instance.SetRotationMode();
    if (Input.GetKeyDown(KeyCode.R)) TransformHandleManager.Instance.SetScaleMode();
    if (Input.GetKeyDown(KeyCode.X)) TransformHandleManager.Instance.ToggleHandleSpace();
    if (Input.GetKeyDown(KeyCode.Escape)) TransformHandleManager.Instance.ClearTarget();
```



Or use the already implemented "Transform Handle Key Manager.
For flexibility this is optional:

![grafik](https://github.com/user-attachments/assets/61fd9cbf-f47b-4331-85fa-3034165a1140)

## 🗺️ Roadmap
- [ ] Multiple simultanious handles

---
⭐ If you find this useful, please consider giving it a star!
