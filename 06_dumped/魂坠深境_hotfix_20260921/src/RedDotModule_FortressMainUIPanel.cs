using System.Linq;
using ContainerOfSoul.Display.Fortress;
using ContainerOfSoul.Display.Fortress.DotManagers;
using UnityEngine;

public class RedDotModule_FortressMainUIPanel : UIDisplayModule
{
	[SerializeField]
	private GameObject ToGachaDrawButtonRedDot;

	[SerializeField]
	private HintRedDot toCrewButtonRedDot;

	[SerializeField]
	private HintRedDot toStorehouseButtonRedDot;

	[SerializeField]
	private HintRedDot toShopRedDot;

	[SerializeField]
	private HintRedDot to_Liberay_ButtonDot;

	[SerializeField]
	private HintRedDot to_IdolRoom_ButtonDot;

	[SerializeField]
	private HintRedDot indexBook_ButtonDot;

	[SerializeField]
	private GameObject mainRoomRedDot;

	[SerializeField]
	private GameObject announcementHintRedDot;

	[SerializeField]
	private GameObject mailHintRedDot;

	[SerializeField]
	private GameObject dailyCheckInRedDot;

	[SerializeField]
	private GameObject eventCheckInRedDot;

	[SerializeField]
	private GameObject dailyQuestInRedDot;

	private bool SuppressRedDots
	{
		get
		{
			FortressStateInfo fortressStateInfo = UIDisplayModule.fortressState;
			if (fortressStateInfo == null)
			{
				return false;
			}
			return fortressStateInfo.uiManager?.mainUIPanel?.SuppressMainInterfaceRedDots == true;
		}
	}

	public override void Init()
	{
		parent.InitUI(toCrewButtonRedDot);
		parent.InitUI(toStorehouseButtonRedDot);
		parent.InitUI(toShopRedDot);
		parent.InitUI(to_Liberay_ButtonDot);
		parent.InitUI(to_IdolRoom_ButtonDot);
		parent.InitUI(indexBook_ButtonDot);
		toCrewButtonRedDot.Setup(UIDisplayModule.fortressState.redDotManager.toCrewButtonModule);
		toStorehouseButtonRedDot.Setup(new CustomModule
		{
			shouldShow = () => UIDisplayModule.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressStoreHousePanel) && UIDisplayModule.applicationState.playerInfoManager.newLoots.Any()
		});
		toShopRedDot.Setup(new CustomModule
		{
			shouldShow = () => UIDisplayModule.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnablePurchasePanel) && (UIDisplayModule.applicationState.playerInfoManager.showFreePurchase || UIDisplayModule.applicationState.playerInfoManager.HasUnviewedDailyCardShop)
		});
		indexBook_ButtonDot.Setup(new CustomModule
		{
			shouldShow = () => IsIndexBookEntryUnlocked() && UIDisplayModule.applicationState.discoverManager.CheckWithAnyPageWithUnGotReward()
		});
		to_Liberay_ButtonDot.Setup(new CustomModule
		{
			shouldShow = () => IsLibraryLayerUnlocked() && (indexBook_ButtonDot.redDotModule.CheckShouldShow() || ShouldShowAchievementEntryRedDot() || ShouldShowForerunnerRewardRedDot())
		});
		to_IdolRoom_ButtonDot.Setup(new CustomModule
		{
			shouldShow = delegate
			{
				if (!ScriptableObjectSingleton<GameDataManager>.instance.lockIdolRoom.isUnlocked)
				{
					return false;
				}
				bool num = IsIdolUpgradable(CharacterClassType.Musician) || IsIdolUpgradable(CharacterClassType.Wizard) || IsIdolUpgradable(CharacterClassType.Warrior) || IsIdolUpgradable(CharacterClassType.XiaKe);
				bool flag = UIDisplayModule.applicationState.idolManager.forgetShrineUnlocked && UIDisplayModule.applicationState.playerInfoManager.HasAnyUnviewedForgetShrinePoint();
				return num | flag;
			}
		});
		static bool IsIdolUpgradable(CharacterClassType classType)
		{
			return UIDisplayModule.applicationState.idolManager.GetIdolState(classType)?.CanUpgrade(UIDisplayModule.applicationState) ?? false;
		}
		static bool IsIndexBookEntryUnlocked()
		{
			return UIDisplayModule.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressStoreHousePanel);
		}
		static bool IsLibraryLayerUnlocked()
		{
			FortressMainUIPanel.MainPanelLayerSetup mainPanelLayerSetup = UIDisplayModule.fortressState.uiManager.mainUIPanel.mainUISwitchGroup.mainPanelLayerSetups.FirstOrDefault((FortressMainUIPanel.MainPanelLayerSetup s) => s.layer == FortressMainUIPanel.MainPanelLayer.Library);
			if (mainPanelLayerSetup == null)
			{
				return false;
			}
			return !mainPanelLayerSetup.isLocked;
		}
		static bool ShouldShowAchievementEntryRedDot()
		{
			if (UIDisplayModule.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableAchievementPanel))
			{
				return UIDisplayModule.fortressState.uiManager.achievementUIPanel.GetNeedReceiveAchievementAndQuestCount() > 0;
			}
			return false;
		}
		static bool ShouldShowForerunnerRewardRedDot()
		{
			if (UIDisplayModule.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressForerunnerLegacyPanel))
			{
				return UIDisplayModule.applicationState.forerunnerManager.HasPendingShareReward;
			}
			return false;
		}
	}

	public override void UpdateDisplay()
	{
		ToGachaDrawButtonRedDot.SetActive(!SuppressRedDots && UIDisplayModule.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableGachaDrawPanel) && UIDisplayModule.applicationState.gachaManager.hasNew);
		mainRoomRedDot.SetActive(value: false);
		bool flag = false;
		announcementHintRedDot.SetActive(!SuppressRedDots & flag);
		bool flag2 = UIDisplayModule.applicationState.mailManager.mailList.Any((MailData m) => !m.isRead || !m.isAccepted);
		mailHintRedDot.SetActive(!SuppressRedDots & flag2);
		bool flag3 = false;
		dailyCheckInRedDot.SetActive(!SuppressRedDots & flag3);
		eventCheckInRedDot.SetActive(!SuppressRedDots & flag3);
	}
}
