#pragma once

#include "pch.h"
#include "Particles/ParticleSettings.h"
#include "Particles/PreviewEffect.h"

using namespace KatanaEngine;

// Live preview of the effect being edited. Recording/export come later (see README.md).
class RecorderScreen : public Screen
{
public:

	RecorderScreen();
	~RecorderScreen() override = default;

	// On failure returns false and describes the problem in error.
	bool ApplySetting(const std::string& name, const std::string& value, std::string& error);
	void Restart();

	int GetActiveParticleCount() const { return m_effect.GetActiveParticleCount(); }

	void Update(const GameTime& gameTime) override;
	void Draw(SpriteBatch& spriteBatch) override;

private:

	bool LoadTexture(const std::string& path, std::string& error);

	ParticleSettings m_settings;	// declared before m_effect, which keeps a reference to it
	PreviewEffect m_effect;
};
