using System;
using Coffee.UIExtensions;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalItemDisplay : CellUI<IGlobalItemDisplaySetup>, IDisplayModulesController
{
	public class DisplayModule : UIDisplayModule
	{
		protected GlobalItemDisplay globalItemDisplay => parent as GlobalItemDisplay;

		protected IGlobalItemDisplaySetup globalItem => globalItemDisplay.setupData;
	}

	[SerializeField]
	private Image iconBackground;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private GameObject countArea;

	[SerializeField]
	private TextMeshProUGUI count;

	[SerializeField]
	private bool showCount = true;

	[SerializeField]
	private SkillDisplay skillDisplay;

	[SerializeField]
	private SkillGemDisplay skillGemDisplay;

	[SerializeField]
	private ItemDisplayBase itemDisplay;

	[SerializeField]
	private RuneDisplay runeDisplay;

	[SerializeField]
	private InfluenceDisplay influenceDisplay;

	[SerializeField]
	private bool isGachaResultDisplay;

	[SerializeField]
	private bool isStorageDisplay;

	[SerializeField]
	private GameObject characterDisplay;

	[SerializeField]
	private Image character_HalfbodyA;

	[SerializeField]
	private UIParticle effect;

	[NonSerialized]
	private PrefabPool<UIParticle> effectPool;

	[NonSerialized]
	private bool suppressCountOnNextSetup;

	[NonSerialized]
	private bool hasDefaultCountAreaAnchoredPosition;

	[NonSerialized]
	private Vector2 defaultCountAreaAnchoredPosition;

	private IGlobalItemDisplaySetup reward => base.setupData;

	protected override void Init()
	{
		if (GetModule<RankModule>() != null)
		{
			GetModule<RankModule>().fQuality = () => reward.GlobalItemQuality;
		}
		if ((UnityEngine.Object)(object)effect != null)
		{
			effectPool = new PrefabPool<UIParticle>(effect);
		}
		skillDisplay.gameObject.SetActive(value: false);
		skillGemDisplay.gameObject.SetActive(value: false);
		if (itemDisplay != null)
		{
			itemDisplay.gameObject.SetActive(value: false);
		}
		if (runeDisplay != null)
		{
			runeDisplay.gameObject.SetActive(value: false);
		}
		if (influenceDisplay != null)
		{
			influenceDisplay.gameObject.SetActive(value: false);
		}
		InitUI(skillDisplay);
		InitUI(skillGemDisplay);
		if (itemDisplay != null)
		{
			InitUI(itemDisplay);
		}
		if (runeDisplay != null)
		{
			InitUI(runeDisplay);
		}
		if (influenceDisplay != null)
		{
			InitUI(influenceDisplay);
		}
		InfoModule module = GetModule<InfoModule>();
		if (module != null)
		{
			FortressUIManager uIInParents = GetUIInParents<FortressUIManager>();
			if (uIInParents != null)
			{
				module.infoPanel = uIInParents.universalCommonInfoPanel;
			}
			GameUIManager uIInParents2 = GetUIInParents<GameUIManager>();
			if (uIInParents2 != null)
			{
				module.infoPanel = uIInParents2.universalCommonInfoPanel;
			}
		}
	}

	protected override void OnSetup(IGlobalItemDisplaySetup setup)
	{
		iconBackground.gameObject.SetActive(value: false);
		if (characterDisplay != null)
		{
			characterDisplay.gameObject.SetActive(value: false);
		}
		skillDisplay.Unset();
		skillDisplay.gameObject.SetActive(value: false);
		skillGemDisplay.Unset();
		skillGemDisplay.gameObject.SetActive(value: false);
		if (itemDisplay != null)
		{
			itemDisplay.Unset();
			itemDisplay.gameObject.SetActive(value: false);
		}
		if (runeDisplay != null)
		{
			runeDisplay.Unset();
			runeDisplay.gameObject.SetActive(value: false);
		}
		if (influenceDisplay != null)
		{
			influenceDisplay.Unset();
			influenceDisplay.gameObject.SetActive(value: false);
		}
		if ((UnityEngine.Object)(object)effect != null)
		{
			effectPool.DespawnAll();
			effectPool.DestroyInActive();
			((Component)(object)effectPool.Spawn()).gameObject.SetActive(value: true);
		}
		int num = (isStorageDisplay ? reward.GetGlobalItemStorageCount(CellUI<IGlobalItemDisplaySetup>.applicationState).GetValueOrDefault() : reward.GlobalItemCount);
		if (suppressCountOnNextSetup || !showCount || (reward.HideNumberTextOnOnlyOne && num == 1))
		{
			countArea.SetActive(value: false);
		}
		else
		{
			countArea.SetActive(value: true);
			count.text = num.ToString();
		}
		if (isGachaResultDisplay && setup.GlobalItemRefPlayerCharacter != null && setup is GlobalRewardInfo { ShowGachaCharacterObsessionDisplay: not false })
		{
			characterDisplay.gameObject.SetActive(value: true);
			character_HalfbodyA.sprite = setup.GlobalItemRefPlayerCharacter.fullBody;
		}
		if (setup.GlobalItemRefSkill != null)
		{
			skillDisplay.gameObject.SetActive(value: true);
			RankModule component = skillDisplay.GetComponent<RankModule>();
			if (component != null)
			{
				component.HideRankIcon = isGachaResultDisplay;
			}
			skillDisplay.Setup(setup.GlobalItemRefSkill.GetGameSkill());
		}
		else if (setup.GlobalSkillGem != null)
		{
			skillGemDisplay.gameObject.SetActive(value: true);
			skillGemDisplay.Setup(setup.GlobalSkillGem);
		}
		else if (setup.GlobalItemRefItem != null && itemDisplay != null)
		{
			itemDisplay.gameObject.SetActive(value: true);
			itemDisplay.Setup(setup.GlobalItemRefItem);
		}
		else if (setup.GlobalItemRefRune != null && runeDisplay != null)
		{
			runeDisplay.gameObject.SetActive(value: true);
			runeDisplay.Setup(setup.GlobalItemRefRune);
		}
		else if (setup.globalItemRefInfluence != null && influenceDisplay != null)
		{
			influenceDisplay.gameObject.SetActive(value: true);
			influenceDisplay.Setup(setup.globalItemRefInfluence);
		}
		else
		{
			iconBackground.gameObject.SetActive(value: true);
			icon.sprite = reward.GlobalItemCellIcon;
		}
		SetDirty();
	}

	public void SetupWithoutCount(IGlobalItemDisplaySetup setup)
	{
		suppressCountOnNextSetup = true;
		try
		{
			Setup(setup);
		}
		finally
		{
			suppressCountOnNextSetup = false;
		}
	}

	public void SetCountAreaAnchoredPosition(Vector2 anchoredPosition)
	{
		if (countArea?.transform is RectTransform rectTransform)
		{
			if (!hasDefaultCountAreaAnchoredPosition)
			{
				defaultCountAreaAnchoredPosition = rectTransform.anchoredPosition;
				hasDefaultCountAreaAnchoredPosition = true;
			}
			rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	public void ResetCountAreaAnchoredPosition()
	{
		if (hasDefaultCountAreaAnchoredPosition && countArea?.transform is RectTransform rectTransform)
		{
			rectTransform.anchoredPosition = defaultCountAreaAnchoredPosition;
		}
	}

	protected override void OnUnset()
	{
		suppressCountOnNextSetup = false;
		ResetCountAreaAnchoredPosition();
	}

	private CharacterClassDef TryGetUICharacterClassDefContext()
	{
		IdolPanel componentInParent = GetComponentInParent<IdolPanel>(includeInactive: true);
		if (componentInParent != null)
		{
			return componentInParent.setupData.classDef;
		}
		ForgetShrinePanel componentInParent2 = GetComponentInParent<ForgetShrinePanel>(includeInactive: true);
		if (componentInParent2 != null)
		{
			return componentInParent2.setupData.classDef;
		}
		return null;
	}
}
