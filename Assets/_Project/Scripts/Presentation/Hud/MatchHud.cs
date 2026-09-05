using UnityEngine;
using TMPro;
using StreetLegends.Gameplay.Match;

namespace StreetLegends.Presentation.Hud
{
    public class MatchHud : MonoBehaviour
    {
        [SerializeField] private MatchController matchController;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private UnityEngine.UI.Button restartButton;

        private void Start()
        {
            if (matchController != null)
            {
                matchController.StateChanged += OnStateChanged;
                matchController.ScoreChanged += OnScoreChanged;
                matchController.CountdownTick += OnCountdownTick;
                matchController.TimerTick += OnTimerTick;
                matchController.MatchFinished += OnMatchFinished;
            }

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (resultPanel != null)
                resultPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (matchController != null)
            {
                matchController.StateChanged -= OnStateChanged;
                matchController.ScoreChanged -= OnScoreChanged;
                matchController.CountdownTick -= OnCountdownTick;
                matchController.TimerTick -= OnTimerTick;
                matchController.MatchFinished -= OnMatchFinished;
            }
        }

        private void OnStateChanged(MatchState state)
        {
            if (stateText == null) return;

            switch (state)
            {
                case MatchState.Countdown:
                    stateText.text = "GET READY";
                    stateText.gameObject.SetActive(true);
                    break;
                case MatchState.Playing:
                    stateText.text = "";
                    stateText.gameObject.SetActive(false);
                    break;
                case MatchState.GoalScored:
                    stateText.text = "GOAL!";
                    stateText.gameObject.SetActive(true);
                    break;
                case MatchState.Overtime:
                    stateText.text = "OVERTIME";
                    stateText.gameObject.SetActive(true);
                    break;
                case MatchState.Finished:
                    stateText.text = "";
                    stateText.gameObject.SetActive(false);
                    break;
            }
        }

        private void OnScoreChanged(int playerScore, int aiScore)
        {
            if (scoreText != null)
                scoreText.text = $"{playerScore} - {aiScore}";
        }

        private void OnCountdownTick(int secondsRemaining)
        {
            if (stateText != null)
            {
                stateText.text = secondsRemaining > 0 ? secondsRemaining.ToString() : "GO!";
            }
        }

        private void OnTimerTick(float remainingTime)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                timerText.text = $"{minutes}:{seconds:00}";
            }
        }

        private void OnMatchFinished(MatchResult result)
        {
            if (resultPanel != null)
                resultPanel.SetActive(true);

            if (resultText != null)
            {
                resultText.text = result switch
                {
                    MatchResult.PlayerWin => "YOU WIN!",
                    MatchResult.AiWin => "YOU LOSE",
                    MatchResult.Draw => "DRAW",
                    _ => ""
                };
            }
        }

        private void OnRestartClicked()
        {
            if (resultPanel != null)
                resultPanel.SetActive(false);

            matchController?.RestartMatch();
        }
    }
}
