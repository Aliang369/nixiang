using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaWishSelectPanel : UIBase, ISelectablePanel
{
	public GlobalItemDisplay wishGlobalItemDisplayPrefab;

	public GameButton backButton;

	public GameButton confirmButton;

	public GameButton viewCharacterInfoButton;

	public GameObject characterInfoArea;

	public Image icon;

	public TextMeshProUGUI nameText;

	public TextMeshProUGUI descriptionText;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> wishPool;

	[NonSerialized]
	private GlobalItemDisplay selected;

	[NonSerialized]
	private GachaPoolState poolState;

	[NonSerialized]
	private GachaPanel parentPanel;

	private PlayerCharacterDef selectedCharacter => selected?.setupData?.GlobalItemRefPlayerCharacter;

	protected override void Init()
	{
		GetPrefabPool(ref wishPool, wishGlobalItemDisplayPrefab);
		parentPanel = GetUIInParents<GachaPanel>();
		confirmButton.onClick.AddListener(delegate
		{
			if (GlobalConfigManager.instance.enableNetwork)
			{
				SetGachaPoolWishTargetRequest setGachaPoolWishTargetRequest = new SetGachaPoolWishTargetRequest();
				setGachaPoolWishTargetRequest.GachaPoolId = poolState.def.FullID;
				setGachaPoolWishTargetRequest.WishTargetId = (selected.setupData as MatterInfo)?.GetAttachedPlayerCharacterDef(UIBase.applicationState).FullID;
				new GeneralGameAsyncOperationRequestOnly<SetGachaPoolWishTargetRequest>(setGachaPoolWishTargetRequest, UIBase.applicationState.ChillyRoomSDK.Purchase.SetGachaPoolWishTargetAsync).OnComplete(AfterConfirmWish).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_setting_gacha_wish);
			}
		});
		viewCharacterInfoButton.onClick.AddListener(delegate
		{
			if (selectedCharacter != null)
			{
				UIBase.fortressState.uiManager.characterInfoBasePanel.ShowUI(selectedCharacter.GetState(UIBase.applicationState));
			}
		});
		backButton.onClick.AddListener(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		void AfterConfirmWish()
		{
			poolState.wishCharacter = (selected.setupData as MatterInfo)?.GetAttachedPlayerCharacterDef(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackFortressGachaSetWish(UIBase.applicationState, poolState.def, poolState.wishCharacter);
			base.gameObject.SetActive(value: false);
			parentPanel.SetDirty();
		}
	}

	public void Setup(GachaPoolState poolState)
	{
		this.poolState = poolState;
		List<IGlobalItemDisplaySetup> list = (from c in poolState.def.baseGachaWeightContents.Keys.OfType<PlayerCharacterDef>()
			select c.obsession into c
			select new MatterInfo(c, 1)).Cast<IGlobalItemDisplaySetup>().ToList();
		wishPool.Sync(list);
	}

	protected override void UpdateDisplay()
	{
		confirmButton.interactable = selected != null;
		characterInfoArea.gameObject.SetActive(value: false);
		if (selectedCharacter != null)
		{
			characterInfoArea.gameObject.SetActive(value: true);
			icon.sprite = selectedCharacter.headIcon;
			nameText.text = selectedCharacter.characterName;
			descriptionText.text = selectedCharacter.description;
		}
	}

	public void Select(SelectableModuleBase module)
	{
		selected = module.parent as GlobalItemDisplay;
		SetDirty();
	}

	public void Unselect(SelectableModuleBase module)
	{
		selected = null;
		SetDirty();
	}

	public bool IsSelected(SelectableModuleBase module)
	{
		return selected == module.parent as GlobalItemDisplay;
	}
}
