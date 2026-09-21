using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;

public class SkillGemArea : UIBase
{
	[SerializeField]
	private GameObject area;

	[SerializeField]
	private TextMeshProUGUI descriptionPrefab;

	[SerializeField]
	private TextMeshProUGUI genCountText;

	[SerializeField]
	private GemMountedDisplay gemMountedDisplay;

	[SerializeField]
	private SkillGemSlotDisplay skillGemSlotDisplay;

	[SerializeField]
	private GemSlotMark gemSlotMark;

	[SerializeField]
	private GameObject hasGemMark;

	[NonSerialized]
	private PrefabPool<TextMeshProUGUI> descriptionPool;

	[NonSerialized]
	private CellUIPrefabPool<GemMountedDisplay, SkillGem> mountedSkillGemPool;

	[NonSerialized]
	public GameSkill gameSkill;

	protected override void Init()
	{
		InitUI(skillGemSlotDisplay);
		if (gemSlotMark != null)
		{
			InitUI(gemSlotMark);
		}
		parent.GetPrefabPool(ref mountedSkillGemPool, gemMountedDisplay);
		descriptionPool = new PrefabPool<TextMeshProUGUI>(descriptionPrefab);
	}

	public void Setup(GameSkill skill)
	{
		gameSkill = skill;
		if (!CanShowGemSlotInfo() || gameSkill.skillHexShape == null)
		{
			HideGemSlotArea();
			return;
		}
		area.gameObject.SetActive(value: true);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (!CanShowGemSlotInfo() || gameSkill.skillHexShape == null)
		{
			HideGemSlotArea();
			return;
		}
		skillGemSlotDisplay.Setup(gameSkill.skillHexShape, HexagonHelper.HexCellSize);
		area.gameObject.SetActive(value: true);
		List<SkillGem> skillGems = gameSkill.skillGems;
		mountedSkillGemPool.DespawnAll();
		foreach (SkillGem item3 in skillGems.OrEmptyIfNull())
		{
			GemMountedDisplay gemMountedDisplay = mountedSkillGemPool.Spawn(item3);
			Vector2 hexPointPosition = HexagonHelper.GetHexPointPosition(item3.offset, HexagonHelper.HexCellSize);
			gemMountedDisplay.transform.localPosition = hexPointPosition;
			gemMountedDisplay.rectTransform.sizeDelta *= this.gemMountedDisplay.transform.parent.RectTransform().rect.width / gemMountedDisplay.rectTransform.rect.width;
		}
		descriptionPool.DespawnAll();
		foreach (var allWordDescription in gameSkill.GetAllWordDescriptions(UIBase.applicationState))
		{
			string item = allWordDescription.Item1;
			TextDataGenerator item2 = allWordDescription.Item2;
			TextMeshProUGUI textMeshProUGUI = descriptionPool.Spawn();
			textMeshProUGUI.text = item;
			new CommonTextLinkObjectDisplayer(UIBase.applicationState, textMeshProUGUI, base.rectTransform).SetGenerator(item2);
		}
		if (gemSlotMark != null)
		{
			gemSlotMark.Setup(gameSkill.profileSkill.hexShape);
			hasGemMark.SetActive(!gameSkill.profileSkill.skillGemsMounted.IsNullOrEmpty());
		}
		if (genCountText != null)
		{
			genCountText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_slot_count.FormatWith(gameSkill.profileSkill.HexShapeSlotCount);
		}
	}

	private void HideGemSlotArea()
	{
		area.gameObject.SetActive(value: false);
		skillGemSlotDisplay.HideAllSlot();
		mountedSkillGemPool?.DespawnAll();
		descriptionPool?.DespawnAll();
		if (gemSlotMark != null)
		{
			gemSlotMark.Unset();
			gemSlotMark.gameObject.SetActive(value: false);
		}
		if (hasGemMark != null)
		{
			hasGemMark.SetActive(value: false);
		}
		if (genCountText != null)
		{
			genCountText.text = string.Empty;
		}
	}

	private bool CanShowGemSlotInfo()
	{
		ApplicationStateInfo applicationStateInfo = UIBase.applicationState;
		if (applicationStateInfo == null)
		{
			return false;
		}
		return applicationStateInfo.globalVariableManager?.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillMountGem) == true;
	}
}
