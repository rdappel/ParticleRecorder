#include "pch.h"
#include "AnimationPreviewScreen.h"

#include "Core/Math/Rectangle.h"

namespace
{
	constexpr float Margin = 16.0f;			// space kept around a fitted frame
	constexpr int CheckerSize = 8;			// checkerboard square, in screen pixels

	const Color DarkBackground(0.12f, 0.12f, 0.14f);	// matches ParticleRecorderGame::Draw
	const Color LightBackground(0.86f, 0.86f, 0.88f);
	const Color CheckerLight(0.62f, 0.62f, 0.64f);
	const Color CheckerDark(0.40f, 0.40f, 0.42f);
	const Color BoundsColor(1.0f, 1.0f, 1.0f, 0.3f);
}

AnimationPreviewScreen::AnimationPreviewScreen(std::unique_ptr<Recording> recording)
	: m_recording(std::move(recording))
{
}

bool AnimationPreviewScreen::CreateFrameTexture(std::string& error)
{
	const RecordingLayout& layout = m_recording->Layout;
	m_frame = std::make_shared<RenderTarget>();
	if (!m_frame->Create(layout.FrameWidth, layout.FrameHeight))
	{
		error = "could not create a texture for the preview";
		return false;
	}

	m_recording->Sheet.CopyTo(*m_frame, layout.GetFrameX(0), layout.GetFrameY(0));
	return true;
}

void AnimationPreviewScreen::Play()
{
	if (!m_isLooping && m_frameIndex == GetLastFrame()) SetFrame(0);

	m_isPlaying = true;
	m_frameElapsedSeconds = 0.0;
}

void AnimationPreviewScreen::Pause()
{
	m_isPlaying = false;
}

void AnimationPreviewScreen::SetFrame(int index)
{
	index = std::clamp(index, 0, GetLastFrame());
	m_frameElapsedSeconds = 0.0;
	if (index == m_frameIndex) return;

	m_frameIndex = index;
	const RecordingLayout& layout = m_recording->Layout;
	m_recording->Sheet.CopyTo(*m_frame, layout.GetFrameX(index), layout.GetFrameY(index));
}

void AnimationPreviewScreen::Update(const GameTime& gameTime)
{
	if (!m_isPlaying) return;

	const double frameDuration = 1.0 / m_recording->Options.Fps;
	m_frameElapsedSeconds += gameTime.ElapsedTime;
	if (m_frameElapsedSeconds < frameDuration) return;

	// Advance as many frames as have elapsed (more than one if the game hitched).
	const int steps = static_cast<int>(m_frameElapsedSeconds / frameDuration);
	const double remainder = m_frameElapsedSeconds - steps * frameDuration;
	const int frameCount = m_recording->Layout.FrameCount;

	int index = m_frameIndex + steps;
	if (m_isLooping) index %= frameCount;
	else if (index >= GetLastFrame())
	{
		index = GetLastFrame();
		m_isPlaying = false;	// holds the last frame, as a non-looping animation does in a game
	}

	SetFrame(index);
	m_frameElapsedSeconds = remainder;
}

void AnimationPreviewScreen::Draw(SpriteBatch& spriteBatch)
{
	const Vector2 screenSize = GetScreenManager()->GetGame().GetScreenSize();
	const RecordingLayout& layout = m_recording->Layout;

	float scale = m_zoom;
	if (scale <= 0.0f)
	{
		const float fitX = (screenSize.X - 2.0f * Margin) / layout.FrameWidth;
		const float fitY = (screenSize.Y - 2.0f * Margin) / layout.FrameHeight;
		scale = std::max(0.01f, std::min(1.0f, std::min(fitX, fitY)));
	}

	const int width = std::max(1, static_cast<int>(std::lround(layout.FrameWidth * scale)));
	const int height = std::max(1, static_cast<int>(std::lround(layout.FrameHeight * scale)));
	const Region area(static_cast<int>(screenSize.X - width) / 2, static_cast<int>(screenSize.Y - height) / 2, width, height);

	if (m_background == Background::CHECKER) UpdateChecker(spriteBatch, width, height);

	spriteBatch.Begin(SpriteSortMode::DEFERRED, BlendState::ALPHA_BLEND);
	DrawBackground(spriteBatch, area);
	spriteBatch.End();

	spriteBatch.Begin(SpriteSortMode::DEFERRED, m_recording->Blend);
	spriteBatch.Draw(m_frame, area, Color::WHITE);
	spriteBatch.End();

	// The frame's edge, just outside it, so the auto-fitted bounds are visible on any background.
	spriteBatch.Begin(SpriteSortMode::DEFERRED, BlendState::ALPHA_BLEND);
	spriteBatch.DrawPrimitive(Rectangle(area.X - 0.5f, area.Y - 0.5f, area.Width + 1.0f, area.Height + 1.0f),
		Color::TRANSPARENT, BoundsColor, 1.0f);
	spriteBatch.End();
}

void AnimationPreviewScreen::DrawBackground(SpriteBatch& spriteBatch, const Region& area)
{
	const Vector2 screenSize = GetScreenManager()->GetGame().GetScreenSize();
	spriteBatch.DrawPrimitive(Rectangle(0.0f, 0.0f, screenSize.X, screenSize.Y), DarkBackground, Color::TRANSPARENT);

	if (m_background == Background::LIGHT) spriteBatch.DrawPrimitive(Rectangle(area), LightBackground, Color::TRANSPARENT);
	else if (m_background == Background::CHECKER && m_checker) spriteBatch.Draw(m_checker, Vector2(static_cast<float>(area.X), static_cast<float>(area.Y)));
}

void AnimationPreviewScreen::UpdateChecker(SpriteBatch& spriteBatch, int width, int height)
{
	if (m_checker && m_checker->GetWidth() == width && m_checker->GetHeight() == height) return;

	m_checker = std::make_shared<RenderTarget>();
	if (!m_checker->Create(width, height))
	{
		m_checker.reset();
		return;
	}

	m_checker->Begin();
	m_checker->Clear(CheckerLight);
	spriteBatch.Begin(SpriteSortMode::DEFERRED, BlendState::OPAQUE);
	for (int y = 0; y < height; y += CheckerSize)
	{
		for (int x = ((y / CheckerSize) % 2) * CheckerSize; x < width; x += 2 * CheckerSize)
		{
			spriteBatch.DrawPrimitive(Rectangle(static_cast<float>(x), static_cast<float>(y), static_cast<float>(CheckerSize), static_cast<float>(CheckerSize)),
				CheckerDark, Color::TRANSPARENT);
		}
	}
	spriteBatch.End();
	m_checker->End();
}
