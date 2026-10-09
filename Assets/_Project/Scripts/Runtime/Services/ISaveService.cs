namespace ArrowBuster
{
    /// <summary>Local JSON save (D-010, 01 §14).</summary>
    public interface ISaveService
    {
        SaveGame Data { get; }
        void Save();
    }
}
