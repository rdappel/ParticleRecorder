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

	// The seed fixes the random sequence, so a recording can replay an effect exactly.
	explicit PreviewEffect(const ParticleSettings& settings, unsigned int seed = std::random_device{}());

	// Call after changing MaxParticles (restarts the effect).
	void Rebuild();

	// nullptr draws circles. Keeps the live particles.
	void SetTexture(std::shared_ptr<Texture> texture);
	const std::shared_ptr<Texture>& GetTexture() const { return m_texture; }

	void SetPosition(const Vector2& position);

	// Advances the effect with the emitter standing still at its position.
	void Update(const GameTime& gameTime);

	// Advances the effect while the emitter moves from -> to during this step. Particles
	// emitted in the step are spread along that movement, so a fast emitter leaves an even
	// trail. rateScale multiplies EmissionRate (the path's rate at this point).
	void Update(const GameTime& gameTime, const Vector2& from, const Vector2& to, float rateScale);
	void Draw(SpriteBatch& spriteBatch);

	// Draws with viewOffset subtracted from every position (e.g. to place the effect in a recording frame).
	void Draw(SpriteBatch& spriteBatch, const Vector2& viewOffset);

	// Calls fn(const SimpleParticle&) for each live particle.
	template<typename TFunction>
	void ForEachParticle(TFunction&& fn) const { m_emitter->ForEachActive(std::forward<TFunction>(fn)); }

	// Distance from a particle's centre to its farthest drawn pixel at the given Scale,
	// whatever its rotation.
	float GetParticleRadius(float scale) const;

	int GetActiveParticleCount() const;

private:

	std::shared_ptr<IParticleDrawer> CreateDrawer() const;

	const ParticleSettings& m_settings;
	std::shared_ptr<std::mt19937> m_random;
	std::shared_ptr<Texture> m_texture;
	std::unique_ptr<ParticleEmitter<SimpleParticle>> m_emitter;
	Vector2 m_position;
	double m_emissionAccumulator = 0.0;	// fractional particles carried to the next step
};
