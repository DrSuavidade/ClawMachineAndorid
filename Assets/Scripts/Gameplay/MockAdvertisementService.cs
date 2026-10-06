using System;
using UnityEngine;
using ClawMachine.Core.Services;

namespace ClawMachine.Gameplay
{
    /// <summary>
    /// Mockable Rewarded Video Ad provider.
    /// In production, swap with Google Mobile Ads (AdMob) or Unity Ads SDK implementation.
    /// Non-P2W: Player can watch a video when low on coins to receive 5 free plays (100 coins).
    /// </summary>
    public class MockAdvertisementService : MonoBehaviour, IAdvertisementService
    {
        public bool IsRewardedAdReady => true;

        private void Awake()
        {
            ServiceLocator.Register<IAdvertisementService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IAdvertisementService>();
        }

        public void ShowRewardedAd(Action onRewardEarned, Action onFailed = null)
        {
            Debug.Log("[MockAdvertisementService] 📺 Showing Rewarded Video Ad (Simulation)...");
            // Simulate instant completion for editor/offline prototype
            onRewardEarned?.Invoke();
        }
    }
}
