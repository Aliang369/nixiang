using System;
using UnityEngine;

public class YuanQiMissionRewardStrip : CellUI<IGlobalItemDisplaySetup>
{
	[SerializeField]
	private GlobalItemDisplay globalItemDisplay;

	[SerializeField]
	private GameObject claimedObject;

	[NonSerialized]
	private bool claimed;

	protected override void Init()
	{
		if (globalItemDisplay != null)
		{
			InitUI(globalItemDisplay);
		}
	}

	protected override void OnSetup(IGlobalItemDisplaySetup setupData)
	{
		if (globalItemDisplay != null)
		{
			globalItemDisplay.Setup(setupData);
		}
		RefreshClaimedObject();
	}

	public void Setup(IGlobalItemDisplaySetup reward, bool isClaimed)
	{
		claimed = isClaimed;
		Setup(reward);
	}

	public void SetClaimed(bool isClaimed)
	{
		claimed = isClaimed;
		RefreshClaimedObject();
	}

	private void RefreshClaimedObject()
	{
		if (claimedObject != null)
		{
			claimedObject.SetActive(claimed);
		}
	}
}
