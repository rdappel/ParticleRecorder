#pragma once

#include "pch.h"
#include "Particles/ParticleSettings.h"

using namespace KatanaEngine;

// How to record an effect. Parsed from the host's "measure"/"export" messages.
struct RecordingOptions
{
	unsigned int Seed = 1;		// same seed = same particles, so a measure matches its export
	float Fps = 30.0f;
	float Duration = 1.0f;		// seconds of animation
	float Prewarm = 0.0f;		// seconds simulated before the first frame (0 = start empty)
	int Padding = 2;			// transparent pixels around the effect's bounds
	int Columns = 0;			// frames per sprite-sheet row (0 = choose automatically)
	bool Loop = true;			// written to the .anim file
	std::string Path;			// export only: the .png to write (the .anim goes next to it)

	int GetFrameCount() const;
};

// Parses "key=value" pairs (seed, fps, duration, prewarm, padding, columns, loop, path).
// path must come last; its value is the rest of the text, so it may contain spaces.
bool ParseRecordingOptions(const std::string& text, RecordingOptions& options, std::string& error);

// Where the recorded effect sits and how the frames are packed.
struct RecordingLayout
{
	int FrameWidth = 0;
	int FrameHeight = 0;
	int FrameCount = 0;
	int Columns = 0;
	int Rows = 0;
	Vector2 Origin;				// simulation point drawn at each frame's top-left

	int GetSheetWidth() const { return FrameWidth * Columns; }
	int GetSheetHeight() const { return FrameHeight * Rows; }
};

// Records an effect deterministically: the simulation advances exactly 1/fps per frame
// with a fixed seed, independent of the live preview and of real time.
//
// The emitter sits at the simulation origin. EmitterPositionAt (Recorder.cpp) is the
// single place that decides where it is at each moment; a drawn emitter path will plug in there.
class Recorder
{
public:

	Recorder(const ParticleSettings& settings, std::shared_ptr<Texture> texture);

	// Simulates the whole recording and fits the frame to every visible particle.
	bool Measure(const RecordingOptions& options, RecordingLayout& layout, std::string& error) const;

	// Measures, then renders every frame into a sprite sheet and writes <Path> plus a
	// matching .anim file (AnimationSet format).
	bool Export(const RecordingOptions& options, RecordingLayout& layout, std::string& error) const;

private:

	ParticleSettings m_settings;	// a copy: edits made while recording don't affect it
	std::shared_ptr<Texture> m_texture;
};
