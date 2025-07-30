using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform headBone;    // rig head bone
    [SerializeField] private Transform cameraRoot;  // child of headBone

    [Header("Spine Bones (bottom→top)")]
    [SerializeField] private Transform[] spineBones;
    [SerializeField, Tooltip("Weights must sum ≤ 1")]
    private float[] spineWeights;

    [Header("Input")]
    [SerializeField] private InputAction lookAction;

    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 10f;
    [SerializeField] private float minPitch = -90f;
    [SerializeField] private float maxPitch = 90f;

    [Header("Head Settings")]
    [SerializeField, Range(0f, 1f)] private float headWeight = 0.643f;
    [SerializeField, Range(-90f, 0f)] private float headMinPitch = -75f;
    [SerializeField, Range(0f, 90f)] private float headMaxPitch = 43.4f;

    private float yaw;
    private float pitch;

    private float initialHeadZ;       // initial local rotation Z of headBone
    private float[] initialSpineZ;    // initial local rotation Z of each spineBone

    void Awake()
    {
        // cache initial local Z (pitch axis) for head and spine
        initialHeadZ = headBone.localEulerAngles.z;
        initialSpineZ = new float[spineBones.Length];
        for (int i = 0; i < spineBones.Length; i++)
            initialSpineZ[i] = spineBones[i].localEulerAngles.z;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => lookAction.Enable();
    void OnDisable() => lookAction.Disable();

    void Update()
    {
        // read look input
        Vector2 li = lookAction.ReadValue<Vector2>();
        yaw += li.x * mouseSensitivity * Time.deltaTime;
        pitch -= li.y * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // rotate body (yaw)
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void LateUpdate()
    {
        // clamp how much head bends relative to full pitch
        float headPitch = Mathf.Clamp(pitch * headWeight, headMinPitch, headMaxPitch);

        // distribute headPitch over spine
        for (int i = 0; i < spineBones.Length; i++)
        {
            float w = (i < spineWeights.Length) ? spineWeights[i] : 0f;
            Vector3 e = spineBones[i].localEulerAngles;
            e.z = initialSpineZ[i] + headPitch * w;
            spineBones[i].localEulerAngles = e;
        }

        // apply head bend (pitch) on headBone using Z axis
        Vector3 h = headBone.localEulerAngles;
        h.z = initialHeadZ + headPitch;
        headBone.localEulerAngles = h;

        // cameraRoot inherits headBone’s transform automatically
    }
}
