using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class CostModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	public Image costIconImage;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private Image valeDeltaHint;

	public override void UpdateDisplay()
	{
		if (base.slotDisplay.indexHide)
		{
			costText.gameObject.SetActive(value: false);
			costIconImage.gameObject.SetActive(value: false);
		}
		else
		{
			costText.gameObject.SetActive(value: true);
			costIconImage.gameObject.SetActive(base.slotDisplay.item.HasSpell);
		}
		if (!costIconImage.gameObject.activeSelf)
		{
			return;
		}
		ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.playerItemAnimConfig;
		costIconImage.sprite = base.slotDisplay.item.GetCostImage();
		CastInfoCache castInfoCache = UIDisplayModule.gameState?.UIManager?.playerItemArea?.controlInfo?.GetCastInfoCache(base.slotDisplay.item);
		if (base.slotDisplay.item.HasSpell)
		{
			if (base.slotDisplay.item.TryGetActiveOverrideSpellCostDisplay(UIDisplayModule.applicationState, base.slotDisplay.triggerType, out var sourceEnchant, out var originalCost, out var overrideCost))
			{
				if (sourceEnchant.def.displayOverrideCostAsMainCost)
				{
					ItemCostDisplayStyle.ApplyCostIcon(costIconImage, base.slotDisplay.item);
					valeDeltaHint.gameObject.SetActive(value: true);
					valeDeltaHint.color = ((overrideCost < originalCost) ? playerItemAnimConfig.positiveEffectColor : playerItemAnimConfig.negativeEffectColor);
					valeDeltaHint.transform.localRotation = Quaternion.Euler((overrideCost < originalCost) ? new Vector3(0f, 0f, 180f) : Vector3.zero);
					ItemCostDisplayStyle.ApplyCostText(costText, base.slotDisplay.item, overrideCost);
				}
				else
				{
					valeDeltaHint.gameObject.SetActive(value: false);
					costText.text = TextDisplayHelper.ToNumSpriteString(originalCost);
					costText.color = base.slotDisplay.item.GetCostTextColor();
				}
				return;
			}
			int num = castInfoCache?.finalCost ?? base.slotDisplay.item.GetDisplayFinalCost(Bootstrap.applicationState, base.slotDisplay.triggerType);
			int num2 = castInfoCache?.baseCost ?? base.slotDisplay.item.GetDisplayBaseCost(UIDisplayModule.applicationState);
			ItemCostDisplayStyle.ApplyCostIcon(costIconImage, base.slotDisplay.item);
			if (num > num2)
			{
				valeDeltaHint.gameObject.SetActive(value: true);
				valeDeltaHint.color = playerItemAnimConfig.negativeEffectColor;
				valeDeltaHint.transform.localRotation = Quaternion.Euler(Vector3.zero);
			}
			else if (num < num2)
			{
				valeDeltaHint.gameObject.SetActive(value: true);
				valeDeltaHint.color = playerItemAnimConfig.positiveEffectColor;
				valeDeltaHint.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
			}
			else
			{
				valeDeltaHint.gameObject.SetActive(value: false);
			}
			ItemCostDisplayStyle.ApplyCostText(costText, base.slotDisplay.item, num);
		}
		else
		{
			valeDeltaHint.gameObject.SetActive(value: false);
		}
	}

	public void OnExile()
	{
		costText.gameObject.SetActive(value: false);
	}
}
