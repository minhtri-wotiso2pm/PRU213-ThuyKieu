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
                case "road": return Mix(id, 20f, Wind(20f, 0.5f, 31), Crickets(20f, 0.10f, 32));
                case "night": return Mix(id, 20f, Wind(20f, 0.35f, 33), Crickets(20f, 0.2f, 34));
                case "house": return Mix(id, 20f, Wind(20f, 0.12f, 35), Murmur(20f, 0.16f, 36));
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

        private static float[] Wind(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            float low = 0, slow = 0;
            for (int i = 0; i < length; i++)
            {
                low += 0.02f * ((float)(random.NextDouble() * 2 - 1) - low);
                float t = i / (float)Rate;
                slow = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * t / seconds * 3f) * Mathf.Sin(2 * Mathf.PI * t / seconds * 1f + 1f);
                data[i] = low * gain * 4f * (0.4f + 0.6f * slow);
            }
            return data;
        }

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

        private static float[] Murmur(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            float band = 0, prev = 0;
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)Rate;
                float noise = (float)(random.NextDouble() * 2 - 1);
                band += 0.08f * (noise - band);
                float voiced = band - prev;
                prev = band;
                float syllables = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 3.1f * t + Mathf.Sin(t * 0.7f) * 3f);
                data[i] = voiced * gain * 6f * syllables;
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
