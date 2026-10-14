#pragma once

#include "pch.h"

using namespace KatanaEngine;

// One point of an emitter path. In and Out are the Bézier handles, as offsets from Position.
struct EmitterPathPoint
{
	Vector2 Position;
	Vector2 In;
	Vector2 Out;
	float Rate = 1.0f;		// emission rate multiplier at this point (1 = EmissionRate)
};

// The path the emitter follows, drawn with the editor's pen tool: cubic Bézier segments
// between points, open (start -> end) or closed (a loop). Positions are in effect space,
// where the origin is the emitter's resting position.
//
// Progress is measured by arc length, so moving progress at a constant rate moves the
// emitter at a constant speed.
class EmitterPath
{
public:

	// Parses "<closed 0|1> x,y,inX,inY,outX,outY,rate%;..." (see ParticleSettings.cs).
	// On failure returns false and leaves the path unchanged.
	bool Parse(const std::string& text);

	bool IsEmpty() const { return m_points.empty(); }
	bool IsClosed() const { return m_isClosed; }
	const std::vector<EmitterPathPoint>& GetPoints() const { return m_points; }

	// Progress 0..1 covers the whole path once. A closed path wraps; an open path clamps.
	Vector2 PositionAt(double progress) const;

	// The interpolated rate multiplier (1 with no path).
	float RateAt(double progress) const;

	// Calls fn(const Vector2& position, float rate) along the path, for drawing it.
	template<typename TFunction>
	void ForEachSample(TFunction&& fn) const
	{
		for (const Sample& sample : m_samples) fn(sample.Position, sample.Rate);
	}

private:

	struct Sample
	{
		Vector2 Position;
		float Distance = 0.0f;	// arc length from the first point
		float Rate = 1.0f;
	};

	void Build();
	float ToDistance(double progress) const;
	float RateAtDistance(float distance) const;

	std::vector<EmitterPathPoint> m_points;
	bool m_isClosed = false;

	std::vector<Sample> m_samples;				// the curve as a polyline, in order
	std::vector<float> m_pointDistances;		// arc length at each point (+ the end of a closed loop)
	float m_length = 0.0f;
};
