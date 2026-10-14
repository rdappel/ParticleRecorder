#pragma once

#include "pch.h"
#include "Particles/ParticleSettings.h"
#include "Particles/PreviewEffect.h"
#include "Recording/Recorder.h"

using namespace KatanaEngine;

// Live preview of the effect being edited, and recording it into a sprite sheet.
//
// The live view plays the animation as it will be recorded, on repeat: each cycle starts
// fresh (prewarmed if enabled), emits for Duration while the emitter follows its path, then
// lets the particles die out if enabled. The emitter rests at the effect origin (0.5, 0.6 of
// the view). The path is drawn over the effect; in pen mode its points and handles are drawn
// too, and mouse input is forwarded to the host, which edits the path.
class RecorderScreen : public Screen
{
public:

	RecorderScreen();
	~RecorderScreen() override = default;

	// On failure returns false and describes the problem in error.
	bool ApplySetting(const std::string& name, const std::string& value, std::string& error);
	void Restart();

	// optionsText is a recording's key=value list (see ParseRecordingOptions).
	bool Measure(const std::string& optionsText, RecordingLayout& layout, std::string& error) const;

	// Records the current effect into memory; nullptr on failure.
	std::unique_ptr<Recording> Record(const std::string& optionsText, std::string& error) const;

	int GetActiveParticleCount() const { return m_effect.GetActiveParticleCount(); }

	// Pen mode forwards mouse input to the host as "mouse ..." messages (see
	// ParticleRecorderGame.h) and draws the path's points and handles.
	void SetPenMode(bool enabled);
	void SetSelectedPoint(int index) { m_selectedPoint = index; }
	void SetMessageSender(std::function<void(const std::string&)> sender) { m_sendMessage = std::move(sender); }

	void HandleInput(const InputState& input) override;
	void Update(const GameTime& gameTime) override;
	void Draw(SpriteBatch& spriteBatch) override;

private:

	bool LoadTexture(const std::string& path, std::string& error);

	// Where the effect origin is drawn in the view.
	Vector2 GetOrigin() const;
	double GetPathProgress() const { return m_cycleTime / m_settings.Duration; }

	// Starts the animation over: clears the particles and runs the prewarm.
	void StartCycle();

	// Moves the emitter along its path by gameTime's elapsed time, emitting as it goes.
	void Advance(const GameTime& gameTime);

	void DrawPath(SpriteBatch& spriteBatch) const;
	void SendMouseMessage(const char* action, const Vector2& position, const char* button) const;

	ParticleSettings m_settings;	// declared before m_effect, which keeps a reference to it
	PreviewEffect m_effect;

	double m_cycleTime = 0.0;		// seconds into the animation (negative during prewarm)
	bool m_isDyingOut = false;		// past Duration, waiting for the last particle to die
	Vector2 m_emitterPosition;		// effect space

	bool m_isPenMode = false;
	int m_selectedPoint = -1;
	Vector2 m_lastMousePosition;
	std::function<void(const std::string&)> m_sendMessage;
};
