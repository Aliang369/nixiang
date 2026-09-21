using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterGetDisplay : CellUI<GlobalRewardInfo>, IDisplayModulesController
{
	[SerializeField]
	private Image characterImage;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI characterNameText;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private TextMeshProUGUI soulRewardText;

	[NonSerialized]
	private GameObject obsessionConvertTokenRewardRoot;

	[NonSerialized]
	private TextMeshProUGUI obsessionConvertTokenPrefixText;

	[NonSerialized]
	private Image obsessionConvertTokenIcon;

	[NonSerialized]
	private TextMeshProUGUI obsessionConvertTokenCountText;

	[NonSerialized]
	private bool rewardTextSizeCached;

	[NonSerialized]
	private float rewardTextBaseFontSize;

	[NonSerialized]
	private float rewardTextBaseFontSizeMin;

	[NonSerialized]
	private float rewardTextBaseFontSizeMax;

	private const float RewardTextFontSizeIncrease = 3f;

	private const float ObsessionConvertTokenIconSize = 40f;

	private const float ObsessionConvertTokenRewardWidthAppend = 120f;

	private const float ObsessionConvertTokenRewardHeightAppend = 8f;

	private GlobalRewardInfo reward => base.setupData;

	private PlayerCharacterDef character => reward.refCharacter;

	protected override void Init()
	{
		GetModule<RankModule>().fQuality = () => base.setupData.GlobalItemQuality;
		UIHelper.RegisterEmptyAreaClickedEvent(base.Unset);
		CacheRewardTextSize();
	}

	protected override void Dispose()
	{
		UIHelper.UnregisterEmptyAreaClickedEvent(base.Unset);
	}

	protected override void OnSetup(GlobalRewardInfo _)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_hero_gather);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.show);
		base.gameObject.SetActive(value: true);
		characterImage.sprite = character.fullBody;
		nameText.text = character.characterName;
		characterNameText.text = character.attachClass.characterName;
		icon.sprite = character.attachClass.professionIcon;
		soulRewardText.transform.parent.gameObject.SetActive(value: false);
		soulRewardText.gameObject.SetActive(value: true);
		ApplyRewardTextSize(soulRewardText);
		SetObsessionConvertTokenRewardActive(active: false);
		if (soulRewardText != null)
		{
			GlobalRewardGiveAppendInfo giveAppendInfo = reward.giveAppendInfo;
			if (giveAppendInfo != null && giveAppendInfo.soul > 0)
			{
				ShowSoulRewardText();
				return;
			}
		}
		if (soulRewardText != null)
		{
			GlobalRewardGiveAppendInfo giveAppendInfo2 = reward.giveAppendInfo;
			if (giveAppendInfo2 != null && giveAppendInfo2.convertObsessionToDebris)
			{
				ShowObsessionConvertText();
			}
		}
	}

	private void ShowSoulRewardText()
	{
		SetObsessionConvertTokenRewardActive(active: false);
		soulRewardText.gameObject.SetActive(value: true);
		string str = (reward.giveAppendInfo.isNewGive ? CellUI<GlobalRewardInfo>.textData.term_character_get_first_soul_reward_arg0_count_arg1 : CellUI<GlobalRewardInfo>.textData.term_character_get_repeat_soul_reward_arg0_count_arg1);
		soulRewardText.text = str.FormatWith(TextDisplayHelper.SoulText(character.attachClass.mClass), reward.giveAppendInfo.soul);
		soulRewardText.transform.parent.gameObject.SetActive(value: true);
	}

	private void ShowObsessionConvertText()
	{
		MatterDef storeTokenMatter = ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter;
		int num = reward.giveAppendInfo?.repeatTokenCompensate ?? 0;
		if (num <= 0)
		{
			SetObsessionConvertTokenRewardActive(active: false);
			soulRewardText.gameObject.SetActive(value: true);
			soulRewardText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_gacha_repeat_token_compensate;
			soulRewardText.transform.parent.gameObject.SetActive(value: true);
		}
		else
		{
			EnsureObsessionConvertTokenReward();
			soulRewardText.gameObject.SetActive(value: false);
			obsessionConvertTokenIcon.sprite = storeTokenMatter.icon;
			obsessionConvertTokenPrefixText.text = GetRepeatRewardPrefixText();
			obsessionConvertTokenCountText.text = GetRepeatRewardCountText(num);
			SetObsessionConvertTokenRewardActive(active: true);
			soulRewardText.transform.parent.gameObject.SetActive(value: true);
		}
	}

	private void EnsureObsessionConvertTokenReward()
	{
		if (!(obsessionConvertTokenRewardRoot != null))
		{
			RectTransform rectTransform = soulRewardText.transform as RectTransform;
			GameObject gameObject = new GameObject("ObsessionConvertTokenReward", typeof(RectTransform), typeof(HorizontalLayoutGroup));
			RectTransform rectTransform2 = gameObject.transform as RectTransform;
			rectTransform2.SetParent(soulRewardText.transform.parent, worldPositionStays: false);
			gameObject.layer = soulRewardText.gameObject.layer;
			rectTransform2.anchorMin = rectTransform.anchorMin;
			rectTransform2.anchorMax = rectTransform.anchorMax;
			rectTransform2.pivot = rectTransform.pivot;
			rectTransform2.anchoredPosition = rectTransform.anchoredPosition;
			rectTransform2.sizeDelta = new Vector2(rectTransform.sizeDelta.x + 120f, rectTransform.sizeDelta.y + 8f);
			HorizontalLayoutGroup component = gameObject.GetComponent<HorizontalLayoutGroup>();
			component.childAlignment = (TextAnchor)4;
			component.spacing = 6f;
			component.childControlWidth = true;
			component.childControlHeight = true;
			component.childForceExpandWidth = false;
			component.childForceExpandHeight = false;
			obsessionConvertTokenPrefixText = CreateTokenRewardText(rectTransform2, "PrefixText");
			obsessionConvertTokenIcon = CreateTokenRewardIcon(rectTransform2);
			obsessionConvertTokenCountText = CreateTokenRewardText(rectTransform2, "CountText");
			obsessionConvertTokenRewardRoot = gameObject;
			SetObsessionConvertTokenRewardActive(active: false);
		}
	}

	private TextMeshProUGUI CreateTokenRewardText(Transform parent, string objectName)
	{
		TextMeshProUGUI textMeshProUGUI = UnityEngine.Object.Instantiate(soulRewardText, parent);
		textMeshProUGUI.name = objectName;
		textMeshProUGUI.gameObject.SetActive(value: true);
		textMeshProUGUI.raycastTarget = false;
		ApplyRewardTextSize(textMeshProUGUI);
		return textMeshProUGUI;
	}

	private Image CreateTokenRewardIcon(Transform parent)
	{
		GameObject obj = new GameObject("TokenIcon", typeof(RectTransform), typeof(Image), typeof(LayoutElement))
		{
			layer = soulRewardText.gameObject.layer
		};
		RectTransform obj2 = obj.transform as RectTransform;
		obj2.SetParent(parent, worldPositionStays: false);
		obj2.sizeDelta = new Vector2(40f, 40f);
		LayoutElement component = obj.GetComponent<LayoutElement>();
		component.preferredWidth = 40f;
		component.preferredHeight = 40f;
		component.minWidth = 40f;
		component.minHeight = 40f;
		Image component2 = obj.GetComponent<Image>();
		component2.preserveAspect = true;
		component2.raycastTarget = false;
		return component2;
	}

	private void CacheRewardTextSize()
	{
		if (!rewardTextSizeCached && !(soulRewardText == null))
		{
			rewardTextBaseFontSize = soulRewardText.fontSize;
			rewardTextBaseFontSizeMin = soulRewardText.fontSizeMin;
			rewardTextBaseFontSizeMax = soulRewardText.fontSizeMax;
			rewardTextSizeCached = true;
		}
	}

	private void ApplyRewardTextSize(TextMeshProUGUI text)
	{
		if (!(text == null))
		{
			CacheRewardTextSize();
			text.fontSize = rewardTextBaseFontSize + 3f;
			text.fontSizeMin = rewardTextBaseFontSizeMin + 3f;
			text.fontSizeMax = rewardTextBaseFontSizeMax + 3f;
		}
	}

	private void SetObsessionConvertTokenRewardActive(bool active)
	{
		if (obsessionConvertTokenRewardRoot != null)
		{
			obsessionConvertTokenRewardRoot.SetActive(active);
		}
	}

	private string GetRepeatRewardPrefixText()
	{
		string term_character_get_repeat_soul_reward_arg0_count_arg = CellUI<GlobalRewardInfo>.textData.term_character_get_repeat_soul_reward_arg0_count_arg1;
		int num = term_character_get_repeat_soul_reward_arg0_count_arg.IndexOf("{0}", StringComparison.Ordinal);
		if (num < 0)
		{
			return term_character_get_repeat_soul_reward_arg0_count_arg;
		}
		return term_character_get_repeat_soul_reward_arg0_count_arg.Substring(0, num);
	}

	private string GetRepeatRewardCountText(int count)
	{
		string term_character_get_repeat_soul_reward_arg0_count_arg = CellUI<GlobalRewardInfo>.textData.term_character_get_repeat_soul_reward_arg0_count_arg1;
		int num = term_character_get_repeat_soul_reward_arg0_count_arg.IndexOf("{0}", StringComparison.Ordinal);
		if (num < 0)
		{
			return $"×{count}";
		}
		string text = term_character_get_repeat_soul_reward_arg0_count_arg;
		int num2 = num + 3;
		return text.Substring(num2, text.Length - num2).FormatWith(string.Empty, count);
	}

	protected override void OnUnset()
	{
		SetObsessionConvertTokenRewardActive(active: false);
		((Behaviour)(object)animator).enabled = false;
		base.gameObject.SetActive(value: false);
	}
}
