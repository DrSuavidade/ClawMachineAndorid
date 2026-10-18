using System;

namespace ClawMachine.Core.Services
{
    public interface IAdService
    {
        bool IsAdReady { get; }
        void ShowRewardedAd(Action<int> onRewarded, Action onFailed = null);
    }
}
