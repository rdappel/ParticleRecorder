#include "pch.h"
#include "ParticleSettings.h"

#include <sstream>

namespace
{
	bool TryParseFloat(const std::string& text, float& result)
	{
		std::istringstream stream(text);
		float value = 0.0f;
		if (!(stream >> value)) return false;

		result = value;
		return true;
	}

	bool TryParseInt(const std::string& text, int& result)
	{
		std::istringstream stream(text);
		int value = 0;
		if (!(stream >> value)) return false;

		result = value;
		return true;
	}

	bool TryParseColor(const std::string& text, Color& result)
	{
		std::istringstream stream(text);
		int components[4] = { 0, 0, 0, 255 };
		char separator = 0;

		for (int i = 0; i < 4; ++i)
		{
			if (i > 0 && !(stream >> separator && separator == ',')) return false;
			if (!(stream >> components[i])) return false;
			components[i] = std::clamp(components[i], 0, 255);
		}

		result = Color(
			static_cast<uint8_t>(components[0]),
			static_cast<uint8_t>(components[1]),
			static_cast<uint8_t>(components[2]),
			static_cast<uint8_t>(components[3]));
		return true;
	}
}

bool ApplySetting(ParticleSettings& settings, const std::string& name, const std::string& value)
{
	if (name == "MaxParticles")
	{
		int count = 0;
		if (!TryParseInt(value, count) || count < 1) return false;
		settings.MaxParticles = count;
		return true;
	}

	if (name == "StartColor") return TryParseColor(value, settings.StartColor);
	if (name == "EndColor") return TryParseColor(value, settings.EndColor);

	if (name == "Texture")
	{
		settings.Texture = value;
		return true;
	}

	if (name == "Path") return settings.Path.Parse(value);

	if (name == "Blend")
	{
		if (value == "Alpha") settings.Blend = BlendState::ALPHA_BLEND;
		else if (value == "Additive") settings.Blend = BlendState::ADDITIVE;
		else return false;
		return true;
	}

	struct FloatSetting { const char* Name; float ParticleSettings::* Field; };
	static const FloatSetting floatSettings[] =
	{
		{ "EmissionRate", &ParticleSettings::EmissionRate },
		{ "LifetimeMin", &ParticleSettings::LifetimeMin },
		{ "LifetimeMax", &ParticleSettings::LifetimeMax },
		{ "SpeedMin", &ParticleSettings::SpeedMin },
		{ "SpeedMax", &ParticleSettings::SpeedMax },
		{ "Direction", &ParticleSettings::Direction },
		{ "Spread", &ParticleSettings::Spread },
		{ "Gravity", &ParticleSettings::Gravity },
		{ "RotationMin", &ParticleSettings::RotationMin },
		{ "RotationMax", &ParticleSettings::RotationMax },
		{ "SpinMin", &ParticleSettings::SpinMin },
		{ "SpinMax", &ParticleSettings::SpinMax },
		{ "StartSize", &ParticleSettings::StartSize },
		{ "EndSize", &ParticleSettings::EndSize },
	};

	for (const auto& setting : floatSettings)
	{
		if (name == setting.Name) return TryParseFloat(value, settings.*setting.Field);
	}

	return false;
}

bool RequiresEmitterRebuild(const std::string& name)
{
	return name == "MaxParticles";
}
