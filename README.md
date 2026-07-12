# Cook Cook Revolution

VR cooking game prototype built in Unity.

This project is designed to be played in VR. Mouse and keyboard support is included as a supplemental desktop test mode so reviewers can inspect the core loop, controls, and scene behavior without a headset. For the intended experience, VR is recommended.

The main scene is:

```text
Assets/Scenes/VR Test Scene (Recovered).unity
```

## Unity Version

Open the project with Unity `6000.3.6f1`.

## Project Setup

1. Clone or download this repository.
2. Open the repository root in Unity Hub.
3. Open `Assets/Scenes/VR Test Scene (Recovered).unity`.
4. Press Play in the Unity Editor.

On first open, Unity will regenerate the local `Library/` folder and restore packages from `Packages/manifest.json`. This can take several minutes and is expected.

## Controls

### VR

Recommended play mode. Use a VR headset and XR controllers/hands through the XR Interaction Toolkit setup.

### Desktop Test Mode

Supplemental mode for understanding and testing the game without VR hardware. In the Unity Editor, a desktop camera/controller is created automatically for mouse and keyboard testing.

- `Enter` / `Space`: start from the title screen
- `WASD`: move
- `Shift`: move faster
- `Space`: move upward
- `Ctrl`: move downward
- Mouse: look around
- Left click: grab or drop objects
- Mouse wheel: adjust held object distance
- `G`: drop held object
- `E`: interact with buttons, stove knobs, the fridge, or the bell
- `Esc`: pause

## Repository Notes

Only commit source project files such as `Assets/`, `Packages/`, and `ProjectSettings/`.
Generated Unity folders such as `Library/`, `Temp/`, `Logs/`, and `UserSettings/` are ignored because they are local cache/settings data and are recreated by Unity.

The root-level `Audio Assets/` folder is ignored because it contains large raw source audio outside Unity's imported `Assets/` folder. Move any needed audio into `Assets/` or use Git LFS if those raw files need to be published.
