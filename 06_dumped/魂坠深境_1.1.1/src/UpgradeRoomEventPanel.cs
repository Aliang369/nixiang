using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class UpgradeRoomEventPanel : WorldEventPanel
{
	public enum AnimState
	{
		Null,
		Idle,
		Upgrade
	}

	[Serializable]
	public class Config
	{
		[SerializeField]
		public float openPanelTime1;

		[SerializeField]
		public float openPanelTime2;

		[SerializeField]
		public float openPanelTime3;

		[SerializeField]
		public float upgradeTime1;

		[SerializeField]
		public float upgradeTime2;

		[SerializeField]
		public float upgradeScale;
	}

	private static readonly int animatorParameter_State = Animator.StringToHash("State");

	private static readonly int animatorParameter_Reset = Animator.StringToHash("Reset");

	[SerializeField]
	public UpgradePanel_Event upgradePanel;

	[SerializeField]
	private LeaveButton exitButton;

	[SerializeField]
	private GameButton openUpgradeButton;

	[SerializeField]
	private GameButton skinUpgradeButton;

	[SerializeField]
	private TextMeshProUGUI setSkinAbleItemCountText;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	public AnimState animState;

	[NonSerialized]
	private UpgradeRoomEvent upgradeRoomEvent;

	[NonSerialized]
	private ItemDisplayBase currentItemDisplayBase;

	[NonSerialized]
	private bool exiting;

	public bool IsAiUpgradePanelActive
	{
		get
		{
			if (base.gameObject.activeInHierarchy)
			{
				return animState == AnimState.Upgrade;
			}
			return false;
		}
	}

	protected override void Init()
	{
		base.Init();
		InitUI(upgradePanel);
		InitUI(exitButton);
		LeaveButton leaveButton = exitButton;
		leaveButton.onLeave = (Action)Delegate.Combine(leaveButton.onLeave, new Action(_OnExitButtonClicked));
		openUpgradeButton.onClick.AddListener(_OnOpenUpgradeButtonClicked);
		skinUpgradeButton.onClick.AddListener(_OnSkinUpgradeButtonClicked);
		skinUpgradeButton.onNotInteractableClick.AddListener(_OnSkinUpgradeButtonLockedClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(SetDirty);
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(SetDirty);
	}

	public override void Setup(WorldEvent worldEvent)
	{
		base.Setup(worldEvent);
		upgradeRoomEvent = (UpgradeRoomEvent)worldEvent;
		upgradePanel.Setup();
		exiting = false;
		animState = AnimState.Idle;
		animator.speed = base.uiTimeScale;
		animator.SetTrigger(animatorParameter_Reset);
		animator.SetInteger(animatorParameter_State, (int)animState);
		exitButton.Show();
		SetDirty();
	}

	public override void Unset()
	{
		base.Unset();
		upgradePanel.Unset();
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		worldEvent.UpdateWorldEventDisplayInfo();
		animator.speed = base.uiTimeScale;
		animator.SetInteger(animatorParameter_State, (int)animState);
		if (!exitButton.isEnable && !exiting && !exitButton.HasAnim())
		{
			exitButton.Show();
			TraceLog.Error($"E1 - {animState} - {GameButton.enableAll} - {UIBase.gameState.actionManager.isEmpty}");
		}
		int num = UIBase.gameState.playerState.DeckContainer.items.Count((Item it) => UIBase.applicationState.itemSkinManager.GetAllUnlockedSkinsOfItem(it.def).Any());
		setSkinAbleItemCountText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_deck_set_skin_able_item_count_arg0.FormatWith(num);
		bool variableFlag = UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableUpgradeRoomSetSkin);
		skinUpgradeButton.gameObject.SetActive(variableFlag);
		skinUpgradeButton.interactable = variableFlag;
		setSkinAbleItemCountText.gameObject.SetActive(variableFlag);
	}

	public override bool HasAnim()
	{
		return exitButton.HasAnim();
	}

	private void _OnOpenUpgradeButtonClicked()
	{
		upgradePanel.SetMode(isSkinUpgrade: false);
		animState = AnimState.Upgrade;
		SetDirty();
	}

	public bool CanOpenCardUpgradeForAiSession()
	{
		if (base.gameObject.activeInHierarchy && animState == AnimState.Idle)
		{
			return upgradePanel.HasAiUpgradableItem();
		}
		return false;
	}

	public void OpenCardUpgradeForAiSession()
	{
		if (CanOpenCardUpgradeForAiSession())
		{
			_OnOpenUpgradeButtonClicked();
		}
	}

	private void _OnSkinUpgradeButtonClicked()
	{
		if (!UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableUpgradeRoomSetSkin))
		{
			ShowSkinUpgradeLockedHint();
			return;
		}
		upgradePanel.SetMode(isSkinUpgrade: true);
		animState = AnimState.Upgrade;
		SetDirty();
	}

	private void _OnSkinUpgradeButtonLockedClicked()
	{
		ShowSkinUpgradeLockedHint();
	}

	private void ShowSkinUpgradeLockedHint()
	{
		UIBase.gameState.UIManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_item_skin_entry_locked);
	}

	private void _OnExitButtonClicked()
	{
		try
		{
			exiting = true;
			UIHelper.DisableAllButton();
			ThinkingAnalyticsExtends.TrackUpgrade(UIBase.gameState, upgradeRoomEvent);
			worldEvent.FinishedWorldEvent();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		UIBase.gameState.actionManager.EnqueueAction(new ActionAction(UIHelper.EnableAllButton));
	}
}
