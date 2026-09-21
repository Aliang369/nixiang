using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Fortress;
using TMPro;
using UnityEngine;

public class GachaPanel : UIBase, IStackPanel
{
	public GameButton backButton;

	public MatterCellGroup matterCellGroup;

	public GameButton closeGachaRuleInfoPanelButton;

	public GameObject gachaRUleInfoPanel;

	public TextMeshProUGUI gachaRUleInfoPanelText;

	public TextMeshProUGUI gachaContentDetailText;

	public GachaPoolState selectedPool;

	public GachaPoolState targetState;

	public bool isChecking;

	public GachaGuideCell prefab;

	public CellUIPrefabPool<GachaGuideCell, GachaPoolState> pool;

	public GachaSet gachaSet;

	public Transform gachaSetParent;

	public GameObject confirmPopup;

	public GameButton confirmYesButton;

	public GameButton confirmNoButton;

	public TextMeshProUGUI confirmDisplayText;

	public GlobalItemDisplay confirmFromMatter;

	public GlobalItemDisplay confirmToMatter;

	public GameButton gachaRecordButton;

	public GachaRecordInfo gachaRecordInfo;

	public GachaWishSelectPanel gachaWishSelectPanel;

	[NonSerialized]
	private Coroutine checkCoroutine;

	public RectTransform BackButtonRect
	{
		get
		{
			if (!(backButton != null))
			{
				return null;
			}
			return backButton.transform as RectTransform;
		}
	}

	protected override void Init()
	{
		InitUI(matterCellGroup);
		InitUI(gachaRecordInfo);
		InitUI(gachaWishSelectPanel);
		InitUI(confirmFromMatter);
		InitUI(confirmToMatter);
		GetPrefabPool(ref pool, prefab);
		List<GachaPoolState> list = UIBase.applicationState.gachaManager.SortedPool();
		pool.Sync(list);
		GachaPoolState gachaPoolState = list.FirstOrDefault((GachaPoolState s) => s == UIBase.applicationState.gachaManager.defaultState) ?? list.FirstOrDefault();
		if (gachaPoolState != null)
		{
			SetState(gachaPoolState);
		}
		closeGachaRuleInfoPanelButton.onClick.AddListener(_OnCloseGachaInfoPanelButtonClicked);
		backButton.onClick.AddListener(Close);
		gachaRecordButton.onClick.AddListener(delegate
		{
			gachaRecordInfo.Show();
		});
		selectedPool = null;
		isChecking = false;
		gachaRecordInfo.gameObject.SetActive(value: false);
		gachaRUleInfoPanel.gameObject.SetActive(value: false);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (gachaSet != null)
		{
			matterCellGroup.Setup(gachaSet.PoolState.def.gachaUIPreviewMatters.Select((MatterDef m) => UIBase.applicationState.matterManager.GetMatterInfo(m)).ToList());
		}
		StartCheckCoroutine();
	}

	public void Show()
	{
		ResetPoolDisplay();
		base.gameObject.SetActive(value: true);
		SetDirty();
	}

	public void ShowDefaultEntryPool()
	{
		GachaPoolState defaultState = UIBase.applicationState.gachaManager.defaultState;
		FortressGuidePanel fortressGuidePanel = UIBase.fortressState.uiManager.fortressGuidePanel;
		GachaPoolState gachaPoolState = (((object)fortressGuidePanel != null && fortressGuidePanel.IsPlayingGachaGuide) ? defaultState : (UIBase.applicationState.gachaManager.SortedPool().FirstOrDefault((GachaPoolState state) => state.def.poolType == GachaPoolDef.GachaPoolType.Limited) ?? defaultState));
		if (gachaPoolState != null)
		{
			SetState(gachaPoolState);
		}
		ResetPoolDisplay();
		this.ShowUI();
	}

	public void ShowEntryPool(GachaPoolDef.GachaPoolType poolType)
	{
		GachaPoolState gachaPoolState = UIBase.applicationState.gachaManager.SortedPool().FirstOrDefault((GachaPoolState state) => state.def.poolType == poolType);
		if (gachaPoolState != null)
		{
			SetState(gachaPoolState);
			ResetPoolDisplay();
			this.ShowUI();
		}
	}

	private void ResetPoolDisplay()
	{
		CancelCheckCoroutine();
		if (gachaSet != null)
		{
			gachaSet.gameObject.SetActive(value: false);
			DisposeUI(gachaSet);
			UnityEngine.Object.Destroy(gachaSet.gameObject);
			gachaSet = null;
		}
		selectedPool = null;
		isChecking = false;
	}

	public void SetState(GachaPoolState state)
	{
		if (targetState != state)
		{
			targetState = state;
			targetState.viewed = true;
			SetDirty();
		}
	}

