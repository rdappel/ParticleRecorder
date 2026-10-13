#include "pch.h"
#include "RecorderScreen.h"

RecorderScreen::RecorderScreen()
	: m_effect(m_settings)
{
}

bool RecorderScreen::ApplySetting(const std::string& name, const std::string& value)
{
	if (!::ApplySetting(m_settings, name, value)) return false;

	if (RequiresEmitterRebuild(name)) m_effect.Rebuild();
	return true;
}

void RecorderScreen::Restart()
{
	m_effect.Rebuild();
}

void RecorderScreen::Update(const GameTime& gameTime)
{
	const Vector2 screenSize = GetScreenManager()->GetGame().GetScreenSize();
	m_effect.SetPosition(Vector2(screenSize.X * 0.5f, screenSize.Y * 0.6f));
	m_effect.Update(gameTime);
}

void RecorderScreen::Draw(SpriteBatch& spriteBatch)
{
	spriteBatch.Begin();
	m_effect.Draw(spriteBatch);
	spriteBatch.End();
}
