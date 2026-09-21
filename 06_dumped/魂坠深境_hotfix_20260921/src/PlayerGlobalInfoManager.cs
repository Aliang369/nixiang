using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.Accounts.Sdk;
using CoreKit.Agent;
using Generated.ChillyRoomSdkClient;
using UnityEngine;

public class PlayerGlobalInfoManager : GlobalGameManager
{
	[SaveField(null, null)]
	public string nickName;

	[SaveField(null, null)]
	public bool vip;

	[SaveField(null, null)]
	public int monthlyCardExpireDayPeriod;

	[SaveField(null, null)]
	public int cheatScore;

	[SaveField(null, null)]
	public int dailyObsessionDebrisRecycleCount;

	[SaveField(null, null)]
	public int localObsessionDebrisRecycleDailyPeriod = int.MinValue;

	[SaveField(null, null)]
	public DateTime lastLoad;

	[SaveField(null, null)]
	private bool _showUnlockTalent;

	[SaveField(null, null)]
	private bool reviewed;

	[SaveField(null, null)]
	private int reviewPopupShowCount;

	[SaveField(null, null)]
	public List<MatterDef> newLoots;

	[SaveField(null, null)]
	private Dictionary<CharacterClassType, int> viewedForgetShrinePoints;

	[SaveField(null, null)]
	private int viewedCardShopDailyPeriod = int.MinValue;

	[SaveField(null, null)]
	private bool _showFreePurchase;

	public SdkState sdkState;

