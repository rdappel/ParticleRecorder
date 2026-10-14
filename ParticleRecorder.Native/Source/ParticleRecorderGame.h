#pragma once

#include "pch.h"

using namespace KatanaEngine;

class RecorderScreen;
class AnimationPreviewScreen;

// The native side of ParticleRecorder, hosted inside the WinForms app's viewport.
//
// Host -> game messages:
//     set <Name> <value>    change a ParticleSettings value (see ParticleSettings.h)
//     restart               restart the preview effect
//     measure <options>     fit a recording to the effect (options: see ParseRecordingOptions)
//     record <options>      record into memory and show the animation preview
//     preview play | pause | loop 0|1 | frame <index> | background dark|light|checker | zoom <scale, 0 = fit>
//     save <png path>       write the previewed recording as <png> + .anim
//     close                 close the animation preview (back to the live effect)
// Game -> host messages:
//     ready                 content loaded, preview running
//     stats <active>        active particle count (a few times per second)
//     measured <frames> <frameW> <frameH> <sheetW> <sheetH>
//     recorded <frames> <frameW> <frameH> <sheetW> <sheetH>
//     frame <index> <playing 0|1>   the animation preview's position, whenever it changes
//     saved                 the files were written
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

	bool HandleRecordingMessage(const std::string& message);
	bool HandlePreviewMessage(const std::string& command, std::string& error);
	void ClosePreview();

	ScreenManager& m_screenManager = GetScreenManager();
	std::shared_ptr<RecorderScreen> m_recorderScreen;
	std::shared_ptr<AnimationPreviewScreen> m_previewScreen;	// shown over the live effect after a recording

	int m_sentFrame = -1;			// last "frame" message, so it is only sent on changes
	bool m_sentPlaying = false;

	double m_statsElapsedSeconds = 0.0;
	static constexpr double StatsIntervalSeconds = 0.25;
};
