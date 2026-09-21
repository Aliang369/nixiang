using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class ChallengeEntrySlotView : MonoBehaviour
{
	[SerializeField]
	private Image artwork;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[FormerlySerializedAs("countdownText")]
	[SerializeField]
	private TextMeshProUGUI timeText;

	[SerializeField]
	private GameObject lockedMask;

	[SerializeField]
	private GameObject lockIcon;

	[SerializeField]
	private GameButton button;

	[SerializeField]
	private Color lockedArtworkColor = new Color(0.68f, 0.72f, 0.82f, 0.62f);

	public Image Artwork => artwork;

	public TextMeshProUGUI TitleText => titleText;

	public TextMeshProUGUI TimeText => timeText;

	public TextMeshProUGUI CountdownText => timeText;

	public GameObject LockedMask => lockedMask;

	public GameObject LockIcon => lockIcon;

	public GameButton Button => button;

	public void Bind(ChallengeEntryRuntimeState state, UnityAction onClick)
	{
		base.gameObject.SetActive(state?.visible ?? false);
		if (state != null && state.visible)
		{
			artwork.sprite = state.artwork;
			artwork.color = (state.unlocked ? Color.white : lockedArtworkColor);
			titleText.text = state.title ?? string.Empty;
			timeText.text = state.timeText ?? string.Empty;
			timeText.gameObject.SetActive(state.unlocked && !string.IsNullOrEmpty(state.timeText));
			lockedMask.SetActive(!state.unlocked);
			lockIcon.SetActive(!state.unlocked);
			button.onClick.RemoveAllListeners();
			button.interactable = state.unlocked && onClick != null;
			if (button.interactable)
			{
				button.onClick.AddListener(onClick);
			}
		}
	}
}
