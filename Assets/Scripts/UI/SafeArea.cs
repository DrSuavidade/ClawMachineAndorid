using UnityEngine;

namespace ClawMachine.UI
{
    /// <summary>
    /// Adjusts RectTransform anchors to match Screen.safeArea, protecting UI from mobile notches,
    /// dynamic islands, home bars, and curved screen cutouts.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private bool conformX = true;
        [SerializeField] private bool conformY = true;

        private RectTransform rectTransform;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;
        private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            RefreshIfNeeded();
        }

        private void RefreshIfNeeded()
        {
            Rect currentSafeArea = Screen.safeArea;
            Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);
            ScreenOrientation currentOrientation = Screen.orientation;

            if (currentSafeArea != lastSafeArea ||
                currentScreenSize != lastScreenSize ||
                currentOrientation != lastOrientation)
            {
                ApplySafeArea();
            }
        }

        public void ApplySafeArea()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
                if (rectTransform == null) return;
            }

            Rect safeArea = Screen.safeArea;
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            if (screenWidth <= 0 || screenHeight <= 0) return;

            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(screenWidth, screenHeight);
            lastOrientation = Screen.orientation;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= screenWidth;
            anchorMin.y /= screenHeight;
            anchorMax.x /= screenWidth;
            anchorMax.y /= screenHeight;

            Vector2 finalAnchorMin = rectTransform.anchorMin;
            Vector2 finalAnchorMax = rectTransform.anchorMax;

            if (conformX)
            {
                finalAnchorMin.x = anchorMin.x;
                finalAnchorMax.x = anchorMax.x;
            }

            if (conformY)
            {
                finalAnchorMin.y = anchorMin.y;
                finalAnchorMax.y = anchorMax.y;
            }

            rectTransform.anchorMin = finalAnchorMin;
            rectTransform.anchorMax = finalAnchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
