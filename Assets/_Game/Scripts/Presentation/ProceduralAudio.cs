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
                // Chapter 1 farewell: slow, low minor phrases over a soft drone.
                case "sad_strings": return Score(id, 36f, 110f, MinorPenta, 1.45f, 0.5f, 0.2f, 1, 17);
                // Chapters 4–6.
                case "chase": return Chase(id, 24f, 110f, 20);
                case "temple": return Score(id, 34f, 220f, MajorPenta, 1.3f, 0.42f, 0.14f, 1, 18);
                case "study": return Score(id, 30f, 174.61f, MajorPenta, 0.95f, 0.45f, 0.1f, 2, 19);
                case "street": return Score(id, 26f, 196f, MajorPenta, 0.42f, 0.5f, 0.06f, 2, 21);
                // Prologue, Chapter 7 and the epilogue.
                case "classroom": return Score(id, 30f, 261.63f, MajorPenta, 0.7f, 0.38f, 0.05f, 1, 22);
                case "morning": return Score(id, 30f, 233.08f, MajorPenta, 0.9f, 0.42f, 0.08f, 1, 23);
                case "trial": return Tension(id, 24f, 98f, 24);
                case "family": return Score(id, 32f, 196f, MajorPenta, 1.1f, 0.55f, 0.16f, 2, 25);
                case "spirit": return Score(id, 36f, 293.66f, MinorPenta, 1.7f, 0.4f, 0.22f, 2, 26);
                case "epilogue": return Score(id, 34f, 220f, MajorPenta, 1.25f, 0.48f, 0.14f, 2, 27);
                // Trang Sách branches: Sở Khanh's sweet-talk, the lost trial (bạc mệnh) and the Nghịch Mệnh farewell.
                case "so_khanh": return Score(id, 26f, 155.56f, MinorPenta, 0.5f, 0.4f, 0.1f, 2, 28);
                case "bac_menh": return Score(id, 40f, 98f, MinorPenta, 2f, 0.42f, 0.26f, 1, 29);
                case "nghich_menh": return Score(id, 36f, 246.94f, MajorPenta, 1.15f, 0.5f, 0.2f, 2, 30);
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
                // Night rain on tiles and the yard, with far thunder now and then.
                case "rain": return Mix(id, 40f, Rain(40f, 0.3f, 41), Thunder(40f, 0.35f, 42));
                case "wind": return Mix(id, 40f, Breeze(40f, 0.3f, 43));
                // The house at night: crickets, the guests' music far off in the hall.
                case "night_house": return Mix(id, 40f, RoomTone(40f, 0.05f, 44), Crickets(40f, 0.1f, 45), Distant(40f, 0.06f, 46), Murmur(40f, 0.035f, 4, 47));
                case "forest_rain": return Mix(id, 40f, Rain(40f, 0.34f, 48), Breeze(40f, 0.22f, 49), Thunder(40f, 0.3f, 50));
                case "market_night": return Mix(id, 40f, Murmur(40f, 0.07f, 10, 51), Crickets(40f, 0.05f, 52), Distant(40f, 0.035f, 53));
                case "temple": return Mix(id, 40f, Birds(40f, 0.09f, 3.5f, 54), Breeze(40f, 0.08f, 55), Bell(40f, 0.12f, 56));
                case "street": return Mix(id, 40f, Murmur(40f, 0.09f, 14, 57), Birds(40f, 0.05f, 7f, 58), Breeze(40f, 0.06f, 59));
                // A school after class: the room's hum, voices far down the corridor, sparrows outside.
                case "classroom": return Mix(id, 40f, RoomTone(40f, 0.06f, 60), Distant(40f, 0.03f, 61), Murmur(40f, 0.025f, 5, 62), Birds(40f, 0.05f, 6f, 63));
                // The same room at sunset: cicadas instead of voices.
                case "classroom_evening": return Mix(id, 40f, RoomTone(40f, 0.05f, 64), Crickets(40f, 0.08f, 65), Birds(40f, 0.03f, 11f, 66));
                // Vương family courtyard in the morning.
                case "courtyard": return Mix(id, 40f, Birds(40f, 0.12f, 3f, 67), Breeze(40f, 0.08f, 68), Chimes(40f, 0.06f, 69));
                // The magistrate's yard: a crowd murmuring behind the gate.
                case "yamen": return Mix(id, 40f, Murmur(40f, 0.1f, 18, 70), Breeze(40f, 0.06f, 71), Birds(40f, 0.03f, 8f, 72));
                case "yamen_hall": return Mix(id, 40f, RoomTone(40f, 0.06f, 73), Murmur(40f, 0.04f, 10, 74));
                // Tú Bà's house, first person. After the rain (Ch3): eaves dripping, sparrows, a wind chime.
                case "after_rain": return Mix(id, 40f, RoomTone(40f, 0.04f, 75), Drips(40f, 0.11f, 76), Birds(40f, 0.045f, 7f, 77), Chimes(40f, 0.04f, 78), Breeze(40f, 0.06f, 79));
                // Night (Ch4): heavy rain on the tiles, gutters, far thunder, the guests' music and laughter through walls.
                case "rain_house": return Mix(id, 40f, Rain(40f, 0.42f, 80), Drips(40f, 0.13f, 81), Thunder(40f, 0.28f, 82), Distant(40f, 0.03f, 83), Murmur(40f, 0.018f, 3, 84));
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
                case "step": case "step_wood": case "step_stone": data = Thud(0.18f, random); break;
                case "door": case "door_open": case "door_close": data = Add(Thud(0.4f, random), Clicks(0.4f, 3, 0.3f, random)); break;
                case "slap": data = Noise(0.16f, 0.9f, 0.95f, random); break;
                case "lock_open": data = Add(Clicks(0.5f, 3, 0.55f, random), Thud(0.3f, random)); break;
                case "fire": data = Add(Swell(1.2f, random), Clicks(1.6f, 14, 0.25f, random)); break;
                case "dog": data = Bark(random); break;
                case "gong": data = Gong(3.5f); break;
                case "knock": data = Add(Add(Thud(0.3f, random), Delay(Thud(0.3f, random), 0.22f)), Delay(Thud(0.3f, random), 0.44f)); break;
                case "clatter": data = Add(Clicks(1.1f, 16, 0.5f, random), Thud(0.5f, random)); break;
                case "creak": data = Creak(1.2f); break;
                case "snore": data = Swell(1.4f, random); break;
                case "stumble": data = Add(Thud(0.45f, random), Noise(0.35f, 0.4f, 0.5f, random)); break;
                case "whoosh": data = Swell(0.45f, random); break;
                case "qte_ok": data = Notes(new[] { 880f, 1318.5f }, 0.05f, 0.5f, 0.35f); break;
                case "qte_fail": data = Bend(180f, 120f, 0.5f, 0.6f); break;
                case "school_bell": data = SchoolBell(2.4f); break;
                case "drum": data = Add(Add(Drum(0.9f), Delay(Drum(0.9f), 0.55f)), Delay(Drum(1.4f), 1.1f)); break;
                case "gavel": data = Add(Thud(0.35f, random), Delay(Clicks(0.25f, 2, 0.6f, random), 0.01f)); break;
                case "objection": data = Add(Drum(0.7f), Notes(new[] { 392f, 587.33f }, 0.06f, 0.8f, 0.45f)); break;
                case "wrong": data = Bend(196f, 146.83f, 0.6f, 0.55f); break;
                // A few plucked notes of the hồ cầm, the opening of a sad "Bạc mệnh" phrase in a minor pentatonic.
                case "dan": data = Notes(new[] { 293.66f, 349.23f, 392f, 349.23f, 293.66f, 261.63f, 220f }, 0.42f, 3.4f, 0.42f); break;
                // Tú Bà's house in first person: floors, doors, locks and the sounds that mean danger.
                case "step_wet": data = Add(Scale(Thud(0.2f, random), 0.8f), Noise(0.16f, 0.45f, 0.85f, random)); break;
                case "step_earth": data = Scale(Thud(0.16f, random), 0.7f); break;
                case "step_mat": data = Scale(Thud(0.12f, random), 0.45f); break;
                // Second takes (a different seed, a little shorter or longer) so steps on one floor do not repeat exactly.
                case "step_wet_2": data = Add(Scale(Thud(0.18f, random), 0.75f), Noise(0.2f, 0.5f, 0.9f, random)); break;
                case "step_earth_2": data = Add(Scale(Thud(0.17f, random), 0.65f), Noise(0.08f, 0.08f, 0.55f, random)); break;
                case "step_mat_2": data = Add(Scale(Thud(0.11f, random), 0.4f), Noise(0.12f, 0.05f, 0.6f, random)); break;
                // Mud in the alley: a soft heel, then the dull suck of the foot coming out.
                case "step_mud": data = Add(Scale(Thud(0.16f, random), 0.6f), Delay(Noise(0.22f, 0.32f, 0.16f, random), 0.06f)); break;
                case "step_mud_2": data = Add(Scale(Thud(0.15f, random), 0.55f), Delay(Noise(0.26f, 0.3f, 0.13f, random), 0.08f)); break;
                case "coins": data = Jingle(0.5f, 5, 0.35f, random); break;
                case "keys_soft": data = Jingle(0.35f, 3, 0.22f, random); break;
                case "bracelet": data = Add(Jingle(0.4f, 2, 0.4f, random), Delay(Jingle(0.3f, 1, 0.25f, random), 0.35f)); break;
                case "door_creak_soft": data = Scale(Creak(0.7f), 0.55f); break;
                case "door_fast": data = Add(Creak(0.35f), Delay(Thud(0.3f, random), 0.25f)); break;
                case "door_slam": data = Add(Scale(Thud(0.55f, random), 1.7f), Clicks(0.45f, 4, 0.45f, random)); break;
                case "slide_soft": data = Scale(Noise(0.7f, 0.3f, 0.12f, random), 0.8f); break;
                case "slide_fast": data = Add(Noise(0.35f, 0.55f, 0.25f, random), Delay(Thud(0.3f, random), 0.3f)); break;
                case "slide_slam": data = Add(Add(Noise(0.22f, 0.8f, 0.3f, random), Delay(Scale(Thud(0.6f, random), 1.9f), 0.18f)), Delay(Clicks(0.4f, 5, 0.5f, random), 0.2f)); break;
                case "key_turn": data = Add(Clicks(0.5f, 3, 0.45f, random), Scale(Noise(0.5f, 0.15f, 0.5f, random), 0.6f)); break;
                case "bolt_slow": data = Add(Scale(Noise(0.9f, 0.32f, 0.1f, random), 0.9f), Clicks(0.9f, 2, 0.15f, random)); break;
                case "bolt_fast": data = Add(Noise(0.25f, 0.75f, 0.2f, random), Delay(Scale(Thud(0.35f, random), 1.4f), 0.2f)); break;
                case "latch_lift": data = Add(Clicks(0.4f, 2, 0.3f, random), Scale(Noise(0.4f, 0.12f, 0.4f, random), 0.6f)); break;
                case "latch_drop": data = Add(Scale(Thud(0.3f, random), 1.3f), Clicks(0.3f, 2, 0.8f, random)); break;
                case "gate_bar": data = Add(Noise(0.9f, 0.5f, 0.07f, random), Delay(Scale(Thud(0.7f, random), 1.8f), 0.8f)); break;
                case "blow": data = Breath(0.5f, 0.35f, 0.55f, random); break;
                case "cloth": data = Noise(0.5f, 0.22f, 0.35f, random); break;
                case "chair_creak": data = Scale(Creak(0.9f), 0.7f); break;
                case "mumble": data = Mumble(1.4f, random); break;
                case "snore_deep": data = Snore(1.5f, random); break;
                case "snore_out": data = Breath(1.4f, 0.22f, 0.08f, random); break;
                case "breath_in": data = Swell(0.45f, random); break;
                // J1: a đàn nguyệt string snapping — a bright pluck, a fall in pitch, a crack.
                case "string_snap": data = Add(Add(Notes(new[] { 1318.5f }, 0f, 0.5f, 0.9f), Bend(1250f, 260f, 0.3f, 0.55f)), Noise(0.08f, 0.9f, 0.9f, random)); break;
                // J2: a low hit under the slam, felt more than heard.
                case "sting_low": data = Add(Scale(Drum(1.4f), 0.9f), Bend(98f, 49f, 1.6f, 0.7f)); break;
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

        /// <summary>Steady rain: a band-limited hiss that breathes slowly, plus scattered drops on tiles.</summary>
        private static float[] Rain(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            float low = 0, lower = 0;
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)Rate;
                float white = (float)(random.NextDouble() * 2 - 1);
                low += 0.35f * (white - low);
                lower += 0.08f * (low - lower);
                // Low-passed noise minus its darker part: a soft band without hiss or rumble.
                float swell = 0.8f + 0.2f * Mathf.Sin(2 * Mathf.PI * t / seconds * 3f);
                data[i] = (low - lower) * gain * swell;
            }
            for (int d = 0; d < seconds * 14; d++)
            {
                int start = random.Next(length);
                float frequency = 1400f + (float)random.NextDouble() * 2600f;
                float level = gain * (0.15f + (float)random.NextDouble() * 0.35f);
                for (int i = 0; i < Rate * 0.03f && start + i < length; i++)
                    data[start + i] += level * Mathf.Sin(2 * Mathf.PI * frequency * i / Rate) * Mathf.Exp(-i / (Rate * 0.004f));
            }
            return data;
        }

        /// <summary>Distant thunder: a slow low rumble every 15–25 s.</summary>
        private static float[] Thunder(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            for (float time = 8f + (float)random.NextDouble() * 4f; time < seconds - 6f; time += 15f + (float)random.NextDouble() * 10f)
            {
                int s0 = (int)(time * Rate), count = (int)(5f * Rate);
                float a = 0, b = 0;
                for (int i = 0; i < count && s0 + i < length; i++)
                {
                    float k = i / (float)count;
                    a += 0.01f * ((float)(random.NextDouble() * 2 - 1) - a);
                    b += 0.01f * (a - b);
                    float envelope = Mathf.Min(1f, k * 12f) * Mathf.Exp(-k * 4f) * (0.7f + 0.3f * Mathf.Sin(k * 40f));
                    data[s0 + i] += b * gain * 40f * envelope;
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

        /// <summary>Running music: a driving drum pulse under quick minor plucks.</summary>
        private static AudioClip Chase(string name, float seconds, float root, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var buffer = new float[length + Rate * 4];
            const float beat = 0.4f; // 150 bpm
            for (int b = 0; b * beat < seconds; b++)
            {
                int start = (int)(b * beat * Rate);
                for (int i = 0; i < Rate * 0.25f && start + i < buffer.Length; i++)
                {
                    float t = i / (float)Rate;
                    buffer[start + i] += (b % 4 == 0 ? 0.55f : 0.35f) * Mathf.Sin(2 * Mathf.PI * (70f - t * 80f) * t) * Mathf.Exp(-t * 16f);
                }
                if (b % 2 == 1)
                {
                    float filtered = 0f;
                    for (int i = 0; i < Rate * 0.12f && start + i < buffer.Length; i++)
                    {
                        filtered += 0.6f * ((float)(random.NextDouble() * 2 - 1) - filtered);
                        buffer[start + i] += 0.16f * filtered * Mathf.Exp(-i / (Rate * 0.03f));
                    }
                }
            }
            int note = 0;
            for (float time = 0.1f; time < seconds; time += beat * 0.5f)
            {
                if (random.NextDouble() < 0.25) continue;
                note = Mathf.Clamp(note + random.Next(-2, 3), 0, 9);
                float frequency = root * 2f * Mathf.Pow(2f, (MinorPenta[note % 5] + 12 * (note / 5)) / 12f);
                Pluck(buffer, (int)(time * Rate), frequency, 0.3f, 1.1f, random);
            }
            Reverb(buffer, 0.2f);
            return Clip(name, Loop(buffer, length));
        }

        /// <summary>Plucked music far away behind walls: sparse, dull and quiet.</summary>
        private static float[] Distant(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            var data = new float[(int)(seconds * Rate) + Rate * 3];
            for (float time = 0.3f; time < seconds; time += 0.5f + (float)random.NextDouble() * 0.6f)
                Pluck(data, (int)(time * Rate), 196f * Mathf.Pow(2f, MajorPenta[random.Next(5)] / 12f) * (random.NextDouble() < 0.3 ? 2f : 1f), gain, 1.8f, random);
            float low = 0f;
            for (int i = 0; i < data.Length; i++) { low += 0.08f * (data[i] - low); data[i] = low * 3f; }
            return data;
        }

        /// <summary>Crowd murmur: several "voices" of band-passed noise rising and falling at speech rate.</summary>
        private static float[] Murmur(float seconds, float gain, int voices, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            for (int v = 0; v < voices; v++)
            {
                float low = 0, lower = 0, rate = 3f + (float)random.NextDouble() * 3f, offset = (float)random.NextDouble() * 10f;
                float width = 0.12f + (float)random.NextDouble() * 0.1f;
                for (int i = 0; i < length; i++)
                {
                    float t = i / (float)Rate + offset;
                    float white = (float)(random.NextDouble() * 2 - 1);
                    low += width * (white - low);
                    lower += 0.02f * (low - lower);
                    float syllables = Mathf.Max(0f, Mathf.Sin(t * rate * Mathf.PI)) * (0.5f + 0.5f * Mathf.Sin(t * 0.37f + v));
                    data[i] += (low - lower) * gain * syllables;
                }
            }
            return data;
        }

        /// <summary>A temple bell struck now and then.</summary>
        private static float[] Bell(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            var data = new float[(int)(seconds * Rate)];
            for (float time = 3f; time < seconds - 6f; time += 12f + (float)random.NextDouble() * 8f)
            {
                var strike = Gong(6f);
                int start = (int)(time * Rate);
                for (int i = 0; i < strike.Length && start + i < data.Length; i++) data[start + i] += strike[i] * gain;
            }
            return data;
        }

        // ------------------------------------------------------------------ effects

        private static float[] Delay(float[] source, float seconds)
        {
            int offset = (int)(seconds * Rate);
            var data = new float[source.Length + offset];
            for (int i = 0; i < source.Length; i++) data[offset + i] = source[i];
            return data;
        }

        /// <summary>Two gruff barks: a falling formant over noise.</summary>
        private static float[] Bark(System.Random random)
        {
            var data = new float[(int)(0.9f * Rate)];
            foreach (float at in new[] { 0f, 0.38f })
            {
                int start = (int)(at * Rate);
                float phase = 0f, filtered = 0f;
                for (int i = 0; i < Rate * 0.22f && start + i < data.Length; i++)
                {
                    float k = i / (Rate * 0.22f);
                    phase += 2 * Mathf.PI * Mathf.Lerp(520f, 260f, k) / Rate;
                    filtered += 0.5f * ((float)(random.NextDouble() * 2 - 1) - filtered);
                    data[start + i] += (Mathf.Sin(phase) * 0.6f + Mathf.Sin(phase * 2.03f) * 0.3f + filtered * 0.35f) * Mathf.Min(1f, k * 20f) * (1 - k);
                }
            }
            return data;
        }

        /// <summary>Inharmonic metal partials with a long decay (gong, bell).</summary>
        private static float[] Gong(float seconds)
        {
            var data = new float[(int)(seconds * Rate)];
            float[] partials = { 1f, 1.48f, 2.21f, 2.76f, 3.9f };
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate;
                float sum = 0f;
                for (int p = 0; p < partials.Length; p++) sum += Mathf.Sin(2 * Mathf.PI * 110f * partials[p] * t) * Mathf.Exp(-t * (0.7f + p * 0.6f)) / (p + 1);
                data[i] = sum * 0.7f * Mathf.Min(1f, t * 200f);
            }
            return data;
        }

        /// <summary>An electric school bell: a fast-struck metal dome ringing on two partials.</summary>
        private static float[] SchoolBell(float seconds)
        {
            var data = new float[(int)(seconds * Rate)];
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate;
                float strike = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(2 * Mathf.PI * 22f * t));
                float tone = Mathf.Sin(2 * Mathf.PI * 1046.5f * t) + 0.55f * Mathf.Sin(2 * Mathf.PI * 2637f * t);
                float envelope = Mathf.Min(1f, t * 60f) * Mathf.Min(1f, (seconds - t) * 4f);
                data[i] = tone * strike * envelope * 0.16f;
            }
            return data;
        }

        /// <summary>A big barrel drum: a pitched-down boom with a skin slap on the attack.</summary>
        private static float[] Drum(float seconds)
        {
            var data = new float[(int)(seconds * Rate)];
            var random = new System.Random(81);
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate;
                float boom = Mathf.Sin(2 * Mathf.PI * (62f + 40f * Mathf.Exp(-t * 18f)) * t) * Mathf.Exp(-t * 4.5f);
                float slap = (float)(random.NextDouble() * 2 - 1) * Mathf.Exp(-t * 60f) * 0.35f;
                data[i] = (boom + slap) * 0.8f;
            }
            return data;
        }

        /// <summary>A wooden door hinge: a slow, wavering low squeal.</summary>
        private static float[] Creak(float seconds)
        {
            var data = new float[(int)(seconds * Rate)];
            float phase = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float k = i / (float)data.Length;
                phase += 2 * Mathf.PI * (180f + 90f * Mathf.Sin(k * 9f) + 40f * k) / Rate;
                float saw = 2f * Mathf.Repeat(phase / (2 * Mathf.PI), 1f) - 1f;
                data[i] = saw * 0.35f * Mathf.Sin(k * Mathf.PI) * (0.6f + 0.4f * Mathf.Sin(k * 70f));
            }
            return data;
        }


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

        /// <summary>Water dripping from eaves into puddles and jars: irregular plinks, a gutter trickling now and then.</summary>
        private static float[] Drips(float seconds, float gain, int seed)
        {
            var random = new System.Random(seed);
            int length = (int)(seconds * Rate);
            var data = new float[length];
            for (float time = 0.1f; time < seconds - 0.2f; time += 0.12f + (float)random.NextDouble() * 0.7f)
            {
                int start = (int)(time * Rate);
                float frequency = 700f + (float)random.NextDouble() * 1500f;
                float level = gain * (0.3f + (float)random.NextDouble() * 0.7f);
                float phase = 0f;
                for (int i = 0; i < Rate * 0.08f && start + i < length; i++)
                {
                    float k = i / (Rate * 0.08f);
                    phase += 2 * Mathf.PI * frequency * (1f + 0.4f * Mathf.Exp(-k * 12f)) / Rate;
                    data[start + i] += level * Mathf.Sin(phase) * Mathf.Exp(-k * 7f);
                }
            }
            float low = 0f;
            for (float time = 3f; time < seconds - 5f; time += 9f + (float)random.NextDouble() * 6f)
            {
                int start = (int)(time * Rate), count = (int)(3f * Rate);
                for (int i = 0; i < count && start + i < length; i++)
                {
                    float k = i / (float)count;
                    low += 0.25f * ((float)(random.NextDouble() * 2 - 1) - low);
                    data[start + i] += low * gain * 0.6f * Mathf.Sin(Mathf.PI * k) * (0.6f + 0.4f * Mathf.Sin(k * 90f));
                }
            }
            return data;
        }

        /// <summary>Small metal pieces knocking together (keys, coins, bangles): bright inharmonic partials.</summary>
        private static float[] Jingle(float seconds, int strikes, float gain, System.Random random)
        {
            var data = new float[(int)(seconds * Rate) + Rate / 2];
            for (int s = 0; s < strikes; s++)
            {
                int start = (int)((s / (float)strikes) * seconds * 0.8f * Rate + random.Next(Rate / 40));
                float frequency = 2400f + (float)random.NextDouble() * 1800f;
                for (int i = 0; i < Rate * 0.25f && start + i < data.Length; i++)
                {
                    float t = i / (float)Rate;
                    float v = Mathf.Sin(2 * Mathf.PI * frequency * t) + 0.5f * Mathf.Sin(2 * Mathf.PI * frequency * 2.71f * t) + 0.25f * Mathf.Sin(2 * Mathf.PI * frequency * 4.13f * t);
                    data[start + i] += gain * v * Mathf.Exp(-t * 26f) * Mathf.Min(1f, i / 8f);
                }
            }
            return data;
        }

        /// <summary>A breath or a puff: noise rising fast and fading, coloured by <paramref name="brightness"/>.</summary>
        private static float[] Breath(float seconds, float gain, float brightness, System.Random random)
        {
            var data = new float[(int)(seconds * Rate)];
            float filtered = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float k = i / (float)data.Length;
                filtered += brightness * ((float)(random.NextDouble() * 2 - 1) - filtered);
                data[i] = filtered * gain * Mathf.Min(1f, k * 8f) * (1f - k);
            }
            return data;
        }

        /// <summary>A sleeper's snore on the in-breath: throat rattle (pulsed low noise) swelling and cut off.</summary>
        private static float[] Snore(float seconds, System.Random random)
        {
            var data = new float[(int)(seconds * Rate)];
            float a = 0f, b = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate, k = i / (float)data.Length;
                a += 0.08f * ((float)(random.NextDouble() * 2 - 1) - a);
                b += 0.08f * (a - b);
                float rattle = 0.45f + 0.55f * Mathf.Max(0f, Mathf.Sin(2 * Mathf.PI * 31f * t));
                float voice = Mathf.Sin(2 * Mathf.PI * 92f * t) * 0.25f;
                float envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Min(1f, k * 1.15f)), 1.5f);
                data[i] = (b * 9f + voice) * rattle * envelope * 0.6f;
            }
            return data;
        }

        /// <summary>Someone half asleep mumbling: a low voice with syllable-rate formant noise.</summary>
        private static float[] Mumble(float seconds, System.Random random)
        {
            var data = new float[(int)(seconds * Rate)];
            float low = 0f, lower = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)Rate, k = i / (float)data.Length;
                low += 0.18f * ((float)(random.NextDouble() * 2 - 1) - low);
                lower += 0.03f * (low - lower);
                float syllables = Mathf.Max(0f, Mathf.Sin(t * 4.3f * Mathf.PI)) * (0.6f + 0.4f * Mathf.Sin(t * 1.7f));
                float pitch = 150f - 25f * k;
                data[i] = ((low - lower) * 0.9f + 0.3f * Mathf.Sin(2 * Mathf.PI * pitch * t)) * syllables * Mathf.Sin(Mathf.PI * k) * 0.7f;
            }
            return data;
        }

        // ------------------------------------------------------------------ helpers

        private static float[] Scale(float[] data, float gain)
        {
            for (int i = 0; i < data.Length; i++) data[i] *= gain;
            return data;
        }

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
