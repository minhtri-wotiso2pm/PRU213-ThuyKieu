using System;
using UnityEngine;

namespace ThuyKieu.Gameplay.Chase
{
    /// <summary>
    /// The fixed bamboo chase course of Chapter 5, as built by the scene setup: two centrelines from the forest edge to the
    /// temple path (the moonlit ditch way and the thick-bamboo way split once and meet again), the obstacles along them,
    /// where everyone starts, the ditches and the ravine she can fall into, and the finish beyond the bridge.
    /// It also puts every changing thing (falling culms, the bridge) back for a new attempt.
    /// The QA pilot reads the same centrelines and obstacles a player learns by running the course.
    /// </summary>
    public class ChaseCourse : MonoBehaviour
    {
        /// <summary>New kinds go at the end: the scene stores the index. Dodge: rocks and broken culms filling one side of the way.</summary>
        public enum ObstacleKind { Jump, Duck, Gap, Squeeze, Hazard, Bridge, Dodge }

        [Serializable]
        public struct Obstacle
        {
            public ObstacleKind Kind;
            public Vector3 Position;
            public Vector3 Forward;
            [Tooltip("Metres along the run it takes (a ducking arch, a ditch).")]
            public float Length;
            [Tooltip("0: both ways · 1: moonlit way only · 2: thick-bamboo way only.")]
            public int Way;
        }

        [SerializeField] private Vector3[] _moonWay;
        [SerializeField] private Vector3[] _thickWay;
        [SerializeField] private Obstacle[] _obstacles;
        [SerializeField] private Transform _start, _pursuerStart, _companionStart;
        [SerializeField] private BoxCollider _finish;
        [Tooltip("Ditch and ravine bottoms: falling in there ends the attempt.")]
        [SerializeField] private BoxCollider[] _falls;
        [SerializeField] private BoxCollider _thickZone;
        [SerializeField] private float _fallBelow = -6f;

        private float[] _moonLengths, _thickLengths;
        private IChaseReset[] _resets;

        public Transform StartPoint => _start;
        public Transform PursuerStart => _pursuerStart;
        public Transform CompanionStart => _companionStart;
        public BoxCollider Finish => _finish;
        public Obstacle[] Obstacles => _obstacles;
        public float Length => Lengths(1)[_moonWay.Length - 1];

        /// <summary>1: the moonlit ditch way (the main line) · 2: the thick-bamboo way.</summary>
        public Vector3[] Way(int way) => way == 2 ? _thickWay : _moonWay;

        public float WayLength(int way) { var l = Lengths(way); return l[l.Length - 1]; }

        public bool InThickWay(Vector3 feet) => Inside(_thickZone, feet + Vector3.up * 0.5f);

        public bool InFinish(Vector3 feet) => Inside(_finish, feet + Vector3.up * 0.5f);

        /// <summary>Inside a box as it is turned (its world bounds would be far bigger than a ditch laid across at an angle).</summary>
        public static bool Inside(BoxCollider box, Vector3 point)
        {
            if (box == null) return false;
            Vector3 local = box.transform.InverseTransformPoint(point) - box.center;
            Vector3 half = box.size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        /// <summary>In a ditch or the ravine, or fallen through the world.</summary>
        public bool Fallen(Vector3 feet)
        {
            if (feet.y < _fallBelow) return true;
            if (_falls == null) return false;
            foreach (var fall in _falls) if (Inside(fall, feet + Vector3.up * 0.3f)) return true;
            return false;
        }

        /// <summary>Every falling culm and the bridge back as they were, armed (or disarmed) for the chase.</summary>
        public void ResetAll(bool armed)
        {
            _resets ??= GetComponentsInChildren<IChaseReset>(true);
            foreach (var reset in _resets) { reset.ResetForChase(); reset.Armed = armed; }
        }

        // ---------------------------------------------------------------- centrelines

        private float[] Lengths(int way)
        {
            if (way == 2) return _thickLengths ??= Measure(_thickWay);
            return _moonLengths ??= Measure(_moonWay);
        }

        private static float[] Measure(Vector3[] points)
        {
            var lengths = new float[points.Length];
            for (int i = 1; i < points.Length; i++) lengths[i] = lengths[i - 1] + Vector3.Distance(points[i - 1], points[i]);
            return lengths;
        }

        public Vector3 PointAt(int way, float distance)
        {
            var points = Way(way);
            var lengths = Lengths(way);
            if (distance <= 0f) return points[0];
            for (int i = 1; i < points.Length; i++)
                if (distance <= lengths[i])
                    return Vector3.Lerp(points[i - 1], points[i], Mathf.InverseLerp(lengths[i - 1], lengths[i], distance));
            return points[points.Length - 1];
        }

        public Vector3 DirectionAt(int way, float distance)
        {
            Vector3 a = PointAt(way, distance - 1f), b = PointAt(way, distance + 1f);
            Vector3 d = b - a;
            d.y = 0f;
            return d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.forward;
        }

        /// <summary>Distance along a way of the point nearest to <paramref name="position"/>, and how far off the line it is.</summary>
        public float Project(int way, Vector3 position, out float off)
        {
            var points = Way(way);
            var lengths = Lengths(way);
            float best = 0f;
            off = float.MaxValue;
            for (int i = 1; i < points.Length; i++)
            {
                Vector3 a = points[i - 1], b = points[i];
                Vector3 ab = b - a;
                float k = Mathf.Clamp01(Vector3.Dot(position - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
                Vector3 nearest = a + ab * k;
                float d = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(nearest.x, nearest.z));
                if (d < off) { off = d; best = lengths[i - 1] + k * (lengths[i] - lengths[i - 1]); }
            }
            return best;
        }

        /// <summary>How far along the course someone is, as a distance on the moonlit way (the thick way is scaled onto it).</summary>
        public float Progress(Vector3 position)
        {
            float moon = Project(1, position, out float moonOff);
            float thick = Project(2, position, out float thickOff);
            return thickOff < moonOff ? thick / Mathf.Max(1f, WayLength(2)) * WayLength(1) : moon;
        }
    }
}
