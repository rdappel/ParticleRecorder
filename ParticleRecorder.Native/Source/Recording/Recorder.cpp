#include "pch.h"
#include "Recorder.h"

#include "Particles/PreviewEffect.h"
#include "Core/Resource/Image.h"
#include "Core/Resource/RenderTarget.h"

#include <cfloat>
#include <climits>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <sstream>

namespace
{
	constexpr int MaxFrames = 10000;	// guards against typos like a 1000-second recording

	// Where the emitter is at a given time (seconds; negative during prewarm).
	// Fixed at the origin for now; a drawn emitter path will be evaluated here.
	Vector2 EmitterPositionAt(double /* time */)
	{
		return Vector2::ZERO;
	}

	// One run of the effect at a fixed timestep, starting after the prewarm.
	class Simulation
	{
	public:
		Simulation(const ParticleSettings& settings, const std::shared_ptr<Texture>& texture, const RecordingOptions& options)
			: m_effect(settings, options.Seed)
			, m_step(1.0 / options.Fps)
		{
			const int prewarmSteps = static_cast<int>(std::lround(options.Prewarm * options.Fps));
			m_time = -prewarmSteps * m_step;

			m_effect.SetTexture(texture);
			m_effect.SetPosition(EmitterPositionAt(m_time));
			for (int i = 0; i < prewarmSteps; ++i) Step();
		}

		void Step()
		{
			m_time += m_step;
			m_effect.SetPosition(EmitterPositionAt(m_time));
			m_effect.Update(GameTime(m_time, m_step));
		}

		PreviewEffect& GetEffect() { return m_effect; }

	private:
		PreviewEffect m_effect;
		double m_step;
		double m_time = 0.0;
	};

	// The column count that keeps the sheet's longer side as short as possible.
	int ChooseColumns(int frameCount, int frameWidth, int frameHeight)
	{
		int bestColumns = 1;
		long long bestSide = LLONG_MAX;
		for (int columns = 1; columns <= frameCount; ++columns)
		{
			const int rows = (frameCount + columns - 1) / columns;
			const long long side = std::max(static_cast<long long>(columns) * frameWidth, static_cast<long long>(rows) * frameHeight);
			if (side < bestSide)
			{
				bestSide = side;
				bestColumns = columns;
			}
		}
		return bestColumns;
	}

	bool WriteAnimationFile(const std::filesystem::path& pngPath, const RecordingOptions& options, const RecordingLayout& layout)
	{
		std::filesystem::path animPath = pngPath;
		animPath.replace_extension(".anim");

		std::ofstream file(animPath);
		if (!file) return false;

		file << "[" << pngPath.stem().u8string() << "]\n"
			<< "FrameWidth=" << layout.FrameWidth << "\n"
			<< "FrameHeight=" << layout.FrameHeight << "\n"
			<< "FramesPerRow=" << layout.Columns << "\n"
			<< "Frames=0-" << (layout.FrameCount - 1) << "\n"
			<< "FrameDuration=" << std::setprecision(9) << (1.0 / options.Fps) << "\n"
			<< "Loop=" << (options.Loop ? "true" : "false") << "\n";
		return static_cast<bool>(file);
	}
}

int RecordingOptions::GetFrameCount() const
{
	return std::max(1, static_cast<int>(std::lround(Duration * Fps)));
}

bool ParseRecordingOptions(const std::string& text, RecordingOptions& options, std::string& error)
{
	size_t position = 0;
	while (position < text.size())
	{
		const size_t equals = text.find('=', position);
		if (equals == std::string::npos) break;

		const std::string key = text.substr(position, equals - position);
		if (key == "path")
		{
			options.Path = text.substr(equals + 1);
			break;
		}

		const size_t end = std::min(text.find(' ', equals), text.size());
		std::istringstream value(text.substr(equals + 1, end - equals - 1));
		position = end + 1;

		bool parsed = false;
		if (key == "seed") parsed = static_cast<bool>(value >> options.Seed);
		else if (key == "fps") parsed = static_cast<bool>(value >> options.Fps);
		else if (key == "duration") parsed = static_cast<bool>(value >> options.Duration);
		else if (key == "prewarm") parsed = static_cast<bool>(value >> options.Prewarm);
		else if (key == "padding") parsed = static_cast<bool>(value >> options.Padding);
		else if (key == "columns") parsed = static_cast<bool>(value >> options.Columns);
		else if (key == "loop") parsed = static_cast<bool>(value >> options.Loop);

		if (!parsed)
		{
			error = "invalid recording option: " + key;
			return false;
		}
	}

	if (options.Fps <= 0.0f || options.Duration <= 0.0f || options.Prewarm < 0.0f || options.Padding < 0 || options.Columns < 0)
	{
		error = "recording options out of range";
		return false;
	}
	if (options.GetFrameCount() > MaxFrames)
	{
		error = "too many frames: " + std::to_string(options.GetFrameCount()) + " (the limit is " + std::to_string(MaxFrames) + ")";
		return false;
	}
	return true;
}

