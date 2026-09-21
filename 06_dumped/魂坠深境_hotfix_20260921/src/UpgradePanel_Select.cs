using System;
using System.Collections.Generic;

public class UpgradePanel_Select : UpgradePanel
{
	public abstract class SetupInfo
	{
		public Action onBack;

		public Action onUpgrade;

		public abstract bool CanUpgrade();

		public abstract int GetCurrentUpgradePrice();

		public abstract bool CanBack();
	}

	public class EncounterSmithSetupInfo : SetupInfo
	{
		public int upgradeTimes;

		public EncounterSmithSetupInfo()
		{
			upgradeTimes = 0;
			onUpgrade = (Action)Delegate.Combine(onUpgrade, (Action)delegate
			{
				upgradeTimes++;
			});
		}

		public override bool CanBack()
		{
			return true;
		}

		public override bool CanUpgrade()
		{
			return upgradeTimes < 1;
		}

		public override int GetCurrentUpgradePrice()
		{
			return 0;
		}
	}

	[NonSerialized]
	public SetupInfo info;

	public void Setup(SetupInfo info)
	{
		this.info = info;
		itemArea.Setup(UIBase.gameState.playerState.DeckContainer);
		upgradedItems = new List<Item>();
		previewItem = null;
	}

	public void Unset()
	{
		itemArea.Unset();
		upgradedItems = new List<Item>();
		previewItem = null;
		info = null;
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		UpdateBackButton();
	}

	public override void UpgradeItem()
	{
		base.UpgradeItem();
		info.onUpgrade?.Invoke();
	}

	public override bool CanConfirmUpgrade()
	{
		if (base.CanConfirmUpgrade())
		{
			return info.CanUpgrade();
		}
		return false;
	}

	public override int GetUpgradeCrystalPrice()
	{
		return info.GetCurrentUpgradePrice();
	}

	public override void HidePanel()
	{
		base.HidePanel();
		info.onBack?.Invoke();
	}

	public override void AfterUpgrade()
	{
		HidePanel();
	}

	private void UpdateBackButton()
	{
		backButton.gameObject.SetActive(info.CanBack());
	}
}
