# CLAUDE.md

Guidance for Claude Code in this repository. See `README.md` for what the tool does, its architecture and the message protocol.

## Solution structure (`ParticleRecorder.slnx`, x64 only)

- **ParticleRecorder** (`ParticleRecorder/`): C# WinForms app (.NET 10). The UI, which owns all effect data.
- **ParticleRecorder.Native** (`ParticleRecorder.Native/`): C++ DLL, a KatanaEngine `Game` hosted in the UI's `KatanaViewport`.
- **KatanaEngine** (`KatanaEngine/`, git submodule `rdappel/KatanaEngine`): the C++ engine (`KatanaEngine.vcxproj`) plus `WinForms/KatanaEngine.WinForms.csproj` (the `KatanaViewport` control).

Build from the command line (MSBuild, not `dotnet build`, because of the C++ projects):

```
MSBuild ParticleRecorder.slnx -t:restore -p:RestorePackagesConfig=true
MSBuild ParticleRecorder.slnx -p:Configuration=Debug -p:Platform=x64
```

Output: `ParticleRecorder/bin/<Configuration>/net10.0-windows/ParticleRecorder.exe` (the native DLL and Allegro DLLs are copied there).

## Rules

### C# owns the data
All effect settings (including Timing and Sprite Sheet) live in `ParticleRecorder/Effects/ParticleSettings.cs` and show in the settings grid. The native side only holds a mirror that it updates from `set` messages (the live view uses the Timing settings), and receives the recording's options with `measure`/`record` (`ToRecordingOptions`); it never decides values on its own. On `ready` the UI resends everything.

### Effect files
`*.keparticle` is JSON written by `ParticleRecorder/Effects/EffectDocument.cs` (`version`, `particles`; colours `#RRGGBB`; texture relative to the file). Older files also have a `recording` section, which `LegacyRecording` maps into the particle settings. New settings get defaults when missing, so old files keep loading. Bump `CurrentVersion` only for breaking changes, and keep reading older versions.

### Recording must stay deterministic
`ParticleRecorder.Native/Source/Recording/Recorder.cpp` simulates with a fixed `1/fps` step and the seed from the message, so a `measure` and the `record` that follows produce the same particles. Don't read real time, the live preview, or an unseeded random source while recording. `save` writes the recording held in memory (the frames the user previewed); it never re-simulates. The emitter's position comes only from `EmitterPositionAt`, which evaluates the effect's `EmitterPath` (`ParticleRecorder.Native/Source/Paths/`) over the recording's Duration.

### Keep the two ParticleSettings in sync
Adding or renaming a setting means updating **both** `ParticleSettings.cs` and `ParticleRecorder.Native/Source/Particles/ParticleSettings.h/.cpp` (`ApplySetting`, `RequiresEmitterRebuild`). Then update the protocol table in `README.md`. The emitter path is part of the settings too: `Effects/EmitterPath.cs` and `Source/Paths/EmitterPath.h/.cpp` must agree on the `set Path` format.

### Messages are the only boundary
The UI and the game talk only through `KatanaViewport.Post` / `MessageReceived` and `OnHostMessage` / `SendHostMessage`. No other exports, no shared memory. New messages must be added to the protocol table in `README.md`.

### Input in the viewport
The hosted game window receives mouse input but **not keyboard input**. Tool controls belong in the WinForms UI; viewport interaction (the pen tool) must be mouse-only. The native side only forwards mouse events (`mouse ...`); `ParticleRecorder/Paths/PenTool.cs` does the editing, so the UI keeps owning the path.

### Threading
- `OnHostMessage` and everything in the native game run on the game thread.
- `MessageReceived` is already marshalled to the UI thread.
- Never block the UI thread on the game except through `GameHost` (it services window messages while waiting).

### Export compatibility
Exported animations must load with the engine's `AnimationSet` (keys: `FrameWidth`, `FrameHeight`, `Frames`, `FrameDuration`, `Loop`, `FramesPerRow`). If the format needs to change, change the engine first.

### Engine vs tool
- Tool features belong here.
- Only truly engine-level pieces go into `KatanaEngine` (e.g. `Image`, which assembles and saves the sprite sheet). Engine rules are in `KatanaEngine/CLAUDE.md` and apply to native code here too (ownership, no per-frame allocations, etc.).
- Engine changes are commits in the engine repo:
  1. Check out `master` in the submodule first.
  2. Commit and push there.
  3. Commit the pointer here.
- AltarsOfEternity shares the engine, so before pushing engine changes, verify Altars still builds.

### Conventions
- **C#:** file-scoped namespaces, `_camelCase` private fields, nullable enabled, warnings are errors. Mirror existing classes before adding new ones.
- **C++:** match the engine's style: tabs, `m_` members, `PascalCase` methods. New screens go under `ParticleRecorder.Native/Source/Screens/`.
- Don't add NuGet packages or other third-party libraries without asking first.
