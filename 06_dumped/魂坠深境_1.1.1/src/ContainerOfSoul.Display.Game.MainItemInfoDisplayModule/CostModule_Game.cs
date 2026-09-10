using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class CostModule_Game : MainItemInfoDisplayBase.DisplayModule, IFrameUpdate
{
	[SerializeField]
	private Image costImage;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private Image valeDeltaHint;

	[NonSerialized]
	private bool isUpgradeModify;

	[NonSerialized]
	private Color color;

	public override void Setup()
	{
		isUpgradeModify = false;
		if (base.item.HasSpell && base.mainItemInfoDisplay.isUpgradeDisplay)
		{
			Item item = new Item(base.item.GetDef());
			Item item2 = base.item;
			isUpgradeModify = item.GetDisplayBaseCost(UIDisplayModule.applicationState) != item2.GetDisplayBaseCost(UIDisplayModule.applicationState);
		}
	}

	public override void UpdateDisplay()
	{
		if (!base.item.HasSpell)
		{
			costImage.gameObject.SetActive(value: false);
			valeDeltaHint.gameObject.SetActive(value: false);
			return;
		}
		int displayFinalCost = base.item.GetDisplayFinalCost(UIDisplayModule.applicationState, base.mainItemInfoDisplay.itemInfoDisplay.triggerType);
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.playerItemAnimConfig;
		costImage.sprite = base.item.GetCostImageLarge();
		ItemCostDisplayStyle.ApplyCostIcon(costImage, base.item);
		costImage.gameObject.SetActive(value: true);
		ItemCostDisplayStyle.ApplyCostText(costText, base.item, displayFinalCost);
		int displayBaseCost = base.item.GetDisplayBaseCost(UIDisplayModule.applicationState);
		if (displayFinalCost > displayBaseCost)
		{
			valeDeltaHint.gameObject.SetActive(value: true);
			valeDeltaHint.color = playerItemAnimConfig.negativeEffectColor;
			valeDeltaHint.transform.localRotation = Quaternion.Euler(Vector3.zero);
		}
		else if (displayFinalCost < displayBaseCost)
		{
			valeDeltaHint.gameObject.SetActive(value: true);
			valeDeltaHint.color = playerItemAnimConfig.positiveEffectColor;
			valeDeltaHint.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
		}
		else
		{
			valeDeltaHint.gameObject.SetActive(value: false);
		}
		color = base.item.GetCostTextColor();
		costText.color = color;
		RefreshColor();
	}

	void IFrameUpdate.FrameUpdate()
	{
		RefreshColor();
	}

	private void RefreshColor()
	{
		if (!base.gameObject.activeInHierarchy || !isUpgradeModify || costText.textInfo.meshInfo == null)
		{
			return;
		}
		for (int i = 0; i < costText.textInfo.meshInfo.Length; i++)
		{
			if (costText.textInfo.meshInfo[i].colors32 != null)
			{
				for (int j = 0; j < costText.textInfo.meshInfo[i].colors32.Length; j++)
				{
					costText.textInfo.meshInfo[i].colors32[j] = TextmeshproEffect.EffectParam.instance.GetColor(color);
				}
			}
		}
		costText.UpdateVertexData();
	}
}
