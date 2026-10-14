#pragma once

#include "pch.h"
#include "Recording/Recorder.h"
#include "Core/Resource/RenderTarget.h"

using namespace KatanaEngine;

// Plays a finished recording before it is saved, the way a game would draw it: one
// frame every 1/fps seconds, with the effect's blend. Covers the live preview while shown;
// the host controls playback with "preview" messages.
class AnimationPreviewScreen : public Screen
{
public:

	enum class Background
	{
		DARK,		// the editor's background
		LIGHT,
		CHECKER		// shows exactly which pixels are transparent
	};

	explicit AnimationPreviewScreen(std::unique_ptr<Recording> recording);
	~AnimationPreviewScreen() override = default;

	// Creates the texture frames are shown through. Call once before showing the screen.
	bool CreateFrameTexture(std::string& error);

	const Recording& GetRecording() const { return *m_recording; }

	// Plays from the current frame, or from the start if a non-looping animation has finished.
	void Play();
	void Pause();
	bool IsPlaying() const { return m_isPlaying; }

	// Shows a frame (clamped to the recording); playback continues from there.
	void SetFrame(int index);
	int GetFrameIndex() const { return m_frameIndex; }

	void SetBackground(Background background) { m_background = background; }

	// Scale the frame is drawn at; 0 fits it: actual size, or smaller if it doesn't fit.
	void SetZoom(float zoom) { m_zoom = std::max(0.0f, zoom); }

	void Update(const GameTime& gameTime) override;
	void Draw(SpriteBatch& spriteBatch) override;

private:

	int GetLastFrame() const { return m_recording->Layout.FrameCount - 1; }
	void DrawBackground(SpriteBatch& spriteBatch, const Region& area);
	void UpdateChecker(SpriteBatch& spriteBatch, int width, int height);

	std::unique_ptr<Recording> m_recording;
	std::shared_ptr<RenderTarget> m_frame;	// the shown frame, copied out of the sheet
	std::shared_ptr<RenderTarget> m_checker;	// checkerboard the size of the drawn frame, rebuilt when that changes
	int m_frameIndex = 0;
	double m_frameElapsedSeconds = 0.0;
	bool m_isPlaying = true;
	Background m_background = Background::DARK;
	float m_zoom = 0.0f;
};
