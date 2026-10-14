#include "pch.h"
#include "RecorderScreen.h"

#include "Core/Resource/Texture.h"
#include "Recording/Recorder.h"

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

bool RecorderScreen::Record(bool exportFiles, const std::string& optionsText, std::string& reply, std::string& error)
{
	RecordingOptions options;
	if (!ParseRecordingOptions(optionsText, options, error)) return false;

	const Recorder recorder(m_settings, m_effect.GetTexture());
	RecordingLayout layout;

	if (exportFiles)
	{
		if (!recorder.Export(options, layout, error)) return false;

		reply = "exported " + std::to_string(layout.FrameCount) + " " + std::to_string(layout.FrameWidth) + " "
			+ std::to_string(layout.FrameHeight) + " " + std::to_string(layout.GetSheetWidth()) + " "
			+ std::to_string(layout.GetSheetHeight());
		return true;
	}

	if (!recorder.Measure(options, layout, error)) return false;

	reply = "measured " + std::to_string(layout.FrameCount) + " " + std::to_string(layout.FrameWidth) + " "
		+ std::to_string(layout.FrameHeight) + " " + std::to_string(layout.GetSheetWidth()) + " "
		+ std::to_string(layout.GetSheetHeight());
	return true;
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
