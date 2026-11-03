using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThuyKieu.Core.Editor
{
    public static class ChapterDiagnostics
    {
        [InitializeOnLoadMethod]
        private static void RunDiagnostics()
        {
            EditorApplication.delayCall += Execute;
        }

        private static void Execute()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== CHAPTER 00 & FULL PROJECT DIAGNOSTICS ===");
            sb.AppendLine($"isPlaying={EditorApplication.isPlaying}, activeScene={SceneManager.GetActiveScene().path}");

            // Check critical FBX models and Avatars
            string[] fbxPaths =
            {
                "Assets/_Game/Art/Characters/ThuyKieu/Models/ThuyKieu_Walk.fbx",
                "Assets/_Game/Art/Characters/ThuyKieu/Models/ThuyKieu_Rigged.fbx",
                "Assets/_Game/Animations/ThuyKieu/TK-Idle.fbx",
                "Assets/_Game/Animations/ThuyKieu/TK-Walk.fbx",
                "Assets/_Game/Animations/ThuyKieu/TK-Sitting Idle.fbx",
                "Assets/_Game/Animations/ThuyKieu/TK-KneelDown.fbx",
                "Assets/_Game/Animations/ThuyKieu/TK-LookAtHands.fbx",
                "Assets/_Game/Animations/animationnuchinh/idle.fbx",
                "Assets/_Game/Animations/animationnuchinh/seat.fbx",
                "Assets/_Game/Animations/animationnuchinh/walk.fbx",
                "Assets/_Game/Art/Characters/ThuyVan/Models/ThuyVan_Rigged.fbx",
                "Assets/_Game/Art/Characters/BaHangNuoc/Models/BaHangNuoc_Rigged.fbx",
                "Assets/_Game/Art/Characters/MeThuyKieu/Models/MeThuyKieu_Rigged.fbx"
            };

            foreach (string path in fbxPaths)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var anim = go != null ? go.GetComponentInChildren<Animator>() : null;
                var smr = go != null ? go.GetComponentInChildren<SkinnedMeshRenderer>() : null;
                sb.AppendLine($"FBX [{path}]: go={(go != null)}, anim={(anim != null)}, avatar={(anim != null && anim.avatar != null ? anim.avatar.name + "(valid=" + anim.avatar.isValid + ",human=" + anim.avatar.isHuman + ")" : "NULL")}, mesh={(smr != null && smr.sharedMesh != null ? smr.sharedMesh.vertexCount.ToString() : "NULL")}");
            }

            // Inspect active scene objects if Chapter00 is open
            InspectActiveScene(sb);

            File.WriteAllText("Logs/chapter_audit.txt", sb.ToString());
            Debug.Log("[ChapterDiagnostics] Wrote Logs/chapter_audit.txt");
        }

        private static void InspectActiveScene(StringBuilder sb)
        {
            sb.AppendLine("--- Active Scene Inspection ---");
            var player = GameObject.Find("ThuyKieu");
            if (player != null)
            {
                var cc = player.GetComponent<CharacterController>();
                sb.AppendLine($"Player 'ThuyKieu': pos={player.transform.position:F3}, rot={player.transform.eulerAngles:F1}, scale={player.transform.localScale:F3}");
                if (cc != null)
                    sb.AppendLine($"  CharacterController: enabled={cc.enabled}, height={cc.height:F3}, center={cc.center:F3}, radius={cc.radius:F3}");

                foreach (Transform child in player.transform)
                {
                    sb.AppendLine($"  Child '{child.name}' (active={child.gameObject.activeSelf}): localPos={child.localPosition:F4}, worldPos={child.position:F4}, localScale={child.localScale:F4}");
                    var anim = child.GetComponentInChildren<Animator>(true);
                    if (anim != null)
                    {
                        var hips = anim.isHuman ? anim.GetBoneTransform(HumanBodyBones.Hips) : null;
                        var lFoot = anim.isHuman ? anim.GetBoneTransform(HumanBodyBones.LeftFoot) : null;
                        var rFoot = anim.isHuman ? anim.GetBoneTransform(HumanBodyBones.RightFoot) : null;
                        sb.AppendLine($"    Animator '{anim.name}': controller={(anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "NULL")}, avatar={(anim.avatar != null ? anim.avatar.name + "(human=" + anim.isHuman + ")" : "NULL")}, localPos={anim.transform.localPosition:F4}, worldPos={anim.transform.position:F4}");
                        if (hips != null) sb.AppendLine($"    Bones: Hips.y={hips.position.y:F4}, LFoot.y={(lFoot != null ? lFoot.position.y : -999f):F4}, RFoot.y={(rFoot != null ? rFoot.position.y : -999f):F4}");
                    }
                    foreach (var r in child.GetComponentsInChildren<Renderer>(true))
                    {
                        sb.AppendLine($"    Renderer '{r.name}': bounds.min={r.bounds.min:F3}, bounds.max={r.bounds.max:F3}, size={r.bounds.size:F3}");
                    }
                }
            }

            string[] actors = { "Ngan", "ThayGiao", "VuongOng", "MeKieu", "VuongQuan", "QuanSai", "QuanSai2" };
            foreach (string a in actors)
            {
                var go = GameObject.Find(a);
                if (go == null) continue;
                var anim = go.GetComponentInChildren<Animator>(true);
                var r = go.GetComponentInChildren<Renderer>(true);
                sb.AppendLine($"Actor '{a}': pos={go.transform.position:F3}, animLocalPos={(anim != null ? anim.transform.localPosition.ToString("F3") : "none")}, boundsMinY={(r != null ? r.bounds.min.y.ToString("F3") : "none")}, boundsMaxY={(r != null ? r.bounds.max.y.ToString("F3") : "none")}");
            }

            string[] floors = { "ClassFloor", "Ground", "RoomFloor", "Veranda" };
            foreach (string f in floors)
            {
                var go = GameObject.Find(f);
                if (go == null) continue;
                var col = go.GetComponent<Collider>();
                sb.AppendLine($"Floor '{f}': pos={go.transform.position:F3}, scale={go.transform.localScale:F3}, col={(col != null ? col.GetType().Name + "(enabled=" + col.enabled + ",boundsMaxY=" + col.bounds.max.y.ToString("F3") + ")" : "NONE")}");
            }
        }
    }
}
