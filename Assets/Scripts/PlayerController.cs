using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float cruiseSpeed = 15f;   // 자동 전진 속도
    public float turnSpeed = 85f;     // 일반 선회 (넓고 완만)
    public float bankAngle = 30f;     // 선회 시 기울기

    [Header("드리프트 (넓은 U턴)")]
    public float driftSpeed = 18f;      // 드리프트 중 속도 (빠르게 카빙)
    public float driftTurnSpeed = 150f; // 드리프트 선회 (넓은 U자 곡선)
    public float driftBankAngle = 60f;  // 드리프트 중 깊은 기울기
    public float driftSmooth = 14f;     // 드리프트 진입 반응성

    [Header("드리프트 부스터 (충전식)")]
    public float boostSpeed = 26f;      // 드리프트 직후 폭발 속도
    public float boostDecel = 12f;      // 부스터가 순항으로 잦아드는 속도
    public float minChargeTime = 0.45f; // 이 시간 이상 '드리프트+선회'해야 부스터 발동 (연타 방지)
    public float boostCooldown = 1.0f;  // 부스터 후 재충전 대기

    [Header("이펙트")]
    public TrailRenderer[] driftTrails;  // 드리프트 중 양 날개 바람 가르기 선

    private float heading;
    private float bank;
    private float turnInput;
    private bool isDrifting;
    private bool wasDrifting;   // 직전 프레임 드리프트 상태 (종료 감지용)
    private float driftCharge;  // 드리프트+선회 유지 시간 (부스터 충전량)
    private float cooldownTimer;// 부스터 쿨다운 남은 시간

    private float curSpeed;
    private float curTurn;
    private float curBankMax;

    // 카메라 등 외부에서 현재 속도를 읽을 수 있게
    public float CurrentSpeed => curSpeed;

    void Start()
    {
        heading = transform.eulerAngles.y;
        curSpeed = cruiseSpeed;
        curTurn = turnSpeed;
        curBankMax = bankAngle;
    }

    void Update()
    {
        HandleInput();

        // 드리프트 중에만 양 날개 트레일(선) 그리기
        if (driftTrails != null)
        {
            foreach (var t in driftTrails)
            {
                if (t != null) t.emitting = isDrifting;
            }
        }

        // --- 선회/뱅킹: 드리프트면 넓은 U턴 값으로 부드럽게 전환 ---
        float targetTurn    = isDrifting ? driftTurnSpeed : turnSpeed;
        float targetBankMax = isDrifting ? driftBankAngle : bankAngle;
        curTurn    = Mathf.Lerp(curTurn, targetTurn, driftSmooth * Time.deltaTime);
        curBankMax = Mathf.Lerp(curBankMax, targetBankMax, driftSmooth * Time.deltaTime);

        // 쿨다운 진행
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        // --- 속도: 드리프트 중 / 부스터 / 순항 ---
        if (isDrifting)
        {
            // 실제로 '선회하면서' 드리프트할 때만 부스터 충전 (직진 드리프트는 충전 X)
            if (Mathf.Abs(turnInput) > 0.1f) driftCharge += Time.deltaTime;

            // 드리프트 중엔 카빙 속도로
            curSpeed = Mathf.Lerp(curSpeed, driftSpeed, driftSmooth * Time.deltaTime);
        }
        else
        {
            // 드리프트가 막 끝난 순간 → 충분히 충전됐고 쿨다운이 아니면 부스터 발동
            if (wasDrifting)
            {
                if (driftCharge >= minChargeTime && cooldownTimer <= 0f)
                {
                    curSpeed = boostSpeed;
                    cooldownTimer = boostCooldown;
                }
                driftCharge = 0f; // 충전량 리셋
            }
            // 부스터에서 순항으로 서서히 잦아듦
            curSpeed = Mathf.MoveTowards(curSpeed, cruiseSpeed, boostDecel * Time.deltaTime);
        }

        wasDrifting = isDrifting;

        // 선회 적용
        heading += turnInput * curTurn * Time.deltaTime;

        // 뱅킹
        bank = Mathf.Lerp(bank, -turnInput * curBankMax, 8f * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, heading, bank);

        // 전진
        transform.position += transform.forward * curSpeed * Time.deltaTime;
    }

    void HandleInput()
    {
        turnInput = 0f;
        isDrifting = false;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.isPressed)  turnInput = -1f;
            if (keyboard.rightArrowKey.isPressed) turnInput = 1f;

            if (keyboard.spaceKey.isPressed) isDrifting = true;
        }

        var touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
        {
            float touchX = touchscreen.primaryTouch.position.ReadValue().x;
            turnInput = touchX < Screen.width / 2f ? -1f : 1f;

            if (touchscreen.touches.Count > 1 && touchscreen.touches[1].press.isPressed)
                isDrifting = true;
        }
    }
}
