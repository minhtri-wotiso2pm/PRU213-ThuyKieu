using System;
using UnityEngine;

namespace ThuyKieu.Presentation
{
    /// <summary>
    /// Synthesises placeholder music, ambience and effects so chapters have sound without audio files.
    /// Music uses Karplus–Strong plucked strings on pentatonic scales (a đàn tranh–like timbre).
    /// Every clip is deterministic; replace with recorded assets through StoryAudio's clip slots.
    /// </summary>
    public static class ProceduralAudio
    {
        public const int Rate = 22050;
        private static readonly float[] MinorPenta = { 0, 3, 5, 7, 10 };
        private static readonly float[] MajorPenta = { 0, 2, 4, 7, 9 };

        public static AudioClip Music(string id)
        {
            switch (id)
            {
                case "road_dusk": return Score(id, 32f, 146.83f, MinorPenta, 1.05f, 0.55f, 0.16f, 1, 11);
                case "road_night": return Score(id, 32f, 130.81f, MinorPenta, 1.6f, 0.45f, 0.12f, 2, 12);
                case "tension_low": return Tension(id, 24f, 73.42f, 13);
                case "stealth": return Tension(id, 20f, 82.41f, 14);
                case "tuba_warm": return Score(id, 30f, 196f, MajorPenta, 0.62f, 0.5f, 0.08f, 1, 15);
                case "resolve": return Score(id, 28f, 164.81f, MajorPenta, 0.8f, 0.6f, 0.18f, 3, 16);
                default: return null;
            }
        }

        public static AudioClip Ambience(string id)
        {
            switch (id)
            {
                // Countryside at dusk: a soft breeze, birds settling in the bamboo, the first crickets.
                case "road": return Mix(id, 40f, Breeze(40f, 0.22f, 31), Birds(40f, 0.11f, 4.5f, 37), Crickets(40f, 0.05f, 32));
                // Night fields: crickets, frogs in the paddies, an owl far off.
                case "night": return Mix(id, 40f, Breeze(40f, 0.14f, 33), Crickets(40f, 0.13f, 34), Frogs(40f, 0.16f, 38), Owl(40f, 0.06f, 39));
                // Courtyard house: almost silent room tone, wind chimes on the veranda, sparrows on the roof.
                case "house": return Mix(id, 40f, RoomTone(40f, 0.05f, 35), Chimes(40f, 0.09f, 36), Birds(40f, 0.05f, 9f, 40));
                default: return null;
            }
        }

        public static AudioClip Sfx(string id)
        {
            var random = new System.Random(id.GetHashCode());
            float[] data;
            switch (id)
            {
                case "clue": data = Notes(new[] { 587.33f, 880f }, 0.11f, 1.6f, 0.5f); break;
                case "suspicion_up": data = Bend(220f, 196f, 0.7f, 0.55f); break;
                case "fail": data = Add(Notes(new[] { 110f, 116.5f, 155.6f }, 0f, 2.2f, 0.45f), Noise(2.2f, 0.25f, 0.3f, random)); break;
                case "chapter_complete": data = Notes(new[] { 293.66f, 349.23f, 440f, 523.25f, 587.33f }, 0.13f, 2.6f, 0.4f); break;
                case "lock_rattle": data = Clicks(0.9f, 9, 0.6f, random); break;
                case "paper": data = Noise(0.6f, 0.35f, 0.75f, random); break;
                case "heartbeat": data = Heartbeat(1.1f); break;
                case "memory": data = Add(Swell(1.6f, random), Notes(new[] { 146.83f, 220f, 293.66f, 440f }, 0f, 3f, 0.35f, 1.6f)); break;
                case "detect": data = Notes(new[] { 1174.66f }, 0f, 0.35f, 0.4f); break;
                case "ui": data = Notes(new[] { 1318.5f }, 0f, 0.12f, 0.25f); break;
                case "step": data = Thud(0.18f, random); break;
                case "door": data = Add(Thud(0.4f, random), Clicks(0.4f, 3, 0.3f, random)); break;
                default: return null;
            }
            return Clip(id, data);
        }

