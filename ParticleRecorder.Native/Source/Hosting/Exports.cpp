#include "pch.h"
#include "ParticleRecorderGame.h"

#include "Core/Hosting/HostExports.h"

namespace
{
	std::unique_ptr<Game> CreateHostedGame(const GameConfig& config)
	{
		return std::make_unique<ParticleRecorderGame>(config);
	}
}

KATANA_DEFINE_HOST_EXPORTS(CreateHostedGame)
