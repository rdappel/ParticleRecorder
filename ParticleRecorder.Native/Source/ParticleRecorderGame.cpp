#include "pch.h"
#include "ParticleRecorderGame.h"
#include "Screens/RecorderScreen.h"

ParticleRecorderGame::ParticleRecorderGame(const GameConfig& config)
	: Game(config)
{
	SetTitle("Particle Recorder");
}

void ParticleRecorderGame::LoadContent(ResourceManager& resourceManager)
{
	m_recorderScreen = std::make_shared<RecorderScreen>();
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

	for (const bool exportFiles : { false, true })
	{
		const std::string prefix = exportFiles ? "export " : "measure ";
		if (message.compare(0, prefix.size(), prefix) != 0) continue;

		std::string reply, error;
		if (m_recorderScreen->Record(exportFiles, message.substr(prefix.size()), reply, error)) SendHostMessage(reply);
		else SendHostMessage("error " + error);
		return;
	}

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
