using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class CoolDownModule_Game : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private Image darkenImage;

	[SerializeField]
	private Image coolDownDisplay;

	[SerializeField]
	private TextMeshProUGUI coolDownDisplayText;

	[SerializeField]
	private bool coolDownOnly;

	public override void UpdateDisplay()
	{
		UpdateCoolDown();
		if (!coolDownOnly)
		{
			UpdateTargetAble();
		}
	}

	private void UpdateCoolDown()
	{
		if (!base.slotDisplay.item.HasSpell || !base.slotDisplay.item.IsCoolDowning || HasActiveOverrideSpell())
		{
			coolDownDisplay.gameObject.SetActive(value: false);
			coolDownDisplayText.gameObject.SetActive(value: false);
			return;
		}
		coolDownDisplay.gameObject.SetActive(value: true);
		ProtectedInt cooldown = base.slotDisplay.item.cooldown;
		if ((int)cooldown == 1)
		{
			coolDownDisplayText.gameObject.SetActive(value: false);
			return;
		}
		coolDownDisplayText.gameObject.SetActive(value: true);
		coolDownDisplayText.text = cooldown.ToString();
	}

	private bool HasActiveOverrideSpell()
	{
		Enchant currentOverrideSpellEnchant = base.slotDisplay.item.GetCurrentOverrideSpellEnchant();
		if ((int?)currentOverrideSpellEnchant?.value > 0)
		{
			return currentOverrideSpellEnchant.def?.overrideSpell == base.slotDisplay.item.CurrentSpell;
		}
		return false;
	}

	private void UpdateTargetAble()
	{
		PlayerItemDisplay selectedDisplay = UIDisplayModule.gameState.UIManager.playerItemArea.GetSelectedDisplay();
		PlayerControlInfo controlInfo = UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo;
		if (selectedDisplay == base.slotDisplay)
		{
			darkenImage.gameObject.SetActive(value: false);
		}
		else if (controlInfo.state != PlayerControlInfo.State.GettingTarget && controlInfo.state != PlayerControlInfo.State.TargetSelected)
		{
			darkenImage.gameObject.SetActive(value: false);
		}
		else if (controlInfo.CheckItemAsTargetAble(base.slotDisplay.item))
		{
			darkenImage.gameObject.SetActive(value: false);
		}
		else
		{
			darkenImage.gameObject.SetActive(value: true);
		}
	}
}
