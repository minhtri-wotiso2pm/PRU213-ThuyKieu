using TMPro;
using UnityEngine;

namespace ThuyKieu.UI
{
    /// <summary>Assets the runtime-built game shell needs (lives in Resources so the shell can boot in any scene).</summary>
    [CreateAssetMenu(menuName = "Thúy Kiều/Game Shell Config")]
    public class GameShellConfig : ScriptableObject
    {
        public const string ResourcePath = "GameShellConfig";
        public TMP_FontAsset Font;
    }
}
