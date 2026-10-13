#include "pch.h"
#include "PreviewEffect.h"

#include "Core/Math/Circle.h"
#include "Particles/ParticleBehaviors.h"

namespace
{
	constexpr float DegreesToRadians = 3.14159265f / 180.0f;

	float Lerp(float start, float end, float amount) { return start + (end - start) * amount; }

	Color Lerp(const Color& start, const Color& end, float amount)
	{
		return Color(
			Lerp(start.Red, end.Red, amount),
			Lerp(start.Green, end.Green, amount),
			Lerp(start.Blue, end.Blue, amount),
			Lerp(start.Alpha, end.Alpha, amount));
	}

	float RandomBetween(std::mt19937& random, float min, float max)
	{
		if (max < min) std::swap(min, max);
		return std::uniform_real_distribution<float>(min, max)(random);
	}

	class SettingsInitializer : public ParticleInitializer<SimpleParticle>
	{
	public:
		SettingsInitializer(const ParticleSettings& settings, std::shared_ptr<std::mt19937> random)
			: m_settings(settings), m_random(std::move(random)) { }

	protected:
		void InitializeParticle(SimpleParticle& particle, const Vector2& emitterPosition) override
		{
			auto& random = *m_random;
			const float angle = (m_settings.Direction + RandomBetween(random, -m_settings.Spread, m_settings.Spread)) * DegreesToRadians;
			const float speed = RandomBetween(random, m_settings.SpeedMin, m_settings.SpeedMax);

			particle.Position = emitterPosition;
			particle.Velocity = Vector2(std::cos(angle) * speed, std::sin(angle) * speed);
			particle.Age = 0.0f;
			particle.Lifetime = std::max(0.01f, RandomBetween(random, m_settings.LifetimeMin, m_settings.LifetimeMax));
			particle.Color = m_settings.StartColor;
			particle.Scale = m_settings.StartSize;
			particle.IsAlive = true;
		}

	private:
		const ParticleSettings& m_settings;
		std::shared_ptr<std::mt19937> m_random;
	};

	class SettingsUpdater : public ParticleUpdater<SimpleParticle>
	{
	public:
		explicit SettingsUpdater(const ParticleSettings& settings) : m_settings(settings) { }

	protected:
		void UpdateParticle(SimpleParticle& particle, const GameTime& gameTime) override
		{
			const float deltaTime = static_cast<float>(gameTime.ElapsedTime);

			particle.Age += deltaTime;
			if (particle.IsExpired())
			{
				particle.IsAlive = false;
				return;
			}

			particle.Velocity.Y += m_settings.Gravity * deltaTime;
			particle.Position += particle.Velocity * deltaTime;

			const float amount = particle.GetNormalizedAge();
			particle.Color = Lerp(m_settings.StartColor, m_settings.EndColor, amount);
			particle.Scale = Lerp(m_settings.StartSize, m_settings.EndSize, amount);
		}

	private:
		const ParticleSettings& m_settings;
	};

	class CircleDrawer : public ParticleDrawer<SimpleParticle>
	{
	protected:
		void DrawParticle(const SimpleParticle& particle, SpriteBatch& spriteBatch) override
		{
			if (particle.Scale <= 0.0f || particle.Color.Alpha <= 0.0f) return;

			// Allegro blends with premultiplied alpha.
			const Color& color = particle.Color;
			const Color premultiplied(color.Red * color.Alpha, color.Green * color.Alpha, color.Blue * color.Alpha, color.Alpha);

			spriteBatch.DrawPrimitive(Circle(particle.Position, particle.Scale), premultiplied, premultiplied, 0.0f);
		}
	};
}

PreviewEffect::PreviewEffect(const ParticleSettings& settings)
	: m_settings(settings)
	, m_random(std::make_shared<std::mt19937>(std::random_device{}()))
{
	Rebuild();
}

void PreviewEffect::Rebuild()
{
	EmitterConfig config;
	config.MaxParticles = m_settings.MaxParticles;
	config.EmissionRate = m_settings.EmissionRate;
	config.IsLooping = true;

	m_emitter = std::make_unique<ParticleEmitter<SimpleParticle>>(config);
	m_emitter->AddInitializer(std::make_shared<SettingsInitializer>(m_settings, m_random));
	m_emitter->AddUpdater(std::make_shared<SettingsUpdater>(m_settings));
	m_emitter->SetDrawer(std::make_shared<CircleDrawer>());
	m_emitter->SetPosition(m_position);
}

void PreviewEffect::SetPosition(const Vector2& position)
{
	m_position = position;
	m_emitter->SetPosition(position);
}

void PreviewEffect::Update(const GameTime& gameTime)
{
	m_emitter->Update(gameTime);
}

void PreviewEffect::Draw(SpriteBatch& spriteBatch)
{
	m_emitter->Draw(spriteBatch);
}

int PreviewEffect::GetActiveParticleCount() const
{
	return m_emitter->GetActiveParticleCount();
}
