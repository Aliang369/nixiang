using TMPro;
using UnityEngine;

public sealed class DailyChallengeEntryView : MonoBehaviour
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private TextMeshProUGUI label;

	public void Setup(StarMapPanel host)
	{
		ApplyLocalizedText();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(host.OpenDailyChallenge);
	}

	private void ApplyLocalizedText()
	{
		label.text = ScriptableObjectSingleton<TextDataPreset>.instance?.term_daily_challenge_title ?? string.Empty;
	}
}
