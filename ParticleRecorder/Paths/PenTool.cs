using System.Numerics;
using ParticleRecorder.Effects;

namespace ParticleRecorder.Paths;

/// <summary>
/// Edits an <see cref="EmitterPath"/> from mouse input on the viewport (in effect space,
/// 1 unit = 1 screen pixel). The viewport gets no keyboard input, so every gesture is
/// mouse-only:
/// <list type="bullet">
/// <item>click empty space: add a corner point at the end; drag while adding: pull out smooth handles</item>
/// <item>click a segment: insert a point there, keeping the curve's shape</item>
/// <item>click the start point of an open path: close it</item>
/// <item>drag a point or a handle: move it (a smooth point keeps its handles mirrored)</item>
/// <item>double-click a point: toggle corner / smooth</item>
/// <item>right-click a point: delete it</item>
/// </list>
/// Each method returns true if the path changed.
/// </summary>
public sealed class PenTool
{
    private const float PointRadius = 7f;
    private const float HandleRadius = 6f;
    private const float SegmentRadius = 6f;
    private const float DragThreshold = 3f;
    private const int SamplesPerSegment = 32;

    private enum DragKind { None, NewPoint, Point, InHandle, OutHandle }

    private DragKind _drag;
    private Vector2 _downPosition;
    private int _lastClickIndex = -1;
    private long _lastClickTime;

    public EmitterPath Path { get; private set; } = new();

    /// <summary>The selected point, or -1.</summary>
    public int SelectedIndex { get; private set; } = -1;

    public PathPoint? SelectedPoint => SelectedIndex >= 0 && SelectedIndex < Path.Points.Count ? Path.Points[SelectedIndex] : null;

    public void SetPath(EmitterPath path)
    {
        Path = path;
        SelectedIndex = -1;
        _drag = DragKind.None;
        _lastClickIndex = -1;
    }

    public bool MouseDown(Vector2 position, MouseButtons button, long timeMilliseconds)
    {
        _drag = DragKind.None;
        _downPosition = position;
        var points = Path.Points;
        var pointIndex = FindPoint(position);

        if (button == MouseButtons.Right)
        {
            if (pointIndex < 0) return false;

            points.RemoveAt(pointIndex);
            if (points.Count < 2) Path.Closed = false;
            SelectedIndex = -1;
            return true;
        }
        if (button != MouseButtons.Left) return false;

        if (pointIndex >= 0)
        {
            var isDoubleClick = pointIndex == _lastClickIndex && timeMilliseconds - _lastClickTime <= SystemInformation.DoubleClickTime;
            _lastClickIndex = isDoubleClick ? -1 : pointIndex;
            _lastClickTime = timeMilliseconds;
            SelectedIndex = pointIndex;

            if (isDoubleClick)
            {
                ToggleSmooth(pointIndex);
                return true;
            }

            _drag = DragKind.Point;
            if (pointIndex != 0 || Path.Closed || points.Count < 2) return false;

            Path.Closed = true;
            return true;
        }

        _lastClickIndex = -1;
        var (handleIndex, isOut) = FindHandle(position);
        if (handleIndex >= 0)
        {
            SelectedIndex = handleIndex;
            _drag = isOut ? DragKind.OutHandle : DragKind.InHandle;
            return false;
        }

        if (FindSegment(position) is { } segment)
        {
            SelectedIndex = SplitSegment(segment.Index, segment.T);
            _drag = DragKind.Point;
            return true;
        }

        points.Add(new PathPoint { Position = position });
        SelectedIndex = points.Count - 1;
        _drag = DragKind.NewPoint;
        return true;
    }

    public bool MouseMove(Vector2 position)
    {
        if (SelectedPoint is not { } point) return false;

        switch (_drag)
        {
            case DragKind.NewPoint:
                if (Vector2.Distance(position, _downPosition) < DragThreshold) return false;
                point.Smooth = true;
                point.Out = position - point.Position;
                point.In = -point.Out;
                return true;

            case DragKind.Point:
                point.Position = position;
                return true;

            case DragKind.InHandle:
                point.In = position - point.Position;
                if (point.Smooth) point.Out = Mirror(point.In, point.Out);
                return true;

            case DragKind.OutHandle:
                point.Out = position - point.Position;
                if (point.Smooth) point.In = Mirror(point.Out, point.In);
                return true;

            default:
                return false;
        }
    }

    public void MouseUp() => _drag = DragKind.None;

    public bool DeleteSelected()
    {
        if (SelectedPoint == null) return false;

        Path.Points.RemoveAt(SelectedIndex);
        if (Path.Points.Count < 2) Path.Closed = false;
        SelectedIndex = -1;
        return true;
    }

    public bool Clear()
    {
        if (Path.IsEmpty) return false;

        Path.Points.Clear();
        Path.Closed = false;
        SelectedIndex = -1;
        return true;
    }

