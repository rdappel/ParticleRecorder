#pragma once

#include "pch.h"

using namespace KatanaEngine;

class RecorderScreen;

// The native side of ParticleRecorder, hosted inside the WinForms app's viewport.
//
// Host -> game messages:
//     set <Name> <value>    change a ParticleSettings value (see ParticleSettings.h)
//     restart               restart the preview effect
// Game -> host messages:
//     ready                 content loaded, preview running
//     stats <active>        active particle count (a few times per second)
//     error <text>          a message could not be applied
class ParticleRecorderGame : public Game
{
public:

	explicit ParticleRecorderGame(const GameConfig& config);
	~ParticleRecorderGame() override = default;

protected:

	void LoadContent(ResourceManager& resourceManager) override;
	void HandleInput(const InputState& input) override;
	void Update(const GameTime& gameTime) override;
	void Draw(SpriteBatch& spriteBatch) override;
	void OnHostMessage(const std::string& message) override;

private:

	ScreenManager& m_screenManager = GetScreenManager();
	std::shared_ptr<RecorderScreen> m_recorderScreen;

	double m_statsElapsedSeconds = 0.0;
	static constexpr double StatsIntervalSeconds = 0.25;
};
