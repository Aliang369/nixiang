using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.Purchase;

[DisallowMultipleComponent]
public class PurchaseBundleRewardPreview : MonoBehaviour
{
	[SerializeField]
	private RectTransform previewRoot;

	[SerializeField]
	private GridLayoutGroup previewLayout;

	[SerializeField]
	private GlobalItemDisplay rewardDisplayPrefab;

	[SerializeField]
	private float rewardDisplayScale = 0.84f;

	[SerializeField]
	private int maxPreviewCount = 6;

	private readonly List<GlobalItemDisplay> rewardDisplays = new List<GlobalItemDisplay>();

	public void Setup(RewardBundlePurchase purchase, UINode owner)
	{
		Setup((purchase?.rewardBundle?.rewards)?.Cast<IGlobalItemDisplaySetup>().ToList(), owner);
	}

	public void Setup(WatchAdGetMatterPurchase purchase, UINode owner)
	{
		Setup((purchase?.gain != null) ? new List<IGlobalItemDisplaySetup> { purchase.gain } : null, owner);
	}

	private void Setup(List<IGlobalItemDisplaySetup> rewards, UINode owner)
	{
		if (owner == null || previewRoot == null || rewardDisplayPrefab == null || rewards == null || rewards.Count == 0 || rewards.Count > maxPreviewCount)
		{
			Hide();
			return;
		}
		previewRoot.gameObject.SetActive(value: true);
		ConfigureLayout(rewards.Count);
		EnsureDisplayCount(rewards.Count, owner);
		for (int i = 0; i < rewardDisplays.Count; i++)
		{
			bool flag = i < rewards.Count;
			GlobalItemDisplay globalItemDisplay = rewardDisplays[i];
			globalItemDisplay.gameObject.SetActive(flag);
			if (flag)
			{
				globalItemDisplay.Setup(rewards[i]);
			}
			else if (globalItemDisplay.setupData != null)
			{
				globalItemDisplay.Unset();
			}
		}
	}

	public void Hide()
	{
		if (previewRoot != null)
		{
			previewRoot.gameObject.SetActive(value: false);
		}
		foreach (GlobalItemDisplay rewardDisplay in rewardDisplays)
		{
			if (!(rewardDisplay == null))
			{
				if (rewardDisplay.setupData != null)
				{
					rewardDisplay.Unset();
				}
				rewardDisplay.gameObject.SetActive(value: false);
			}
		}
	}

	private void ConfigureLayout(int rewardCount)
	{
		if (!(previewLayout == null))
		{
			previewLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			previewLayout.constraintCount = ((rewardCount <= 3) ? rewardCount : ((rewardCount == 4) ? 2 : 3));
		}
	}

	private void EnsureDisplayCount(int count, UINode owner)
	{
		while (rewardDisplays.Count < count)
		{
			GlobalItemDisplay globalItemDisplay = Object.Instantiate(rewardDisplayPrefab, previewRoot);
			globalItemDisplay.transform.localScale = Vector3.one * rewardDisplayScale;
			owner.InitUI(globalItemDisplay);
			globalItemDisplay.gameObject.SetActive(value: false);
			rewardDisplays.Add(globalItemDisplay);
		}
	}
}
