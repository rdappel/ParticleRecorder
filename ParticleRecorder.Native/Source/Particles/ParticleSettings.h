#pragma once

#include "pch.h"

using namespace KatanaEngine;

// Everything the editor can tune about the preview effect.
// Mirrored on the C# side by ParticleRecorder/Effects/ParticleSettings.cs; the host
// changes values with "set <Name> <value>" messages (see ApplySetting).
struct ParticleSettings
{
	int MaxParticles = 500;
	float EmissionRate = 60.0f;		// particles per second

	float LifetimeMin = 0.6f;		// seconds
	float LifetimeMax = 1.4f;

	float SpeedMin = 60.0f;			// pixels per second
	float SpeedMax = 160.0f;
	float Direction = -90.0f;		// degrees, 0 = right, -90 = up
	float Spread = 40.0f;			// degrees either side of Direction
	float Gravity = 120.0f;			// pixels per second squared (positive = down)

	float RotationMin = 0.0f;		// initial rotation, degrees
	float RotationMax = 0.0f;
	float SpinMin = 0.0f;			// degrees per second
	float SpinMax = 0.0f;

	float StartSize = 12.0f;		// diameter in pixels (texture: its longer side)
	float EndSize = 2.0f;
	Color StartColor = Color(1.0f, 0.75f, 0.2f, 1.0f);
	Color EndColor = Color(0.9f, 0.1f, 0.05f, 0.0f);

	std::string Texture;			// absolute path; empty = draw circles
	BlendState Blend = BlendState::ALPHA_BLEND;
};

// Applies one "Name value" pair. Colors are "r,g,b,a" with 0-255 components; Blend is
// "Alpha" or "Additive"; Texture is a file path (may be empty).
// Returns false for unknown names or malformed values (the settings are left unchanged).
bool ApplySetting(ParticleSettings& settings, const std::string& name, const std::string& value);

// True when changing this setting requires the emitter to be rebuilt.
bool RequiresEmitterRebuild(const std::string& name);
