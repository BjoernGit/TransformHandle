# Unity Runtime Transform Handles

A professional runtime transform handle system for Unity, inspired by Unity's editor transform tools. Manipulate GameObjects at runtime with visual handles for translation, rotation, and scale (coming soon).

## ✨ Features

- 🎯 **Translation Handles** - Move objects along X, Y, Z axes with arrow handles
- 🔄 **Rotation Handles** - Rotate objects with circular handles
- 🌍 **Local/Global Space** - Toggle between local and world space (X key)
- 🎨 **Visual Feedback** - Hover highlighting and thick, colored lines
- ⌨️ **Editor-Style Controls** - W for translation, E for rotation, just like Unity Editor
- 📐 **Accurate Ellipse Tangents** - Proper rotation even with extreme camera angles
- 🚀 **Performant** - GL-based rendering, no GameObjects needed

## 📦 Installation

### Recommended: Download the Scripts
1. Download this repository as ZIP
2. Extract and copy the `Assets/Scripts/TransformHandle` folder into your Unity project
3. Make sure you have the Input System package installed

### Coming Soon: Unity Package
.unitypackage releases are planned for easier installation

### Option 2: Manual Installation
1. Clone or download this repository
2. Copy the `Assets/Scripts/TransformHandle` folder into your Unity project

## 🚀 Quick Start

1. Add the `TransformHandleManager` component to any GameObject
2. Assign your target transform in the inspector
3. Run the scene and use:
   - **W** - Switch to translation mode
   - **E** - Switch to rotation mode  
   - **X** - Toggle Local/Global space


## 🗺️ Roadmap

- [ ] Scale handles
- [ ] Multi-object selection
- [ ] Per-axis space configuration (mix local/global per axis)
- [ ] Axis constraints and locking


---

⭐ If you find this useful, please consider giving it a star!
