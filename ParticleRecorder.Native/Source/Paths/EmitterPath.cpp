#include "pch.h"
#include "EmitterPath.h"

#include "Core/Math/Spline.h"

#include <sstream>

namespace
{
	constexpr int SamplesPerSegment = 32;

	Vector2 Lerp(const Vector2& start, const Vector2& end, float amount)
	{
		return start + (end - start) * amount;
	}
}

bool EmitterPath::Parse(const std::string& text)
{
	std::istringstream stream(text);
	int closed = 0;
	if (!(stream >> closed)) return false;

	std::vector<EmitterPathPoint> points;
	std::string item;
	stream >> std::ws;
	while (std::getline(stream, item, ';'))
	{
		if (item.empty()) continue;

		std::istringstream values(item);
		EmitterPathPoint point;
		char comma = 0;
		float rate = 100.0f;
		if (!(values >> point.Position.X >> comma >> point.Position.Y >> comma
			>> point.In.X >> comma >> point.In.Y >> comma
			>> point.Out.X >> comma >> point.Out.Y >> comma >> rate)) return false;

		point.Rate = std::max(0.0f, rate / 100.0f);
		points.push_back(point);
	}

	m_points = std::move(points);
	m_isClosed = closed != 0;
	Build();
	return true;
}

void EmitterPath::Build()
{
	m_samples.clear();
	m_pointDistances.clear();
	m_length = 0.0f;
	if (m_points.empty()) return;

	const size_t count = m_points.size();
	const size_t segments = count < 2 ? 0 : (m_isClosed ? count : count - 1);

	m_samples.push_back({ m_points[0].Position, 0.0f, m_points[0].Rate });
	for (size_t index = 0; index < segments; ++index)
	{
		const EmitterPathPoint& start = m_points[index];
		const EmitterPathPoint& end = m_points[(index + 1) % count];
		const Spline curve(start.Position, start.Position + start.Out, end.Position + end.In, end.Position);

		m_pointDistances.push_back(m_length);
		for (int step = 1; step <= SamplesPerSegment; ++step)
		{
			const Vector2 position = curve.Lerp(static_cast<float>(step) / SamplesPerSegment);
			m_length += Vector2::Distance(m_samples.back().Position, position);
			m_samples.push_back({ position, m_length, 1.0f });
		}
	}
	m_pointDistances.push_back(m_length);	// open: the last point; closed: back at the first

	for (Sample& sample : m_samples) sample.Rate = RateAtDistance(sample.Distance);
}

float EmitterPath::ToDistance(double progress) const
{
	const double amount = m_isClosed ? progress - std::floor(progress) : std::clamp(progress, 0.0, 1.0);
	return static_cast<float>(amount * m_length);
}

Vector2 EmitterPath::PositionAt(double progress) const
{
	if (m_samples.empty()) return Vector2::ZERO;
	if (m_samples.size() == 1 || m_length <= 0.0f) return m_samples.front().Position;

	const float distance = ToDistance(progress);
	const auto next = std::upper_bound(m_samples.begin(), m_samples.end(), distance,
		[](float value, const Sample& sample) { return value < sample.Distance; });
	if (next == m_samples.begin()) return m_samples.front().Position;
	if (next == m_samples.end()) return m_samples.back().Position;

	const Sample& previous = *(next - 1);
	const float span = next->Distance - previous.Distance;
	return span <= 0.0f ? previous.Position : Lerp(previous.Position, next->Position, (distance - previous.Distance) / span);
}

float EmitterPath::RateAt(double progress) const
{
	if (m_points.empty()) return 1.0f;
	if (m_points.size() == 1 || m_length <= 0.0f) return m_points.front().Rate;

	return RateAtDistance(ToDistance(progress));
}

float EmitterPath::RateAtDistance(float distance) const
{
	if (m_points.size() < 2 || m_pointDistances.size() < 2) return m_points.empty() ? 1.0f : m_points.front().Rate;

	// m_pointDistances[i] is where point i sits; the last entry is the end of the path
	// (the last point, or the first point again for a closed loop).
	const size_t count = m_points.size();
	for (size_t index = 0; index + 1 < m_pointDistances.size(); ++index)
	{
		const float start = m_pointDistances[index];
		const float end = m_pointDistances[index + 1];
		if (distance > end && index + 2 < m_pointDistances.size()) continue;

		const float from = m_points[index].Rate;
		const float to = m_points[(index + 1) % count].Rate;
		const float amount = end > start ? std::clamp((distance - start) / (end - start), 0.0f, 1.0f) : 0.0f;
		return from + (to - from) * amount;
	}
	return m_points.back().Rate;
}
