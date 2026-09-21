using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BannerHintDisplay : UIBase
{
	public enum BackgroundType
	{
		Default,
		BattleStart,
		TurnStart,
		TurnEnd
	}

	public enum TextType
	{
		Default,
		TurnStart
	}

	public enum SoundType
	{
		None,
		PlayerTurnStart,
		EnemyTurnStart
	}

	public class SetupInfo
	{
		public BackgroundType backgroundType;

		public SoundType soundType;

		public string hintText;

		public float stayTime = 0.8f;

		public string subText;

		public TextType textType;

		public float tweenTime = 0.25f;
	}

	[SerializeField]
	private TextMeshProUGUI text_Default;

	[SerializeField]
	private RectTransform textPanel_Default;

	[SerializeField]
	private TextMeshProUGUI text_StartTurn;

	[SerializeField]
	private TextMeshProUGUI text_StartTurnSubText;

	[SerializeField]
	private RectTransform textPanel_StartTurn;

	[SerializeField]
	private Image backgrounds_Default;

	[SerializeField]
	private Image backgrounds_BattleStart;

	[SerializeField]
	private Image backgrounds_TurnStart;

	[SerializeField]
	private Image backgrounds_TurnEnd;

	[NonSerialized]
	private Sequence sequence;

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
		backgrounds_Default.gameObject.SetActive(value: false);
		backgrounds_BattleStart.gameObject.SetActive(value: false);
		backgrounds_TurnStart.gameObject.SetActive(value: false);
		backgrounds_TurnEnd.gameObject.SetActive(value: false);
		textPanel_Default.gameObject.SetActive(value: false);
		textPanel_StartTurn.gameObject.SetActive(value: false);
	}

	public IEnumerator ShowBannerHint(SetupInfo info, float scale)
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.timeScale = scale;
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		SetImage(info.backgroundType);
		SetText(info);
		base.gameObject.SetActive(value: true);
		base.transform.localScale = new Vector3(1f, 0f, 1f);
		sequence.AppendCallback(delegate
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			switch (info.soundType)
			{
			case SoundType.PlayerTurnStart:
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_turn_banner_player);
				break;
			case SoundType.EnemyTurnStart:
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_turn_banner_enemy);
				break;
			case SoundType.None:
				break;
			}
		});
		sequence.Append(base.transform.DOScale(new Vector3(1f, 0f, 1f), 0f));
		sequence.Append(base.transform.DOScale(new Vector3(1f, 1f, 1f), info.tweenTime));
		sequence.AppendInterval(info.stayTime);
		sequence.Append(base.transform.DOScale(new Vector3(1f, 0f, 1f), info.tweenTime));
		sequence.AppendCallback(delegate
		{
			sequence = null;
			base.gameObject.SetActive(value: false);
		});
		sequence.Play();
		if (UIBase.applicationState.settingManager.uiTimeScaleTypeSetting == TimeScaleManager.TimeScaleType.X1)
		{
			yield return sequence.WaitForKill();
		}
	}

	private void SetImage(BackgroundType type)
	{
		backgrounds_Default.gameObject.SetActive(value: false);
		backgrounds_BattleStart.gameObject.SetActive(value: false);
		backgrounds_TurnStart.gameObject.SetActive(value: false);
		backgrounds_TurnEnd.gameObject.SetActive(value: false);
		switch (type)
		{
		case BackgroundType.Default:
			backgrounds_Default.gameObject.SetActive(value: true);
			break;
		case BackgroundType.BattleStart:
			backgrounds_BattleStart.gameObject.SetActive(value: true);
			break;
		case BackgroundType.TurnStart:
			backgrounds_TurnStart.gameObject.SetActive(value: true);
			break;
		case BackgroundType.TurnEnd:
			backgrounds_TurnEnd.gameObject.SetActive(value: true);
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	private void SetText(SetupInfo info)
	{
		textPanel_Default.gameObject.SetActive(value: false);
		textPanel_StartTurn.gameObject.SetActive(value: false);
		switch (info.textType)
		{
		case TextType.Default:
			text_Default.text = info.hintText;
			textPanel_Default.gameObject.SetActive(value: true);
			break;
		case TextType.TurnStart:
			text_StartTurn.text = info.hintText;
			text_StartTurnSubText.text = info.subText;
			textPanel_StartTurn.gameObject.SetActive(value: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
