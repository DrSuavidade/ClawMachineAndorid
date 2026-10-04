using UnityEngine;

namespace ClawMachine.Gameplay
{
    [RequireComponent(typeof(LineRenderer))]
    public class ClawCableVisual : MonoBehaviour
    {
        [SerializeField] private Transform topAnchor;
        [SerializeField] private Transform bottomAnchor;
        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = 0.024f;
            lineRenderer.endWidth = 0.024f;
        }

        public void Initialize(Transform top, Transform bottom)
        {
            topAnchor = top;
            bottomAnchor = bottom;
        }

        private void LateUpdate()
        {
            if (topAnchor == null || bottomAnchor == null || lineRenderer == null) return;
            lineRenderer.SetPosition(0, topAnchor.position);
            lineRenderer.SetPosition(1, bottomAnchor.position);
        }
    }
}
