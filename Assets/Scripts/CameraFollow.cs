using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // 따라갈 대상 (Player)
    public float height = 20f;      // 위에서 내려다보는 높이
    public float smoothSpeed = 5f;  // 따라붙는 부드러움

    [Header("부스터 속도감 (FOV 킥)")]
    public float fovPerSpeed = 1.2f; // 순항 초과 속도 1당 늘어나는 FOV
    public float maxExtraFov = 12f;  // 최대 추가 FOV
    public float fovLerp = 6f;       // FOV 변화 부드러움

    [Header("화면 흔들림")]
    public float shakeDecay = 6f;     // 흔들림이 잦아드는 속도

    public static CameraFollow Instance;
    private Camera cam;
    private float baseFov;
    private PlayerController player;
    private float shakeAmount;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null) baseFov = cam.fieldOfView;
        if (target != null) player = target.GetComponent<PlayerController>();
    }

    // 외부에서 호출: 흔들림 발생 (값이 클수록 크게 흔들림)
    public void Shake(float amount)
    {
        if (amount > shakeAmount) shakeAmount = amount;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 위치 따라가기
        Vector3 desiredPosition = new Vector3(target.position.x, height, target.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 흔들림 적용 (timeScale 0에서도 작동하게 unscaled 사용)
        if (shakeAmount > 0.001f)
        {
            Vector3 jitter = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * shakeAmount;
            transform.position += jitter;
            shakeAmount = Mathf.Lerp(shakeAmount, 0f, shakeDecay * Time.unscaledDeltaTime);
        }

        // 속도가 순항보다 빠를수록(=드리프트/부스터) 시야를 넓혀 속도감 연출
        if (cam != null && player != null)
        {
            float over = Mathf.Max(0f, player.CurrentSpeed - player.cruiseSpeed);
            float targetFov = baseFov + Mathf.Min(maxExtraFov, over * fovPerSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, fovLerp * Time.deltaTime);
        }
    }
}
