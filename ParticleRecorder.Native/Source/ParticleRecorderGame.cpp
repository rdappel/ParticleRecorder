#include "pch.h"
#include "ParticleRecorderGame.h"
#include "Screens/RecorderScreen.h"
#include "Screens/AnimationPreviewScreen.h"


ParticleRecorderGame::ParticleRecorderGame(const GameConfig& config)
	: Game(config)
{
	SetTitle("Particle Recorder");
}

void ParticleRecorderGame::LoadContent(ResourceManager& resourceManager)
{
	m_recorderScreen = std::make_shared<RecorderScreen>();
	m_recorderScreen->SetMessageSender([this](const std::string& message) { SendHostMessage(message); });
	m_screenManager.AddScreen(m_recorderScreen);

	SendHostMessage("ready");
}

void ParticleRecorderGame::HandleInput(const InputState& input)
{
	m_screenManager.HandleInput(input);
}

void ParticleRecorderGame::Update(const GameTime& gameTime)
{
	m_screenManager.Update(gameTime);

	if (m_previewScreen && (m_previewScreen->GetFrameIndex() != m_sentFrame || m_previewScreen->IsPlaying() != m_sentPlaying))
	{
		m_sentFrame = m_previewScreen->GetFrameIndex();
		m_sentPlaying = m_previewScreen->IsPlaying();
		SendHostMessage("frame " + std::to_string(m_sentFrame) + " " + (m_sentPlaying ? "1" : "0"));
	}

	m_statsElapsedSeconds += gameTime.ElapsedTime;
	if (m_statsElapsedSeconds < StatsIntervalSeconds) return;

	m_statsElapsedSeconds = 0.0;
	SendHostMessage("stats " + std::to_string(m_recorderScreen->GetActiveParticleCount()));
}

void ParticleRecorderGame::Draw(SpriteBatch& spriteBatch)
{
	Clear(Color(0.12f, 0.12f, 0.14f));

	m_screenManager.Draw(spriteBatch);
}

void ParticleRecorderGame::OnHostMessage(const std::string& message)
{
	if (message == "restart")
	{
		m_recorderScreen->Restart();
		return;
	}

	if (HandleRecordingMessage(message)) return;
	if (HandlePathMessage(message)) return;

	const std::string setPrefix = "set ";
	if (message.compare(0, setPrefix.size(), setPrefix) == 0)
	{
		const size_t nameEnd = message.find(' ', setPrefix.size());
		const std::string name = message.substr(setPrefix.size(), nameEnd - setPrefix.size());
		const std::string value = nameEnd == std::string::npos ? std::string() : message.substr(nameEnd + 1);

		std::string error;
		if (!m_recorderScreen->ApplySetting(name, value, error)) SendHostMessage("error " + error);
		return;
	}

	SendHostMessage("error unknown message: " + message);
}

namespace
{
	bool StartsWith(const std::string& text, const std::string& prefix)
	{
		return text.compare(0, prefix.size(), prefix) == 0;
	}

	std::string LayoutReply(const std::string& name, const RecordingLayout& layout)
	{
		return name + " " + std::to_string(layout.FrameCount) + " " + std::to_string(layout.FrameWidth) + " "
			+ std::to_string(layout.FrameHeight) + " " + std::to_string(layout.GetSheetWidth()) + " "
			+ std::to_string(layout.GetSheetHeight());
	}
}

// Returns false if message isn't a recording or preview message.
bool ParticleRecorderGame::HandleRecordingMessage(const std::string& message)
{
	std::string error;

	if (StartsWith(message, "measure "))
	{
		RecordingLayout layout;
		if (m_recorderScreen->Measure(message.substr(8), layout, error)) SendHostMessage(LayoutReply("measured", layout));
		else SendHostMessage("error " + error);
		return true;
	}

	if (StartsWith(message, "record "))
	{
		std::unique_ptr<Recording> recording = m_recorderScreen->Record(message.substr(7), error);
		auto previewScreen = recording ? std::make_shared<AnimationPreviewScreen>(std::move(recording)) : nullptr;
		if (!previewScreen || !previewScreen->CreateFrameTexture(error))
		{
			SendHostMessage("error " + error);
			return true;
		}

		ClosePreview();
		m_previewScreen = previewScreen;
		m_screenManager.AddScreen(m_previewScreen);
		SendHostMessage(LayoutReply("recorded", m_previewScreen->GetRecording().Layout));
		return true;
	}

	if (StartsWith(message, "save "))
	{
		if (!m_previewScreen) SendHostMessage("error there is no recording to save");
		else if (m_previewScreen->GetRecording().Save(message.substr(5), error)) SendHostMessage("saved");
		else SendHostMessage("error " + error);
		return true;
	}

	if (message == "close")
	{
		ClosePreview();
		return true;
	}

	if (StartsWith(message, "preview "))
	{
		if (!HandlePreviewMessage(message.substr(8), error)) SendHostMessage("error " + error);
		return true;
	}

	return false;
}

bool ParticleRecorderGame::HandlePreviewMessage(const std::string& command, std::string& error)
{
	if (!m_previewScreen)
	{
		error = "the animation preview isn't open";
		return false;
	}

	const size_t space = command.find(' ');
	const std::string name = command.substr(0, space);
	const std::string value = space == std::string::npos ? std::string() : command.substr(space + 1);

	if (name == "play") m_previewScreen->Play();
	else if (name == "pause") m_previewScreen->Pause();
	else if (name == "loop") m_previewScreen->SetLooping(value == "1");
	else if (name == "frame") m_previewScreen->SetFrame(std::atoi(value.c_str()));
	else if (name == "zoom") m_previewScreen->SetZoom(static_cast<float>(std::atof(value.c_str())));
	else if (name == "background" && value == "dark") m_previewScreen->SetBackground(AnimationPreviewScreen::Background::DARK);
	else if (name == "background" && value == "light") m_previewScreen->SetBackground(AnimationPreviewScreen::Background::LIGHT);
	else if (name == "background" && value == "checker") m_previewScreen->SetBackground(AnimationPreviewScreen::Background::CHECKER);
	else
	{
		error = "unknown preview command: " + command;
		return false;
	}
	return true;
}

void ParticleRecorderGame::ClosePreview()
{
	if (!m_previewScreen) return;

	m_screenManager.RemoveScreen(m_previewScreen);
	m_previewScreen.reset();
	m_sentFrame = -1;
}

// Returns false if message isn't a path message.
bool ParticleRecorderGame::HandlePathMessage(const std::string& message)
{
	if (message == "pen on" || message == "pen off")
	{
		m_recorderScreen->SetPenMode(message == "pen on");
		return true;
	}

	if (StartsWith(message, "pen select "))
	{
		m_recorderScreen->SetSelectedPoint(std::atoi(message.c_str() + 11));
		return true;
	}

	return false;
}
