using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score")]
    [SerializeField] private float scorePerSecond = 10f;

    [Header("Difficulty Scaling")]
    [SerializeField] private float speedIncreaseEvery = 20f;   // every 20 score
    [SerializeField] private float speedIncreaseAmount = 0.15f;
    [SerializeField] private float maxSpeedMultiplier = 3f;

    private float scoreFloat = 0f;

    public bool IsGameOver { get; private set; }

    public int Score => Mathf.FloorToInt(scoreFloat);

    public float SpeedMultiplier
    {
        get
        {
            float steps = Mathf.Floor(scoreFloat / speedIncreaseEvery);
            float mult = 1f + steps * speedIncreaseAmount;
            return Mathf.Min(mult, maxSpeedMultiplier);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (IsGameOver)
            return;

        scoreFloat += scorePerSecond * Time.deltaTime;
    }

    public void GameOver()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnGUI()
    {
        GUIStyle scoreStyle = new GUIStyle(GUI.skin.label);
        scoreStyle.fontSize = 24;
        scoreStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(20, 20, 300, 40), "Score: " + Score, scoreStyle);

        if (!IsGameOver)
            return;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 36;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.red;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 24;

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        GUI.Label(
            new Rect(centerX - 150f, centerY - 80f, 300f, 50f),
            "GAME OVER",
            titleStyle
        );

        if (GUI.Button(
            new Rect(centerX - 100f, centerY, 200f, 50f),
            "Play Again",
            buttonStyle))
        {
            RestartGame();
        }
    }
}