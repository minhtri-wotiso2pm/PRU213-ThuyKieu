using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThuyKieu.Core.Editor
{
    /// <summary>
    /// Puts the generated story props (see <see cref="GeneratedPropSetup.SetupStoryProps"/>) into the open scene in place of
    /// the procedural stand-ins the scene kits built: the open Truyện Kiều textbook on the student's desk, the painted
    /// hanging scroll in Kiều's room and the carved câu đối boards either side of the Vương gate and altar alcove.
    /// The stand-ins are only deactivated, and re-running replaces earlier placements.
    ///
    ///   Menu: ThuyKieu/Tools/Place story props in open scene
    /// </summary>
    public static class StoryPropPlacement
    {
        private const string PrefabFolder = "Assets/_Game/Prefabs/Props";

        [MenuItem("ThuyKieu/Tools/Place story props in open scene")]
        public static void PlaceInOpenScene()
        {
            int placed = PlaceBook() + PlaceClassmateBooks() + PlaceRoomScroll() + PlaceCouplets();
            if (placed == 0) { Debug.Log("StoryPropPlacement: no stand-ins found in the open scene."); return; }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("StoryPropPlacement: placed " + placed + " prop(s).");
        }

        /// <summary>The textbook goes on the desk where the stand-in cover and pages lay.</summary>
        private static int PlaceBook()
        {
            var anchor = Find("Book");
            if (anchor == null) return 0;
            var book = Replace(anchor.parent, "SachTruyenKieu", "SachTruyenKieu", anchor.position, anchor.rotation);
            // The interaction marker sits above the desk; a collider round the book would block the line of sight to it.
            Object.DestroyImmediate(book.GetComponent<BoxCollider>());
            foreach (Transform sibling in anchor.parent)
                if (sibling.name == "BookCover" || sibling.name == "BookPage" || sibling.name == "BookVerses") sibling.gameObject.SetActive(false);
            return 1;
        }

        /// <summary>
        /// Every seated classmate, her deskmate Ngân included, has the same textbook open on the desk in front of them,
        /// placed as the student's own copy is relative to her seat. Desk clutter the book would land on is put away, and
        /// the student's pencil case is turned to lie in the gap between her book and Ngân's.
        /// </summary>
        private static int PlaceClassmateBooks()
        {
            var own = Find("Book");
            if (own == null) return 0;
            var seats = new List<Transform>();
            foreach (Transform child in own.parent)
                if (child.name.StartsWith("Seat_")) seats.Add(child);
            int placed = 0;
            foreach (var actor in Object.FindObjectsByType<Story.StoryActor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (actor.SpeakerId != "BanHoc" && actor.SpeakerId != "Ngan") continue;
                Transform seat = null;
                foreach (var candidate in seats)
                    if (seat == null || Distance2D(candidate.position, actor.transform.position) < Distance2D(seat.position, actor.transform.position))
                        seat = candidate;
                if (seat == null) continue;
                // Her copy lies 0.35 m ahead of her seat mark and 0.08 m in from it towards the middle of the desk.
                float inward = seat.name.EndsWith("L") ? 0.08f : -0.08f;
                var position = new Vector3(seat.position.x + inward, own.position.y, seat.position.z + 0.35f);
                var book = Replace(own.parent, "SachTruyenKieu", "SachTruyenKieu@" + seat.name, position, own.rotation);
                Object.DestroyImmediate(book.GetComponent<BoxCollider>());
                var footprint = book.GetComponentInChildren<Renderer>().bounds;
                footprint.Expand(new Vector3(0.04f, 0.2f, 0.04f));
                foreach (Transform child in own.parent)
                    if ((child.name == "Notebook" || child.name == "Bottle" || child.name == "BottleCap")
                        && child.TryGetComponent(out Renderer clutter) && clutter.bounds.Intersects(footprint))
                        child.gameObject.SetActive(false);
                if (actor.SpeakerId == "Ngan") MovePencilCase(own, book.transform);
                placed++;
            }
            return placed;
        }

        private static void MovePencilCase(Transform ownBook, Transform nganBook)
        {
            var pencilCase = ownBook.parent.Find("PencilCase");
            if (pencilCase == null) return;
            var position = pencilCase.position;
            position.x = (ownBook.position.x + nganBook.position.x) * 0.5f;
            position.z = ownBook.position.z;
            pencilCase.SetPositionAndRotation(position, Quaternion.Euler(0f, 90f, 0f));
        }

        private static float Distance2D(Vector3 a, Vector3 b) => new Vector2(a.x - b.x, a.z - b.z).magnitude;

        /// <summary>The painted scroll hangs where the stand-in scroll hung, centred on the same point of the wall.</summary>
        private static int PlaceRoomScroll()
        {
            var anchor = Find("HangingScroll");
            if (anchor == null) return 0;
            var scroll = Replace(anchor.parent, "tranhphongkieu", "tranhphongkieu", anchor.position, anchor.rotation);
            var bounds = scroll.GetComponentInChildren<Renderer>().bounds;
            float halfDepth = Mathf.Abs(anchor.forward.x) * bounds.extents.x + Mathf.Abs(anchor.forward.z) * bounds.extents.z;
            // The prefab stands on its lower edge; lift it so its middle meets the old centre, and keep its back off the wall.
            scroll.transform.position += Vector3.up * (anchor.position.y - bounds.center.y) + anchor.forward * (halfDepth + 0.01f);
            anchor.gameObject.SetActive(false);
            return 1;
        }

        /// <summary>
        /// Each stand-in couplet board is swapped for the carved board on its side of the doorway, as seen by someone
        /// standing in front of it (the stand-ins face their local -Z, like the carved prefabs).
        /// </summary>
        private static int PlaceCouplets()
        {
            var boards = new List<Transform>();
            foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (transform.name == "Couplet" && transform.parent != null) boards.Add(transform);
            int placed = 0;
            foreach (var board in boards)
            {
                Transform partner = null;
                foreach (var other in boards)
                    if (other != board && Mathf.Abs(Vector3.Dot(other.position - board.position, board.forward)) < 0.5f
                        && (partner == null || Vector3.Distance(other.position, board.position) < Vector3.Distance(partner.position, board.position)))
                        partner = other;
                if (partner == null) continue;
                // Someone reading the board looks along its +Z, so their right hand is the board's +X.
                bool onLeft = Vector3.Dot(board.position - partner.position, board.right) < 0f;
                string prefab = onLeft ? "CauDoi_Trai" : "CauDoi_Phai";
                var carved = Replace(board.parent, prefab, prefab + "@" + board.GetSiblingIndex(), board.position, board.rotation);
                // The carved board is curved and deeper than the flat stand-in; set its back where the stand-in's back
                // met the wall, or its face sinks into the wall behind it.
                carved.transform.position -= board.forward * (Back(carved.GetComponent<Renderer>().bounds, board.forward)
                    - Back(board.GetComponentInChildren<Renderer>().bounds, board.forward));
                board.gameObject.SetActive(false);
                placed++;
            }
            return placed;
        }

        /// <summary>How far along <paramref name="back"/> the box reaches.</summary>
        private static float Back(Bounds bounds, Vector3 back) =>
            Vector3.Dot(bounds.center, back) + Mathf.Abs(back.x) * bounds.extents.x + Mathf.Abs(back.y) * bounds.extents.y
            + Mathf.Abs(back.z) * bounds.extents.z;

        /// <summary>Instantiates the prefab as <paramref name="name"/> under the parent, removing an earlier placement of that name.</summary>
        private static GameObject Replace(Transform parent, string prefab, string name, Vector3 position, Quaternion rotation)
        {
            var previous = parent.Find(name);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/" + prefab + ".prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            instance.name = name;
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.isStatic = true;
            return instance;
        }

        private static Transform Find(string name)
        {
            foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (transform.name == name && transform.parent != null) return transform;
            return null;
        }
    }
}
