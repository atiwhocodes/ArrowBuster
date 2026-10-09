namespace ArrowBuster
{
    /// <summary>Remote config with local defaults (06 §14). Firebase Remote Config adapter in M7.</summary>
    public interface IRemoteConfigService
    {
        int GetInt(string key, int fallback);
        float GetFloat(string key, float fallback);
        bool GetBool(string key, bool fallback);
    }
}
