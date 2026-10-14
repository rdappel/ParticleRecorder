#pragma once

#include "pch.h"
#include "Particles/ParticleSettings.h"
#include "Core/Resource/Image.h"

using namespace KatanaEngine;

// How to record an effect. Parsed from the host's "measure"/"record" messages.
struct RecordingOptions
{
	unsigned int Seed = 1;		// same seed = same particles, so a measure matches its recording
	float Fps = 30.0f;
	float Duration = 1.0f;		// seconds of animation
	float Prewarm = 0.0f;		// seconds simulated before the first frame (0 = start empty)
	int Padding = 2;			// transparent pixels around the effect's bounds
	int Columns = 0;			// frames per sprite-sheet row (0 = choose automatically)
	bool Loop = true;			// written to the .anim file
	bool DieOut = false;		// after Duration, stop emitting and keep recording until every particle has died

	// Frames recorded while emitting (Duration x Fps). Die-out frames come on top of these.
	int GetFrameCount() const;
};

// Parses space-separated "key=value" pairs (seed, fps, duration, prewarm, padding, columns, loop, dieout).
bool ParseRecordingOptions(const std::string& text, RecordingOptions& options, std::string& error);

// Where the recorded effect sits and how the frames are packed.
struct RecordingLayout
{
	int FrameWidth = 0;
	int FrameHeight = 0;
	int FrameCount = 0;			// including die-out frames
	int Columns = 0;
	int Rows = 0;
	Vector2 Origin;				// simulation point drawn at each frame's top-left

	int GetSheetWidth() const { return FrameWidth * Columns; }
	int GetSheetHeight() const { return FrameHeight * Rows; }

	// Where frame index's cell starts in the sheet.
	int GetFrameX(int index) const { return (index % Columns) * FrameWidth; }
	int GetFrameY(int index) const { return (index / Columns) * FrameHeight; }
};

// A finished recording, held in memory until it is saved or discarded.
struct Recording
{
	RecordingOptions Options;
	RecordingLayout Layout;
	BlendState Blend = BlendState::ALPHA_BLEND;	// how the effect was drawn, and how a game should draw it
	Image Sheet;								// every frame, packed as Layout describes (premultiplied)

	// Writes the sheet to pngPath and a matching .anim (AnimationSet format) next to it.
	bool Save(const std::string& pngPath, std::string& error) const;
};

// Records an effect deterministically: the simulation advances exactly 1/fps per frame
// with a fixed seed, independent of the live preview and of real time.
//
// The emitter follows the effect's path (EmitterPath), covering it once per Duration, or
// stays at the origin without one. EmitterPositionAt (Recorder.cpp) is the single place that
// decides where it is at each moment.
class Recorder
{
public:

	Recorder(const ParticleSettings& settings, std::shared_ptr<Texture> texture);

	// Simulates the whole recording and fits the frame to every visible particle.
	bool Measure(const RecordingOptions& options, RecordingLayout& layout, std::string& error) const;

	// Measures, then renders every frame into recording's sprite sheet.
	bool Record(const RecordingOptions& options, Recording& recording, std::string& error) const;

private:

	ParticleSettings m_settings;	// a copy: edits made while recording don't affect it
	std::shared_ptr<Texture> m_texture;
};
