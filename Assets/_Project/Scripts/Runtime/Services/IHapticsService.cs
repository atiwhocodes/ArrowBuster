namespace ArrowBuster
{
    /// <summary>Thin haptics bridge (D-032). Respects the haptics setting and a 50 ms rate limit.</summary>
    public interface IHapticsService
    {
        bool Enabled { get; set; }
        void Play(HapticKind kind);
    }
}
