using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Rig head bone")]
    [SerializeField] private Transform headBone;
    [Tooltip("Camera root; child of PlayerArmature or EyePivot")]
    [SerializeField] private Transform cameraRoot;

    [Header("Spine Bones (bottom→top)")]
    [SerializeField] private Transform[] spineBones;
    [SerializeField, Tooltip("Weights must sum ≤ 1")]
    private float[] spineWeights;

    [Header("Input")]
    [SerializeField] private InputAction lookAction;

    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 10f;

    [Header("Camera Limits")]
    [Tooltip("Lowest angle camera can pitch (degrees)")]
    [SerializeField, Range(-180f, 0f)] private float cameraMinPitch = -90f;
    [Tooltip("Highest angle camera can pitch (degrees)")]
    [SerializeField, Range(0f, 180f)]  private float cameraMaxPitch =  90f;
    [Tooltip("Extra look-down angle beyond head limits")]
    [SerializeField, Range(0f,45f)]    private float extraDownAngle = 15f;

    [Header("Mesh Bend Settings")]
    [SerializeField, Range(0f,1f), Tooltip("0=no head bend, 1=full pitch")]
    private float headWeight    = 0.643f;
    [SerializeField, Range(-90f,0f)] private float headMinPitch = -75f;
    [SerializeField, Range(0f,90f)]  private float headMaxPitch =  43.4f;

    [Header("Camera Smoothing")]
    [Tooltip("Time for camera to catch up to target pitch")]
    [SerializeField] private float smoothTime = 0.1f;

    private float yaw, pitch;
    private float initialHeadX;
    private float[] initialSpineX;
    private Vector3 cameraOffset;

    // smoothing state
    private float currentPitch;
    private float currentPitchVelocity;

    void Awake()
    {
        // cache initial X-rotation for head + spine bones
        initialHeadX  = headBone.localEulerAngles.x;
        initialSpineX = new float[spineBones.Length];
        for (int i = 0; i < spineBones.Length; i++)
            initialSpineX[i] = spineBones[i].localEulerAngles.x;

        // cache camera offset relative to headBone
        cameraOffset = headBone.InverseTransformPoint(cameraRoot.position);

        // initialize smoothing so camera doesn't jump on start
        currentPitch = 0f;
        currentPitchVelocity = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void OnEnable()  => lookAction.Enable();
    void OnDisable() => lookAction.Disable();

    void Update()
    {
        // read look input
        Vector2 li = lookAction.ReadValue<Vector2>();
        yaw   += li.x * mouseSensitivity * Time.deltaTime;
        pitch -= li.y * mouseSensitivity * Time.deltaTime;

        // apply camera-specific pitch limits with extra down angle
        float camMin = cameraMinPitch - extraDownAngle;
        float camMax = cameraMaxPitch;
        pitch = Mathf.Clamp(pitch, camMin, camMax);

        // rotate player body around Y
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void LateUpdate()
    {
        // 1) position camera at eye-level
        cameraRoot.position = headBone.TransformPoint(cameraOffset);

        // 2) smooth raw pitch into currentPitch
        currentPitch = Mathf.SmoothDamp(
            currentPitch,
            pitch,
            ref currentPitchVelocity,
            smoothTime
        );

        // 3) apply smoothed pitch to camera
        cameraRoot.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

        // 4) compute head-bend for mesh (limited by headWeight)
        float headPitch = Mathf.Clamp(pitch * headWeight, headMinPitch, headMaxPitch);

        // 5) distribute bend over spine bones (X-axis)
        for (int i = 0; i < spineBones.Length; i++)
        {
            float w = (i < spineWeights.Length) ? spineWeights[i] : 0f;
            Vector3 e = spineBones[i].localEulerAngles;
            e.x = initialSpineX[i] + headPitch * w;
            spineBones[i].localEulerAngles = e;
        }

        // 6) bend the head bone itself (X-axis)
        Vector3 h = headBone.localEulerAngles;
        h.x = initialHeadX + headPitch;
        headBone.localEulerAngles = h;
    }
}