Recorder::Recorder(const ParticleSettings& settings, std::shared_ptr<Texture> texture)
	: m_settings(settings)
	, m_texture(std::move(texture))
{
}

bool Recorder::Measure(const RecordingOptions& options, RecordingLayout& layout, std::string& error) const
{
	Simulation simulation(m_settings, m_texture, options);
	const PreviewEffect& effect = simulation.GetEffect();

	float minX = FLT_MAX, minY = FLT_MAX, maxX = -FLT_MAX, maxY = -FLT_MAX;
	const int frameCount = options.GetFrameCount();
	for (int frame = 0; frame < frameCount; ++frame)
	{
		simulation.Step();
		effect.ForEachParticle([&](const SimpleParticle& particle)
		{
			if (particle.Scale <= 0.0f || particle.Color.Alpha <= 0.0f) return;	// not drawn

			const float radius = effect.GetParticleRadius(particle.Scale);
			minX = std::min(minX, particle.Position.X - radius);
			minY = std::min(minY, particle.Position.Y - radius);
			maxX = std::max(maxX, particle.Position.X + radius);
			maxY = std::max(maxY, particle.Position.Y + radius);
		});
	}

	if (minX > maxX)
	{
		error = "nothing to record: no particles are visible during the recording";
		return false;
	}

	// Whole-pixel origin, so particles land on the same pixel grid as in the preview.
	const float left = std::floor(minX) - options.Padding;
	const float top = std::floor(minY) - options.Padding;
	layout.Origin = Vector2(left, top);
	layout.FrameWidth = static_cast<int>(std::ceil(maxX) + options.Padding - left);
	layout.FrameHeight = static_cast<int>(std::ceil(maxY) + options.Padding - top);
	layout.FrameCount = frameCount;
	layout.Columns = options.Columns > 0
		? std::min(options.Columns, frameCount)
		: ChooseColumns(frameCount, layout.FrameWidth, layout.FrameHeight);
	layout.Rows = (frameCount + layout.Columns - 1) / layout.Columns;
	return true;
}

bool Recorder::Export(const RecordingOptions& options, RecordingLayout& layout, std::string& error) const
{
	if (options.Path.empty())
	{
		error = "no export path given";
		return false;
	}
	if (!Measure(options, layout, error)) return false;

	auto frame = std::make_shared<RenderTarget>();
	if (!frame->Create(layout.FrameWidth, layout.FrameHeight))
	{
		error = "could not create a " + std::to_string(layout.FrameWidth) + "x" + std::to_string(layout.FrameHeight)
			+ " frame (larger than the graphics card allows?)";
		return false;
	}

	Image sheet;
	if (!sheet.Create(layout.GetSheetWidth(), layout.GetSheetHeight()))
	{
		error = "not enough memory for a " + std::to_string(layout.GetSheetWidth()) + "x"
			+ std::to_string(layout.GetSheetHeight()) + " sprite sheet";
		return false;
	}

	SpriteBatch spriteBatch;
	Simulation simulation(m_settings, m_texture, options);
	for (int index = 0; index < layout.FrameCount; ++index)
	{
		simulation.Step();

		frame->Begin();
		frame->Clear(Color(0.0f, 0.0f, 0.0f, 0.0f));
		simulation.GetEffect().Draw(spriteBatch, layout.Origin);
		frame->End();

		sheet.CopyFrom(*frame, (index % layout.Columns) * layout.FrameWidth, (index / layout.Columns) * layout.FrameHeight);
	}

	if (!sheet.Save(options.Path))
	{
		error = "could not save " + options.Path;
		return false;
	}

	if (!WriteAnimationFile(std::filesystem::u8path(options.Path), options, layout))
	{
		error = "could not write the .anim file next to " + options.Path;
		return false;
	}
	return true;
}
