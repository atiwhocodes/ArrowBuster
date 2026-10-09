using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Minimal in-house tweening (D-031): scale, fade, move and punch on unscaled time, pooled, no per-frame GC.
    /// </summary>
    public static class UiTween
    {
        private enum Kind { Scale, Fade, Move, Punch, Rotate }

        private sealed class Tween
        {
            public Kind Kind;
            public UnityEngine.Object Target;
            public Transform Transform;
            public CanvasGroup Group;
            public RectTransform Rect;
            public Vector3 From;
            public Vector3 To;
            public float Start;
            public float Duration;
            public Ease Ease;
            public Action Done;
        }

        private sealed class Runner : MonoBehaviour
        {
            private void Update() => Tick(Time.unscaledTime);
        }

        private static readonly List<Tween> Active = new List<Tween>(64);
        private static readonly Stack<Tween> Free = new Stack<Tween>(64);
        private static Runner _runner;

        public static void Scale(Transform t, Vector3 from, Vector3 to, float duration, Ease ease = Ease.OutBack, float delay = 0f, Action done = null)
        {
            Tween tw = Add(Kind.Scale, t, duration, ease, delay, done);
            tw.Transform = t;
            tw.From = from;
            tw.To = to;
            t.localScale = from;
        }

        public static void Fade(CanvasGroup group, float from, float to, float duration, float delay = 0f, Action done = null)
        {
            Tween tw = Add(Kind.Fade, group, duration, Ease.InOutSine, delay, done);
            tw.Group = group;
            tw.From = new Vector3(from, 0f, 0f);
            tw.To = new Vector3(to, 0f, 0f);
            group.alpha = from;
        }

        public static void Move(RectTransform rect, Vector2 from, Vector2 to, float duration, Ease ease = Ease.OutCubic, float delay = 0f, Action done = null)
        {
            Tween tw = Add(Kind.Move, rect, duration, ease, delay, done);
            tw.Rect = rect;
            tw.From = from;
            tw.To = to;
            rect.anchoredPosition = from;
        }

        /// <summary>Quick scale punch around the current scale (button press, icon pop).</summary>
        public static void Punch(Transform t, float amount = 0.15f, float duration = 0.25f)
        {
            Tween tw = Add(Kind.Punch, t, duration, Ease.Linear, 0f, null);
            tw.Transform = t;
            tw.From = Vector3.one;
            tw.To = Vector3.one * amount;
        }

        public static void Rotate(Transform t, float fromDeg, float toDeg, float duration, Ease ease = Ease.InOutSine, float delay = 0f, Action done = null)
        {
            Tween tw = Add(Kind.Rotate, t, duration, ease, delay, done);
            tw.Transform = t;
            tw.From = new Vector3(0f, 0f, fromDeg);
            tw.To = new Vector3(0f, 0f, toDeg);
        }

        /// <summary>Delayed callback on unscaled time.</summary>
        public static void Delay(float seconds, Action done) => Add(Kind.Fade, null, seconds, Ease.Linear, 0f, done);

        public static void Kill(UnityEngine.Object target)
        {
            for (int i = Active.Count - 1; i >= 0; i--)
                if (Active[i].Target == target) Recycle(i);
        }

        public static float Evaluate(Ease ease, float t)
        {
            t = Mathf.Clamp01(t);
            switch (ease)
            {
                case Ease.OutCubic: return 1f - Mathf.Pow(1f - t, 3f);
                case Ease.InCubic: return t * t * t;
                case Ease.InOutSine: return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
                case Ease.OutBack:
                {
                    const float c1 = 1.70158f, c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                }
                case Ease.OutElastic:
                    return t <= 0f ? 0f : t >= 1f ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * (2f * Mathf.PI / 3f)) + 1f;
                default: return t;
            }
        }

        private static Tween Add(Kind kind, UnityEngine.Object target, float duration, Ease ease, float delay, Action done)
        {
            EnsureRunner();
            if (target != null) Kill(target);
            Tween tw = Free.Count > 0 ? Free.Pop() : new Tween();
            tw.Kind = kind;
            tw.Target = target;
            tw.Transform = null;
            tw.Group = null;
            tw.Rect = null;
            tw.Start = Time.unscaledTime + delay;
            tw.Duration = Mathf.Max(0.0001f, duration);
            tw.Ease = ease;
            tw.Done = done;
            Active.Add(tw);
            return tw;
        }

        private static void Tick(float now)
        {
            for (int i = Active.Count - 1; i >= 0; i--)
            {
                if (i >= Active.Count) continue;
                Tween tw = Active[i];
                if (now < tw.Start) continue;
                float raw = (now - tw.Start) / tw.Duration;
                float k = Evaluate(tw.Ease, raw);
                bool alive = tw.Target == null || (tw.Target as UnityEngine.Object) != null;
                if (tw.Target != null && !alive)
                {
                    Recycle(i);
                    continue;
                }
                switch (tw.Kind)
                {
                    case Kind.Scale: tw.Transform.localScale = Vector3.LerpUnclamped(tw.From, tw.To, k); break;
                    case Kind.Fade: if (tw.Group != null) tw.Group.alpha = Mathf.LerpUnclamped(tw.From.x, tw.To.x, k); break;
                    case Kind.Move: tw.Rect.anchoredPosition = Vector2.LerpUnclamped(tw.From, tw.To, k); break;
                    case Kind.Rotate: tw.Transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(tw.From.z, tw.To.z, k)); break;
                    case Kind.Punch:
                        float s = 1f + tw.To.x * Mathf.Sin(Mathf.Clamp01(raw) * Mathf.PI) * (1f - Mathf.Clamp01(raw) * 0.5f);
                        tw.Transform.localScale = new Vector3(s, s, 1f);
                        break;
                }
                if (raw >= 1f)
                {
                    Action done = tw.Done;
                    Recycle(i);
                    done?.Invoke();
                }
            }
        }

        private static void Recycle(int index)
        {
            Tween tw = Active[index];
            Active.RemoveAt(index);
            tw.Target = null;
            tw.Done = null;
            Free.Push(tw);
        }

        private static void EnsureRunner()
        {
            if (_runner != null) return;
            var go = new GameObject("UiTweenRunner") { hideFlags = HideFlags.HideInHierarchy };
            UnityEngine.Object.DontDestroyOnLoad(go);
            _runner = go.AddComponent<Runner>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Active.Clear();
            Free.Clear();
            _runner = null;
        }
    }
}
