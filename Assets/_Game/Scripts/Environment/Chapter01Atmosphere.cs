using ThuyKieu.Core.Story;
using UnityEngine;

namespace ThuyKieu.Environment
{
    /// <summary>Blends the Chapter 1 hall, entrance and contract lamps to the moment of the night (Ink scene tags).</summary>
    public sealed class Chapter01Atmosphere : MonoBehaviour
    {
        [SerializeField] private StoryDirector _director;
        [SerializeField] private Light _hall;
        [SerializeField] private Light _entrance;
        [SerializeField] private Light _contract;
        private Vector3 _target = new Vector3(4.2f, 0.8f, 2.4f);
        public string Moment { get; private set; } = "Opening";

        private void OnEnable() { if (_director != null) _director.SceneChanged += OnScene; }
        private void OnDisable() { if (_director != null) _director.SceneChanged -= OnScene; }
        private void OnScene(string scene)
        {
            switch (scene)
            {
                case "VuongGia_DemMua": Moment = "Opening"; _target = new Vector3(4.2f, .8f, 2.4f); break;
                case "VuongGia_MaDen": Moment = "Entrance"; _target = new Vector3(4.8f, 2.4f, 2.8f); break;
                case "VuongGia_DamPhan": case "VuongGia_BanKyKhe": Moment = "Contract"; _target = new Vector3(4, 1.2f, 5.2f); break;
                case "VuongGia_RaDi": Moment = "Ending"; _target = new Vector3(3.8f, 1, 2.5f); break;
            }
        }
        private void Update()
        {
            float blend = 1 - Mathf.Exp(-2 * Time.deltaTime);
            if (_hall != null) _hall.intensity = Mathf.Lerp(_hall.intensity, _target.x, blend);
            if (_entrance != null) _entrance.intensity = Mathf.Lerp(_entrance.intensity, _target.y, blend);
            if (_contract != null) _contract.intensity = Mathf.Lerp(_contract.intensity, _target.z, blend);
        }
    }
}