        // ------------------------------------------------------------------ music

        private static AudioClip Score(string name, float seconds, float root, float[] scale, float interval, float gain, float drone, int octaves, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var buffer = new float[length + Rate * 4];
            float time = 0.2f;
            int phraseNote = random.Next(scale.Length);
            while (time < seconds)
            {
                // Phrases wander stepwise through the scale like a melody, with occasional leaps and rests.
                phraseNote += random.Next(-2, 3);
                if (phraseNote < 0) phraseNote += scale.Length;
                int octave = Math.Min(octaves, phraseNote / scale.Length);
                float semitone = scale[phraseNote % scale.Length] + 12 * octave;
                float frequency = root * 2f * Mathf.Pow(2f, semitone / 12f);
                float velocity = gain * (0.55f + (float)random.NextDouble() * 0.45f);
                Pluck(buffer, (int)(time * Rate), frequency, velocity, 2.8f, random);
                if (random.NextDouble() < 0.18) Pluck(buffer, (int)((time + 0.09f) * Rate), frequency * 1.5f, velocity * 0.5f, 2f, random);
                time += interval * (random.NextDouble() < 0.2 ? 2f : random.NextDouble() < 0.3 ? 0.5f : 1f);
                if (phraseNote > scale.Length * (octaves + 1)) phraseNote = scale.Length;
            }
            if (drone > 0f)
                for (int i = 0; i < buffer.Length; i++)
                {
                    float t = i / (float)Rate;
                    buffer[i] += drone * (Mathf.Sin(2 * Mathf.PI * root * t) * 0.6f + Mathf.Sin(2 * Mathf.PI * root * 1.5f * t) * 0.25f)
                                 * (0.75f + 0.25f * Mathf.Sin(2 * Mathf.PI * t / seconds * 2f));
                }
            Reverb(buffer, 0.35f);
            return Clip(name, Loop(buffer, length));
        }

