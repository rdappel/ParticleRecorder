#pragma once

#include "pch.h"
#include "Particles/ParticleSettings.h"
#include "Particles/PreviewEffect.h"
#include "Recording/Recorder.h"

using namespace KatanaEngine;

// Live preview of the effect being edited, and recording it into a sprite sheet.
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

	void Update(const GameTime& gameTime) override;
	void Draw(SpriteBatch& spriteBatch) override;

private:

	bool LoadTexture(const std::string& path, std::string& error);

	ParticleSettings m_settings;	// declared before m_effect, which keeps a reference to it
	PreviewEffect m_effect;
};
