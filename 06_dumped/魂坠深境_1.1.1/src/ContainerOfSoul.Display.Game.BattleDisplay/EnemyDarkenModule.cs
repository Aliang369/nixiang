using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

[Serializable]
public class EnemyDarkenModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private Image darkenDisplay;

	[SerializeField]
	private Image coolDownDisplay;

	[SerializeField]
	private TextMeshProUGUI coolDownDisplayText;

	public override void UpdateDisplay()
	{
		PlayerControlInfo controlInfo = UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo;
		if (base.slotDisplay.item.IsCoolDowning)
		{
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
		else
		{
			coolDownDisplay.gameObject.SetActive(value: false);
			coolDownDisplayText.gameObject.SetActive(value: false);
		}
		if (controlInfo.state != PlayerControlInfo.State.GettingTarget && controlInfo.state != PlayerControlInfo.State.TargetSelected)
		{
			darkenDisplay.gameObject.SetActive(value: false);
		}
		else if (controlInfo.CheckItemAsTargetAble(base.slotDisplay.item))
		{
			darkenDisplay.gameObject.SetActive(value: false);
		}
		else
		{
			darkenDisplay.gameObject.SetActive(value: true);
		}
	}
}
