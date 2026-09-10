using System.Collections;
using System.Collections.Generic;
using ContainerOfSoul.Display.Game.BattleDisplay;
using UnityEngine;

public class MultiInfoDisplayPanel_GatherModule : MultiInfoDisplayPanel.DisplayModule
{
	[SerializeField]
	private GameButton backgroundButton;

	public override void Init()
	{
		backgroundButton.onClick.AddListener(_OnBackgroundButtonClicked);
	}

	private IEnumerator OnHidePanel()
	{
		List<WrapperEnumerator> list = new List<WrapperEnumerator>();
		foreach (ItemInfoDisplay_Normal activeElement in base.display.itemInfoDisplayPool.GetActiveElements())
		{
			list.Add(UIDisplayModule.gameState.StartCoroutine(activeElement.GetModule<MultiInfoStateModule>().OnHide));
		}
		foreach (InfluenceDisplay activeElement2 in base.display.influenceInfoDisplayPool.GetActiveElements())
		{
			list.Add(UIDisplayModule.gameState.StartCoroutine(activeElement2.GetModule<InfluenceMultiInfoPanelModule>().OnHide));
		}
		foreach (RuneDisplay activeElement3 in base.display.runeDisplayPool.GetActiveElements())
		{
			list.Add(UIDisplayModule.gameState.StartCoroutine(activeElement3.GetModule<RuneMultiInfoPanelModule>().OnHide));
		}
		yield return WrapperEnumerator.WaitAll(list);
		base.display.Unset();
	}

	private void _OnBackgroundButtonClicked()
	{
		UIDisplayModule.gameState.StartCoroutine(OnHidePanel);
	}
}
