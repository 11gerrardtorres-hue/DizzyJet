using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;   // 어디서든 접근 가능하게

    [Header("UI 연결")]
    public TMP_Text scoreText;            // 화면 위 점수
    public GameObject startPanel;         // 시작 화면
    public GameObject gameOverPanel;      // 게임오버 화면
    public TMP_Text finalScoreText;       // 이번 점수
    public TMP_Text bestScoreText;        // 최고 기록

    private float score;
    private bool isGameOver;
    private bool hasStarted;
    private static bool skipIntro = false; // 재시작 시 시작화면 건너뛰기

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        isGameOver = false;
        score = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

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

        // 생존 시간 = 점수
        score += Time.deltaTime;
        if (scoreText != null)
            scoreText.text = Mathf.FloorToInt(score).ToString();
    }

    public void GameOver()
    {
        if (isGameOver) return;   // 중복 호출 방지
        isGameOver = true;

        // 피격 타격감: 화면 흔들림
        if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.35f);

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