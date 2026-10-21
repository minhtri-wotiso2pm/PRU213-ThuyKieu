using System;
using System.Collections;
using System.Collections.Generic;
using ThuyKieu.Core;
using ThuyKieu.Core.Story;
using ThuyKieu.Presentation;
using ThuyKieu.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThuyKieu.Gameplay.Sequences
{
    /// <summary>
    /// Free play on the hồ cầm: six keys pluck the ngũ cung strings (hò xự xang xê cống liu), Space sweeps them all,
    /// E puts the instrument down. Playing the opening of "Bạc mệnh" (xự xang xê xang xự) draws a word from Kiều Thật.
    /// The world animates the plucking hand through <see cref="Plucked"/>.
    /// </summary>
    public class LuteSequence : StorySequence
    {
        private static readonly float[] Pitches = { 261.63f, 293.66f, 349.23f, 392f, 440f, 523.25f };
        private static readonly string[] Names = { "Hò", "Xự", "Xang", "Xê", "Cống", "Liu" };
        private static readonly string[] Labels = { "A", "S", "D", "F", "G", "H" };
        private static readonly Key[] Keys = { Key.A, Key.S, Key.D, Key.F, Key.G, Key.H };
        private static readonly Key[] Digits = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6 };
        private static readonly int[] BacMenh = { 1, 2, 3, 2, 1 };
        private const float BaseNote = 261.63f;

        private readonly List<int> _played = new List<int>();
        private AudioSource[] _strings;

        /// <summary>A string was plucked (its index, low to high).</summary>
        public event Action<int> Plucked;

        /// <summary>The HUD controls line to put back when she lays the instrument down.</summary>
        public string ControlsAfter { get; set; }

        public static LuteSequence Create(GameObject host, string id)
        {
            var sequence = host.AddComponent<LuteSequence>();
            sequence.AssignId(id);
            return sequence;
        }

        public override IEnumerator Run(StoryDirector director)
        {
            var clip = AudioBank.Get(AudioBank.Sfx, "dan_note");
            _strings = new AudioSource[Pitches.Length];
            for (int i = 0; i < _strings.Length; i++)
            {
                _strings[i] = gameObject.AddComponent<AudioSource>();
                _strings[i].playOnAwake = false;
                _strings[i].spatialBlend = 0f;
                _strings[i].clip = clip;
                _strings[i].pitch = Pitches[i] / BaseNote;
            }
            _played.Clear();
            var view = LuteView.Create(Labels, Names, "Phím A–H hoặc 1–6: gảy từng dây · Space: vuốt dây · E: đặt đàn xuống\n" +
                "<color=#E8C27A>Thử mở đầu khúc Bạc mệnh: S · D · F · D · S</color>");
            director.Hud.SetControls("A–H / 1–6: gảy dây · Space: vuốt dây · E: đặt đàn xuống");
            bool heard = false;

            if (director.Autopilot)
            {
                foreach (int note in BacMenh) { Pluck(note, view); yield return new WaitForSecondsRealtime(0.05f); }
            }
            else
            {
                yield return null; // the key that closed the last line must not pluck or put the lute down
                for (float t = 0f; ; t += Time.deltaTime)
                {
                    if (!GamePause.IsPaused)
                    {
                        if (t > 0.5f && InputReader.InteractPressedThisFrame) break;
                        for (int i = 0; i < Keys.Length; i++)
                            if (InputReader.KeyPressedThisFrame(Keys[i]) || InputReader.KeyPressedThisFrame(Digits[i])) Pluck(i, view);
                        if (InputReader.KeyPressedThisFrame(Key.Space)) StartCoroutine(Sweep(view));
                        if (!heard && EndsWithBacMenh())
                        {
                            heard = true;
                            StartCoroutine(Remember(director, view));
                        }
                    }
                    yield return null;
                }
            }

            Destroy(view.gameObject);
            foreach (var source in _strings) Destroy(source, 3f);
            if (!string.IsNullOrEmpty(ControlsAfter)) director.Hud.SetControls(ControlsAfter);
        }

        private void Pluck(int index, LuteView view)
        {
            _strings[index].PlayOneShot(_strings[index].clip, 0.75f * GameSettings.Effects);
            view.Pluck(index);
            _played.Add(index);
            if (_played.Count > BacMenh.Length) _played.RemoveAt(0);
            Plucked?.Invoke(index);
        }

        /// <summary>Space: the fingers run across every string, low to high.</summary>
        private IEnumerator Sweep(LuteView view)
        {
            for (int i = 0; i < _strings.Length && view != null; i++)
            {
                Pluck(i, view);
                yield return new WaitForSeconds(0.06f);
            }
        }

        /// <summary>The motif is recognised: Kiều Thật speaks, every string lights and the whole "Bạc mệnh" phrase rings out.</summary>
        private IEnumerator Remember(StoryDirector director, LuteView view)
        {
            view.SetFeedback("<size=130%><color=#E8C27A>Khúc Bạc mệnh</color></size>");
            director.Hud.ShowBark("KieuThat", "Khúc ấy... ngón tay này vẫn còn nhớ.", 4.5f);
            yield return new WaitForSeconds(0.35f);
            if (director.Audio != null) director.Audio.PlaySfx("dan", 0.9f);
            for (float t = 0f; t < 3f && view != null; t += 0.12f)
            {
                view.Pluck(Mathf.FloorToInt(t / 0.12f) % Pitches.Length);
                yield return new WaitForSeconds(0.12f);
            }
        }

        private bool EndsWithBacMenh()
        {
            if (_played.Count < BacMenh.Length) return false;
            for (int i = 0; i < BacMenh.Length; i++) if (_played[i] != BacMenh[i]) return false;
            return true;
        }
    }
}
