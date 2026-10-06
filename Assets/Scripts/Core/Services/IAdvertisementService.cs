using System;

namespace ClawMachine.Core.Services
{
    public interface IAdvertisementService
    {
        bool IsRewardedAdReady { get; }
        void ShowRewardedAd(Action onRewardEarned, Action onFailed = null);
    }
}
