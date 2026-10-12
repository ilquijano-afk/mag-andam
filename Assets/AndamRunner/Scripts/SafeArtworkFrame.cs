using UnityEngine;

namespace AndamRunner
{
    // Uses the final safe-area dimensions, including after CanvasScaler or orientation changes.
    // The complete PNG and all painted-button targets remain in this single fitted rectangle.
    public sealed class SafeArtworkFrame : MonoBehaviour
    {
        private RectTransform frame, parent;
        private float aspect;
        private Vector2 lastSize;

        public void Initialize(float imageAspect)
        {
            frame=(RectTransform)transform;
            parent=transform.parent as RectTransform;
            aspect=imageAspect;
            frame.anchorMin=frame.anchorMax=frame.pivot=new Vector2(.5f,.5f);
            frame.anchoredPosition=Vector2.zero;
            FitNow();
        }

        private void OnEnable() { FitNow(); }
        private void OnRectTransformDimensionsChange() { FitNow(); }
        private void LateUpdate() { if(parent!=null && lastSize!=parent.rect.size) FitNow(); }

        public void FitNow()
        {
            if(frame==null || parent==null || aspect<=0) return;
            Vector2 bounds=parent.rect.size;
            if(bounds.x<=0 || bounds.y<=0) return;
            lastSize=bounds;
            float width=Mathf.Min(bounds.x,bounds.y*aspect);
            Vector2 size=new Vector2(width,width/aspect);
            if((frame.sizeDelta-size).sqrMagnitude>.0001f) frame.sizeDelta=size;
        }
    }
}
