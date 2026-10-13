# Particle Recorder

A desktop tool for designing particle effects with **KatanaEngine** and "recording" them into sprite-sheet animations that any Katana game can load.

You tune an emitter in a WinForms editor and watch the result live in an embedded KatanaEngine viewport. Recording captures the effect frame by frame and exports a PNG sprite sheet plus an `AnimationSet` file. A game then plays the effect as a cheap animation instead of simulating particles at runtime.

> **Status:** early. The editor, the live preview and the C# ↔ C++ message channel work. Recording and export are next (see [Roadmap](#roadmap)).

## Architecture

```
ParticleRecorder.exe (C#, WinForms, .NET 10)
├── PropertyGrid ── edits ──► ParticleSettings (C# owns all effect data)
└── KatanaViewport (KatanaEngine.WinForms) ── hosts ──► ParticleRecorder.Native.dll (C++, KatanaEngine)
        │  "set <Name> <value>", "restart"   ──►  ParticleRecorderGame → RecorderScreen → PreviewEffect
        ◄──  "ready", "stats <n>", "error <text>"
```

| Project | Language | Role |
|---|---|---|
| `ParticleRecorder/` | C# WinForms | Editor UI: settings grid, toolbar, status bar. Owns the effect data and pushes every change to the preview. |
| `ParticleRecorder.Native/` | C++ DLL | The KatanaEngine game that renders the preview (and, later, records it). Exports the hosting API via `KATANA_DEFINE_HOST_EXPORTS`. |
| `KatanaEngine/` | submodule | The engine ([rdappel/KatanaEngine](https://github.com/rdappel/KatanaEngine)), including `KatanaEngine.WinForms`, the reusable `KatanaViewport` control. |

The viewport is a real KatanaEngine/Allegro window embedded as a child of the WinForms control. That means the preview, and later the recording, uses the same `SpriteBatch` and particle code a game uses: what you record is what a game would draw. See the engine README's *Hosting a game inside a WinForms app* section for how this works.

### Message protocol

All messages are UTF-8 strings. Values use the invariant culture.

| Direction | Message | Meaning |
|---|---|---|
| UI → game | `set <Name> <value>` | Change one `ParticleSettings` value. Colors are `r,g,b,a` (0–255). |
| UI → game | `restart` | Restart the preview effect. |
| game → UI | `ready` | Preview loaded. The UI responds by sending every setting. |
| game → UI | `stats <active>` | Active particle count, a few times per second. |
| game → UI | `error <text>` | A message could not be applied. |

`ParticleSettings` exists twice and must stay in sync: `ParticleRecorder/Effects/ParticleSettings.cs` (UI, source of truth) and `ParticleRecorder.Native/Source/Particles/ParticleSettings.h` (`ApplySetting`).

## Workflow (planned)

1. **Design:** edit emission, lifetime, motion and appearance in the settings panel and watch the live preview.
2. **Set up the recording:** canvas size, frames per second, duration or frame count, warm-up time (pre-simulate so a looping effect starts "full"), and whether the animation loops.
3. **Record:** the effect is simulated with a fixed timestep (`1/fps` per frame, independent of real time) and each frame is drawn into an offscreen `RenderTarget` with a transparent background, so recordings are deterministic.
4. **Export:**
   - `<name>.png`: frames packed in a grid, `FramesPerRow` columns.
   - `<name>.anim`: an `AnimationSet` file the engine loads with `ResourceManager`:
     ```ini
     [<name>]
     FrameWidth=<w>
     FrameHeight=<h>
     FramesPerRow=<columns>
     Frames=0-<count-1>
     FrameDuration=<1/fps>
     Loop=true
     ```
     These are exactly the keys `KatanaEngine/Source/Core/Resource/AnimationSet.cpp` parses.
5. **Presets:** save and load effect settings to reuse or tweak later. The file format is still to be decided.

## Roadmap

- [x] WinForms shell with a live KatanaEngine viewport (`KatanaViewport`)
- [x] Emitter settings in a `PropertyGrid`, synced to the preview
- [ ] Recording settings (canvas, fps, duration, warm-up, loop)
- [ ] Fixed-timestep frame capture into a `RenderTarget`
- [ ] Sprite-sheet + `AnimationSet` export
- [ ] Save/load effect presets
- [ ] Preview of the exported animation (play it back through `AnimationSet`)
- [ ] Textured particles (sprite instead of circles)

**Likely engine additions** (engine-level work, in the KatanaEngine repo):
- Saving a `RenderTarget`/bitmap to PNG.
- Possibly reading pixels back from a render target.

**Ideas for later:** PNG-sequence or GIF export, helpers for seamless loops, and multiple emitters per effect.

## Building

Requirements: Visual Studio 2026 (C++ desktop and .NET desktop workloads, v145 toolset) and the .NET 10 SDK. x64 only.

```powershell
git clone --recurse-submodules <repo-url> ParticleRecorder   # or: git submodule update --init
```

Open `ParticleRecorder.slnx` and build `x64` (Debug or Release). The first build restores Allegro through NuGet (pinned in `KatanaEngine/packages.config`). The C# project builds the native DLL first and copies it, along with the Allegro DLLs, next to `ParticleRecorder.exe`. Run the `ParticleRecorder` project.

## Working on the engine

`KatanaEngine/` is a submodule shared with other games (e.g. Altars of Eternity):

1. `cd KatanaEngine && git checkout master` (submodules start detached).
2. Make the change, then commit and push in the engine repo.
3. Commit the updated `KatanaEngine` pointer here.
4. Other projects pick it up with `git submodule update --remote KatanaEngine`.