        private static AudioClip Tension(string name, float seconds, float root, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var buffer = new float[length + Rate * 4];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)Rate;
                float swell = 0.6f + 0.4f * Mathf.Sin(2 * Mathf.PI * t / seconds * 3f);
                buffer[i] = 0.18f * swell * (Mathf.Sin(2 * Mathf.PI * root * t) + 0.5f * Mathf.Sin(2 * Mathf.PI * root * 1.06f * t));
                // Slow pulse like a held breath.
                float beat = Mathf.Repeat(t * 1.1f, 1f);
                buffer[i] += 0.22f * Mathf.Exp(-beat * 18f) * Mathf.Sin(2 * Mathf.PI * 52f * t);
            }
            for (float time = 1.5f; time < seconds; time += 2.4f + (float)random.NextDouble() * 2f)
                Pluck(buffer, (int)(time * Rate), root * 4f * Mathf.Pow(2f, (random.Next(3) == 0 ? 1 : 6) / 12f), 0.35f, 2.5f, random);
            Reverb(buffer, 0.4f);
            return Clip(name, Loop(buffer, length));
        }

        private static void Pluck(float[] output, int start, float frequency, float velocity, float seconds, System.Random random)
        {
            int period = Mathf.Max(2, (int)(Rate / frequency));
            var ring = new float[period];
            for (int i = 0; i < period; i++) ring[i] = (float)(random.NextDouble() * 2 - 1);
            int count = Math.Min((int)(seconds * Rate), output.Length - start);
            float decay = Mathf.Pow(0.001f, 1f / (seconds * frequency));
            for (int i = 0, index = 0; i < count; i++)
            {
                int next = (index + 1) % period;
                float sample = ring[index];
                ring[index] = 0.5f * (ring[index] + ring[next]) * decay;
                index = next;
                float attack = Mathf.Min(1f, i / 40f);
                output[start + i] += sample * velocity * attack;
            }
        }

        // ------------------------------------------------------------------ ambience

        private static float[] Crickets(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            for (float time = 0; time < seconds - 0.5f; time += 0.35f + (float)random.NextDouble() * 0.9f)
            {
                float frequency = 3900f + (float)random.NextDouble() * 900f;
                int chirps = 2 + random.Next(3);
                float g = gain * (0.4f + (float)random.NextDouble() * 0.6f);
                for (int c = 0; c < chirps; c++)
                {
                    int start = (int)((time + c * 0.055f) * Rate);
                    for (int i = 0; i < Rate * 0.035f && start + i < length; i++)
                        data[start + i] += g * Mathf.Sin(2 * Mathf.PI * frequency * i / Rate) * Mathf.Sin(Mathf.PI * i / (Rate * 0.035f));
                }
            }
            return data;
        }

        /// <summary>Wind as slow, deep swells (two-pole low-pass): no hiss band, so it never reads as sweeping.</summary>
        private static float[] Breeze(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            float a = 0, b = 0;
            float phase = (float)random.NextDouble() * 6f;
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)Rate;
                a += 0.012f * ((float)(random.NextDouble() * 2 - 1) - a);
                b += 0.012f * (a - b);
                // Gusts every ~10 s, locked to the loop length so the seam is invisible.
                float gust = 0.35f + 0.65f * Mathf.Pow(0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * t / seconds * 4f + phase), 2f);
                data[i] = b * gain * 14f * gust;
            }
            return data;
        }

        /// <summary>Very low room tone: presence without any audible texture.</summary>
        private static float[] RoomTone(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            float a = 0, b = 0;
            for (int i = 0; i < length; i++)
            {
                a += 0.006f * ((float)(random.NextDouble() * 2 - 1) - a);
                b += 0.006f * (a - b);
                data[i] = b * gain * 30f;
            }
            return data;
        }

        /// <summary>Short rising/falling whistles grouped into phrases, about <paramref name="spacing"/> seconds apart.</summary>
        private static float[] Birds(float seconds, float gain, float spacing, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            for (float time = 1f + (float)random.NextDouble() * 2f; time < seconds - 2f; time += spacing * (0.5f + (float)random.NextDouble()))
            {
                float near = 0.4f + (float)random.NextDouble() * 0.6f;
                float baseFrequency = 2600f + (float)random.NextDouble() * 1600f;
                int notes = 2 + random.Next(5);
                float start = time;
                for (int n = 0; n < notes; n++)
                {
                    float duration = 0.05f + (float)random.NextDouble() * 0.09f;
                    float from = baseFrequency * (0.85f + (float)random.NextDouble() * 0.3f);
                    float to = from * (random.Next(2) == 0 ? 1.35f : 0.75f);
                    int s0 = (int)(start * Rate), count = (int)(duration * Rate);
                    float phase = 0;
                    for (int i = 0; i < count && s0 + i < length; i++)
                    {
                        float k = i / (float)count;
                        phase += 2 * Mathf.PI * Mathf.Lerp(from, to, k) / Rate;
                        data[s0 + i] += gain * near * Mathf.Sin(phase + 0.6f * Mathf.Sin(phase * 0.5f)) * Mathf.Sin(Mathf.PI * k);
                    }
                    start += duration + 0.03f + (float)random.NextDouble() * 0.08f;
                }
            }
            return data;
        }

        /// <summary>Bronze wind chimes: inharmonic bell partials with long decays, a few strikes per gust.</summary>
        private static float[] Chimes(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length + Rate * 5];
            float[] notes = { 1046.5f, 1174.7f, 1396.9f, 1568f, 1760f };
            float[] partials = { 1f, 2.76f, 5.4f };
            float[] weights = { 1f, 0.45f, 0.2f };
            for (float time = 0.5f; time < seconds; time += 5f + (float)random.NextDouble() * 6f)
            {
                int strikes = 1 + random.Next(4);
                for (int s = 0; s < strikes; s++)
                {
                    float frequency = notes[random.Next(notes.Length)];
                    float velocity = gain * (0.5f + (float)random.NextDouble() * 0.5f);
                    int s0 = (int)((time + s * (0.15f + (float)random.NextDouble() * 0.35f)) * Rate);
                    for (int i = 0; i < Rate * 4.5f && s0 + i < data.Length; i++)
                    {
                        float t = i / (float)Rate;
                        float v = 0;
                        for (int p = 0; p < partials.Length; p++) v += weights[p] * Mathf.Sin(2 * Mathf.PI * frequency * partials[p] * t) * Mathf.Exp(-t * (0.9f + p * 1.6f));
                        data[s0 + i] += velocity * v * Mathf.Min(1f, i / 30f);
                    }
                }
            }
            return Loop(data, length);
        }

        /// <summary>Paddy frogs: low pulsed croaks in short runs, a few voices answering each other.</summary>
        private static float[] Frogs(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            for (int voice = 0; voice < 3; voice++)
            {
                float pitch = 280f + voice * 90f + (float)random.NextDouble() * 40f;
                float level = gain * (1f - voice * 0.25f);
                for (float time = (float)random.NextDouble() * 3f; time < seconds - 1.5f; time += 1.6f + (float)random.NextDouble() * 3.5f)
                {
                    int croaks = 2 + random.Next(4);
                    for (int c = 0; c < croaks; c++)
                    {
                        int s0 = (int)((time + c * 0.26f) * Rate), count = (int)(0.16f * Rate);
                        for (int i = 0; i < count && s0 + i < length; i++)
                        {
                            float t = i / (float)Rate, k = i / (float)count;
                            float pulse = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 38f * t);
                            data[s0 + i] += level * pulse * Mathf.Sin(2 * Mathf.PI * pitch * t) * Mathf.Sin(Mathf.PI * k);
                        }
                    }
                }
            }
            return data;
        }

        /// <summary>A distant owl: two soft hoots, rarely.</summary>
        private static float[] Owl(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            for (float time = 6f + (float)random.NextDouble() * 4f; time < seconds - 3f; time += 14f + (float)random.NextDouble() * 8f)
                foreach (float offset in new[] { 0f, 0.55f })
                {
                    int s0 = (int)((time + offset) * Rate), count = (int)(0.42f * Rate);
                    for (int i = 0; i < count && s0 + i < length; i++)
                    {
                        float t = i / (float)Rate, k = i / (float)count;
                        float frequency = 390f * (1f - 0.06f * k);
                        data[s0 + i] += gain * Mathf.Sin(2 * Mathf.PI * frequency * t) * Mathf.Pow(Mathf.Sin(Mathf.PI * k), 1.5f);
                    }
                }
            return data;
        }

        // ------------------------------------------------------------------ effects

        private static float[] Notes(float[] frequencies, float spacing, float seconds, float gain, float delay = 0f)
        {
            var random = new System.Random(7);
            var data = new float[(int)((seconds + delay + spacing * frequencies.Length) * Rate)];
            for (int n = 0; n < frequencies.Length; n++)
                Pluck(data, (int)((delay + n * spacing) * Rate), frequencies[n], gain, seconds, random);
            Reverb(data, 0.25f);
            return data;
        }

        private static float[] Bend(float from, float to, float seconds, float gain)
        {
            var data = new float[(int)(seconds * Rate)];
            float phase = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float k = i / (float)data.Length;
                phase += 2 * Mathf.PI * Mathf.Lerp(from, to, k) / Rate;
                data[i] = gain * Mathf.Sin(phase) * (1 - k) * Mathf.Min(1, i / 200f) * (0.8f + 0.2f * Mathf.Sin(phase * 2.01f));
            }
            return data;
        }

        private static float[] Noise(float seconds, float gain, float brightness, System.Random random)
        {
            var data = new float[(int)(seconds * Rate)];
            float filtered = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float k = i / (float)data.Length;
                filtered += brightness * ((float)(random.NextDouble() * 2 - 1) - filtered);
                float flutter = 0.5f + 0.5f * Mathf.Sin(k * 60f) * Mathf.Sin(k * 23f);
                data[i] = filtered * gain * (1 - k) * flutter;
            }
            return data;
        }

        private static float[] Clicks(float seconds, int count, float gain, System.Random random)
        {
            var data = new float[(int)(seconds * Rate)];
            for (int c = 0; c < count; c++)
            {
                int start = (int)((c / (float)count * 0.8f + (float)random.NextDouble() * 0.05f) * seconds * Rate);
                float frequency = 1800f + (float)random.NextDouble() * 1400f;
                for (int i = 0; i < Rate * 0.06f && start + i < data.Length; i++)
                    data[start + i] += gain * Mathf.Sin(2 * Mathf.PI * frequency * i / Rate) * Mathf.Exp(-i / (Rate * 0.008f));
            }
            return data;
        }

        private static float[] Heartbeat(float seconds)
        {
            var data = new float[(int)(seconds * Rate)];
            foreach (float beat in new[] { 0f, 0.26f })
            {
                int start = (int)(beat * Rate);
                for (int i = 0; i < Rate * 0.25f && start + i < data.Length; i++)
                {
                    float t = i / (float)Rate;
                    data[start + i] += 0.9f * Mathf.Sin(2 * Mathf.PI * (55f - t * 60f) * t) * Mathf.Exp(-t * 18f) * (beat == 0 ? 1f : 0.7f);
                }
            }
            return data;
        }

        private static float[] Thud(float seconds, System.Random random)
        {
            var data = new float[(int)(seconds * Rate)];
            float filtered = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate;
                filtered += 0.12f * ((float)(random.NextDouble() * 2 - 1) - filtered);
                data[i] = (filtered * 1.5f + 0.4f * Mathf.Sin(2 * Mathf.PI * 90f * t)) * Mathf.Exp(-t * 30f);
            }
            return data;
        }

        private static float[] Swell(float seconds, System.Random random)
        {
            var data = new float[(int)(seconds * Rate)];
            float filtered = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float k = i / (float)data.Length;
                filtered += (0.05f + 0.6f * k) * ((float)(random.NextDouble() * 2 - 1) - filtered);
                data[i] = filtered * 0.5f * k * k;
            }
            return data;
        }

        // ------------------------------------------------------------------ helpers

        private static float[] Add(float[] a, float[] b)
        {
            var data = new float[Math.Max(a.Length, b.Length)];
            for (int i = 0; i < data.Length; i++) data[i] = (i < a.Length ? a[i] : 0) + (i < b.Length ? b[i] : 0);
            return data;
        }

        private static AudioClip Mix(string name, float seconds, params float[][] layers)
        {
            int length = (int)(seconds * Rate);
            var data = new float[length];
            foreach (var layer in layers) for (int i = 0; i < length && i < layer.Length; i++) data[i] += layer[i];
            // Crossfade the ends so the ambience loops without a click.
            int fade = Rate / 2;
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                data[i] = data[i] * k + data[length - fade + i] * (1 - k);
            }
            return Clip(name, data, length - fade);
        }

        private static void Reverb(float[] data, float amount)
        {
            int[] delays = { (int)(0.113f * Rate), (int)(0.171f * Rate), (int)(0.241f * Rate) };
            foreach (int delay in delays)
                for (int i = delay; i < data.Length; i++) data[i] += data[i - delay] * amount * 0.45f;
        }

        // Folds the tail past the loop point back to the start so notes ring across the seam.
        private static float[] Loop(float[] buffer, int length)
        {
            var data = new float[length];
            for (int i = 0; i < buffer.Length; i++) data[i % length] += buffer[i];
            return data;
        }

        private static AudioClip Clip(string name, float[] data, int length = -1)
        {
            if (length < 0) length = data.Length;
            float peak = 0.0001f;
            for (int i = 0; i < length; i++) peak = Mathf.Max(peak, Mathf.Abs(data[i]));
            float scale = peak > 0.95f ? 0.95f / peak : 1f;
            var samples = new float[length];
            for (int i = 0; i < length; i++) samples[i] = data[i] * scale;
            var clip = AudioClip.Create("Proc_" + name, length, 1, Rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
