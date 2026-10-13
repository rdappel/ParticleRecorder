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
Effect settings live in `ParticleRecorder/Effects/ParticleSettings.cs`. The native side only holds a mirror that it updates from `set` messages; it never decides values on its own. On `ready` the UI resends everything.

### Keep the two ParticleSettings in sync
Adding or renaming a setting means updating **both** `ParticleSettings.cs` and `ParticleRecorder.Native/Source/Particles/ParticleSettings.h/.cpp` (`ApplySetting`, `RequiresEmitterRebuild`). Then update the protocol table in `README.md`.

### Messages are the only boundary
The UI and the game talk only through `KatanaViewport.Post` / `MessageReceived` and `OnHostMessage` / `SendHostMessage`. No other exports, no shared memory. New messages must be added to the protocol table in `README.md`.

### Threading
- `OnHostMessage` and everything in the native game run on the game thread.
- `MessageReceived` is already marshalled to the UI thread.
- Never block the UI thread on the game except through `GameHost` (it services window messages while waiting).

### Export compatibility
Exported animations must load with the engine's `AnimationSet` (keys: `FrameWidth`, `FrameHeight`, `Frames`, `FrameDuration`, `Loop`, `FramesPerRow`). If the format needs to change, change the engine first.

### Engine vs tool
- Tool features belong here.
- Only truly engine-level pieces go into `KatanaEngine` (e.g. saving a render target to PNG). Engine rules are in `KatanaEngine/CLAUDE.md` and apply to native code here too (ownership, no per-frame allocations, etc.).
- Engine changes are commits in the engine repo:
  1. Check out `master` in the submodule first.
  2. Commit and push there.
  3. Commit the pointer here.
- AltarsOfEternity shares the engine, so before pushing engine changes, verify Altars still builds.

### Conventions
- **C#:** file-scoped namespaces, `_camelCase` private fields, nullable enabled, warnings are errors. Mirror existing classes before adding new ones.
- **C++:** match the engine's style: tabs, `m_` members, `PascalCase` methods. New screens go under `ParticleRecorder.Native/Source/Screens/`.
- Don't add NuGet packages or other third-party libraries without asking first.
