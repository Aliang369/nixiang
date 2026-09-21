using System;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Fortress.SkillOverview;
using UnityEngine;

public class UniversalCommonInfoPanel : UIBase, ICommonInfoPanel
{
	[SerializeField]
	private UniversalCommonInfoCell prefab;

	[SerializeField]
	private GameButton hideButton;

	[NonSerialized]
	private CellUIPrefabPool<UniversalCommonInfoCell, CommonInfoData> pool;

	protected override void Init()
	{
		base.Init();
		hideButton?.onClick.AddListener(HideAll);
		hideButton.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: true);
		GetPrefabPool(ref pool, prefab);
	}

	public void AppendInfo(CommonInfoData data)
	{
		if (ShouldShow(data))
		{
			TraceLog.Debug($"AppendInfo({data})");
			base.gameObject.SetActive(value: true);
			hideButton.gameObject.SetActive(data.showHideButton);
			pool.Spawn(data);
		}
	}

	public void HideAll()
	{
		if (inited)
		{
			TraceLog.Debug("HideAll()");
			pool.DespawnAll();
			base.gameObject.SetActive(value: false);
		}
	}

	public RectTransform GetActiveSkillEquipButtonRectTransform()
	{
		return (from cell in pool.GetActiveElements()
			select cell.ActiveSkillEquipButtonRectTransform).FirstOrDefault((RectTransform rectTransform) => rectTransform != null && rectTransform.gameObject.activeInHierarchy);
	}

	public void ShowCommonInfo(CommonInfoData data)
	{
		if (ShouldShow(data))
		{
			pool.DespawnAll();
			if (data.target is RewardBundlePurchase setupData && UIBase.fortressState != null)
			{
				UIBase.fortressState.uiManager.purchaseBundleCheckPanel.ShowUI(setupData);
			}
			else
			{
				AppendInfo(data);
			}
		}
	}

	public void HideCommonInfo(object obj, CommonInfoData.ControlType controlType)
	{
		foreach (UniversalCommonInfoCell activeElement in pool.GetActiveElements())
		{
			if (activeElement.setupData.target == obj)
			{
				pool.Despawn(activeElement);
				break;
			}
		}
	}

	private static bool ShouldShow(CommonInfoData data)
	{
		if (data == null || data.target == null)
		{
			return false;
		}
		if (data.target is ProfileSkillPresetSlotCell.Info info && info.slot?.skill == null)
		{
			return false;
		}
		return true;
	}
}
