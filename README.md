# Particle Recorder

A desktop tool for designing particle effects with **KatanaEngine** and "recording" them into sprite-sheet animations that any Katana game can load.

You tune an emitter in a WinForms editor and watch the result live in an embedded KatanaEngine viewport. Recording captures the effect frame by frame and exports a PNG sprite sheet plus an `AnimationSet` file. A game then plays the effect as a cheap animation instead of simulating particles at runtime.

> **Status:** early. The editor and the live preview work, including textured particles and alpha/additive blending. Recording and export are next (see [Roadmap](#roadmap)).

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
| UI → game | `set <Name> <value>` | Change one `ParticleSettings` value. Colors are `r,g,b,a` (0–255). `Blend` is `Alpha` or `Additive`. `Texture` is an absolute image path, or empty for circles. |
| UI → game | `restart` | Restart the preview effect. |
| game → UI | `ready` | Preview loaded. The UI responds by sending every setting. |
| game → UI | `stats <active>` | Active particle count, a few times per second. |
| game → UI | `error <text>` | A message could not be applied. |

`ParticleSettings` exists twice and must stay in sync: `ParticleRecorder/Effects/ParticleSettings.cs` (UI, source of truth) and `ParticleRecorder.Native/Source/Particles/ParticleSettings.h` (`ApplySetting`).

## Designing an effect

Every setting is in the panel on the left and applies to the live preview immediately.

| Group | Settings |
|---|---|
| Emission | `MaxParticles`, `EmissionRate` (per second) |
| Lifetime | `LifetimeMin` / `LifetimeMax` (seconds) |
| Motion | `SpeedMin` / `SpeedMax`, `Direction` (degrees, -90 = up), `Spread`, `Gravity` |
| Rotation | `RotationMin` / `RotationMax` (initial, degrees), `SpinMin` / `SpinMax` (degrees per second) |
| Appearance | `Texture`, `Blend`, `StartSize` / `EndSize` (pixels), `StartColor` / `EndColor` + `StartAlpha` / `EndAlpha` (interpolated over each particle's life) |

**Textures:** use **Load Texture...** in the toolbar, or the `...` button on the `Texture` row, to pick a PNG, JPG, BMP or TGA from your computer. **Clear Texture** goes back to plain circles. The texture is tinted by the particle color, so a white texture takes the color exactly. Size is the texture's longer side in pixels. If an image can't be loaded, the status bar says so and the preview falls back to circles.

**Blend modes:**
- **Alpha:** normal transparency. Particles cover what's behind them (smoke, dust, leaves).
- **Additive:** particles add light, so overlaps get brighter (fire, sparks, magic, glow).

KatanaEngine blends with premultiplied alpha, so soft-edged textures don't get dark fringes in either mode.

## Recording (planned)

1. **Design** the effect as above.
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
- [x] Textured particles loaded from disk, alpha/additive blending, rotation and spin

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
