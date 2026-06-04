using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;   // 어디서든 접근 가능하게

    [Header("UI 연결")]
    public TMP_Text scoreText;            // 화면 위 점수
    public TMP_Text comboText;            // 드리프트 콤보 배율
    public GameObject startPanel;         // 시작 화면
    public GameObject gameOverPanel;      // 게임오버 화면
    public TMP_Text finalScoreText;       // 이번 점수
    public TMP_Text bestScoreText;        // 최고 기록

    [Header("니어미스 (드리프트로 아슬아슬하게 스칠 때)")]
    public float survivalRate = 1f;       // 생존 초당 점수
    public float nearMissBonus = 50f;     // 니어미스 1회 보너스 점수
    public float nearMissFlashTime = 0.6f;// "NEAR MISS!" 표시 시간

    [Header("코인 콤보")]
    public float coinComboWindow = 2f;    // 이 시간 안에 다음 코인 먹으면 콤보 유지
    public int maxCoinCombo = 5;          // 최대 콤보(적립 배수)

    private float score;
    private bool isGameOver;
    private bool hasStarted;
    private static bool skipIntro = false; // 재시작 시 시작화면 건너뛰기

    private float nearMissFlash;
    private int coinCombo;
    private float coinComboTimer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        isGameOver = false;
        score = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (comboText != null) comboText.gameObject.SetActive(false); // 니어미스 때만 보이게

        if (skipIntro)
        {
            // 재시작: 시작화면 없이 바로 플레이
            skipIntro = false;
            BeginPlay();
        }
        else
        {
            // 첫 실행: 시작화면 띄우고 멈춤
            hasStarted = false;
            if (startPanel != null) startPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    void BeginPlay()
    {
        hasStarted = true;
        if (startPanel != null) startPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // 시작 화면 탭 → 게임 시작
    public void StartGame()
    {
        if (!hasStarted) BeginPlay();
    }

    void Update()
    {
        if (!hasStarted || isGameOver) return;

        float dt = Time.deltaTime;

        // 생존 점수
        score += survivalRate * dt;
        if (scoreText != null) scoreText.text = Mathf.FloorToInt(score).ToString();

        // "NEAR MISS!" 팝업 잠깐 보였다 사라짐
        if (nearMissFlash > 0f)
        {
            nearMissFlash -= dt;
            if (nearMissFlash <= 0f && comboText != null)
                comboText.gameObject.SetActive(false);
        }

        // 코인 콤보 시간 경과 → 끊김
        if (coinComboTimer > 0f)
        {
            coinComboTimer -= dt;
            if (coinComboTimer <= 0f) coinCombo = 0;
        }
    }

    // 코인 1개 획득 (콤보로 연속 수집 시 더 많이 적립)
    public void CollectCoin()
    {
        if (!hasStarted || isGameOver) return;

        // 콤보 유지 중이면 +1, 아니면 1부터
        if (coinComboTimer > 0f) coinCombo = Mathf.Min(maxCoinCombo, coinCombo + 1);
        else coinCombo = 1;
        coinComboTimer = coinComboWindow;

        Wallet.Add(coinCombo);  // 연속일수록 더 많이 (1 → 2 → 3 …)

        // 2연속부터 팝업
        if (coinCombo >= 2 && comboText != null)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = "COIN COMBO x" + coinCombo;
            nearMissFlash = nearMissFlashTime;
        }
    }

    // 드리프트로 위협을 아슬아슬하게 스칠 때 호출 (니어미스)
    public void RegisterNearMiss()
    {
        if (!hasStarted || isGameOver) return;
        score += nearMissBonus;

        if (comboText != null)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = "NEAR MISS! +" + Mathf.RoundToInt(nearMissBonus);
            nearMissFlash = nearMissFlashTime;
        }
    }

    // 코인 등으로 점수 추가
    public void AddScore(float amount)
    {
        if (!hasStarted || isGameOver) return;
        score += amount;
    }

    public void GameOver()
    {
        if (isGameOver) return;   // 중복 호출 방지
        isGameOver = true;

        // 피격 타격감: 화면 흔들림 + 폭발음
        if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.35f);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosion();

        // 최고 기록 저장 (로컬)
        int finalScore = Mathf.FloorToInt(score);
        int best = PlayerPrefs.GetInt("BestScore", 0);
        if (finalScore > best)
        {
            best = finalScore;
            PlayerPrefs.SetInt("BestScore", best);
        }

        // 게임오버 화면 표시
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Score: " + finalScore;
        if (bestScoreText != null) bestScoreText.text = "Best: " + best;

        Time.timeScale = 0f;   // 게임 멈춤
    }

    public void Restart()
    {
        skipIntro = true;      // 재시작은 시작화면 건너뜀
        Time.timeScale = 1f;   // 다시 정상 속도
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}