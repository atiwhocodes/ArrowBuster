namespace ArrowBuster
{
    /// <summary>Gameplay states (02 §1.1).</summary>
    public enum GameplayState
    {
        Loading = 0,
        Intro = 1,
        Ready = 2,
        Drawing = 3,
        Cooldown = 4,
        AwaitingResolution = 5,
        WinPending = 6,
        Won = 7,
        Failed = 8,
        Paused = 9
    }
}
