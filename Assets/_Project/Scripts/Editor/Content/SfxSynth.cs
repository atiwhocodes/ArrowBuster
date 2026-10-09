using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArrowBuster.Editor
{
    /// <summary>
    /// Procedural, original sound design (05 §10): every clip is synthesised from oscillators, noise, filters and
    /// envelopes with a fixed seed, then written as 16-bit mono WAV. Placeholder-quality but readable per material.
    /// </summary>
    public static class SfxSynth
    {
        public const int Rate = 44100;

        public delegate void Generator(float[] buffer, System.Random random);

        /// <summary>Clip name → (seconds, generator). Variations are produced with different seeds.</summary>
        public static readonly Dictionary<string, (float seconds, Generator gen)> Clips = new Dictionary<string, (float, Generator)>
        {
            ["SFX_Bow_Draw"] = (0.45f, BowDraw),
            ["SFX_Bow_FullDraw"] = (0.12f, (b, r) => Ping(b, r, 1500f, 0.05f, 0.25f)),
            ["SFX_Bow_Release"] = (0.35f, BowRelease),
            ["SFX_Bow_Cancel"] = (0.25f, (b, r) => { Pluck(b, r, 140f, 0.996f, 0.35f); }),
            ["SFX_Arrow_Whoosh"] = (0.45f, Whoosh),
            ["SFX_Impact_Timber"] = (0.25f, (b, r) => { Thump(b, 170f, 85f, 0.12f, 0.8f); Click(b, r, 0.02f, 0.5f, 0.35f); }),
            ["SFX_Impact_Straw"] = (0.25f, (b, r) => { Thump(b, 120f, 70f, 0.08f, 0.4f); Rustle(b, r, 0.18f, 0.35f); }),
            ["SFX_Impact_Stone"] = (0.3f, (b, r) => { Thump(b, 95f, 55f, 0.18f, 0.9f); Click(b, r, 0.03f, 0.25f, 0.4f); }),
            ["SFX_Impact_Ice"] = (0.5f, (b, r) => { Bell(b, 2400f, 0.35f, 0.4f); Click(b, r, 0.02f, 0.8f, 0.3f); }),
            ["SFX_Impact_Metal"] = (0.6f, (b, r) => { Bell(b, 1750f, 0.45f, 0.45f); Bell(b, 2630f, 0.3f, 0.25f); }),
            ["SFX_Impact_Earth"] = (0.2f, (b, r) => { Thump(b, 110f, 60f, 0.08f, 0.6f); Rustle(b, r, 0.1f, 0.25f); }),
            ["SFX_Arrow_EmbedWood"] = (0.3f, (b, r) => { Thump(b, 210f, 120f, 0.09f, 0.85f); Pluck(b, r, 95f, 0.99f, 0.25f); }),
            ["SFX_Break_Timber"] = (0.6f, BreakTimber),
            ["SFX_Break_Straw"] = (0.5f, (b, r) => { Rustle(b, r, 0.45f, 0.7f); Thump(b, 90f, 60f, 0.1f, 0.3f); }),
            ["SFX_Break_Stone"] = (0.7f, (b, r) => { Rumble(b, r, 0.6f, 0.8f); Thump(b, 70f, 45f, 0.3f, 0.8f); }),
            ["SFX_Break_Ice"] = (0.8f, BreakGlass),
            ["SFX_Break_Crest"] = (0.6f, (b, r) => { BreakTimber(b, r); Bell(b, 1320f, 0.3f, 0.25f); }),
            ["SFX_Break_Vase"] = (0.9f, BreakGlass),
            ["SFX_Rope_Cut"] = (0.45f, (b, r) => { Pluck(b, r, 330f, 0.993f, 0.6f); Click(b, r, 0.015f, 0.9f, 0.5f); }),
            ["SFX_Splash"] = (0.8f, Splash),
            ["SFX_Objective_Cleared"] = (0.7f, (b, r) => { Bell(b, 880f, 0.5f, 0.35f); Bell(b, 1320f, 0.4f, 0.25f); }),
            ["SFX_Chain_Hit"] = (0.35f, (b, r) => { Thump(b, 160f, 80f, 0.15f, 0.7f); Click(b, r, 0.03f, 0.3f, 0.3f); }),
            ["SFX_Win_Sting"] = (1.6f, (b, r) => Arpeggio(b, new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.13f, 0.9f)),
            ["SFX_Fail_Sting"] = (1.1f, (b, r) => Arpeggio(b, new[] { 392f, 311.13f }, 0.28f, 0.7f)),
            ["SFX_Protected_Alarm"] = (0.7f, (b, r) => Arpeggio(b, new[] { 523.25f, 440f, 523.25f, 440f }, 0.12f, 0.45f, square: true)),
            ["SFX_UI_Tap"] = (0.08f, (b, r) => Ping(b, r, 1100f, 0.025f, 0.35f)),
            ["SFX_UI_Star"] = (0.6f, (b, r) => { Bell(b, 1568f, 0.4f, 0.35f); Bell(b, 2349f, 0.25f, 0.2f); }),
            ["SFX_Body_Thud"] = (0.3f, (b, r) => Thump(b, 80f, 50f, 0.2f, 0.7f)),
        };

        /// <summary>Writes every clip (with <paramref name="variations"/> seeds each) into <paramref name="folder"/>.</summary>
        public static List<string> WriteAll(string folder, int variations = 2)
        {
            Directory.CreateDirectory(folder);
            var written = new List<string>();
            foreach (var pair in Clips)
            {
                int count = pair.Key.StartsWith("SFX_UI") || pair.Key.Contains("Sting") || pair.Key.Contains("Alarm") ? 1 : variations;
                for (int v = 0; v < count; v++)
                {
                    var random = new System.Random(StableHash(pair.Key) ^ (v * 7919));
                    var buffer = new float[Mathf.CeilToInt(pair.Value.seconds * Rate)];
                    pair.Value.gen(buffer, random);
                    if (v > 0) Detune(buffer, 1f + (v % 2 == 0 ? -0.05f : 0.05f));
                    Normalize(buffer, 0.85f);
                    FadeEdges(buffer);
                    string path = Path.Combine(folder, $"{pair.Key}_{v + 1:00}.wav");
                    WriteWav(path, buffer);
                    written.Add(path);
                }
            }
            return written;
        }

        // ---------------------------------------------------------------- generators

        private static void BowDraw(float[] b, System.Random r)
        {
            float lp = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.3f) * (1f - Mathf.Clamp01((t - 0.35f) / 0.1f));
                float n = (float)(r.NextDouble() * 2 - 1);
                lp += (n - lp) * 0.04f;
                float creak = Mathf.Sin(2f * Mathf.PI * (80f + t * 60f) * t) * (0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 22f * t));
                b[i] += (lp * 0.8f + creak * 0.12f) * env;
            }
        }

        private static void BowRelease(float[] b, System.Random r)
        {
            Pluck(b, r, 196f, 0.995f, 0.9f);
            Thump(b, 120f, 70f, 0.06f, 0.5f);
            Whoosh(b, r);
        }

        private static void Whoosh(float[] b, System.Random r)
        {
            float lp = 0f, bp = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = i / (float)Rate;
                float k = Mathf.Lerp(0.02f, 0.25f, Mathf.Clamp01(t / 0.3f));
                float n = (float)(r.NextDouble() * 2 - 1);
                lp += (n - lp) * k;
                bp += (lp - bp) * 0.5f;
                float env = Mathf.Sin(Mathf.Clamp01(t / 0.4f) * Mathf.PI);
                b[i] += (lp - bp) * env * 0.9f;
            }
        }

        private static void BreakTimber(float[] b, System.Random r)
        {
            Thump(b, 120f, 60f, 0.2f, 0.8f);
            int cracks = 4 + r.Next(3);
            for (int c = 0; c < cracks; c++)
            {
                int start = (int)((0.01f + c * 0.05f + (float)r.NextDouble() * 0.03f) * Rate);
                float lp = 0f;
                int len = (int)(0.07f * Rate);
                for (int i = 0; i < len && start + i < b.Length; i++)
                {
                    float env = Mathf.Exp(-i / (0.012f * Rate));
                    float n = (float)(r.NextDouble() * 2 - 1);
                    lp += (n - lp) * 0.35f;
                    b[start + i] += lp * env * 0.9f;
                }
            }
        }

        private static void BreakGlass(float[] b, System.Random r)
        {
            Click(b, r, 0.05f, 0.95f, 0.8f);
            float[] partials = { 1180f, 1760f, 2350f, 3120f, 3990f };
            foreach (float f in partials) Bell(b, f * (0.97f + (float)r.NextDouble() * 0.06f), 0.25f + (float)r.NextDouble() * 0.3f, 0.18f);
            for (int s = 0; s < 6; s++)
            {
                int start = (int)((0.05f + (float)r.NextDouble() * 0.4f) * Rate);
                float f = 2200f + (float)r.NextDouble() * 2800f;
                int len = (int)(0.08f * Rate);
                for (int i = 0; i < len && start + i < b.Length; i++)
                    b[start + i] += Mathf.Sin(2f * Mathf.PI * f * i / Rate) * Mathf.Exp(-i / (0.015f * Rate)) * 0.15f;
            }
        }

        private static void Splash(float[] b, System.Random r)
        {
            float lp = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Exp(-t * 5f) * Mathf.Clamp01(t / 0.01f);
                float n = (float)(r.NextDouble() * 2 - 1);
                lp += (n - lp) * 0.08f;
                b[i] += lp * env * 1.4f;
            }
            for (int s = 0; s < 5; s++)
            {
                int start = (int)((0.08f + s * 0.09f + (float)r.NextDouble() * 0.04f) * Rate);
                float f0 = 500f + (float)r.NextDouble() * 400f;
                int len = (int)(0.06f * Rate);
                for (int i = 0; i < len && start + i < b.Length; i++)
                {
                    float t = i / (float)Rate;
                    b[start + i] += Mathf.Sin(2f * Mathf.PI * (f0 + t * 6000f) * t) * Mathf.Exp(-t * 60f) * 0.25f;
                }
            }
        }

        // ---------------------------------------------------------------- building blocks

        private static void Thump(float[] b, float f0, float f1, float decay, float gain)
        {
            float phase = 0f;
            for (int i = 0; i < b.Length; i++)
            {
                float t = i / (float)Rate;
                float f = Mathf.Lerp(f1, f0, Mathf.Exp(-t / (decay * 0.5f)));
                phase += 2f * Mathf.PI * f / Rate;
                b[i] += Mathf.Sin(phase) * Mathf.Exp(-t / decay) * gain * Mathf.Clamp01(t / 0.002f);
            }
        }

        private static void Click(float[] b, System.Random r, float length, float brightness, float gain)
        {
            int len = Mathf.Min(b.Length, (int)(length * Rate));
            float lp = 0f;
            for (int i = 0; i < len; i++)
            {
                float n = (float)(r.NextDouble() * 2 - 1);
                lp += (n - lp) * brightness;
                b[i] += lp * Mathf.Exp(-i / (length * 0.3f * Rate)) * gain;
            }
        }

        private static void Rustle(float[] b, System.Random r, float length, float gain)
        {
            int len = Mathf.Min(b.Length, (int)(length * Rate));
            float lp = 0f, hp = 0f;
            for (int i = 0; i < len; i++)
            {
                float n = (float)(r.NextDouble() * 2 - 1);
                lp += (n - lp) * 0.3f;
                hp += (lp - hp) * 0.05f;
                float grain = r.NextDouble() < 0.02 ? 1f : 0.35f;
                b[i] += (lp - hp) * Mathf.Exp(-i / (length * 0.4f * Rate)) * gain * grain;
            }
        }

        private static void Rumble(float[] b, System.Random r, float length, float gain)
        {
            int len = Mathf.Min(b.Length, (int)(length * Rate));
            float lp = 0f;
            for (int i = 0; i < len; i++)
            {
                float n = (float)(r.NextDouble() * 2 - 1);
                lp += (n - lp) * 0.03f;
                b[i] += lp * 3f * Mathf.Exp(-i / (length * 0.35f * Rate)) * gain;
            }
        }

        private static void Bell(float[] b, float f, float decay, float gain)
        {
            for (int i = 0; i < b.Length; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Exp(-t / decay) * Mathf.Clamp01(t / 0.003f);
                b[i] += (Mathf.Sin(2f * Mathf.PI * f * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * f * 2.76f * t)) * env * gain;
            }
        }

        private static void Ping(float[] b, System.Random r, float f, float decay, float gain) => Bell(b, f, decay, gain);

        /// <summary>Karplus–Strong plucked string.</summary>
        private static void Pluck(float[] b, System.Random r, float f, float damping, float gain)
        {
            int period = Mathf.Max(2, (int)(Rate / f));
            var line = new float[period];
            for (int i = 0; i < period; i++) line[i] = (float)(r.NextDouble() * 2 - 1);
            int index = 0;
            for (int i = 0; i < b.Length; i++)
            {
                int next = (index + 1) % period;
                float value = line[index];
                line[index] = (line[index] + line[next]) * 0.5f * damping;
                index = next;
                b[i] += value * gain;
            }
        }

        private static void Arpeggio(float[] b, float[] notes, float step, float gain, bool square = false)
        {
            for (int n = 0; n < notes.Length; n++)
            {
                int start = (int)(n * step * Rate);
                float f = notes[n];
                bool last = n == notes.Length - 1;
                float decay = last ? 0.55f : 0.22f;
                for (int i = start; i < b.Length; i++)
                {
                    float t = (i - start) / (float)Rate;
                    float s = Mathf.Sin(2f * Mathf.PI * f * t);
                    float tone = square ? Mathf.Clamp(s * 3f, -1f, 1f) * 0.6f : s + 0.25f * Mathf.Sin(4f * Mathf.PI * f * t);
                    b[i] += tone * Mathf.Exp(-t / decay) * Mathf.Clamp01(t / 0.004f) * gain * 0.6f;
                }
            }
        }

        // ---------------------------------------------------------------- music

        /// <summary>A gentle 16-bar pentatonic loop (Greenwood): plucked melody, soft bass, shaker. Loops seamlessly.</summary>
        public static void WriteMusicLoop(string path, int seed = 7)
        {
            const float bpm = 96f;
            float beat = 60f / bpm;
            int bars = 16;
            int samples = Mathf.RoundToInt(bars * 4 * beat * Rate);
            var b = new float[samples];
            var r = new System.Random(seed);
            float[] scale = { 0, 2, 4, 7, 9, 12, 14, 16 };
            int[] chordRoots = { 0, 9, 5, 7 }; // I – vi – IV – V (in semitones from C)
            float root = 261.63f;

            for (int bar = 0; bar < bars; bar++)
            {
                int chord = chordRoots[(bar / 2) % 4];
                // Bass: root on beats 1 and 3.
                for (int beatIndex = 0; beatIndex < 4; beatIndex += 2)
                    Note(b, (bar * 4 + beatIndex) * beat, root / 4f * Mathf.Pow(2f, chord / 12f), beat * 1.8f, 0.32f, 0.5f, triangle: true);
                // Pad: soft fifth.
                Note(b, bar * 4 * beat, root / 2f * Mathf.Pow(2f, (chord + 7) / 12f), beat * 4f, 0.07f, 1.5f, triangle: true);
                // Melody: eighth notes from the pentatonic scale, sparse.
                for (int e = 0; e < 8; e++)
                {
                    if (r.NextDouble() < 0.38) continue;
                    float semis = scale[r.Next(scale.Length)] + (chord % 12 == 9 ? -3 : 0);
                    float f = root * Mathf.Pow(2f, semis / 12f);
                    PluckAt(b, (bar * 4 + e * 0.5f) * beat, f, 0.16f);
                }
                // Shaker on off-beats.
                for (int e = 0; e < 8; e++)
                    if (e % 2 == 1) NoiseHit(b, r, (bar * 4 + e * 0.5f) * beat, 0.04f, 0.05f);
            }
            Normalize(b, 0.7f);
            WriteWav(path, b);
        }

        private static void Note(float[] b, float start, float f, float length, float gain, float decay, bool triangle)
        {
            int s0 = (int)(start * Rate);
            int len = (int)(length * Rate);
            for (int i = 0; i < len; i++)
            {
                int idx = (s0 + i) % b.Length;
                float t = i / (float)Rate;
                float phase = f * t;
                float wave = triangle ? 1f - 4f * Mathf.Abs(phase - Mathf.Floor(phase + 0.5f)) : Mathf.Sin(2f * Mathf.PI * phase);
                float env = Mathf.Clamp01(t / 0.02f) * Mathf.Exp(-t / decay) * Mathf.Clamp01((length - t) / 0.05f);
                b[idx] += wave * env * gain;
            }
        }

        private static void PluckAt(float[] b, float start, float f, float gain)
        {
            int s0 = (int)(start * Rate);
            int len = (int)(0.6f * Rate);
            var tmp = new float[len];
            Pluck(tmp, new System.Random((int)(f * 13 + start * 1000)), f, 0.996f, gain);
            for (int i = 0; i < len; i++) b[(s0 + i) % b.Length] += tmp[i] * Mathf.Clamp01((len - i) / (0.05f * Rate));
        }

        private static void NoiseHit(float[] b, System.Random r, float start, float length, float gain)
        {
            int s0 = (int)(start * Rate);
            int len = (int)(length * Rate);
            float hp = 0f;
            for (int i = 0; i < len; i++)
            {
                float n = (float)(r.NextDouble() * 2 - 1);
                hp += (n - hp) * 0.1f;
                b[(s0 + i) % b.Length] += (n - hp) * Mathf.Exp(-i / (length * 0.3f * Rate)) * gain;
            }
        }

        // ---------------------------------------------------------------- utilities

        /// <summary>FNV-1a: stable across runs and platforms (string.GetHashCode is not guaranteed to be).</summary>
        private static int StableHash(string text)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in text) hash = (hash ^ c) * 16777619;
                return (int)hash;
            }
        }

        private static void Detune(float[] b, float ratio)
        {
            var copy = (float[])b.Clone();
            for (int i = 0; i < b.Length; i++)
            {
                float src = i * ratio;
                int i0 = (int)src;
                float frac = src - i0;
                b[i] = i0 + 1 < copy.Length ? Mathf.Lerp(copy[i0], copy[i0 + 1], frac) : 0f;
            }
        }

        private static void Normalize(float[] b, float peak)
        {
            float max = 0.0001f;
            foreach (float s in b) max = Mathf.Max(max, Mathf.Abs(s));
            float k = peak / max;
            for (int i = 0; i < b.Length; i++) b[i] = Mathf.Clamp(b[i] * k, -1f, 1f);
        }

        private static void FadeEdges(float[] b)
        {
            int fade = Mathf.Min(b.Length / 4, (int)(0.01f * Rate));
            for (int i = 0; i < fade; i++) b[b.Length - 1 - i] *= i / (float)fade;
        }

        public static void WriteWav(string path, float[] samples)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            using (var w = new BinaryWriter(stream))
            {
                int dataBytes = samples.Length * 2;
                w.Write(new[] { 'R', 'I', 'F', 'F' });
                w.Write(36 + dataBytes);
                w.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                w.Write(16);
                w.Write((short)1);
                w.Write((short)1);
                w.Write(Rate);
                w.Write(Rate * 2);
                w.Write((short)2);
                w.Write((short)16);
                w.Write(new[] { 'd', 'a', 't', 'a' });
                w.Write(dataBytes);
                foreach (float s in samples) w.Write((short)Mathf.RoundToInt(Mathf.Clamp(s, -1f, 1f) * short.MaxValue));
            }
        }
    }
}
