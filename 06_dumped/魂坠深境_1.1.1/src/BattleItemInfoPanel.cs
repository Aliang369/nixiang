using System;
using System.Collections;
using Common.UI;
using DG.Tweening;
using UnityEngine;

public class BattleItemInfoPanel : UIBase, IInfoPanelController
{
	[SerializeField]
	public ItemInfoDisplay_Battle battleItemInfoDisplay;

	[SerializeField]
	private RectTransform _playerTablePivot;

	[SerializeField]
	private RectTransform _playerDrawPilePivot;

	[SerializeField]
	private RectTransform _playerWastePilePivot;

	[SerializeField]
	private RectTransform _eventSelectPilePivot;

	[SerializeField]
	private RectTransform _enemyPivot;

	[SerializeField]
	private RectTransform _companionPivot;

	[NonSerialized]
	private Item currentItem;

	[NonSerialized]
	private PivotManager.PivotType currentType;

	[NonSerialized]
	private bool playingShowInfoAnim;

	[NonSerialized]
	private PivotManager.PivotType showingType;

	[SerializeField]
	private ItemInfoDisplay_Battle itemInfoDisplayPrefab;

	[SerializeField]
	private RectTransform itemInfoDisplayParent;

	[NonSerialized]
	public CellUIPrefabPool<ItemInfoDisplay_Battle, Item> itemInfoDisplayPool;

	public ConvertablePivot playerTablePivot => new ConvertablePivot(_playerTablePivot, UIBase.gameState.UIManager.mainCanvas);

	public ConvertablePivot playerDrawPilePivot => new ConvertablePivot(_playerDrawPilePivot, UIBase.gameState.UIManager.mainCanvas);

	public ConvertablePivot playerWastePilePivot => new ConvertablePivot(_playerWastePilePivot, UIBase.gameState.UIManager.mainCanvas);

	public ConvertablePivot eventSelectPilePivot => new ConvertablePivot(_eventSelectPilePivot, UIBase.gameState.UIManager.mainCanvas);

	public ConvertablePivot enemyPivot => new ConvertablePivot(_enemyPivot, UIBase.gameState.UIManager.mainCanvas);

	public ConvertablePivot companionPivot => new ConvertablePivot(_companionPivot, UIBase.gameState.UIManager.mainCanvas);

	private Item showingItem => battleItemInfoDisplay.item;

	bool IInfoPanelController.HasShowingPanel()
	{
		return currentItem != null;
	}

	void IInfoPanelController.OnHideAllInfo()
	{
		Hide();
	}

	protected override void Init()
	{
		InitInfoAnim();
		InitActionAnim();
		UIBase.gameState.actionManager.actionFrameFinishEvent += _OnActionFrameFinishEvent;
		manager.RegisterUIHideAllInfoEvent(this);
	}

	protected override void Dispose()
	{
		UIBase.gameState.actionManager.actionFrameFinishEvent -= _OnActionFrameFinishEvent;
		manager.UnregisterUIHideAllInfoEvent(this);
	}

	protected override void UpdateDisplay()
	{
		UpdateInfoAnim();
	}

	private void _OnActionFrameFinishEvent()
	{
		SetDirty();
		battleItemInfoDisplay?.mainItemInfoDisplay?.SetDirty();
	}

	public void Show(Item item, PivotManager.PivotType type)
	{
		if (currentItem != item || currentType != type)
		{
			currentItem = item;
			currentType = type;
			SetDirty();
		}
	}

	public void Hide()
	{
		if (currentItem != null || currentType != PivotManager.PivotType.None)
		{
			currentItem = null;
			currentType = PivotManager.PivotType.None;
			SetDirty();
		}
	}

	private void InitInfoAnim()
	{
		currentItem = null;
		currentType = PivotManager.PivotType.None;
		showingType = PivotManager.PivotType.None;
		playingShowInfoAnim = false;
		InitUI(battleItemInfoDisplay);
	}

	private void UpdateInfoAnim()
	{
		if (currentItem != showingItem && !playingShowInfoAnim)
		{
			if (showingItem != null)
			{
				UIBase.gameState.StartCoroutine(HideItemInfoAsync);
			}
			else
			{
				UIBase.gameState.StartCoroutine(ShowItemInfoAsync);
			}
		}
	}

