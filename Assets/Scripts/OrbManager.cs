using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Pool;
using TMPro;

using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class OrbManager : MonoBehaviour
{
    [SerializeField] int targetOrbCount = 20;
    [SerializeField] TMP_Text counterText;
    [SerializeField] TMP_Text infoText;
    [SerializeField] GameObject winScreen;
    [SerializeField] LayerMask orbLayers;
    [SerializeField] Orb orbPrefab;
    ARRaycastManager raycastManager;
    ARPlaneManager planeManager;

    [HideInInspector] public IObjectPool<Orb> pool;

    Camera cam;

    public static OrbManager Instance;

    int _activeOrbCount = 0;
    public int GreenOrbCount
    {
        get => _activeOrbCount;
        private set
        {
            _activeOrbCount = value;
            counterText.text = GreenOrbCount + "/" + targetOrbCount;

            if (_activeOrbCount >= targetOrbCount) 
            {
                winScreen.SetActive(true);
            } 
        }
    }

    void Awake()
    {
        Instance = this;

        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        cam = Camera.main;

        pool = new ObjectPool<Orb>(
            CreateObject,
            OnGetObject,
            OnReleaseObject,
            OnDestroyObject
        );

        GreenOrbCount = 0;
        infoText.text = "Create " + targetOrbCount + " green orbs";
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

            if (RaycastOrb(screenPos, out Orb hitOrb))
            {
                bool wasGreen = hitOrb.IsGreen;
                hitOrb.ChangeColor();
                if (wasGreen && !hitOrb.IsGreen) 
                {
                    GreenOrbCount--;
                }
                else if (!wasGreen && hitOrb.IsGreen) 
                {
                    GreenOrbCount++;
                }
            }
            else
            {
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
                {
                    ARRaycastHit hit = hits[0];
                    Orb orb = pool.Get();
                    orb.transform.position = hit.pose.position;

                    // To prevent objects from falling through ground
                    Vector3 dir = (cam.transform.position - orb.transform.position).normalized;
                    orb.transform.position += dir * orb.Radius;
                }
            }
        }
    }

    bool RaycastOrb(Vector2 screenPos, out Orb orb)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit orbHit, cam.farClipPlane, orbLayers))
        {
            orb = orbHit.collider.GetComponent<Orb>();
            return true;
        }

        orb = null;
        return false;
    }

    // ObjectPool methods

    Orb CreateObject()
    {
        return Instantiate(orbPrefab);
    }

    void OnReleaseObject(Orb orb)
    {
        orb.gameObject.SetActive(false);

        if (orb.IsGreen)
        {
            GreenOrbCount--;
        }
    }

    void OnGetObject(Orb orb)
    {
        orb.ResetProperties();

        if (orb.IsGreen)
        {
            GreenOrbCount++;
        }

        orb.gameObject.SetActive(true);
    }

    void OnDestroyObject(Orb orb)
    {
        Destroy(orb.gameObject);
    }
}