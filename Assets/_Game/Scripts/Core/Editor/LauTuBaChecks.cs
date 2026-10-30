using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThuyKieu.Gameplay.Chapters.LauTuBa;
using ThuyKieu.Gameplay.Stealth;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Automated checks for the first-person Chapters 3–4 (the project has no NUnit test assembly; this follows the batch
    /// QA style of the other tools):
    /// <list type="bullet">
    /// <item>Rules: sight combination limits, the second scare's conditions, who knows a hiding place.</item>
    /// <item>Map: the walkable mesh (the slim agent the chapters use) connects every place the story needs, through the
    /// real doorways; the guard's rounds and chị Lam's way out are walkable; every hiding place has room for Kiều.</item>
    /// </list>
    ///   -executeMethod ThuyKieu.Core.Editor.LauTuBaChecks.RunBatch  → Tools/LauTuBa/QA/Checks.txt
    /// </summary>
    public static class LauTuBaChecks
    {
        public const string ReportPath = "Tools/LauTuBa/QA/Checks.txt";

        [MenuItem("ThuyKieu/Lầu Tú Bà/Run checks")]
        public static void RunMenu() { Debug.Log(Run(out _)); }

        public static void RunBatch()
        {
            bool ok;
            string report;
            try { report = Run(out ok); }
            catch (System.Exception exception) { ok = false; report = "EXCEPTION " + exception; }
            Debug.Log(report);
            EditorApplication.Exit(ok ? 0 : 1);
        }

        public static string Run(out bool ok)
        {
            var log = new StringBuilder();
            var failures = new List<string>();
            Rules(log, failures);
            Map(LauTuBaSetup.Chapter3Path, log, failures, false);
            Map(LauTuBaSetup.Chapter4Path, log, failures, true);
            Floors(log, failures);
            YellClip(log);
            ok = failures.Count == 0;
            log.Insert(0, (ok ? "LAU TU BA CHECKS PASS" : "LAU TU BA CHECKS FAIL · " + failures.Count + " problems") + "\n");
            foreach (string failure in failures) log.AppendLine("PROBLEM " + failure);
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, log.ToString(), new UTF8Encoding(false));
            return log.ToString();
        }

        // ------------------------------------------------------------------ floors under her feet (Chapter 4, open)

        /// <summary>
        /// Every walkable half metre of the open Chapter 4 scene, probed the way her footsteps probe it (a rug or mat first,
        /// then the tagged collider under her feet): how much of the map is each floor, and any walkable floor that has no tag
        /// (it would sound like boards whatever it looks like).
        /// </summary>
        private static void Floors(StringBuilder log, List<string> failures)
        {
            log.AppendLine("Floors (Chapter 4, walkable points every 0.5 m)");
            var patches = Object.FindObjectsByType<ThuyKieu.Environment.SurfaceTag>()
                .Where(t => new SerializedObject(t).FindProperty("_patch").boolValue)
                .Select(t =>
                {
                    var renderers = t.GetComponentsInChildren<Renderer>();
                    var b = renderers[0].bounds;
                    foreach (var r in renderers) b.Encapsulate(r.bounds);
                    return (tag: t, bounds: b);
                }).ToList();
            var counts = new Dictionary<string, int>();
            var untagged = new Dictionary<string, int>();
            var nav = Object.FindAnyObjectByType<ThuyKieu.Environment.NarrowNavMesh>();
            foreach (var obstacle in Object.FindObjectsByType<ThuyKieu.Gameplay.House.HouseDoor>().SelectMany(d => d.GetComponentsInChildren<NavMeshObstacle>()))
                obstacle.enabled = false;
            NavMesh.RemoveAllNavMeshData();
            nav.Build();
            var filter = new NavMeshQueryFilter { agentTypeID = nav.AgentTypeId, areaMask = NavMesh.AllAreas };
            for (float x = -25f; x <= 21.5f; x += 0.5f)
                for (float z = -22f; z <= 19.5f; z += 0.5f)
                {
                    if (!NavMesh.SamplePosition(new Vector3(x, 0.4f, z), out var hit, 0.5f, filter) || Mathf.Abs(hit.position.x - x) > 0.25f || Mathf.Abs(hit.position.z - z) > 0.25f) continue;
                    Vector3 feet = hit.position;
                    string surface = null;
                    foreach (var (tag, b) in patches)
                        if (feet.x >= b.min.x && feet.x <= b.max.x && feet.z >= b.min.z && feet.z <= b.max.z && Mathf.Abs(feet.y - b.max.y) <= 0.12f) { surface = tag.Surface + " (covering)"; break; }
                    if (surface == null && Physics.Raycast(feet + Vector3.up * 0.3f, Vector3.down, out var ground, 0.8f, 1 << 8, QueryTriggerInteraction.Ignore))
                    {
                        var tag = ground.collider.GetComponentInParent<ThuyKieu.Environment.SurfaceTag>();
                        if (tag != null) surface = tag.Surface.ToString();
                        else
                        {
                            var hitTransform = ground.collider.transform;
                            string name = hitTransform.parent != null ? hitTransform.parent.name + "/" + hitTransform.name : hitTransform.name;
                            untagged[name] = untagged.TryGetValue(name, out int n) ? n + 1 : 1;
                            surface = "untagged";
                        }
                    }
                    surface = surface ?? "no floor hit";
                    counts[surface] = counts.TryGetValue(surface, out int c) ? c + 1 : 1;
                }
            log.AppendLine("  info " + string.Join(" · ", counts.OrderByDescending(p => p.Value).Select(p => $"{p.Key} {p.Value}")) + $" · {patches.Count} rugs/mats");
            if (untagged.Count > 0) log.AppendLine("  info untagged floor under: " + string.Join(", ", untagged.OrderByDescending(p => p.Value).Select(p => $"{p.Key} ×{p.Value}")));
            // Untagged sounds like boards: right for sills and furniture tops she can stand on, wrong for a floor slab.
            int bad = untagged.Where(p => System.Text.RegularExpressions.Regex.IsMatch(p.Key, "Floor|Slab|Alley|Yard|Court|Kitchen|Hall|Veranda|Step", System.Text.RegularExpressions.RegexOptions.IgnoreCase)).Sum(p => p.Value);
            bool ok = counts.Values.Sum() > 500 && bad == 0;
            log.AppendLine((ok ? "  ok   " : "  FAIL ") + $"floor slabs and steps without a surface: {bad} of {counts.Values.Sum()} points ({untagged.Values.Sum()} on untagged sills and furniture, heard as wood)");
            if (!ok) failures.Add($"Chapter 4: {bad} walkable points have no surface tag");
            NavMesh.RemoveAllNavMeshData();
        }

        /// <summary>What the TuBa-Yell clip does (length, loop, which muscles move most), to decide whether it suits a grab.</summary>
        private static void YellClip(StringBuilder log)
        {
            string path = AssetDatabase.FindAssets("TuBa-Yell").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p => p.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase));
            var clip = path != null ? AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview")) : null;
            if (clip == null) { log.AppendLine("  info TuBa-Yell: not found"); return; }
            var ranges = AnimationUtility.GetCurveBindings(clip)
                .Select(binding => (name: binding.propertyName, curve: AnimationUtility.GetEditorCurve(clip, binding)))
                .Where(c => c.curve != null && c.curve.length > 1 && !c.name.StartsWith("Root") && !c.name.StartsWith("Motion"))
                .Select(c =>
                {
                    float lo = float.MaxValue, hi = float.MinValue;
                    for (float t = 0f; t <= clip.length; t += clip.length / 40f) { float v = c.curve.Evaluate(t); lo = Mathf.Min(lo, v); hi = Mathf.Max(hi, v); }
                    return (c.name, range: hi - lo);
                }).OrderByDescending(c => c.range).Take(8);
            log.AppendLine($"  info TuBa-Yell: {clip.length:F2} s · loop {clip.isLooping} · moves most: " + string.Join(", ", ranges.Select(r => $"{r.name} {r.range:F2}")));
        }

        // ------------------------------------------------------------------ rules

        private static void Rules(StringBuilder log, List<string> failures)
        {
            void Check(bool condition, string what) { log.AppendLine((condition ? "  ok   " : "  FAIL ") + what); if (!condition) failures.Add(what); }
            log.AppendLine("Rules");
            SightModel.Input Input(float distance, float angle, float light, bool crouched, float speed, bool running = false) =>
                new SightModel.Input { Distance = distance, Angle = angle, HalfAngle = 40f, Range = 9f, Light = light, Crouched = crouched, Speed = speed, Running = running };
            // Every reduction stacked (dark, crouched, still) never makes Kiều invisible when she is close in front of someone.
            float close = SightModel.Rate(Input(1.5f, 5f, 0f, true, 0f));
            Check(close >= SightModel.CloseFloor * 0.99f, "standing close in front is noticed within ~1.1 s even dark + crouched + still (rate " + close.ToString("F2") + "/s)");
            // ...and the stacked reductions are clamped, not multiplied down to nothing, a little further away.
            float stacked = SightModel.Rate(Input(3f, 5f, 0f, true, 0f));
            Check(stacked > 0.15f, "dark + crouched + still at 3 m still builds awareness (rate " + stacked.ToString("F2") + "/s)");
            // Darkness shortens how far a watcher can see; light extends it.
            Check(SightModel.Rate(Input(7f, 5f, 0.05f, false, 1f)) == 0f && SightModel.Rate(Input(7f, 5f, 0.8f, false, 1f)) > 0f, "at 7 m: unseen in the dark, seen in lamplight");
            // Behind the watcher nothing is seen; at the edge of his eye only close by.
            Check(SightModel.Rate(Input(2f, 150f, 1f, false, 2f)) == 0f, "nothing is seen behind him");
            Check(SightModel.Rate(Input(3f, 60f, 1f, false, 1f)) > 0f && SightModel.Rate(Input(6f, 60f, 1f, false, 1f)) == 0f, "the edge of his eye only reaches 4 m");
            // Running in light is the fastest way to be seen; never above the cap.
            float worst = SightModel.Rate(Input(1f, 0f, 1f, false, 4f, true));
            Check(worst <= SightModel.MaxCombined + 0.001f, "rate is capped (" + worst.ToString("F2") + ")");

            // J2 only after J1, a minute's rest, in sight of the door and never forced by a timer.
            Check(!ScareRules.SecondAllowed(false, 999f, false, false, true, 6f, false), "J2 never before J1 is over");
            Check(!ScareRules.SecondAllowed(true, 59f, false, false, true, 6f, false), "J2 waits a full minute after J1");
            Check(ScareRules.SecondAllowed(true, 61f, false, false, true, 6f, false), "J2 allowed after the rest, in the corridor");
            Check(!ScareRules.SecondAllowed(true, 300f, true, false, true, 6f, false), "J2 waits while she hides");
            Check(!ScareRules.SecondAllowed(true, 300f, false, true, true, 6f, false), "J2 waits while she is chased");
            Check(!ScareRules.SecondAllowed(true, 300f, false, false, true, 2f, false), "J2 never on top of her (closer than 3.5 m)");
            Check(!ScareRules.SecondAllowed(true, 300f, false, false, true, 6f, true), "J2 never while she is in Tú Bà's room");
            Check(!ScareRules.SecondAllowed(true, 300f, false, false, false, 6f, false), "J2 only where the door can be seen");

            // A hiding place is known only to someone who saw her go in.
            Check(!Watchman.WitnessesHiding(false, 0.9f), "an unseen hider stays hidden however suspicious he is");
            Check(!Watchman.WitnessesHiding(true, 0.1f), "a glimpse he has not registered does not give her away");
            Check(Watchman.WitnessesHiding(true, 0.5f), "seen slipping in while he watches: he knows the place");
        }

        // ------------------------------------------------------------------ map

        private static void Map(string scenePath, StringBuilder log, List<string> failures, bool night)
        {
            EditorSceneManager.OpenScene(scenePath);
            log.AppendLine(Path.GetFileNameWithoutExtension(scenePath));
            void Check(bool condition, string what) { log.AppendLine((condition ? "  ok   " : "  FAIL ") + what); if (!condition) failures.Add(Path.GetFileNameWithoutExtension(scenePath) + ": " + what); }
            if (night) foreach (var (ok, what) in LauTuBaGuide.Validate()) Check(ok, what);

            var nav = Object.FindAnyObjectByType<ThuyKieu.Environment.NarrowNavMesh>();
            // Doorways are tested as if every door stood open: a shut leaf carves its doorway closed, as it should.
            foreach (var obstacle in Object.FindObjectsByType<ThuyKieu.Gameplay.House.HouseDoor>().SelectMany(d => d.GetComponentsInChildren<NavMeshObstacle>()))
                obstacle.enabled = false;
            NavMesh.RemoveAllNavMeshData();
            nav.Build();
            var filter = new NavMeshQueryFilter { agentTypeID = nav.AgentTypeId, areaMask = NavMesh.AllAreas };
            float F = LauTuBaKit.Floor;
            bool Reach(Vector3 from, Vector3 to, out float length)
            {
                length = 0f;
                if (!NavMesh.SamplePosition(from, out var a, 1f, filter) || !NavMesh.SamplePosition(to, out var b, 1f, filter)) return false;
                var path = new NavMeshPath();
                if (!NavMesh.CalculatePath(a.position, b.position, filter, path) || path.status != NavMeshPathStatus.PathComplete) return false;
                for (int i = 1; i < path.corners.Length; i++) length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                return true;
            }
            void Route(string name, params Vector3[] stops)
            {
                for (int i = 1; i < stops.Length; i++)
                {
                    bool ok = Reach(stops[i - 1], stops[i], out float length);
                    Check(ok, name + " " + i + ": " + Fmt(stops[i - 1]) + " → " + Fmt(stops[i]) + (ok ? " (" + length.ToString("F1") + " m)" : " UNREACHABLE"));
                }
            }

            // The rooms the story uses are connected through their real doorways (door leaves are left out of the mesh).
            var kieu = new Vector3(-15.5f, F, 3.6f);
            var lam = new Vector3(-17f, F, -4f);
            var corridorWest = new Vector3(-10.5f, F, 9.5f);
            var store = new Vector3(-7f, F, 14f);
            var tuBa = new Vector3(2.5f, F, 14f);
            var office = new Vector3(11f, F, 13.5f);
            var archive = new Vector3(18f, F, 14f);
            var kitchen = new Vector3(-16f, 0.12f, 15f);
            var backDoor = new Vector3(-20.2f, 0.12f, 14.6f);
            var alley = new Vector3(-22.9f, 0f, 14.6f);
            var hall = new Vector3(-2.6f, F, -12.5f);
            var yard = new Vector3(0f, 0f, 0f);
            var hong = new Vector3(16f, F, 3f);
            Route("main way", kieu, corridorWest, store, tuBa, office, archive);
            Route("way back", archive, new Vector3(11f, F, 9.5f), corridorWest, kitchen, backDoor);
            Route("kitchen through the store", store, kitchen);
            Route("Lam and the yard", lam, yard, hall, hong);
            // Doorways the design needs, checked as short hops (a long detour would mean the doorway is shut).
            var hops = new[]
            {
                ("door 2 (Kiều)", new Vector3(-12.9f, F, 5.6f), new Vector3(-11.1f, F, 5.6f), 2.6f),
                ("door 3 (Lam)", new Vector3(-12.9f, F, -3.0f), new Vector3(-11.1f, F, -3.0f), 2.6f),
                ("door 6 (kitchen)", new Vector3(-14f, F, 10.2f), new Vector3(-14f, 0.12f, 11.8f), 2.6f),
                ("door 8 (kitchen–store)", new Vector3(-11.8f, 0.12f, 15.2f), new Vector3(-10.2f, F, 15.2f), 2.6f),
                ("door 9 (store)", new Vector3(-6f, F, 10.2f), new Vector3(-6f, F, 11.8f), 2.6f),
                ("door A (Tú Bà)", new Vector3(0f, F, 10.2f), new Vector3(0f, F, 11.8f), 2.6f),
                ("door B (Tú Bà–office)", new Vector3(7.2f, F, 15.6f), new Vector3(8.8f, F, 15.6f), 2.6f),
                ("door C (office)", new Vector3(11f, F, 10.2f), new Vector3(11f, F, 11.8f), 2.6f),
                ("back door (kitchen–alley)", new Vector3(-20.2f, 0.12f, 14.6f), new Vector3(-22.0f, 0f, 14.6f), 2.8f),
                ("archive doorway", new Vector3(14.4f, F, 15f), new Vector3(15.6f, F, 15f), 2.6f),
                ("west steps", new Vector3(-7.5f, 0f, 1f), new Vector3(-10f, F, 1f), 3.6f),
                ("east steps", new Vector3(7.5f, 0f, 1f), new Vector3(10f, F, 1f), 3.6f),
                ("south steps", new Vector3(0f, 0f, -5f), new Vector3(0f, F, -7f), 3f),
                ("north gap", new Vector3(0f, 0f, 7f), new Vector3(0f, F, 9f), 3f)
            };
            foreach (var (name, a, b, max) in hops)
            {
                bool ok = Reach(a, b, out float length);
                string blockers = "";
                if (!ok)
                {
                    // Name what stands in the doorway so a failure says where to look.
                    var middle = (a + b) * 0.5f + Vector3.up * 0.6f;
                    blockers = " · in the way: " + string.Join(", ", Physics.OverlapBox(middle, new Vector3(0.45f, 0.5f, 0.45f), Quaternion.identity, ~0, QueryTriggerInteraction.Collide)
                        .Select(c => c.transform.parent != null ? c.transform.parent.name + "/" + c.name + "[" + c.gameObject.layer + (c.isTrigger ? ",trigger" : "") + "]" : c.name));
                    blockers += NavMesh.SamplePosition(middle, out var hit, 0.6f, filter) ? " · mesh at " + hit.position.ToString("F2") : " · no mesh in the doorway";
                }
                Check(ok && length <= max, name + (ok ? " · " + length.ToString("F1") + " m" : " UNREACHABLE" + blockers));
            }
            // Walls are walls: a straight hop through the slatted store partition must be a detour, not a gap.
            bool through = Reach(new Vector3(-3.6f, F, 13f), new Vector3(-2.4f, F, 13f), out float detour);
            Check(!through || detour > 4f, "store slats are not a doorway (" + (through ? detour.ToString("F1") + " m around" : "no path") + ")");

            // How far apart the things to do are (the larger house must not just add empty walking). Times at Kiều's
            // walking pace (1.55 m/s) and crouched pace (0.95 m/s).
            if (night)
            {
                var sap = new Vector3(-1.9f, F, 13.9f);
                var cabinet = new Vector3(19.4f, F, 15.2f);
                foreach (var (leg, a, b) in new[] { ("room → Tú Bà's sập", kieu, sap), ("sập → cabinet (door B)", sap, cabinet), ("cabinet → chị Lam", cabinet, lam),
                             ("chị Lam → kitchen", lam, kitchen), ("kitchen → back door", kitchen, backDoor), ("back door → alley", backDoor, alley) })
                {
                    bool ok = Reach(a, b, out float length);
                    log.AppendLine($"  info {leg}: " + (ok ? $"{length:F1} m · walk {length / 1.55f:F0} s · crouch {length / 0.95f:F0} s" : "no path"));
                }
            }

            // Guard rounds and chị Lam's way out are walkable.
            foreach (var watch in Object.FindObjectsByType<Watchman>())
            {
                var waypoints = new SerializedObject(watch).FindProperty("_waypoints");
                var stops = new List<Vector3>();
                for (int i = 0; i < waypoints.arraySize; i++)
                    if (waypoints.GetArrayElementAtIndex(i).objectReferenceValue is Transform t) stops.Add(t.position);
                if (stops.Count > 1) Route("round of " + watch.name, stops.Concat(new[] { stops[0] }).ToArray());
            }
            foreach (var root in Object.FindObjectsByType<Transform>().Where(t => t.name.StartsWith("Night") && t.parent != null && t.parent.name == "GuardRoutes"))
                Route("route " + root.name, root.Cast<Transform>().Select(t => t.position).Concat(new[] { root.GetChild(0).position }).ToArray());
            var lamCompanion = Object.FindAnyObjectByType<LamCompanion>();
            if (lamCompanion != null)
            {
                var nodes = new SerializedObject(lamCompanion).FindProperty("_nodes");
                var stops = new List<Vector3>();
                for (int i = 0; i < nodes.arraySize; i++) stops.Add(((Transform)nodes.GetArrayElementAtIndex(i).objectReferenceValue).position);
                Route("chị Lam's way out", stops.ToArray());
            }

            // Every hiding place has room for Kiều's capsule (a crouched one for low cover) without touching a wall.
            foreach (var spot in Object.FindObjectsByType<HideSpot>())
            {
                var inside = new SerializedObject(spot).FindProperty("_inside").objectReferenceValue as Transform;
                bool crouch = new SerializedObject(spot).FindProperty("_crouch").boolValue;
                float height = crouch ? 1.08f : 1.65f, radius = 0.25f;
                Vector3 bottom = inside.position + Vector3.up * (radius + 0.05f), top = inside.position + Vector3.up * (height - radius);
                bool blocked = Physics.CheckCapsule(bottom, top, radius * 0.9f, 1 << StorySceneKit.EnvironmentLayer, QueryTriggerInteraction.Ignore);
                Check(!blocked, "hiding place '" + spot.name + "' has room" + (crouch ? " (crouched)" : ""));
            }
            if (night)
            {
                // The way out is the back door, never the gate: the gate leaves cannot be used by the player.
                foreach (var door in Object.FindObjectsByType<ThuyKieu.Gameplay.House.HouseDoor>().Where(d => d.name.StartsWith("Gate")))
                    Check(!new SerializedObject(door).FindProperty("_playerCanUse").boolValue, door.name + " cannot be opened by the player");
            }
            // Shut doors really shut: with every leaf closed (its obstacle carving the doorway), the hops through doors B
            // and C and the back door become long detours or impossible, never a gap a guard walks through.
            foreach (var obstacle in Object.FindObjectsByType<ThuyKieu.Gameplay.House.HouseDoor>().SelectMany(d => d.GetComponentsInChildren<NavMeshObstacle>()))
                obstacle.enabled = true;
            NavMesh.RemoveAllNavMeshData();
            nav.Build();
            foreach (var (name, a, b, _) in hops.Where(h => h.Item1.StartsWith("door B") || h.Item1.StartsWith("door C") || h.Item1.StartsWith("back door")))
            {
                bool ok = Reach(a, b, out float length);
                Check(!ok || length > 6f, name + " is shut when its leaf is closed (" + (ok ? length.ToString("F1") + " m around" : "no path") + ")");
            }
            NavMesh.RemoveAllNavMeshData();
        }

        private static string Fmt(Vector3 v) => "(" + v.x.ToString("F1") + ", " + v.z.ToString("F1") + ")";
    }
}
