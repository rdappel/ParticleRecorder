# Particle Recorder

A desktop tool for designing particle effects with **KatanaEngine** and "recording" them into sprite-sheet animations that any Katana game can load.

You tune an emitter in a WinForms editor and watch the result live in an embedded KatanaEngine viewport. Recording captures the effect frame by frame, plays it back for you to check, and saves a PNG sprite sheet plus an `AnimationSet` file. A game then plays the effect as a cheap animation instead of simulating particles at runtime.

> **Status:** early but usable: design an effect, save it, record it, preview the animation, and save it as a sprite sheet + `.anim`. The emitter can follow a path drawn with a pen tool.

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
| `ParticleRecorder/` | C# WinForms | Editor UI: menu bar, settings grid, path and animation preview bars, status bar. Owns the effect data (and its `.keparticle` files) and pushes every change to the preview. |
| `ParticleRecorder.Native/` | C++ DLL | The KatanaEngine game that renders the live effect, records it into a sprite sheet in memory, plays the recording back, and saves it. Exports the hosting API via `KATANA_DEFINE_HOST_EXPORTS`. |
| `KatanaEngine/` | submodule | The engine ([rdappel/KatanaEngine](https://github.com/rdappel/KatanaEngine)), including `KatanaEngine.WinForms`, the reusable `KatanaViewport` control. |

The viewport is a real KatanaEngine/Allegro window embedded as a child of the WinForms control. That means the preview and the recording use the same `SpriteBatch` and particle code a game uses: what you record is what a game would draw. See the engine README's *Hosting a game inside a WinForms app* section for how this works.

### Message protocol

All messages are UTF-8 strings. Values use the invariant culture.

| Direction | Message | Meaning |
|---|---|---|
| UI → game | `set <Name> <value>` | Change one `ParticleSettings` value. Colors are `r,g,b,a` (0–255). `Blend` is `Alpha` or `Additive`. Booleans (`Prewarm`, `DieOut`) are `0`/`1`. `Texture` is an absolute image path, or empty for circles. `Path` is `<closed 0\|1> x,y,inX,inY,outX,outY,rate;...` (handles are offsets from the point, rate is %); empty after the flag = no path. |
| UI → game | `restart` | Restart the preview effect. |
| UI → game | `pen on` / `pen off` | Path editing: forward the mouse to the UI, draw points and handles. |
| UI → game | `pen select <index>` | Highlight a path point (`-1` = none). |
| UI → game | `measure <options>` | Simulate a recording and fit the canvas to it. Options are `key=value` pairs: `seed`, `fps`, `duration`, `prewarm` (seconds, 0 = off), `padding`, `columns` (0 = auto), `loop` (0/1), `dieout` (0/1). |
| UI → game | `record <options>` | Record into memory (same options as `measure`) and show the animation preview over the live effect. Replaces any previous recording. |
| UI → game | `preview play` / `preview pause` | Start or stop playback. Playing after playback has finished starts it again. |
| UI → game | `preview loop 0\|1` | Whether playback repeats (default `0`: stop on the last frame). Preview only; doesn't change the `.anim`. |
| UI → game | `preview frame <index>` | Show a frame (0-based). |
| UI → game | `preview background dark\|light\|checker` | What is drawn behind the frame. |
| UI → game | `preview zoom <scale>` | Draw scale; `0` fits the frame (actual size, or smaller if it doesn't fit). |
| UI → game | `save <png>` | Write the recording as `<png>` plus a matching `.anim`. The path is the rest of the message and may contain spaces. |
| UI → game | `close` | Close the animation preview and show the live effect again. |
| game → UI | `ready` | Preview loaded. The UI responds by sending every setting. |
| game → UI | `stats <active>` | Active particle count, a few times per second. |
| game → UI | `mouse down\|up <x> <y> left\|right`, `mouse move <x> <y>` | Pen mode only: mouse input on the viewport, in effect space (pixels from the emitter's resting point). Moves are sent only while a button is held. |
| game → UI | `measured <frames> <frameW> <frameH> <sheetW> <sheetH>` | Reply to `measure`. |
| game → UI | `recorded <frames> <frameW> <frameH> <sheetW> <sheetH>` | Reply to `record`: the preview is showing. |
| game → UI | `frame <index> <playing>` | The preview's frame and whether it is playing (`0`/`1`), sent whenever either changes. |
| game → UI | `saved` | Reply to `save`: the files were written. |
| game → UI | `error <text>` | A message could not be applied (during `measure`, `record` or `save`, that step failed). |

`ParticleSettings` exists twice and must stay in sync: `ParticleRecorder/Effects/ParticleSettings.cs` (UI, source of truth) and `ParticleRecorder.Native/Source/Particles/ParticleSettings.h` (`ApplySetting`).

## Designing an effect

Every setting is in the panel on the left and applies to the live preview immediately.

The live preview plays the animation **as it will be recorded**, on repeat: each cycle starts fresh (prewarmed if `Prewarm` is on), emits for `Duration` while the emitter follows its path, lets the particles die out if `DieOut` is on, then starts again. So you see prewarm, die-out and the cut at the end of the animation while you design. (The random particles differ each cycle.)

| Group | Settings |
|---|---|
| Emission | `MaxParticles`, `EmissionRate` (per second) |
| Lifetime | `LifetimeMin` / `LifetimeMax` (seconds) |
| Motion | `SpeedMin` / `SpeedMax`, `Direction` (degrees, -90 = up), `Spread`, `Gravity` |
| Rotation | `RotationMin` / `RotationMax` (initial, degrees), `SpinMin` / `SpinMax` (degrees per second) |
| Appearance | `Texture`, `Blend`, `StartSize` / `EndSize` (pixels), `StartColor` / `EndColor` + `StartAlpha` / `EndAlpha` (interpolated over each particle's life) |
| Timing | `Duration` (seconds of emitting; the emitter covers its path once), `Prewarm` + `PrewarmSeconds` (start "full": simulate before the first frame; turn off for effects that start empty, like bursts), `DieOut` (after Duration, stop emitting until every particle has died, so the animation ends empty instead of cutting off), `Loop` (written to the `.anim`), `Frames` (read-only: the frame count, `+ die-out` when that adds more) |
| Sprite Sheet | `Fps`, `Padding` (transparent pixels around the effect), `FramesPerRow` (0 = the most square sheet). There is no canvas size: each frame is **fitted automatically** to every particle visible at any point in the animation. |

**Textures:** use **Edit > Load Texture...**, or the `...` button on the `Texture` row, to pick a PNG, JPG, BMP or TGA from your computer. **Edit > Clear Texture** goes back to plain circles. The texture is tinted by the particle color, so a white texture takes the color exactly. Size is the texture's longer side in pixels. If an image can't be loaded, the status bar says so and the preview falls back to circles.

**Blend modes:**
- **Alpha:** normal transparency. Particles cover what's behind them (smoke, dust, leaves).
- **Additive:** particles add light, so overlaps get brighter (fire, sparks, magic, glow).

KatanaEngine blends with premultiplied alpha, so soft-edged textures don't get dark fringes in either mode.

### Menu

| Menu | Items |
|---|---|
| File | New Effect (Ctrl+N), Open Effect... (Ctrl+O), Save Effect (Ctrl+S), Save Effect As... (Ctrl+Shift+S), Record Animation (Ctrl+R), Save Animation... (Ctrl+E, while previewing), Exit |
| Edit | Load Texture..., Clear Texture, Emitter Path (Ctrl+P) |
| View | Restart Preview (F5; while previewing a recording, plays it from the start), Settings Panel (show/hide the grid) |

The title bar shows the effect's name, with `*` when there are unsaved changes; New, Open and closing the window ask before discarding them.

## Emitter path

By default the emitter stays still. **Edit > Emitter Path** (Ctrl+P) lets you draw a path for it with a pen tool, right on the viewport; the live effect follows the path while you edit. A bar above the viewport has **Done**, **Closed (loop)**, **Clear Path**, the selected point's **rate**, and **Delete Point**.

| Gesture | Does |
|---|---|
| Click empty space | Add a corner point at the end of the path |
| Click and drag empty space | Add a smooth point and pull out its handles |
| Click the path | Insert a point there (the curve keeps its shape) |
| Click the green start point | Close the path into a loop |
| Drag a point / a handle | Move it. A smooth point keeps its two handles in line |
| Double-click a point | Toggle corner (sharp) / smooth |
| Right-click a point | Delete it |
| Click a point | Select it, to set its rate |

(The viewport can't receive keyboard input, so there are no modifier keys.)

- **Open path:** the emitter travels from the start point to the end once per animation. With prewarm, it waits at the start during the prewarm. Turn on `DieOut` so the animation doesn't end abruptly when the emitter reaches the end.
- **Closed path:** the emitter goes round the loop once per animation, and prewarm continues round the loop, so a looping animation joins up seamlessly.
- **Speed:** the emitter covers the path at a constant speed over `Duration`. A longer path or a shorter Duration means a faster emitter.
- **Rate:** each point has an emission rate as a percentage of `EmissionRate` (default 100%). It blends smoothly between points along the path. The path is drawn thicker where the rate is higher; 0% stops emission there.
- Particles emitted during one step are spread along the emitter's movement, so a fast emitter leaves an even trail.
- The canvas still fits automatically, so it grows to cover wherever the emitter goes.

## Recording

1. **Design** the effect as above, including its **Timing** and **Sprite Sheet** settings.
2. **File > Record Animation** (Ctrl+R). The tool:
   - simulates the recording once to measure it (fixed `1/fps` step and a fixed random seed, so it's repeatable and independent of the live preview);
   - **warns if the sheet would be larger than 4096 px** on either side, which some graphics cards can't load. You can record anyway; there's no hard limit;
   - records the same simulation frame by frame into a transparent frame and packs the frames into a sheet in memory;
   - opens the **animation preview**.
3. **Check it in the preview.** The viewport plays the recording exactly as a game would (one frame every `1/fps` seconds, with the effect's blend). The bar above it has:
   - **Play/Pause**, **◀ ▶** to step one frame, and a slider to scrub (arrow keys step while it has focus);
   - **Loop:** off by default, so playback runs once and stops on the last frame (Play starts it again). Tick it to keep the animation repeating. This only affects the preview; the `.anim` file's Loop comes from the `Loop` setting;
   - **Background:** Dark, Light, or Checkerboard (shows exactly which pixels are transparent). The thin outline is the frame's auto-fitted edge;
   - **Zoom:** Fit (actual size, or smaller if the frame doesn't fit), 100%, 200% or 400% (pixels stay sharp);
   - **Record Again**: a new recording with a new random seed, picking up any changes to the effect (the status bar says when the effect has changed since the recording);
   - **Back to Editor**: discards the recording and returns to the live effect.
4. **Save...** (or **File > Save Animation...**, Ctrl+E) and choose a `.png` name. It writes `<name>.png` and `<name>.anim` side by side, from exactly the frames you previewed. You can keep previewing and save again.

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

**File > Save Effect** writes a `.keparticle` file: JSON with every setting, including timing, sprite sheet and the emitter path. The texture path is stored relative to the effect file, so keep textures next to (or below) your effects and move them together. Settings missing from a file get their defaults, so files from older versions keep opening (including ones with the old separate `recording` section).

## Roadmap

- [x] WinForms shell with a live KatanaEngine viewport (`KatanaViewport`)
- [x] Emitter settings in a `PropertyGrid`, synced to the preview
- [x] Textured particles loaded from disk, alpha/additive blending, rotation and spin
- [x] Menu bar, effect files (`.keparticle`)
- [x] Recording settings, auto-fit canvas, prewarm
- [x] Deterministic frame capture and sprite-sheet + `.anim` export
- [x] Animation preview before saving: playback, frame stepping, backgrounds, zoom
- [x] Emitter path drawn with a pen tool (open or closed), with an emission rate per point

**Ideas for later:** undo for path edits, aiming particles along the path's direction, per-point timing (slowing down or speeding up along the path), PNG-sequence or GIF export, and multiple emitters per effect.

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
