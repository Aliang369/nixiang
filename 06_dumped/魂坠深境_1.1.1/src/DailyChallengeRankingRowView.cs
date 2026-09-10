using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DailyChallengeRankingRowView : MonoBehaviour
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Image rankBadge;

	[SerializeField]
	private TextMeshProUGUI rank;

	[SerializeField]
	private Image avatar;

	[SerializeField]
	private DailyChallengeMarqueeText playerName;

	[SerializeField]
	private DailyChallengeMarqueeText characterName;

	[SerializeField]
	private DailyChallengeMarqueeText difficulty;

	[SerializeField]
	private DailyChallengeMarqueeText score;

	[SerializeField]
	private Color firstPlaceColor = new Color(0.35f, 0.29f, 0.17f, 0.96f);

	[SerializeField]
	private Color secondPlaceColor = new Color(0.16f, 0.22f, 0.28f, 0.96f);

	[SerializeField]
	private Color thirdPlaceColor = new Color(0.28f, 0.2f, 0.15f, 0.96f);

	[SerializeField]
	private Color regularColor = new Color(0.1f, 0.11f, 0.12f, 0.94f);

	[SerializeField]
	private Color selfColor = new Color(0.13f, 0.14f, 0.15f, 0.98f);

	private Action<long, string> openRecord;

	public long PlayerId { get; private set; }

	public string RecordId { get; private set; }

	public bool IsInteractable => button.interactable;

	public void Initialize(Action<long, string> onOpenRecord)
	{
		openRecord = onOpenRecord;
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(delegate
		{
			openRecord?.Invoke(PlayerId, RecordId);
		});
	}

	public void Set(DailyChallengeRankingRow row, Sprite defaultAvatar, bool isSelf = false)
	{
		base.gameObject.SetActive(row != null);
		if (row == null)
		{
			PlayerId = 0L;
			RecordId = null;
			return;
		}
		EnsureAvatarBinding();
		PlayerId = row.playerId;
		RecordId = (row.hasRecord ? row.recordId : null);
		rank.fontSize = 29f;
		rank.text = row.rank.ToString();
		avatar.sprite = ((row.avatar != null) ? row.avatar : defaultAvatar);
		avatar.color = ((avatar.sprite == null) ? new Color(0.42f, 0.44f, 0.48f, 1f) : Color.white);
		playerName.SetText(row.playerName);
		characterName.SetText(row.characterName);
		difficulty.SetText(row.difficultyName);
		score.SetText(row.score.ToString());
		button.interactable = row.hasRecord;
		ApplyStyle(row.rank, isSelf);
	}

	public void SetUnchallenged(Sprite defaultAvatar, string currentPlayerName)
	{
		base.gameObject.SetActive(value: true);
		EnsureAvatarBinding();
		PlayerId = 0L;
		RecordId = null;
		rank.fontSize = 20f;
		rank.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_challenge_unchallenged;
		avatar.sprite = defaultAvatar;
		avatar.color = ((avatar.sprite == null) ? new Color(0.42f, 0.44f, 0.48f, 1f) : Color.white);
		playerName.SetText(currentPlayerName);
		characterName.SetText("--");
		difficulty.SetText("--");
		score.SetText("--");
		button.interactable = false;
		ApplyStyle(0, isSelf: true);
	}

	private void EnsureAvatarBinding()
	{
		if (avatar != null)
		{
			return;
		}
		Transform transform = base.transform.Find("AvatarFrame/AvatarMask/Avatar");
		if (transform == null)
		{
			Image[] componentsInChildren = GetComponentsInChildren<Image>(includeInactive: true);
			foreach (Image image in componentsInChildren)
			{
				if (image.gameObject.name == "Avatar")
				{
					transform = image.transform;
					break;
				}
			}
		}
		avatar = ((transform != null) ? transform.GetComponent<Image>() : null);
		if (!(avatar == null))
		{
			return;
		}
		throw new InvalidOperationException(base.name + " is missing an Avatar Image under AvatarFrame/AvatarMask.");
	}

	private void ApplyStyle(int place, bool isSelf)
	{
		Color color = place switch
		{
			1 => new Color(1f, 0.86f, 0.48f, 1f), 
			2 => new Color(0.72f, 0.86f, 0.95f, 1f), 
			3 => new Color(0.92f, 0.65f, 0.4f, 1f), 
			_ => Color.white, 
		};
		Image image = background;
		Color color2 = ((!isSelf) ? (place switch
		{
			1 => firstPlaceColor, 
			2 => secondPlaceColor, 
			3 => thirdPlaceColor, 
			_ => regularColor, 
		}) : selfColor);
		image.color = color2;
		rankBadge.gameObject.SetActive(!isSelf && place >= 1 && place <= 3);
		rankBadge.color = color;
		rank.color = color;
	}
}