	public string DisplayName
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(nickName))
			{
				return nickName;
			}
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_default_player_name_arg0.FormatWith(Bootstrap.instance.CurrentAccountID);
		}
	}

	public string DataName
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(nickName))
			{
				return nickName;
			}
			return Bootstrap.instance.CurrentAccountID;
		}
	}

	public bool showFreePurchase
	{
		get
		{
			return _showFreePurchase;
		}
		set
		{
			if (_showFreePurchase != value)
			{
				_showFreePurchase = value;
				base.applicationState.savingManager.SetProfileDirtyToSave();
			}
		}
	}

	public bool HasUnviewedDailyCardShop
	{
		get
		{
			if (ScriptableObjectSingleton<CalendarDataManager>.instance.skinPurchaseGroupDef == null)
			{
				return false;
			}
			CardSkinOfferController cardSkinOffers = base.applicationState.purchaseManager.CardSkinOffers;
			cardSkinOffers.EnsureValidOffers();
			if (cardSkinOffers.IsLoaded && viewedCardShopDailyPeriod != cardSkinOffers.State.dailyPeriod)
			{
				return cardSkinOffers.HasAnyOffer();
			}
			return false;
		}
	}

	public int RemainingMonthlyCardDays
	{
		get
		{
			int num = monthlyCardExpireDayPeriod;
			int dailyPeriod = ScriptableObjectSingleton<CalendarDataManager>.instance.GetDailyPeriod(DateTime.UtcNow);
			return num - dailyPeriod;
		}
	}

	public bool IsMonthlyCardActive
	{
		get
		{
			int num = monthlyCardExpireDayPeriod;
			int dailyPeriod = ScriptableObjectSingleton<CalendarDataManager>.instance.GetDailyPeriod(DateTime.UtcNow);
			if (num > 0)
			{
				return dailyPeriod <= num;
			}
			return false;
		}
	}

	public bool showUnlockTalent
	{
		get
		{
			return _showUnlockTalent;
		}
		set
		{
			if (_showUnlockTalent != value)
			{
				_showUnlockTalent = value;
				base.applicationState.savingManager.SetProfileDirtyToSave();
			}
		}
	}

	public void MarkDailyCardShopViewed()
	{
		CardSkinOfferController cardSkinOffers = base.applicationState.purchaseManager.CardSkinOffers;
		cardSkinOffers.EnsureValidOffers();
		if (cardSkinOffers.IsLoaded)
		{
			int dailyPeriod = cardSkinOffers.State.dailyPeriod;
			if (viewedCardShopDailyPeriod != dailyPeriod)
			{
				viewedCardShopDailyPeriod = dailyPeriod;
				base.applicationState.savingManager.SetProfileDirtyToSave();
			}
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (newLoots == null)
		{
			newLoots = new List<MatterDef>();
		}
		if (viewedForgetShrinePoints == null)
		{
			viewedForgetShrinePoints = new Dictionary<CharacterClassType, int>();
		}
		string text = "（默认名称:" + Bootstrap.instance.CurrentAccountID + "）";
		if (nickName == text)
		{
			nickName = null;
		}
		if (DateTime.UtcNow.Day > lastLoad.Day)
		{
			showUnlockTalent = true;
			showFreePurchase = true;
		}
		lastLoad = DateTime.UtcNow;
		applicationState.savingManager.SetProfileDirtyToSave();
		UpdateUserProfile();
	}

	public bool HasUnviewedForgetShrinePoint(CharacterClassState characterClassState)
	{
		if (characterClassState?.classDef == null)
		{
			return false;
		}
		int value;
		return GetCurrentForgetShrinePoint(characterClassState) > (viewedForgetShrinePoints.TryGetValue(characterClassState.classDef.mClass, out value) ? value : 0);
	}

	public bool HasAnyUnviewedForgetShrinePoint()
	{
		ApplicationStateInfo applicationStateInfo = base.applicationState;
		if (applicationStateInfo == null)
		{
			return false;
		}
		return applicationStateInfo.characterManager?.characterClassStates?.Any(HasUnviewedForgetShrinePoint) == true;
	}

	public bool MarkForgetShrinePointViewed(CharacterClassState characterClassState)
	{
		if (characterClassState?.classDef == null)
		{
			return false;
		}
		CharacterClassType mClass = characterClassState.classDef.mClass;
		if (!HasUnviewedForgetShrinePoint(characterClassState))
		{
			return false;
		}
		viewedForgetShrinePoints[mClass] = GetCurrentForgetShrinePoint(characterClassState);
		base.applicationState.savingManager.SetProfileDirtyToSave();
		return true;
	}

	private static int GetCurrentForgetShrinePoint(CharacterClassState characterClassState)
	{
		return (characterClassState?.GetIdolState()?.GetForgetShrinePoint()).GetValueOrDefault();
	}

	public void SetNickName(string name)
	{
		nickName = name;
		base.applicationState.savingManager.SetProfileDirtyToSave();
	}

	public IEnumerator TryTriggerReview()
	{
		return TryTriggerReview(null);
	}

	public IEnumerator TryTriggerReview(Action beforeVisibleInterruption)
	{
		yield return new WaitUntil(() => base.applicationState.pleaseReviewPopupManager.configLoaded);
		int pleaseReviewPopupType = base.applicationState.pleaseReviewPopupManager.pleaseReviewPopupType;
		if (pleaseReviewPopupType == -1 || reviewed)
		{
			yield break;
		}
		int reviewPopupIndexToShow = GetReviewPopupIndexToShow();
		if (reviewPopupIndexToShow == 0)
		{
			yield break;
		}
		reviewPopupShowCount++;
		reviewed = reviewPopupShowCount >= 2;
		base.applicationState.savingManager.SaveProfile();
		beforeVisibleInterruption?.Invoke();
		if (GlobalConfigManager.instance.showCustomReviewPopup)
		{
			if (base.fortressState?.uiManager != null)
			{
				if (reviewPopupIndexToShow == 1)
				{
					base.fortressState.uiManager.pleaseReviewPopup.ShowFirstPop(pleaseReviewPopupType);
				}
				else
				{
					base.fortressState.uiManager.pleaseReviewPopup.ShowSecondPop(pleaseReviewPopupType);
				}
				yield return new WaitUntil(() => !base.fortressState.uiManager.pleaseReviewPopup.gameObject.activeInHierarchy);
			}
		}
		else
		{
			AgentHub.Instance.Special.callReview();
		}
	}

	private int GetReviewPopupIndexToShow()
	{
		if (reviewPopupShowCount == 0 && base.applicationState.gameStatisticsManager.gameCount >= 2)
		{
			return 1;
		}
		if (reviewPopupShowCount == 1 && base.applicationState.gameStatisticsManager.gameCount >= 7 && base.applicationState.lastGameEndReason == LastGameEndReason.Win)
		{
			return 2;
		}
		return 0;
	}

	public async void UpdateUserProfile()
	{
		ChillyRoomService sdk = ChillyRoomService.GetSdk();
		if (sdk != null)
		{
			sdkState = await sdk.Accounts.GetUserProfile();
		}
	}
}
