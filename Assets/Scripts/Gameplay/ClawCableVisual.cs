using UnityEngine;

namespace ClawMachine.Gameplay
{
    [RequireComponent(typeof(LineRenderer))]
    public class ClawCableVisual : MonoBehaviour
    {
        [SerializeField] private Transform topAnchor;
        [SerializeField] private Transform bottomAnchor;
        [SerializeField] private Material cableMaterial;
        [SerializeField] private float cableWidth = 0.016f;
        [SerializeField] private float twinCableSpacing = 0.05f;

        private LineRenderer lineRendererLeft;
        private LineRenderer lineRendererRight;

        private void Awake()
        {
            lineRendererLeft = GetComponent<LineRenderer>();
            ConfigureLineRenderer(lineRendererLeft);

            Transform rightTr = transform.Find("CableRight");
            if (rightTr == null)
            {
                var rightGo = new GameObject("CableRight");
                rightGo.transform.SetParent(transform, false);
                lineRendererRight = rightGo.AddComponent<LineRenderer>();
            }
            else
            {
                lineRendererRight = rightTr.GetComponent<LineRenderer>() ?? rightTr.gameObject.AddComponent<LineRenderer>();
            }
            ConfigureLineRenderer(lineRendererRight);
        }

        private void ConfigureLineRenderer(LineRenderer lr)
        {
            if (lr == null) return;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.startWidth = cableWidth;
            lr.endWidth = cableWidth;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            lr.receiveShadows = true;

            if (cableMaterial != null)
            {
                lr.sharedMaterial = cableMaterial;
            }
            else if (lr.sharedMaterial == null || lr.sharedMaterial.shader == null || lr.sharedMaterial.shader.name == "Standard")
            {
                var toonShader = Shader.Find("ClawMachine/ToonOutline") ?? Shader.Find("Universal Render Pipeline/Lit");
                if (toonShader != null)
                {
                    var mat = new Material(toonShader);
                    mat.color = new Color(0.22f, 0.24f, 0.28f, 1f);
                    lr.sharedMaterial = mat;
                }
            }
        }

        public void Initialize(Transform top, Transform bottom)
        {
            topAnchor = top;
            bottomAnchor = bottom;
        }

        private void LateUpdate()
        {
            if (topAnchor == null || bottomAnchor == null) return;

            Vector3 topPos = topAnchor.position;
            Vector3 botPos = bottomAnchor.position;
            Vector3 dir = (botPos - topPos).normalized;
            Vector3 lateral = Vector3.Cross(dir, Vector3.forward).normalized;
            if (lateral.sqrMagnitude < 0.01f) lateral = Vector3.right;

            Vector3 offset = lateral * (twinCableSpacing * 0.5f);

            if (lineRendererLeft != null)
            {
                lineRendererLeft.SetPosition(0, topPos - offset);
                lineRendererLeft.SetPosition(1, botPos - offset);
            }

            if (lineRendererRight != null)
            {
                lineRendererRight.SetPosition(0, topPos + offset);
                lineRendererRight.SetPosition(1, botPos + offset);
            }
        }
    }
}
