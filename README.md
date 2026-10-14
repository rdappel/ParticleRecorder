# Particle Recorder

A desktop tool for designing particle effects with **KatanaEngine** and "recording" them into sprite-sheet animations that any Katana game can load.

You tune an emitter in a WinForms editor and watch the result live in an embedded KatanaEngine viewport. Recording captures the effect frame by frame, plays it back for you to check, and saves a PNG sprite sheet plus an `AnimationSet` file. A game then plays the effect as a cheap animation instead of simulating particles at runtime.

> **Status:** early but usable: design an effect, save it, record it, preview the animation, and save it as a sprite sheet + `.anim`. A movable emitter is next (see [Roadmap](#roadmap)).

## Architecture

```
ParticleRecorder.exe (C#, WinForms, .NET 10)
├── PropertyGrid ── edits ──► ParticleSettings (C# owns all effect data)
└── KatanaViewport (KatanaEngine.WinForms) ── hosts ──► ParticleRecorder.Native.dll (C++, KatanaEngine)
        │  "set", "restart", "measure", "record", "preview", "save", "close"
        │      ──►  ParticleRecorderGame → RecorderScreen (PreviewEffect, Recorder) / AnimationPreviewScreen
        ◄──  "ready", "stats", "measured", "recorded", "frame", "saved", "error"
```

| Project | Language | Role |
|---|---|---|
| `ParticleRecorder/` | C# WinForms | Editor UI: menu bar, settings grid, Recording Settings dialog, animation preview bar, status bar. Owns the effect data (and its `.keparticle` files) and pushes every change to the preview. |
| `ParticleRecorder.Native/` | C++ DLL | The KatanaEngine game that renders the live effect, records it into a sprite sheet in memory, plays the recording back, and saves it. Exports the hosting API via `KATANA_DEFINE_HOST_EXPORTS`. |
| `KatanaEngine/` | submodule | The engine ([rdappel/KatanaEngine](https://github.com/rdappel/KatanaEngine)), including `KatanaEngine.WinForms`, the reusable `KatanaViewport` control. |

The viewport is a real KatanaEngine/Allegro window embedded as a child of the WinForms control. That means the preview and the recording use the same `SpriteBatch` and particle code a game uses: what you record is what a game would draw. See the engine README's *Hosting a game inside a WinForms app* section for how this works.

### Message protocol

All messages are UTF-8 strings. Values use the invariant culture.

| Direction | Message | Meaning |
|---|---|---|
| UI → game | `set <Name> <value>` | Change one `ParticleSettings` value. Colors are `r,g,b,a` (0–255). `Blend` is `Alpha` or `Additive`. `Texture` is an absolute image path, or empty for circles. |
| UI → game | `restart` | Restart the preview effect. |
| UI → game | `measure <options>` | Simulate a recording and fit the canvas to it. Options are `key=value` pairs: `seed`, `fps`, `duration`, `prewarm` (seconds, 0 = off), `padding`, `columns` (0 = auto), `loop` (0/1). |
| UI → game | `record <options>` | Record into memory (same options as `measure`) and show the animation preview over the live effect. Replaces any previous recording. |
| UI → game | `preview play` / `preview pause` | Start or stop playback. Playing a finished non-looping animation starts it again. |
| UI → game | `preview frame <index>` | Show a frame (0-based). |
| UI → game | `preview background dark\|light\|checker` | What is drawn behind the frame. |
| UI → game | `preview zoom <scale>` | Draw scale; `0` fits the frame (actual size, or smaller if it doesn't fit). |
| UI → game | `save <png>` | Write the recording as `<png>` plus a matching `.anim`. The path is the rest of the message and may contain spaces. |
| UI → game | `close` | Close the animation preview and show the live effect again. |
| game → UI | `ready` | Preview loaded. The UI responds by sending every setting. |
| game → UI | `stats <active>` | Active particle count, a few times per second. |
| game → UI | `measured <frames> <frameW> <frameH> <sheetW> <sheetH>` | Reply to `measure`. |
| game → UI | `recorded <frames> <frameW> <frameH> <sheetW> <sheetH>` | Reply to `record`: the preview is showing. |
| game → UI | `frame <index> <playing>` | The preview's frame and whether it is playing (`0`/`1`), sent whenever either changes. |
| game → UI | `saved` | Reply to `save`: the files were written. |
| game → UI | `error <text>` | A message could not be applied (during `measure`, `record` or `save`, that step failed). |

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

**Textures:** use **Edit > Load Texture...**, or the `...` button on the `Texture` row, to pick a PNG, JPG, BMP or TGA from your computer. **Edit > Clear Texture** goes back to plain circles. The texture is tinted by the particle color, so a white texture takes the color exactly. Size is the texture's longer side in pixels. If an image can't be loaded, the status bar says so and the preview falls back to circles.

**Blend modes:**
- **Alpha:** normal transparency. Particles cover what's behind them (smoke, dust, leaves).
- **Additive:** particles add light, so overlaps get brighter (fire, sparks, magic, glow).

KatanaEngine blends with premultiplied alpha, so soft-edged textures don't get dark fringes in either mode.

### Menu

| Menu | Items |
|---|---|
| File | New Effect (Ctrl+N), Open Effect... (Ctrl+O), Save Effect (Ctrl+S), Save Effect As... (Ctrl+Shift+S), Record Animation (Ctrl+R), Save Animation... (Ctrl+E, while previewing), Exit |
| Edit | Load Texture..., Clear Texture, Recording Settings... |
| View | Restart Preview (F5; while previewing a recording, plays it from the start), Settings Panel (show/hide the grid) |

The title bar shows the effect's name, with `*` when there are unsaved changes; New, Open and closing the window ask before discarding them.

## Recording

1. **Design** the effect as above.
2. **Edit > Recording Settings...:**
   - **Frames per second** and **Duration**: the dialog shows the resulting frame count.
   - **Prewarm**: when ticked, the effect is simulated for the given number of seconds before the first frame, so it starts "full" instead of growing from nothing. Use it for looping effects; untick it for effects that should start empty (bursts, explosions).
   - **Loop**: written to the `.anim` file.
   - **Padding**: transparent pixels around the effect.
   - **Frames per row**: 0 picks the layout that keeps the sheet closest to square.
   - There is no canvas size: it is **fitted automatically** to every particle that is visible at any point in the animation.
3. **File > Record Animation** (Ctrl+R). The tool:
   - simulates the recording once to measure it (fixed `1/fps` step and a fixed random seed, so it's repeatable and independent of the live preview);
   - **warns if the sheet would be larger than 4096 px** on either side, which some graphics cards can't load. You can record anyway; there's no hard limit;
   - records the same simulation frame by frame into a transparent frame and packs the frames into a sheet in memory;
   - opens the **animation preview**.
4. **Check it in the preview.** The viewport plays the recording exactly as a game would (one frame every `1/fps` seconds, with the effect's blend, holding the last frame if Loop is off). The bar above it has:
   - **Play/Pause**, **◀ ▶** to step one frame, and a slider to scrub (arrow keys step while it has focus);
   - **Background:** Dark, Light, or Checkerboard (shows exactly which pixels are transparent). The thin outline is the frame's auto-fitted edge;
   - **Zoom:** Fit (actual size, or smaller if the frame doesn't fit), 100%, 200% or 400% (pixels stay sharp);
   - **Record Again**: a new recording with a new random seed, picking up any changes to the effect or recording settings (the status bar says when the effect has changed since the recording);
   - **Back to Editor**: discards the recording and returns to the live effect.
5. **Save...** (or **File > Save Animation...**, Ctrl+E) and choose a `.png` name. It writes `<name>.png` and `<name>.anim` side by side, from exactly the frames you previewed. You can keep previewing and save again.

The `.anim` file uses exactly the keys `KatanaEngine/Source/Core/Resource/AnimationSet.cpp` parses:

```ini
[<name>]
FrameWidth=<w>
FrameHeight=<h>
FramesPerRow=<columns>
Frames=0-<count-1>
FrameDuration=<1/fps>
Loop=true
```

The PNG has straight (non-premultiplied) alpha, as image files normally do; KatanaEngine premultiplies it when loading. Draw it in a game with the same blend the effect used: an **Additive** effect should be drawn with `BlendState::ADDITIVE`, or it will look darker than in the preview.

## Effect files

**File > Save Effect** writes a `.keparticle` file: JSON with every particle setting and the recording settings. The texture path is stored relative to the effect file, so keep textures next to (or below) your effects and move them together. Settings missing from a file get their defaults, so files from older versions keep opening.

## Roadmap

- [x] WinForms shell with a live KatanaEngine viewport (`KatanaViewport`)
- [x] Emitter settings in a `PropertyGrid`, synced to the preview
- [x] Textured particles loaded from disk, alpha/additive blending, rotation and spin
- [x] Menu bar, effect files (`.keparticle`)
- [x] Recording settings, auto-fit canvas, prewarm
- [x] Deterministic frame capture and sprite-sheet + `.anim` export
- [x] Animation preview before saving: playback, frame stepping, backgrounds, zoom
- [ ] **Emitter path:** draw a path with a pen tool (a group of splines; the engine already has `Spline`), and the emitter moves along it during the animation. The recorder already takes the emitter's position from a single function (`EmitterPositionAt` in `Recorder.cpp`) for this.

**Ideas for later:** PNG-sequence or GIF export, helpers for seamless loops, and multiple emitters per effect.

## Building

Requirements: Visual Studio 2026 (C++ desktop and .NET desktop workloads, v145 toolset) and the .NET 10 SDK. x64 only.

The engine is a git submodule, so clone with `--recurse-submodules`:

```powershell
git clone --recurse-submodules https://github.com/rdappel/ParticleRecorder.git
```

**Already cloned without it?** A plain `git clone` (or some IDE clone dialogs) leaves the `KatanaEngine/` folder empty. Visual Studio then reports the KatanaEngine projects as not found, and the build stops with *"The KatanaEngine submodule is missing"*. Fix it from the `ParticleRecorder` folder, then reload the solution:

```powershell
git submodule update --init
```

Optionally, run `git config --global submodule.recurse true` once so `git pull` and `git checkout` keep the submodule in sync automatically. Otherwise, after pulling a commit that moves the engine pointer, run `git submodule update` again.

Open `ParticleRecorder.slnx` and build `x64` (Debug or Release). The first build restores Allegro through NuGet (pinned in `KatanaEngine/packages.config`). The C# project builds the native DLL first and copies it, along with the Allegro DLLs, next to `ParticleRecorder.exe`. Run the `ParticleRecorder` project.

## Working on the engine

`KatanaEngine/` is a submodule shared with other games (e.g. Altars of Eternity):

1. `cd KatanaEngine && git checkout master` (submodules start detached).
2. Make the change, then commit and push in the engine repo.
3. Commit the updated `KatanaEngine` pointer here.
4. Other projects pick it up with `git submodule update --remote KatanaEngine`.
