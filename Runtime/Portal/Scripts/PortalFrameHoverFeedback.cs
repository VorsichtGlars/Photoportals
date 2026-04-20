using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

using DG.Tweening;

using VRSYS.Photoportals.Extensions;


namespace VRSYS.Photoportals {
    public class PortalFrameHoverFeedback : MonoBehaviour {
        [SerializeField] private bool effectEnabled = true;
        [SerializeField] private Renderer frameRenderer;
        [SerializeField] private ColliderEvents frameInteractionZone;
        [SerializeField] private XRGrabInteractable grabInteractable;

        [SerializeField] private Color pulseColor = Color.white;
        [SerializeField] private float pulseDuration = 0.6f;

        private bool isZoneIntersected;
        private int hoverCount;
        private Tween pulseTween;
        private Color originalColor;

        void Awake() {
            if (frameRenderer == null)
                frameRenderer = transform.Find("Frame")?.GetComponent<Renderer>();
            if (frameInteractionZone == null)
                frameInteractionZone = transform.Find("Frame Interaction Zone")?.GetComponent<ColliderEvents>();
            if (grabInteractable == null)
                grabInteractable = GetComponent<XRGrabInteractable>();

            originalColor = frameRenderer.material.color;
        }

        void Start() {
            frameInteractionZone.OnEnter.AddListener(_ => { isZoneIntersected = true; UpdateFeedback(); });
            frameInteractionZone.OnExit.AddListener(_ => { isZoneIntersected = false; UpdateFeedback(); });

            grabInteractable.hoverEntered.AddListener(_ => { hoverCount++; UpdateFeedback(); });
            grabInteractable.hoverExited.AddListener(_ => { hoverCount--; UpdateFeedback(); });
            grabInteractable.firstSelectEntered.AddListener(_ => UpdateFeedback());
            grabInteractable.lastSelectExited.AddListener(_ => UpdateFeedback());
        }

        void OnDestroy() {
            pulseTween?.Kill();
        }

        private void UpdateFeedback() {
            if(effectEnabled == false) return;
            bool frameIntersected = isZoneIntersected;
            bool handHovering = hoverCount > 0;
            bool portalGrabbed = grabInteractable.isSelected;

            pulseTween?.Kill();

            if (frameIntersected && handHovering && !portalGrabbed) {
                pulseTween = frameRenderer.material.DOColor(pulseColor, pulseDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            } else {
                pulseTween = frameRenderer.material.DOColor(originalColor, pulseDuration * 0.5f);
            }
        }
    }
}
