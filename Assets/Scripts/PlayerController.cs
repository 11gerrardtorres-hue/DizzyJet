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

    [Header("드리프트 부스터 (빵 치고나가기)")]
    public float boostSpeed = 26f;    // 드리프트 직후 폭발 속도
    public float boostDecel = 12f;    // 부스터가 순항으로 잦아드는 속도

    private float heading;
    private float bank;
    private float turnInput;
    private bool isDrifting;
    private bool wasDrifting;   // 직전 프레임 드리프트 상태 (종료 감지용)

    private float curSpeed;
    private float curTurn;
    private float curBankMax;

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

        // --- 선회/뱅킹: 드리프트면 넓은 U턴 값으로 부드럽게 전환 ---
        float targetTurn    = isDrifting ? driftTurnSpeed : turnSpeed;
        float targetBankMax = isDrifting ? driftBankAngle : bankAngle;
        curTurn    = Mathf.Lerp(curTurn, targetTurn, driftSmooth * Time.deltaTime);
        curBankMax = Mathf.Lerp(curBankMax, targetBankMax, driftSmooth * Time.deltaTime);

        // --- 속도: 드리프트 중 / 부스터 / 순항 ---
        if (isDrifting)
        {
            // 드리프트 중엔 카빙 속도로 (살짝 빠르게)
            curSpeed = Mathf.Lerp(curSpeed, driftSpeed, driftSmooth * Time.deltaTime);
        }
        else
        {
            // 드리프트가 막 끝난 순간 → 부스터 폭발
            if (wasDrifting) curSpeed = boostSpeed;
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
