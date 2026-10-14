#include "pch.h"
#include "RecorderScreen.h"

#include "Core/Resource/Texture.h"

RecorderScreen::RecorderScreen()
	: m_effect(m_settings)
{
}

bool RecorderScreen::ApplySetting(const std::string& name, const std::string& value, std::string& error)
{
	if (!::ApplySetting(m_settings, name, value))
	{
		error = "invalid setting: " + name + " " + value;
		return false;
	}

	if (name == "Texture") return LoadTexture(value, error);

	if (RequiresEmitterRebuild(name)) m_effect.Rebuild();
	return true;
}

bool RecorderScreen::LoadTexture(const std::string& path, std::string& error)
{
	if (path.empty())
	{
		m_effect.SetTexture(nullptr);
		return true;
	}

	auto texture = GetScreenManager()->GetGame().GetResourceManager().Load<Texture>(path);
	m_effect.SetTexture(texture);
	if (texture) return true;

	error = "could not load texture: " + path;
	return false;
}

void RecorderScreen::Restart()
{
	m_effect.Rebuild();
}

bool RecorderScreen::Measure(const std::string& optionsText, RecordingLayout& layout, std::string& error) const
{
	RecordingOptions options;
	if (!ParseRecordingOptions(optionsText, options, error)) return false;

	return Recorder(m_settings, m_effect.GetTexture()).Measure(options, layout, error);
}

std::unique_ptr<Recording> RecorderScreen::Record(const std::string& optionsText, std::string& error) const
{
	RecordingOptions options;
	if (!ParseRecordingOptions(optionsText, options, error)) return nullptr;

	auto recording = std::make_unique<Recording>();
	if (!Recorder(m_settings, m_effect.GetTexture()).Record(options, *recording, error)) return nullptr;
	return recording;
}

void RecorderScreen::Update(const GameTime& gameTime)
{
	const Vector2 screenSize = GetScreenManager()->GetGame().GetScreenSize();
	m_effect.SetPosition(Vector2(screenSize.X * 0.5f, screenSize.Y * 0.6f));
	m_effect.Update(gameTime);
}

void RecorderScreen::Draw(SpriteBatch& spriteBatch)
{
	m_effect.Draw(spriteBatch);
}
