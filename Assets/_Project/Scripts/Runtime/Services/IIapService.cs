using System;

namespace ArrowBuster
{
    /// <summary>In-app purchases (Unity IAP in M7). Products: remove_ads, starter_pack (D-091).</summary>
    public interface IIapService
    {
        bool IsOwned(string productId);
        void Purchase(string productId, Action<bool> onComplete);
        void Restore(Action<bool> onComplete);
    }
}
