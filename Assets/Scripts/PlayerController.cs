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
    public ParticleSystem[] driftFx;     // 드리프트 중 스파크/스피드라인 (좌/우 날개)

    [Header("비행기 스프라이트 (상태별 뱅킹)")]
    public SpriteRenderer jetSprite;     // 비행기 스프라이트 렌더러
    public Sprite sprStraight;           // 직진
    public Sprite sprLeft;               // 좌선회
    public Sprite sprRight;              // 우선회
    public Sprite sprDriftLeft;          // 드리프트 좌 (없으면 sprLeft 사용)
    public Sprite sprDriftRight;         // 드리프트 우 (없으면 sprRight 사용)

    [Header("모바일 조작 (화면 좌/우 절반)")]
    public HoldButton btnLeft;   // 왼쪽 절반
    public HoldButton btnRight;  // 오른쪽 절반

    private float heading;
    private float bank;
    private float turnInput;
    private bool isDrifting;
    private bool wasDrifting;   // 직전 프레임 드리프트 상태 (종료 감지용)
    private float driftCharge;  // 드리프트+선회 유지 시간 (부스터 충전량)
    private float cooldownTimer;// 부스터 쿨다운 남은 시간
    private float boostFlashTimer; // 부스터 직후 날개 트레일 뿜는 시간

    // 좌/우 버튼 누른 순서 추적 (둘 다 누르면 먼저 누른 쪽으로 드리프트)
    private bool prevLeft, prevRight;
    private float leftDownTime, rightDownTime;

    private float curSpeed;
    private float curTurn;
    private float curBankMax;

    // 카메라 등 외부에서 현재 속도를 읽을 수 있게
    public float CurrentSpeed => curSpeed;
    // 콤보/이펙트에서 드리프트 중인지 읽을 수 있게
    public bool IsDrifting => isDrifting;

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

        // 부스터 직후 잠깐만 양 날개 트레일(선) 그리기 (드리프트 중엔 X)
        if (boostFlashTimer > 0f) boostFlashTimer -= Time.deltaTime;
        bool trailOn = boostFlashTimer > 0f;
        if (driftTrails != null)
        {
            foreach (var t in driftTrails)
            {
                if (t != null) t.emitting = trailOn;
            }
        }

        // 드리프트 중에만 스파크/스피드라인 파티클 (좌/우)
        if (driftFx != null)
        {
            foreach (var fx in driftFx)
            {
                if (fx == null) continue;
                var em = fx.emission;
                em.enabled = isDrifting;
            }
        }

        // 드리프트 지속음
        if (AudioManager.Instance != null) AudioManager.Instance.SetDrift(isDrifting);

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
                    boostFlashTimer = 0.4f; // 부스터 순간 날개 트레일 쫙
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayBoost();
                }
                driftCharge = 0f; // 충전량 리셋
            }
            // 부스터에서 순항으로 서서히 잦아듦
            curSpeed = Mathf.MoveTowards(curSpeed, cruiseSpeed, boostDecel * Time.deltaTime);
        }

        wasDrifting = isDrifting;

        // 선회 적용
        heading += turnInput * curTurn * Time.deltaTime;

        // 뱅킹(롤) 적용 — 단일 스프라이트에선 이 기울기가 뱅킹 연출
        bank = Mathf.Lerp(bank, -turnInput * curBankMax, 8f * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, heading, bank);

        // 상태에 맞는 스프라이트로 교체
        UpdateJetSprite();

        // 전진
        transform.position += transform.forward * curSpeed * Time.deltaTime;
    }

    void UpdateJetSprite()
    {
        if (jetSprite == null) return;

        Sprite s = sprStraight;
        if (turnInput < -0.1f)      s = isDrifting ? (sprDriftLeft  != null ? sprDriftLeft  : sprLeft)  : sprLeft;
        else if (turnInput > 0.1f)  s = isDrifting ? (sprDriftRight != null ? sprDriftRight : sprRight) : sprRight;

        if (s != null) jetSprite.sprite = s;
    }

    void HandleInput()
    {
        turnInput = 0f;
        isDrifting = false;

        // 키보드(에디터) + 모바일 좌/우 절반 버튼을 하나로 통합
        var kb = Keyboard.current;
        bool kl = kb != null && kb.leftArrowKey.isPressed;
        bool kr = kb != null && kb.rightArrowKey.isPressed;
        bool l = kl || (btnLeft != null && btnLeft.IsHeld);
        bool r = kr || (btnRight != null && btnRight.IsHeld);

        // 각 버튼을 '새로 누른' 시각 기록 (순서 판별용)
        if (l && !prevLeft)  leftDownTime  = Time.time;
        if (r && !prevRight) rightDownTime = Time.time;
        prevLeft = l;
        prevRight = r;

        if (l && r)
        {
            // 둘 다 누름 → 먼저 누른 방향으로 드리프트
            isDrifting = true;
            turnInput = (leftDownTime <= rightDownTime) ? -1f : 1f;
        }
        else if (l)
        {
            turnInput = -1f;
        }
        else if (r)
        {
            turnInput = 1f;
        }
    }
}