    // The opposite handle of a smooth point: pointing away from moved, keeping its own length.
    private static Vector2 Mirror(Vector2 moved, Vector2 opposite)
    {
        var length = moved.Length();
        if (length < 0.001f) return opposite;

        var oppositeLength = opposite.Length();
        return -moved / length * (oppositeLength < 0.001f ? length : oppositeLength);
    }

    private void ToggleSmooth(int index)
    {
        var point = Path.Points[index];
        if (point.Smooth)
        {
            point.Smooth = false;
            point.In = point.Out = Vector2.Zero;
            return;
        }

        point.Smooth = true;
        var previous = Neighbour(index, -1);
        var next = Neighbour(index, 1);
        if (previous == null && next == null) return;

        // Handles along the line through the neighbours, a third of the way to each.
        var tangent = Vector2.Normalize((next?.Position ?? point.Position) - (previous?.Position ?? point.Position));
        if (float.IsNaN(tangent.X)) return;

        if (next != null) point.Out = tangent * Vector2.Distance(point.Position, next.Position) / 3f;
        if (previous != null) point.In = -tangent * Vector2.Distance(point.Position, previous.Position) / 3f;
    }

    private PathPoint? Neighbour(int index, int offset)
    {
        var count = Path.Points.Count;
        var neighbour = index + offset;
        if (Path.Closed && count > 1) return Path.Points[(neighbour + count) % count];
        return neighbour >= 0 && neighbour < count ? Path.Points[neighbour] : null;
    }

    private int FindPoint(Vector2 position)
    {
        // Last drawn wins, so the newest point is picked when points overlap.
        for (var index = Path.Points.Count - 1; index >= 0; index--)
        {
            if (Vector2.Distance(Path.Points[index].Position, position) <= PointRadius) return index;
        }
        return -1;
    }

    private (int Index, bool IsOut) FindHandle(Vector2 position)
    {
        for (var index = Path.Points.Count - 1; index >= 0; index--)
        {
            var point = Path.Points[index];
            if (point.Out != Vector2.Zero && Vector2.Distance(point.Position + point.Out, position) <= HandleRadius) return (index, true);
            if (point.In != Vector2.Zero && Vector2.Distance(point.Position + point.In, position) <= HandleRadius) return (index, false);
        }
        return (-1, false);
    }

    private int SegmentCount => Path.Points.Count < 2 ? 0 : Path.Closed ? Path.Points.Count : Path.Points.Count - 1;

    // The control points of segment index (from point index to the next).
    private (Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3) GetSegment(int index)
    {
        var start = Path.Points[index];
        var end = Path.Points[(index + 1) % Path.Points.Count];
        return (start.Position, start.Position + start.Out, end.Position + end.In, end.Position);
    }

    private static Vector2 Evaluate((Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3) c, float t)
    {
        var u = 1 - t;
        return u * u * u * c.P0 + 3 * u * u * t * c.P1 + 3 * u * t * t * c.P2 + t * t * t * c.P3;
    }

    private (int Index, float T)? FindSegment(Vector2 position)
    {
        (int Index, float T)? best = null;
        var bestDistance = SegmentRadius;
        for (var index = 0; index < SegmentCount; index++)
        {
            var segment = GetSegment(index);
            for (var step = 0; step <= SamplesPerSegment; step++)
            {
                var t = (float)step / SamplesPerSegment;
                var distance = Vector2.Distance(Evaluate(segment, t), position);
                if (distance > bestDistance) continue;

                bestDistance = distance;
                best = (index, t);
            }
        }

        // Ends of a segment are the points themselves; those clicks select instead.
        return best is { T: > 0f and < 1f } ? best : null;
    }

    // Splits segment index at t (de Casteljau), keeping the curve's shape. Returns the new point's index.
    private int SplitSegment(int index, float t)
    {
        var (p0, p1, p2, p3) = GetSegment(index);
        var p01 = Vector2.Lerp(p0, p1, t);
        var p12 = Vector2.Lerp(p1, p2, t);
        var p23 = Vector2.Lerp(p2, p3, t);
        var p012 = Vector2.Lerp(p01, p12, t);
        var p123 = Vector2.Lerp(p12, p23, t);
        var split = Vector2.Lerp(p012, p123, t);

        var start = Path.Points[index];
        var end = Path.Points[(index + 1) % Path.Points.Count];
        start.Out = p01 - p0;
        end.In = p23 - p3;

        var point = new PathPoint
        {
            Position = split,
            In = p012 - split,
            Out = p123 - split,
            Smooth = p012 != split || p123 != split,
            Rate = start.Rate + (end.Rate - start.Rate) * t,
        };
        Path.Points.Insert(index + 1, point);
        return index + 1;
    }
}
