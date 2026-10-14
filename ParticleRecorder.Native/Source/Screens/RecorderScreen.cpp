#include "pch.h"
#include "RecorderScreen.h"

#include "Core/Resource/Texture.h"
#include "Core/Math/Circle.h"
#include "Core/Math/Line.h"
#include "Core/Math/Rectangle.h"

#include <cstdio>

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

	if (RequiresEmitterRebuild(name) || AffectsTiming(name))
	{
		StartCycle();
		return true;
	}

	// Continue from the matching place on the edited path, rather than streaking particles
	// across from where the old path had the emitter.
	if (name == "Path") m_emitterPosition = m_settings.Path.PositionAt(GetPathProgress());
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
	StartCycle();
}

void RecorderScreen::StartCycle()
{
	constexpr double prewarmStep = 1.0 / 60.0;

	m_effect.Rebuild();
	m_isDyingOut = false;

	// Same as a recording: the prewarm runs before time 0, with the emitter on its path.
	const double prewarm = m_settings.PrewarmEnabled ? m_settings.PrewarmSeconds : 0.0;
	const int steps = static_cast<int>(std::lround(prewarm / prewarmStep));
	m_cycleTime = -steps * prewarmStep;
	m_emitterPosition = m_settings.Path.PositionAt(GetPathProgress());
	m_effect.SetPosition(GetOrigin() + m_emitterPosition);

	for (int step = 0; step < steps; ++step) Advance(GameTime(m_cycleTime + prewarmStep, prewarmStep));
}

void RecorderScreen::SetPenMode(bool enabled)
{
	m_isPenMode = enabled;
	GetScreenManager()->GetGame().SetMouseCursor(enabled ? MouseCursor::PRECISION : MouseCursor::DEFAULT);
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

Vector2 RecorderScreen::GetOrigin() const
{
	const Vector2 screenSize = GetScreenManager()->GetGame().GetScreenSize();
	return Vector2(std::floor(screenSize.X * 0.5f), std::floor(screenSize.Y * 0.6f));
}

void RecorderScreen::HandleInput(const InputState& input)
{
	if (!m_isPenMode || !m_sendMessage) return;

	const Vector2 position = input.GetMousePosition() - GetOrigin();
	const bool isDragging = input.IsMouseButtonDown(MouseButton::LEFT) || input.IsMouseButtonDown(MouseButton::RIGHT);

	if (input.IsMouseButtonPressed(MouseButton::LEFT)) SendMouseMessage("down", position, "left");
	if (input.IsMouseButtonPressed(MouseButton::RIGHT)) SendMouseMessage("down", position, "right");
	if (isDragging && position != m_lastMousePosition) SendMouseMessage("move", position, nullptr);
	if (input.IsMouseButtonReleased(MouseButton::LEFT)) SendMouseMessage("up", position, "left");
	if (input.IsMouseButtonReleased(MouseButton::RIGHT)) SendMouseMessage("up", position, "right");

	m_lastMousePosition = position;
}

void RecorderScreen::SendMouseMessage(const char* action, const Vector2& position, const char* button) const
{
	char message[96];
	std::snprintf(message, sizeof(message), "mouse %s %.1f %.1f%s%s", action, position.X, position.Y,
		button ? " " : "", button ? button : "");
	m_sendMessage(message);
}

void RecorderScreen::Update(const GameTime& gameTime)
{
	const Vector2 position = GetOrigin() + m_emitterPosition;

	if (!m_isDyingOut && m_cycleTime >= m_settings.Duration)
	{
		if (!m_settings.DieOut) StartCycle();
		else m_isDyingOut = true;
	}

	if (m_isDyingOut)
	{
		if (m_effect.GetActiveParticleCount() > 0)
		{
			m_effect.Update(gameTime, position, position, 0.0f);
			return;
		}

		StartCycle();	// everything has died
	}

	Advance(gameTime);
}

void RecorderScreen::Advance(const GameTime& gameTime)
{
	const EmitterPath& path = m_settings.Path;
	const Vector2 from = m_emitterPosition;
	m_cycleTime += gameTime.ElapsedTime;
	m_emitterPosition = path.PositionAt(GetPathProgress());

	const Vector2 origin = GetOrigin();
	m_effect.Update(gameTime, origin + from, origin + m_emitterPosition, path.RateAt(GetPathProgress()));
}
void RecorderScreen::Draw(SpriteBatch& spriteBatch)
{
	m_effect.Draw(spriteBatch);
	DrawPath(spriteBatch);
}

void RecorderScreen::DrawPath(SpriteBatch& spriteBatch) const
{
	const EmitterPath& path = m_settings.Path;
	if (path.IsEmpty() && !m_isPenMode) return;

	const Color lineColor(1.0f, 1.0f, 1.0f, m_isPenMode ? 0.6f : 0.25f);
	const Color guideColor(1.0f, 1.0f, 1.0f, 0.35f);
	const Color handleColor(0.45f, 0.75f, 1.0f, 0.9f);
	const Color pointColor(0.15f, 0.15f, 0.18f);
	const Color startColor(0.3f, 0.85f, 0.4f);
	const Color selectedColor(1.0f, 0.82f, 0.2f);

	spriteBatch.Begin(SpriteSortMode::DEFERRED, BlendState::ALPHA_BLEND, nullptr, Vector2::ZERO - GetOrigin());

	// The curve, thicker where the emission rate is higher.
	bool hasPrevious = false;
	Vector2 previous;
	path.ForEachSample([&](const Vector2& position, float rate)
	{
		if (hasPrevious) spriteBatch.DrawPrimitive(Line(previous, position), lineColor, 1.0f + 1.5f * std::min(rate, 3.0f));
		previous = position;
		hasPrevious = true;
	});

	if (m_isPenMode)
	{
		// The origin: where the emitter rests without a path.
		spriteBatch.DrawPrimitive(Line(-8.0f, 0.0f, 8.0f, 0.0f), guideColor);
		spriteBatch.DrawPrimitive(Line(0.0f, -8.0f, 0.0f, 8.0f), guideColor);

		const auto& points = path.GetPoints();
		for (size_t index = 0; index < points.size(); ++index)
		{
			const EmitterPathPoint& point = points[index];
			for (const Vector2& handle : { point.In, point.Out })
			{
				if (handle.LengthSquared() < 0.25f) continue;
				spriteBatch.DrawPrimitive(Line(point.Position, point.Position + handle), handleColor);
				spriteBatch.DrawPrimitive(Circle(point.Position + handle, 3.0f), handleColor, Color::TRANSPARENT);
			}

			const float size = index == 0 ? 10.0f : 8.0f;
			const Color& fill = static_cast<int>(index) == m_selectedPoint ? selectedColor : (index == 0 ? startColor : pointColor);
			spriteBatch.DrawPrimitive(Rectangle(point.Position.X - size / 2, point.Position.Y - size / 2, size, size), fill, Color::WHITE);
		}
	}

	if (!path.IsEmpty()) spriteBatch.DrawPrimitive(Circle(m_emitterPosition, 5.0f), Color::TRANSPARENT, Color(1.0f, 1.0f, 1.0f, 0.7f), 1.5f);

	spriteBatch.End();
}
