using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
[RequireComponent(typeof(ARAnchorManager))]
public class HologramManager : MonoBehaviour
{
    [SerializeField] LayerMask hologramLayers;
    [SerializeField] Hologram hologramPrefab;
    ARRaycastManager raycastManager;
    ARPlaneManager planeManager;

    ARAnchorManager anchorManager;

    Camera cam;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        anchorManager = GetComponent<ARAnchorManager>();

        cam = Camera.main;
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        TouchSimulation.Enable();
#endif
        EnhancedTouchSupport.Enable();
        Touch.onFingerUp += FingerUp;
    }

    void OnDisable()
    {
        Touch.onFingerUp -= FingerUp;
        EnhancedTouchSupport.Disable();
#if UNITY_EDITOR
        TouchSimulation.Disable();
#endif
    }

    void FingerUp(Finger finger)
    {
        if (finger.currentTouch.isTap)
        {
            Vector3 screenPos = finger.currentTouch.screenPosition;

            if (RaycastHologram(screenPos, out Hologram hitHologram))
            {
                hitHologram.ChangeColor();
            }
            else
            {
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
                {
                    ARRaycastHit arHit = hits[0];
                    Pose pose = arHit.pose;
                    ARPlane plane = planeManager.GetPlane(arHit.trackableId);

                    ARAnchor anchor = anchorManager.AttachAnchor(plane, pose);
                    CreateHologram(anchor.transform);
                }
            }
        }
    }

    bool RaycastHologram(Vector2 screenPos, out Hologram hologram)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hologramHit, cam.farClipPlane, hologramLayers))
        {
            hologram = hologramHit.collider.GetComponent<Hologram>();
            return true;
        }

        hologram = null;
        return false;
    }

    void CreateHologram(Transform parent)
    {
        Transform newHologram = Instantiate(hologramPrefab, parent).transform;

        // To prevent objects from falling through ground
        Vector3 dir = (cam.transform.position - newHologram.position).normalized;
        newHologram.position += Vector3.Scale(dir * newHologram.GetComponent<SphereCollider>().radius, newHologram.localScale);
    }
}