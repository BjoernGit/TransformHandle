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

### Option 1: Unity Package (Recommended)
1. Download the latest `MeshFreeHandles_v1.0.0.unitypackage` from [Releases](https://github.com/BjoernGit/TransformHandle/releases)
2. Import into Unity: Assets → Import Package → Custom Package
3. Done! No dependencies required

### Option 2: Manual Installation
1. Clone or download this repository
2. Copy the `Assets/Scripts/MeshFreeHandles` folder into your Unity project
3. That's it - the system initializes automatically

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
