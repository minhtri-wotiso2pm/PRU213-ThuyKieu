using UnityEngine;

namespace ThuyKieu.Core
{
    /// <summary>
    /// Root of the game-wide managers (dialogue, quest, later audio/save...).
    /// Spawned automatically before the first scene loads, so any scene can be
    /// opened and played directly, and manager state survives scene changes.
    /// The prefab lives at Resources/Systems/PF_GameSystems.
    /// </summary>
    public class PersistentSystems : MonoBehaviour
    {
        private const string PrefabPath = "Systems/PF_GameSystems";

        public static PersistentSystems Instance { get; private set; }

        /// <summary>The project disables domain reload on Play, so statics must be reset by hand.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (Instance != null)
            {
                return;
            }

            GameObject prefab = Resources.Load<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("[PersistentSystems] Missing prefab at Resources/" + PrefabPath);
                return;
            }

            GameObject systems = Instantiate(prefab);
            systems.name = prefab.name;
        }

        private void Awake()
        {
            // A scene may still contain its own copy (old test scenes): the first one wins.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
