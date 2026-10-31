using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows the right panel for each game state and wires the buttons.</summary>
public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("HUD")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Color timerNormalColor = Color.white;
    [SerializeField] private Color timerWarningColor = new Color(1f, 0.35f, 0.35f);
    [SerializeField] private int warningSeconds = 10;

    [Header("Game Over")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text bestScoreText;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    private int lastShownSeconds = -1;
    private Coroutine punchRoutine;

    private void Awake()
    {
        // Buttons are wired in code, so no manual OnClick setup is needed.
        if (startButton != null) startButton.onClick.AddListener(OnStartButton);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartButton);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuButton);
    }

    private void Start()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("UIManager: no GameManager found in the scene.");
            return;
        }
        gm.StateChanged += HandleStateChanged;
        gm.ScoreChanged += HandleScoreChanged;
        HandleStateChanged(gm.State);
        HandleScoreChanged(gm.Score);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateChanged -= HandleStateChanged;
            GameManager.Instance.ScoreChanged -= HandleScoreChanged;
        }
    }

    private void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing || timerText == null) return;

        int secs = Mathf.CeilToInt(gm.TimeRemaining);
        if (secs != lastShownSeconds)
        {
            lastShownSeconds = secs;
            timerText.text = "Time: " + secs;
            timerText.color = secs <= warningSeconds ? timerWarningColor : timerNormalColor;
        }

        float pulse = secs <= warningSeconds ? 1f + 0.08f * Mathf.Abs(Mathf.Sin(Time.time * 6f)) : 1f;
        timerText.rectTransform.localScale = Vector3.one * pulse;
    }

    // ---------- Button handlers ----------
    public void OnStartButton()
    {
        PlayClick();
        GameManager.Instance.StartGame();
    }

    public void OnRestartButton()
    {
        PlayClick();
        GameManager.Instance.StartGame();
    }

    public void OnMainMenuButton()
    {
        PlayClick();
        GameManager.Instance.ReturnToMenu();
    }

    private static void PlayClick()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
    }

    // ---------- Event handlers ----------
    private void HandleStateChanged(GameState state)
    {
        SetPanel(menuPanel, state == GameState.MainMenu);
        SetPanel(hudPanel, state == GameState.Playing);
        SetPanel(gameOverPanel, state == GameState.GameOver);

        if (state == GameState.Playing)
        {
            lastShownSeconds = -1;
            if (timerText != null)
            {
                timerText.rectTransform.localScale = Vector3.one;
                timerText.color = timerNormalColor;
            }
        }
        else if (state == GameState.GameOver)
        {
            GameManager gm = GameManager.Instance;
            if (finalScoreText != null) finalScoreText.text = "Final Score: " + gm.Score;
            if (bestScoreText != null)
                bestScoreText.text = gm.IsNewBest ? "NEW BEST: " + gm.BestScore + "!" : "Best: " + gm.BestScore;
        }
    }

    private void HandleScoreChanged(int score)
    {
        if (scoreText == null) return;
        scoreText.text = "Score: " + score;

        if (score > 0)
        {
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(Punch(scoreText.rectTransform));
        }
    }

    // ---------- Helpers ----------
    private void SetPanel(GameObject panel, bool show)
    {
        if (panel == null) return;

        if (show && !panel.activeSelf)
        {
            panel.SetActive(true);
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (group != null) StartCoroutine(FadeIn(group));
        }
        else if (!show)
        {
            panel.SetActive(false);
        }
    }

    private IEnumerator FadeIn(CanvasGroup group)
    {
        const float duration = 0.3f;
        group.alpha = 0f;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            group.alpha = t / duration;
            yield return null;
        }
        group.alpha = 1f;
    }

    private IEnumerator Punch(RectTransform target)
    {
        const float duration = 0.18f;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = 1f - t / duration;
            target.localScale = Vector3.one * (1f + 0.35f * k * k);
            yield return null;
        }
        target.localScale = Vector3.one;
    }
}