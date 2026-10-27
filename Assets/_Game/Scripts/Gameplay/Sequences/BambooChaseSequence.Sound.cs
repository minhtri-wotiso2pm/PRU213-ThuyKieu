using System.Collections.Generic;
using System.Linq;
using ThuyKieu.Core.Story;
using ThuyKieu.Gameplay.Chase;
using ThuyKieu.Player;
using ThuyKieu.Presentation;
using UnityEngine;

namespace ThuyKieu.Gameplay.Sequences
{
    // QA for the chase's sound logic (not how it sounds: the runs are muted). Every effect started during an attempt is
    // counted by id, with where she was (in the air, just landed), so a run can be checked for: one sound per bridge event,
    // three beats per falling culm, no step in the air, no step on top of a landing, none of the old synthesised stand-ins.
    public partial class BambooChaseSequence
    {
        private static readonly string[] Retired = { "stumble", "step_mud", "clatter", "blow", "string_snap", "whoosh", "creak", "slide_slam" };

        private sealed class SoundLog
        {
            public readonly Dictionary<string, int> Counts = new Dictionary<string, int>();
            public int Steps, StepsInAir, StepsOnLanding;
            /// <summary>Where a landing came without a jump before it, or a step on top of a landing (to find what she dropped off).</summary>
            public readonly List<string> Odd = new List<string>();
            private PlayerMovement _player;
            /// <summary>Where a point is on the course, for the notes.</summary>
            public System.Func<Vector3, string> Where;
            private float _landedAt = -1f, _jumpedAt = -9f;

            public int this[string id] => Counts.TryGetValue(id, out int n) ? n : 0;
            public int Starting(string prefix) => Counts.Where(p => p.Key.StartsWith(prefix)).Sum(p => p.Value);

            public void Attach(PlayerMovement player)
            {
                _player = player;
                StoryAudio.EffectPlayed += Played;
                player.Landed += Landed;
                player.Jumped += Jumped;
            }

            public void Detach()
            {
                StoryAudio.EffectPlayed -= Played;
                if (_player != null) { _player.Landed -= Landed; _player.Jumped -= Jumped; }
            }

            private void Jumped() => _jumpedAt = Time.time;

            private void Landed(float speed)
            {
                if (speed < 2f) return;
                _landedAt = Time.time;
                if (Time.time - _jumpedAt > 1.5f) Odd.Add($"drop {speed:F1} m/s at {_player.transform.position:F1} {Where?.Invoke(_player.transform.position)}");
            }

            private void Played(string id)
            {
                Counts[id] = this[id] + 1;
                if (!id.StartsWith("step_")) return;
                Steps++;
                if (!_player.IsGrounded) StepsInAir++;
                if (_landedAt > 0f && Time.time - _landedAt < 0.15f) { StepsOnLanding++; Odd.Add($"step on a landing at {_player.transform.position:F1}"); }
            }
        }

        private int _topLevel;

        /// <summary>The checks one clean run's sound must pass.</summary>
        private void CheckSound(SoundLog log, Pilot pilot, FallingBamboo[] culms, float metres, int way)
        {
            Check(log["stake_kick"] == 1 && log["rope_snap"] == 1 && log["deck_swing"] == 1 && log["deck_hit"] == 1,
                $"way {way} sound: the bridge's kick, rope, swing and impact once each ({log["stake_kick"]}, {log["rope_snap"]}, {log["deck_swing"]}, {log["deck_hit"]}) · rope strain on the planks {log["rope_strain"]}");
            int fallen = culms.Count(c => c.WarnedAt > 0f);
            Check(log["bamboo_crack"] == 2 * fallen && log["bamboo_sweep"] == fallen && log["bamboo_impact"] == fallen && log["leaf_settle"] == fallen,
                $"way {way} sound: {fallen} falling culms in three beats (crack {log["bamboo_crack"]}, sweep {log["bamboo_sweep"]}, impact {log["bamboo_impact"]}, settle {log["leaf_settle"]})");
            Check(log.StepsInAir == 0 && log.StepsOnLanding == 0, $"way {way} sound: no step in the air ({log.StepsInAir}) or on top of a landing ({log.StepsOnLanding}){(log.Odd.Count > 0 ? " · " + string.Join(", ", log.Odd) : "")}");
            float perStride = metres > 0f ? log.Steps * 1.35f / metres : 0f;
            Check(perStride > 0.8f && perStride < 1.1f, $"way {way} sound: {log.Steps} steps over {metres:F0} m (one per {(log.Steps > 0 ? metres / log.Steps : 0f):F2} m; stride 1.35 m)");
            Check(log.Starting("jump_") == pilot.Jumps && log.Starting("land_") >= pilot.Jumps,
                $"way {way} sound: {pilot.Jumps} jumps → push-offs {log.Starting("jump_")}, landings {log.Starting("land_")} (earth {log["land_earth"]}, wood {log["land_wood"]}, stone {log["land_stone"]})");
            int retired = Retired.Sum(id => log[id]);
            Check(retired == 0, $"way {way} sound: none of the old synthesised stand-ins ({string.Join(", ", Retired.Where(id => log[id] > 0).Select(id => id + " ×" + log[id]))})");
            if (_music != null) Check(_topLevel == 2, $"way {way} music: reached its top level by the bridge ({_topLevel})");
            StoryAutopilot.Note($"FOREST way {way} sounds: {string.Join(", ", log.Counts.OrderBy(p => p.Key).Select(p => p.Key + " " + p.Value))}");
        }
    }
}