	private IEnumerator ShowItemInfoAsync()
	{
		playingShowInfoAnim = true;
		showingType = currentType;
		battleItemInfoDisplay.Setup(currentItem);
		battleItemInfoDisplay.SetDirty();
		battleItemInfoDisplay.mainItemInfoDisplay.gameObject.SetActive(value: false);
		MainItemInfoDisplayModule_BattleState battleStateModule = battleItemInfoDisplay.mainItemInfoDisplay.battleStateModule;
		Vector3 pivotPosition = GetPivotPosition(UIBase.gameState, currentType);
		Vector3 hidePosition = ItemInfoDisplayBase.AnimSetupInfo.GetHidePosition(UIBase.gameState, pivotPosition, battleStateModule.RectTransform());
		yield return battleStateModule.PlayTween(new ItemInfoDisplayBase.AnimSetupInfo
		{
			animType = ItemInfoDisplayBase.AnimSetupInfo.AnimType.Show,
			startShowPosition = hidePosition,
			stayPosition = pivotPosition,
			hidePosition = hidePosition,
			flip = false
		});
		battleItemInfoDisplay.ShowAllEffectMarkReference();
		battleItemInfoDisplay.GetModule<ItemInfoReferenceModule>().ShowAllSideReference();
		playingShowInfoAnim = false;
		SetDirty();
	}

