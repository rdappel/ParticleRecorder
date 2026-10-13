#pragma once

#include "pch.h"
#include "Particles/ParticleSettings.h"
#include "Particles/Particle.h"
#include "Particles/ParticleEmitter.h"

#include <random>

using namespace KatanaEngine;
using namespace KatanaEngine::Particles;

// The effect being edited: a looping emitter whose behaviors read the current
// ParticleSettings, so most changes apply to new particles immediately.
class PreviewEffect
{
public:

	explicit PreviewEffect(const ParticleSettings& settings);

	// Call after changing MaxParticles or EmissionRate (restarts the effect).
	void Rebuild();

	// nullptr draws circles. Keeps the live particles.
	void SetTexture(std::shared_ptr<Texture> texture);

	void SetPosition(const Vector2& position);
	void Update(const GameTime& gameTime);
	void Draw(SpriteBatch& spriteBatch);

	int GetActiveParticleCount() const;

private:

	std::shared_ptr<IParticleDrawer> CreateDrawer() const;

	const ParticleSettings& m_settings;
	std::shared_ptr<std::mt19937> m_random;
	std::shared_ptr<Texture> m_texture;
	std::unique_ptr<ParticleEmitter<SimpleParticle>> m_emitter;
	Vector2 m_position;
};