	public void ResetAnim()
	{
		isChecking = false;
		if (gachaSet != null)
		{
			((Behaviour)(object)gachaSet.animator).enabled = false;
		}
	}

	private void Close()
	{
		CancelCheckCoroutine();
		base.gameObject.SetActive(value: false);
		UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
		UIBase.fortressState.uiManager.SetDirty();
	}

	private void OnDisable()
	{
		CancelCheckCoroutine();
	}

	private void _OnCloseGachaInfoPanelButtonClicked()
	{
		gachaRUleInfoPanel.SetActive(value: false);
	}

	public void SetupConfirmPopup(string confirmText, Action confirmCallBack, MatterInfo from, MatterInfo to)
	{
		confirmPopup.SetActive(value: true);
		confirmDisplayText.text = confirmText;
		confirmYesButton.onClick.RemoveAllListeners();
		confirmYesButton.onClick.AddListener(delegate
		{
			confirmCallBack();
		});
		confirmNoButton.onClick.RemoveAllListeners();
		confirmNoButton.onClick.AddListener(delegate
		{
			confirmPopup.SetActive(value: false);
		});
		confirmFromMatter.Setup(from);
		confirmToMatter.Setup(to);
	}

	public void ShowBuyGachaTicketsPopup(MatterInfo neededTicketMatter, Action afterPurchase, PurchaseInfo convertToTicketPurchaseInfo)
	{
		int needRemainTicket = neededTicketMatter.count;
		MatterInfo currentMatterCost = convertToTicketPurchaseInfo.CurrentMatterCost(UIBase.applicationState);
		MatterInfo matterInfo = needRemainTicket * currentMatterCost;
		string confirmText = ScriptableObjectSingleton<TextDataPreset>.instance.term_gacha_gacha_ticket_not_enough_consume_arg0_count_arg1_matter_to_buy_arg2_count_arg3_matter.FormatWith(matterInfo.count, matterInfo.matterDef.matterName, needRemainTicket, neededTicketMatter.matterDef.matterName);
		SetupConfirmPopup(confirmText, delegate
		{
			if (convertToTicketPurchaseInfo.IsEnoughCost(UIBase.applicationState, needRemainTicket))
			{
				UIBase.applicationState.purchaseManager.DoPurchase(convertToTicketPurchaseInfo, needRemainTicket, delegate
				{
					confirmPopup.SetActive(value: false);
					afterPurchase?.Invoke();
				});
			}
			else
			{
				ShowJumpToShopToBuyMatterPopup(neededTicketMatter, currentMatterCost.matterDef);
			}
		}, matterInfo, neededTicketMatter);
	}

	public void ShowJumpToShopToBuyMatterPopup(MatterInfo neededTicketMatter, MatterDef matterDef)
	{
		PurchaseInfo convertToGachaTicketMatterPurchase = UIBase.applicationState.gachaManager.ConvertToGachaTicketMatterPurchase;
		MatterInfo matterInfo = neededTicketMatter.count * convertToGachaTicketMatterPurchase.CurrentMatterCost(UIBase.applicationState);
		string confirmText = ScriptableObjectSingleton<TextDataPreset>.instance.term_not_enough_matter_arg0_jump_to_purchase.FormatWith(matterDef.matterName);
		SetupConfirmPopup(confirmText, delegate
		{
			if (UIBase.fortressState.uiManager.purchaseUIPanel.TryShowPurchaseGroup(PurchaseGroupType.RealMoneyBuy))
			{
				confirmPopup.SetActive(value: false);
				base.gameObject.SetActive(value: false);
			}
		}, matterInfo, neededTicketMatter);
	}

	private void StartCheckCoroutine()
	{
		if (base.gameObject.activeInHierarchy && checkCoroutine == null)
		{
			checkCoroutine = StartCoroutine(Check());
		}
	}

	private void CancelCheckCoroutine()
	{
		if (checkCoroutine != null)
		{
			StopCoroutine(checkCoroutine);
			checkCoroutine = null;
		}
		isChecking = false;
	}

	private IEnumerator Check()
	{
		try
		{
			if (isChecking || targetState == selectedPool)
			{
				yield break;
			}
			isChecking = true;
			selectedPool = targetState;
			if (gachaSet != null)
			{
				yield return gachaSet.Hide();
				if (gachaSet != null)
				{
					DisposeUI(gachaSet);
					UnityEngine.Object.Destroy(gachaSet.gameObject);
					gachaSet = null;
				}
			}
			gachaSet = UnityEngine.Object.Instantiate(selectedPool.def.gachaSet, gachaSetParent);
			InitUI(gachaSet);
			gachaSet.Setup(selectedPool);
			SetDirty();
			yield return gachaSet.Show();
		}
		finally
		{
			GachaPanel gachaPanel = this;
			gachaPanel.isChecking = false;
			gachaPanel.checkCoroutine = null;
		}
		SetDirty();
	}
}
