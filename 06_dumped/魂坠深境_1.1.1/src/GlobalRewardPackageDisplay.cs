using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalRewardPackageDisplay : UIBase
{
	private class DisplayInfo
	{
		public GlobalRewardInfo globalRewardInfo;

		public bool displayCell;

		public bool displayCharacterGet;

		public GlobalItemDisplay globalItemDisplay;
	}

	public TextMeshProUGUI rewardTitle;

	public TextMeshProUGUI rewardHint;

	public CanvasGroup canvasGroup;

	public GameButton closeButton;

	public MultiInfoDisplayPanel multiInfoDisplayPanel;

	public GameObject tokenCompenstateArea;

	public TextMeshProUGUI tokenCompenstateText;

	public MatterCell tokenCompenstateMatterCell;

	public GlobalItemDisplay globalItemDisplay;

	public Sprite obsessionConvertArrowSprite;

	public SkeletonGraphic anim;

	public GachaAnimPlayer gachaAnimPlayer;

	public ScrollRect scrollRect;

	[NonSerialized]
	private GlobalRewardPackage reward;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> pool;

	[NonSerialized]
	private bool showAll;

	[NonSerialized]
	private bool showEnd;

	[NonSerialized]
	private bool isPlaying;

	[NonSerialized]
	private bool displayingItemSkinGet;

	[NonSerialized]
	private string packageTitleText;

	[NonSerialized]
	private GridLayoutGroup rewardGridLayout;

	[NonSerialized]
	private RectTransform rewardContentRect;

	[NonSerialized]
	private float rewardContentHeightBeforeConvert;

	[NonSerialized]
	private bool rewardGridLayoutEnabledBeforeConvert;

	[NonSerialized]
	private bool rewardGridLayoutDisabledByConvert;

	private readonly List<GameObject> obsessionConvertRows = new List<GameObject>();

	private readonly List<GlobalItemDisplay> obsessionConvertDisplays = new List<GlobalItemDisplay>();

	private const float ObsessionConvertRowWidth = 670f;

	private const float ObsessionConvertRowHeight = 190f;

	private const float ObsessionConvertRowSpacing = 30f;

	private const float ObsessionConvertTopPadding = 190f;

	private const float ObsessionConvertBottomPadding = 200f;

	private const float ObsessionConvertTitleReservedHeight = 160f;

	private Sequence sequence;

	private List<ProfileSkill> rewardSkills => reward.rewards.FindAll((GlobalRewardInfo r) => r.skill != null).ConvertAll((GlobalRewardInfo r) => r.skill);

	private List<MatterInfo> rewardMatters => reward.rewards.FindAll((GlobalRewardInfo r) => r.matter != null).ConvertAll((GlobalRewardInfo r) => r.matter);

	private List<PlayerCharacterDef> rewardCharacters => reward.rewards.FindAll((GlobalRewardInfo r) => r.refCharacter != null).ConvertAll((GlobalRewardInfo r) => r.refCharacter);

	private List<ItemSkinDef> rewardItemSkins => reward.rewards.FindAll((GlobalRewardInfo r) => r.unlockItemSkin != null).ConvertAll((GlobalRewardInfo r) => r.unlockItemSkin);

	private List<StartGameBuff> rewardStartGameBuffs => reward.rewards.FindAll((GlobalRewardInfo r) => r.getStartGameBuff != null).ConvertAll((GlobalRewardInfo r) => r.getStartGameBuff);

	private List<CTScriptableObject> unlockCTSOs => reward.rewards.FindAll((GlobalRewardInfo r) => r.unlockCTSOs != null).SelectMany((GlobalRewardInfo r) => r.unlockCTSOs).ToList();

	protected override void Init()
	{
		InitUI(multiInfoDisplayPanel);
		InitUI(tokenCompenstateMatterCell);
		pool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(globalItemDisplay, globalItemDisplay.transform.parent);
		rewardGridLayout = globalItemDisplay.transform.parent.GetComponent<GridLayoutGroup>();
		rewardContentRect = globalItemDisplay.transform.parent as RectTransform;
		gachaAnimPlayer = new GachaAnimPlayer();
		closeButton.onClick.AddListener(_OnCloseButtonClicked);
	}

	public IEnumerator ShowNestedRewardPackSeperateCor(GlobalRewardPackage pack)
	{
		if (pack == null || !CanDisplayRewardPackage(pack))
		{
			yield break;
		}
		if (pack.IsObsessionConvertPackage)
		{
			Setup(pack);
			yield return new WaitUntil(() => !base.gameObject.activeInHierarchy && !isPlaying);
			yield return new WaitForSeconds(0.1f);
			yield break;
		}
		List<GlobalRewardPackage> list = pack.SeparateRewardPackageByNestedType();
		foreach (GlobalRewardPackage item in list)
		{
			Setup(item);
			yield return new WaitUntil(() => !base.gameObject.activeInHierarchy && !isPlaying);
			yield return new WaitForSeconds(0.1f);
		}
		if (UIBase.fortressState != null)
		{
			UIBase.fortressState.uiManager.SetDirty();
		}
	}

	private IEnumerator WaitForSecondsOrCondition(float secondsToWait, Func<bool> condition)
	{
		float timeElapsed = 0f;
		while (timeElapsed < secondsToWait && !condition())
		{
			timeElapsed += Time.deltaTime;
			yield return null;
		}
	}

	public void Setup(GlobalRewardPackage reward)
	{
		reward.rewards = (reward.rewards.IsNullOrEmpty() ? new List<GlobalRewardInfo>() : GlobalRewardInfo.UnfoldNestedReward(reward.rewards));
		UIBase.fortressState?.uiManager.universalCommonInfoPanel.HideAll();
		this.reward = reward;
		if (CanDisplayRewardPackage(reward))
		{
			if (reward.IsObsessionConvertPackage)
			{
				packageTitleText = reward.title;
			}
			else if (reward.rewards.Any((GlobalRewardInfo r) => r.unlockCTSOs != null))
			{
				packageTitleText = base.textData.term_unlock;
			}
			else
			{
				packageTitleText = base.textData.term_get_globalItem;
			}
			rewardTitle.text = packageTitleText;
			rewardHint.text = reward.hint;
			canvasGroup.alpha = 0f;
			DOTweenModuleUI.DOFade(canvasGroup, 1f, 0.25f);
			base.gameObject.SetActive(value: true);
			pool.DespawnAll();
			ClearObsessionConvertRows();
			SetRewardGridLayoutActive(active: true);
			multiInfoDisplayPanel.gameObject.SetActive(value: false);
			tokenCompenstateArea.gameObject.SetActive(value: false);
			showAll = false;
			displayingItemSkinGet = false;
			scrollRect.verticalNormalizedPosition = 1f;
			showEnd = false;
			Bootstrap.instance.StartCoroutine(Anim());
		}
		IEnumerator Anim()
		{
			isPlaying = true;
			if (reward.IsObsessionConvertPackage)
			{
				((Component)(object)anim).gameObject.SetActive(value: false);
				yield return ShowObsessionConvertItems();
			}
			else if (reward.nestedType == GlobalRewardInfo.RewardNestedType.GachaDraw)
			{
				yield return DisplayGachaAnim(reward.rewards);
			}
			else if (reward.nestedType == GlobalRewardInfo.RewardNestedType.RandomChest)
			{
				yield return PlayRandomChestAnim(reward.qualityLevel);
			}
			if (!reward.IsObsessionConvertPackage)
			{
				((Component)(object)anim).gameObject.SetActive(value: false);
				yield return ShowItems();
			}
			isPlaying = false;
			showEnd = true;
		}
		IEnumerator DisplayGachaAnim(List<GlobalRewardInfo> rewards)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gacha_draw);
			yield return gachaAnimPlayer.DisplayGachaAnim(rewards, null, null);
		}
		static IEnumerator ShowCharacterGetAnim(DisplayInfo displayInfo)
		{
			if (UIBase.fortressState != null)
			{
				GlobalRewardInfo globalRewardInfo = displayInfo.globalRewardInfo;
				if (displayInfo.displayCharacterGet)
				{
					UIBase.fortressState.uiManager.characterGetDisplay.Setup(globalRewardInfo);
					yield return new WaitUntil(() => !UIBase.fortressState.uiManager.characterGetDisplay.gameObject.activeInHierarchy);
				}
			}
		}
		IEnumerator ShowItemSkinGetAnim(DisplayInfo displayInfo)
		{
			ItemSkinDef unlockItemSkin = displayInfo.globalRewardInfo.unlockItemSkin;
			if (!(unlockItemSkin == null))
			{
				displayingItemSkinGet = true;
				rewardTitle.text = base.textData.term_get + base.textData.term_cardskin;
				multiInfoDisplayPanel.Setup(new MultiInfoDisplayPanel.SetupInfo
				{
					items = new List<Item> { unlockItemSkin.GenerateSkinedItem() }
				});
				SetupItemSkinGetPreviewLinks(unlockItemSkin);
				yield return new WaitUntil(() => !multiInfoDisplayPanel.gameObject.activeInHierarchy);
				rewardTitle.text = packageTitleText;
				displayingItemSkinGet = false;
			}
		}
		IEnumerator ShowItems()
		{
			pool.DespawnAll();
			List<DisplayInfo> displayInfos = new List<DisplayInfo>();
			foreach (GlobalRewardInfo reward2 in reward.rewards)
			{
				DisplayInfo displayInfo = new DisplayInfo
				{
					globalRewardInfo = reward2
				};
				displayInfos.Add(displayInfo);
				if (!reward2.isHidden)
				{
					displayInfo.displayCell = true;
				}
				if (reward2.RewardType == GlobalRewardInfo.GlobalRewardType.MatterReward && reward2.matter.count == 0)
				{
					displayInfo.displayCell = false;
				}
				if (reward2.RewardType == GlobalRewardInfo.GlobalRewardType.MatterReward && reward2.matter.matterDef.matterType == MatterDef.MatterType.Obsession && (reward2.giveAppendInfo?.convertObsessionToUnlockCharacter ?? false))
				{
					displayInfos.Add(new DisplayInfo
					{
						globalRewardInfo = new GlobalRewardInfo
						{
							matter = new MatterInfo(reward2.matter.matterDef, 1),
							giveAppendInfo = reward2.giveAppendInfo
						},
						displayCell = false,
						displayCharacterGet = true
					});
				}
				if (reward2.RewardType == GlobalRewardInfo.GlobalRewardType.UnlockCTSO)
				{
					displayInfo.displayCell = false;
				}
				if (reward2.RewardType == GlobalRewardInfo.GlobalRewardType.UnlockItemSkin)
				{
					displayInfo.displayCell = false;
				}
				if (reward2.matter != null && reward2.matter.matterDef.matterType == MatterDef.MatterType.Obsession && reward2.matter.count > 0)
				{
					displayInfo.displayCharacterGet = true;
				}
			}
			if (reward.SumItemSkinTokenCompensate > 0)
			{
				displayInfos.Add(new DisplayInfo
				{
					globalRewardInfo = new GlobalRewardInfo
					{
						matter = new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.itemSkinTokenMatter, reward.SumItemSkinTokenCompensate)
					},
					displayCell = true
				});
			}
			foreach (DisplayInfo item in displayInfos)
			{
				if (item.displayCell)
				{
					item.globalItemDisplay = pool.Spawn(item.globalRewardInfo);
				}
			}
			foreach (GlobalItemDisplay activeElement in pool.GetActiveElements())
			{
				activeElement.GetComponent<CanvasGroup>().alpha = 0f;
			}
			foreach (DisplayInfo displayInfo2 in displayInfos)
			{
				yield return ShowCharacterGetAnim(displayInfo2);
				yield return ShowItemSkinGetAnim(displayInfo2);
				if (displayInfo2.globalItemDisplay != null)
				{
					Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_item_acquire);
					if (!showAll)
					{
						yield return new WaitForSeconds(0.2f);
					}
					displayInfo2.globalItemDisplay.GetComponent<CanvasGroup>().alpha = 1f;
				}
			}
			if (!unlockCTSOs.IsNullOrEmpty())
			{
				MultiInfoDisplayPanel.SetupInfo setupInfo = new MultiInfoDisplayPanel.SetupInfo
				{
					items = new List<Item>(),
					runes = new List<(Rune, Item, int)>(),
					skills = new List<GameSkill>(),
					influences = new List<Influence>()
				};
				foreach (CTScriptableObject unlockCTSO in unlockCTSOs)
				{
					if (unlockCTSO is InfluenceDef def)
					{
						setupInfo.influences.Add(new Influence(def));
					}
					else if (unlockCTSO is ItemDefinition def2)
					{
						setupInfo.items.Add(new Item(def2));
					}
					else if (unlockCTSO is RuneDef runeDef)
					{
						setupInfo.runes.Add((new Rune(runeDef), null, runeDef.attachGenerationInfo.stack));
					}
					else if (unlockCTSO is SkillDef def3)
					{
						setupInfo.skills.Add(new GameSkill(new ProfileSkill(def3, 0)));
					}
				}
				multiInfoDisplayPanel.SetActiveAndDirty(active: true);
				multiInfoDisplayPanel.Setup(setupInfo);
			}
			else
			{
				multiInfoDisplayPanel.gameObject.SetActive(value: false);
			}
			if (reward.SumTokenCompensate > 0)
			{
				tokenCompenstateArea.gameObject.SetActive(value: true);
				tokenCompenstateMatterCell.Setup(new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter, reward.SumTokenCompensate));
				tokenCompenstateText.text = (reward.WithAnyRepeatReward ? ScriptableObjectSingleton<TextDataPreset>.instance.term_gacha_repeat_token_compensate : ScriptableObjectSingleton<TextDataPreset>.instance.term_gacha_extra_token_compensate);
			}
			else
			{
				tokenCompenstateArea.gameObject.SetActive(value: false);
			}
			if (displayInfos.Count((DisplayInfo i) => i.displayCell) == 0 && !multiInfoDisplayPanel.gameObject.activeSelf && reward.SumTokenCompensate <= 0)
			{
				Hide();
			}
			ForceUpdate();
			showAll = true;
		}
		IEnumerator ShowObsessionConvertItems()
		{
			pool.DespawnAll();
			ClearObsessionConvertRows();
			SetRewardGridLayoutActive(active: false);
			multiInfoDisplayPanel.gameObject.SetActive(value: false);
			foreach (GlobalRewardPackage.ObsessionConvertInfo obsessionConvertInfo in reward.obsessionConvertInfos)
			{
				CreateObsessionConvertRow(obsessionConvertInfo);
			}
			LayoutObsessionConvertRows();
			foreach (GameObject obsessionConvertRow in obsessionConvertRows)
			{
				obsessionConvertRow.GetOrAddComponent<CanvasGroup>().alpha = 0f;
			}
			foreach (GameObject row in obsessionConvertRows)
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_item_acquire);
				if (!showAll)
				{
					yield return new WaitForSeconds(0.2f);
				}
				row.GetOrAddComponent<CanvasGroup>().alpha = 1f;
			}
			tokenCompenstateArea.gameObject.SetActive(value: false);
			ForceUpdate();
			showAll = true;
		}
	}

	private void SetupItemSkinGetPreviewLinks(ItemSkinDef itemSkin)
	{
		MainItemInfoDisplayBase mainItemInfoDisplayBase = multiInfoDisplayPanel.itemInfoDisplayPool.GetActiveElements().FirstOrDefault()?.mainItemInfoDisplay;
		if (!(mainItemInfoDisplayBase == null) && !(itemSkin?.attachItem == null))
		{
			ViewUpgradeButtonPreviewSetup viewUpgradeButtonPreviewSetup = ViewUpgradeButtonPreviewSetup.CreateItemSkinPreviewSetup(itemSkin, UIBase.applicationState);
			if (viewUpgradeButtonPreviewSetup != null)
			{
				mainItemInfoDisplayBase.viewUpgradeButtonPreviewSetup = viewUpgradeButtonPreviewSetup;
				mainItemInfoDisplayBase.SetDirty();
				mainItemInfoDisplayBase.ForceUpdate();
			}
		}
	}

	private void CreateObsessionConvertRow(GlobalRewardPackage.ObsessionConvertInfo convertInfo)
	{
		if (convertInfo?.fromReward != null && convertInfo.toReward != null)
		{
			GameObject gameObject = new GameObject("ObsessionConvertRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
			RectTransform rectTransform = gameObject.transform as RectTransform;
			rectTransform.SetParent(globalItemDisplay.transform.parent, worldPositionStays: false);
			rectTransform.localScale = Vector3.one;
			rectTransform.SetAsLastSibling();
			int count = obsessionConvertRows.Count;
			rectTransform.anchorMin = new Vector2(0.5f, 1f);
			rectTransform.anchorMax = new Vector2(0.5f, 1f);
			rectTransform.pivot = new Vector2(0.5f, 1f);
			rectTransform.sizeDelta = new Vector2(670f, 190f);
			rectTransform.anchoredPosition = GetObsessionConvertRowPosition(count, 1);
			HorizontalLayoutGroup component = gameObject.GetComponent<HorizontalLayoutGroup>();
			component.childAlignment = (TextAnchor)4;
			component.spacing = 42f;
			component.childControlWidth = true;
			component.childControlHeight = true;
			component.childForceExpandWidth = false;
			component.childForceExpandHeight = false;
			LayoutElement component2 = gameObject.GetComponent<LayoutElement>();
			component2.preferredWidth = 670f;
			component2.minWidth = 670f;
			component2.preferredHeight = 190f;
			component2.minHeight = 190f;
			CreateObsessionConvertSlot(rectTransform, convertInfo.fromReward);
			CreateObsessionConvertArrow(rectTransform);
			CreateObsessionConvertSlot(rectTransform, convertInfo.toReward);
			obsessionConvertRows.Add(gameObject);
			UpdateObsessionConvertContentHeight();
		}
	}

	private void CreateObsessionConvertSlot(Transform parent, GlobalRewardInfo rewardInfo)
	{
		GameObject obj = new GameObject("RewardSlot", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
		RectTransform rectTransform = obj.transform as RectTransform;
		rectTransform.SetParent(parent, worldPositionStays: false);
		rectTransform.localScale = Vector3.one;
		VerticalLayoutGroup component = obj.GetComponent<VerticalLayoutGroup>();
		component.childAlignment = (TextAnchor)4;
		component.spacing = 8f;
		component.childControlWidth = true;
		component.childControlHeight = true;
		component.childForceExpandWidth = false;
		component.childForceExpandHeight = false;
		LayoutElement component2 = obj.GetComponent<LayoutElement>();
		component2.preferredWidth = 240f;
		component2.minWidth = 240f;
		component2.preferredHeight = 180f;
		GlobalItemDisplay globalItemDisplay = UnityEngine.Object.Instantiate(this.globalItemDisplay, rectTransform);
		InitUI(globalItemDisplay);
		globalItemDisplay.gameObject.SetActive(value: true);
		globalItemDisplay.Setup(rewardInfo);
		LayoutElement orAddComponent = globalItemDisplay.gameObject.GetOrAddComponent<LayoutElement>();
		orAddComponent.preferredWidth = 110f;
		orAddComponent.preferredHeight = 110f;
		orAddComponent.minWidth = 110f;
		orAddComponent.minHeight = 110f;
		obsessionConvertDisplays.Add(globalItemDisplay);
	}

	private void CreateObsessionConvertArrow(Transform parent)
	{
		GameObject gameObject = new GameObject("ConvertArrow", typeof(RectTransform));
		RectTransform obj = gameObject.transform as RectTransform;
		obj.SetParent(parent, worldPositionStays: false);
		obj.localScale = Vector3.one;
		LayoutElement layoutElement = gameObject.AddComponent<LayoutElement>();
		layoutElement.preferredWidth = 110f;
		layoutElement.preferredHeight = 85f;
		layoutElement.minWidth = 110f;
		layoutElement.minHeight = 85f;
		if (obsessionConvertArrowSprite != null)
		{
			Image image = gameObject.AddComponent<Image>();
			image.sprite = obsessionConvertArrowSprite;
			image.color = new Color(1f, 1f, 1f, 0.82f);
			image.preserveAspect = true;
			image.raycastTarget = false;
		}
		else
		{
			TextMeshProUGUI textMeshProUGUI = gameObject.AddComponent<TextMeshProUGUI>();
			textMeshProUGUI.font = rewardHint.font;
			textMeshProUGUI.fontSharedMaterial = rewardHint.fontSharedMaterial;
			textMeshProUGUI.fontSize = 44f;
			textMeshProUGUI.color = Color.white;
			textMeshProUGUI.alignment = TextAlignmentOptions.Center;
			textMeshProUGUI.raycastTarget = false;
			textMeshProUGUI.text = ">";
		}
	}

	private void LayoutObsessionConvertRows()
	{
		int count = obsessionConvertRows.Count;
		for (int i = 0; i < count; i++)
		{
			(obsessionConvertRows[i].transform as RectTransform).anchoredPosition = GetObsessionConvertRowPosition(i, count);
		}
		UpdateObsessionConvertContentHeight();
	}

	private Vector2 GetObsessionConvertRowPosition(int rowIndex, int rowCount)
	{
		float obsessionConvertRowsHeight = GetObsessionConvertRowsHeight(rowCount);
		float obsessionConvertViewportHeight = GetObsessionConvertViewportHeight();
		float num = Mathf.Max(obsessionConvertRowsHeight, obsessionConvertViewportHeight - 160f);
		float num2 = 160f + Mathf.Max(0f, (num - obsessionConvertRowsHeight) * 0.5f);
		return new Vector2(0f, 0f - num2 - (float)rowIndex * 220f);
	}

	private float GetObsessionConvertRowsHeight(int rowCount)
	{
		return (float)rowCount * 190f + (float)Mathf.Max(0, rowCount - 1) * 30f;
	}

	private float GetObsessionConvertViewportHeight()
	{
		RectTransform rectTransform = ((scrollRect != null) ? scrollRect.viewport : null);
		if (rectTransform != null && rectTransform.rect.height > 0f)
		{
			return rectTransform.rect.height;
		}
		if (!(rewardContentHeightBeforeConvert > 0f))
		{
			return 1080f;
		}
		return rewardContentHeightBeforeConvert;
	}

	private void ClearObsessionConvertRows()
	{
		foreach (GlobalItemDisplay obsessionConvertDisplay in obsessionConvertDisplays)
		{
			if (!(obsessionConvertDisplay == null))
			{
				obsessionConvertDisplay.Unset();
				DisposeUI(obsessionConvertDisplay);
			}
		}
		obsessionConvertDisplays.Clear();
		foreach (GameObject obsessionConvertRow in obsessionConvertRows)
		{
			if (obsessionConvertRow != null)
			{
				UnityEngine.Object.Destroy(obsessionConvertRow);
			}
		}
		obsessionConvertRows.Clear();
	}

	private void SetRewardGridLayoutActive(bool active)
	{
		if (!active)
		{
			if (!rewardGridLayoutDisabledByConvert)
			{
				rewardGridLayoutEnabledBeforeConvert = rewardGridLayout == null || rewardGridLayout.enabled;
				rewardContentHeightBeforeConvert = ((rewardContentRect != null) ? rewardContentRect.sizeDelta.y : 0f);
				rewardGridLayoutDisabledByConvert = true;
			}
			if (rewardGridLayout != null)
			{
				rewardGridLayout.enabled = false;
			}
		}
		else if (rewardGridLayoutDisabledByConvert)
		{
			if (rewardGridLayout != null)
			{
				rewardGridLayout.enabled = rewardGridLayoutEnabledBeforeConvert;
			}
			if (rewardContentRect != null)
			{
				Vector2 sizeDelta = rewardContentRect.sizeDelta;
				sizeDelta.y = rewardContentHeightBeforeConvert;
				rewardContentRect.sizeDelta = sizeDelta;
			}
			rewardGridLayoutDisabledByConvert = false;
		}
	}

	private void UpdateObsessionConvertContentHeight()
	{
		if (!(rewardContentRect == null))
		{
			int count = obsessionConvertRows.Count;
			float obsessionConvertRowsHeight = GetObsessionConvertRowsHeight(count);
			float b = 190f + obsessionConvertRowsHeight + 200f;
			float a = (rewardGridLayoutDisabledByConvert ? rewardContentHeightBeforeConvert : rewardContentRect.sizeDelta.y);
			Vector2 sizeDelta = rewardContentRect.sizeDelta;
			sizeDelta.y = Mathf.Max(a, b);
			rewardContentRect.sizeDelta = sizeDelta;
		}
	}

	private void Hide()
	{
		GlobalRewardPackage followUpReward = CreateFollowUpRewardPackage();
		canvasGroup.alpha = 1f;
		Sequence obj = DOTween.Sequence();
		obj.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0.25f));
		obj.AppendCallback(delegate
		{
			ClearObsessionConvertRows();
			SetRewardGridLayoutActive(active: true);
			if (followUpReward != null)
			{
				Setup(followUpReward);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		});
		obj.Play();
		UIBase.fortressState?.uiManager.universalCommonInfoPanel.HideAll();
		UIBase.gameState?.UIManager.universalCommonInfoPanel.HideAll();
	}

	private GlobalRewardPackage CreateFollowUpRewardPackage()
	{
		if (reward == null || reward.IsObsessionConvertPackage)
		{
			return null;
		}
		return GlobalRewardPackage.CreateObsessionConvertPackage(reward.rewards);
	}

	private void _OnCloseButtonClicked()
	{
		if (displayingItemSkinGet)
		{
			if (multiInfoDisplayPanel.gameObject.activeInHierarchy)
			{
				multiInfoDisplayPanel.Unset();
			}
		}
		else if (!showAll)
		{
			showAll = true;
		}
		else if (showEnd)
		{
			Hide();
		}
	}

	private void TestPlayRandomChestAnim(QualityLevel qualityLevel)
	{
		showAll = false;
		Bootstrap.instance.StartCoroutine(PlayRandomChestAnim(qualityLevel));
	}

	private IEnumerator PlayRandomChestAnim(QualityLevel qualityLevel)
	{
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_treasure_open);
		((Component)(object)anim).gameObject.SetActive(value: true);
		switch (qualityLevel)
		{
		case QualityLevel.Base:
			anim.PlayAnimation("GetItem_Rank1", loop: false);
			break;
		case QualityLevel.Common:
			anim.PlayAnimation("GetItem_Rank2", loop: false);
			break;
		case QualityLevel.Uncommon:
			anim.PlayAnimation("GetItem_Rank3", loop: false);
			break;
		case QualityLevel.Rare:
		case QualityLevel.Epic:
			anim.PlayAnimation("GetItem_Rank4", loop: false);
			break;
		default:
			anim.PlayAnimation("GetItem_Rank1", loop: false);
			break;
		}
		yield return WaitForSecondsOrCondition(2f, () => showAll);
		((Component)(object)anim).gameObject.SetActive(value: false);
	}

	private static bool CanDisplayRewardPackage(GlobalRewardPackage pack)
	{
		if (pack == null)
		{
			return false;
		}
		if (pack.IsObsessionConvertPackage)
		{
			return true;
		}
		if (!pack.rewards.IsNullOrEmpty())
		{
			return pack.rewards.Any((GlobalRewardInfo r) => !r.isHidden);
		}
		return false;
	}
}