	private IEnumerator HideItemInfoAsync()
	{
		playingShowInfoAnim = true;
		MainItemInfoDisplayModule_BattleState battleStateModule = battleItemInfoDisplay.mainItemInfoDisplay.battleStateModule;
		MainItemInfoFlyCrawlModule[] components = battleItemInfoDisplay.mainItemInfoDisplay.GetComponents<MainItemInfoFlyCrawlModule>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].BeginExitFlyAway();
		}
		battleItemInfoDisplay.GetModule<ItemInfoReferenceModule>().HideAllSideReference();
		battleItemInfoDisplay.HideAllEffectMarkReference();
		battleItemInfoDisplay.GetModule<ItemInfoReferenceModule>().UnsetItemReference();
		battleItemInfoDisplay.GetModule<ItemInfoReferenceModule>().UnsetCommonReference();
		Vector3 pivotPosition = GetPivotPosition(UIBase.gameState, showingType);
		Vector3 hidePosition = ItemInfoDisplayBase.AnimSetupInfo.GetHidePosition(UIBase.gameState, pivotPosition, battleStateModule.RectTransform());
		yield return battleStateModule.PlayTween(new ItemInfoDisplayBase.AnimSetupInfo
		{
			animType = ItemInfoDisplayBase.AnimSetupInfo.AnimType.Hide,
			startShowPosition = hidePosition,
			stayPosition = pivotPosition,
			hidePosition = hidePosition,
			flip = false
		});
		battleItemInfoDisplay.gameObject.SetActive(value: false);
		battleItemInfoDisplay.Unset();
		showingType = PivotManager.PivotType.None;
		playingShowInfoAnim = false;
		SetDirty();
	}

	public void StartTween(Item item, ItemInfoDisplayBase.AnimSetupInfo setupInfo, Action onFinish = null)
	{
		UIBase.gameState.StartCoroutine(() => PlayActionAnim(item, setupInfo, onFinish));
	}

	public void ConfigTween_Insert(Sequence sequence, ItemDisplayBase display, Item source, Vector3 endPos)
	{
		Item item = display.item;
		ItemInfoDisplay_Battle infoDisplay = itemInfoDisplayPool.Spawn(item);
		infoDisplay.enableInteract = false;
		infoDisplay.mainItemInfoDisplay.battleStateModule.ConfigSequence_Insert(sequence, display, source, endPos);
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			itemInfoDisplayPool.Despawn(infoDisplay);
		});
	}

	public void ConfigTween_Spell(Sequence sequence, ItemDisplayBase display, Item source, PivotManager.PivotType type)
	{
		Item item = display.item;
		ItemInfoDisplay_Battle infoDisplay = itemInfoDisplayPool.Spawn(item);
		infoDisplay.enableInteract = false;
		infoDisplay.mainItemInfoDisplay.battleStateModule.ConfigSequence_Spell(sequence, display, source, type);
		sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
		{
			itemInfoDisplayPool.Despawn(infoDisplay);
		});
		infoDisplay.SetDirty();
	}

	private void InitActionAnim()
	{
		itemInfoDisplayPrefab.gameObject.SetActive(value: false);
		itemInfoDisplayPool = GetPrefabPool<ItemInfoDisplay_Battle, Item>(itemInfoDisplayPrefab, itemInfoDisplayParent);
	}

	private IEnumerator PlayActionAnim(Item item, ItemInfoDisplayBase.AnimSetupInfo setupInfo, Action onFinish)
	{
		UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
		ItemInfoDisplay_Battle display = itemInfoDisplayPool.Spawn(item);
		display.enableInteract = setupInfo.interactable;
		yield return display.mainItemInfoDisplay.battleStateModule.PlayTween(setupInfo);
		itemInfoDisplayPool.Despawn(display);
		UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
		SetDirty();
		onFinish?.Invoke();
	}

	public static void TriggerPlayerItemAbilityAnim(GameStateInfo gameState, Item item)
	{
		Vector3 itemPosition = GetItemPosition(gameState, item);
		ItemInfoDisplayBase.AnimSetupInfo setupInfo = new ItemInfoDisplayBase.AnimSetupInfo
		{
			animType = ItemInfoDisplayBase.AnimSetupInfo.AnimType.Stay,
			startShowPosition = itemPosition,
			stayPosition = GetPivotPosition(gameState, PivotManager.PivotType.EnemyPivot),
			hidePosition = itemPosition,
			flip = true,
			interactable = false
		};
		gameState.UIManager.battleItemInfoPanel.StartTween(item, setupInfo);
	}

	public static void TriggerEventShowItemInfoAnim(GameStateInfo gameState, Item item)
	{
		Vector3 itemPosition = GetItemPosition(gameState, item);
		ItemInfoDisplayBase.AnimSetupInfo setupInfo = new ItemInfoDisplayBase.AnimSetupInfo
		{
			animType = ItemInfoDisplayBase.AnimSetupInfo.AnimType.Stay,
			startShowPosition = itemPosition,
			stayPosition = GetPivotPosition(gameState, PivotManager.PivotType.EnemyPivot),
			hidePosition = itemPosition,
			flip = true,
			interactable = false
		};
		gameState.UIManager.battleItemInfoPanel.StartTween(item, setupInfo);
	}

	public static void TriggerCompanionShowItemInfoAnim(GameStateInfo gameState, Item item)
	{
		MainItemInfoDisplayModule_BattleState battleStateModule = gameState.UIManager.battleItemInfoPanel.battleItemInfoDisplay.mainItemInfoDisplay.battleStateModule;
		Vector3 itemPosition = GetItemPosition(gameState, item);
		Vector3 pivotPosition = GetPivotPosition(gameState, PivotManager.PivotType.CompanionPivot);
		Vector3 hidePosition = ItemInfoDisplayBase.AnimSetupInfo.GetHidePosition(gameState, pivotPosition, battleStateModule.RectTransform(), left: false);
		ItemInfoDisplayBase.AnimSetupInfo setupInfo = new ItemInfoDisplayBase.AnimSetupInfo
		{
			animType = ItemInfoDisplayBase.AnimSetupInfo.AnimType.Stay,
			startShowPosition = itemPosition,
			stayPosition = pivotPosition,
			hidePosition = hidePosition,
			flip = false,
			interactable = false
		};
		gameState.UIManager.battleItemInfoPanel.StartTween(item, setupInfo);
	}

	private static Vector3 GetPivotPosition(GameStateInfo gameState, PivotManager.PivotType type)
	{
		return ItemPosition.GetCanvasWorldPosition(gameState, type, CanvasType.MainCanvas) ?? Vector3.zero;
	}

	private static Vector3 GetItemPosition(GameStateInfo gameState, Item item)
	{
		return item.GetCanvasWorldPosition(gameState, CanvasType.MainCanvas) ?? Vector3.zero;
	}
}
