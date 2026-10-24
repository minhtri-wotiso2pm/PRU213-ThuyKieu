using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace ThuyKieu.Environment
{
    /// <summary>
    /// Walkable area for a house of real one-metre doorways. The project's default agent (0.5 m radius) erodes such a
    /// doorway shut, so this builds a navmesh at runtime for a slimmer person (about 0.22 m) without touching the project's
    /// navigation settings, and moves every actor in the scene onto that agent type. Door leaves (marked with a
    /// <see cref="NavMeshModifier"/> that ignores them) are left out: they carve the mesh while shut instead.
    /// </summary>
    public class NarrowNavMesh : MonoBehaviour
    {
        [SerializeField] private Vector3 _centre = new Vector3(-1.5f, 1.5f, 0f);
        [SerializeField] private Vector3 _size = new Vector3(40f, 6f, 30f);
        [SerializeField] private LayerMask _geometry = 1 << 8;
        [SerializeField] private float _radius = 0.22f;
        [SerializeField] private float _height = 1.75f;
        [SerializeField] private float _climb = 0.3f;

        public int AgentTypeId { get; private set; } = -1;

        public void Build()
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var settings = NavMesh.CreateSettings();
            settings.agentRadius = _radius;
            settings.agentHeight = _height;
            settings.agentClimb = _climb;
            settings.agentSlope = 40f;
            settings.overrideVoxelSize = true;
            settings.voxelSize = _radius / 3f;
            AgentTypeId = settings.agentTypeID;

            var bounds = new Bounds(transform.TransformPoint(_centre), _size);
            var sources = new List<NavMeshBuildSource>();
            // Door leaves are skipped through markups (collider sources do not carry their component to filter on later).
            var markups = new List<NavMeshBuildMarkup>();
            foreach (var modifier in FindObjectsByType<NavMeshModifier>(FindObjectsInactive.Include))
                if (modifier.ignoreFromBuild) markups.Add(new NavMeshBuildMarkup { root = modifier.transform, overrideIgnore = true, ignoreFromBuild = true, applyToChildren = true });
            NavMeshBuilder.CollectSources(bounds, _geometry, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);
            var data = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);
            NavMesh.AddNavMeshData(data);
            ThuyKieu.Core.Story.StoryAutopilot.Note($"NAVMESH built in {timer.ElapsedMilliseconds} ms · {sources.Count} sources");

            foreach (var agent in FindObjectsByType<NavMeshAgent>(FindObjectsInactive.Include))
            {
                bool was = agent.enabled;
                agent.enabled = false;
                agent.agentTypeID = AgentTypeId;
                agent.radius = Mathf.Min(agent.radius, _radius);
                agent.enabled = was;
            }
        }
    }
}
