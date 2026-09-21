using System;
using Common.UI;
using UnityEngine;

public class CrystalUpgradeEventPanel : WorldEventPanel
{
	private enum AnimState
	{
		Null,
		Idle,
		Crystal
	}

	private static readonly int animatorParameter_State = Animator.StringToHash("State");

	private static readonly int animatorParameter_Reset = Animator.StringToHash("Reset");

	[SerializeField]
	private WorldEventChoiceButtonDisplay choicePrefab;

	[SerializeField]
	public CrystalUpgradePanel crystalUpgradePanel;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private LeaveButton leaveButton;

	[SerializeField]
	private UIBase[] childrenUI;

	[NonSerialized]
	private CellUIPrefabPool<WorldEventChoiceButtonDisplay, WorldEventChoice> choicePool;

	[NonSerialized]
	private AnimState animState;

	[NonSerialized]
	public CrystalUpgradeWorldEvent crystalUpgradeWorldEvent;

	public bool IsSelectable => animState == AnimState.Crystal;

	private bool crystalUpgraded => crystalUpgradeWorldEvent.upgraded.Count > 0;

	protected override void Init()
	{
		base.Init();
		choicePrefab.gameObject.SetActive(value: false);
		choicePool = GetPrefabPool<WorldEventChoiceButtonDisplay, WorldEventChoice>(choicePrefab, choicePrefab.transform.parent);
		LeaveButton obj = leaveButton;
		obj.onLeave = (Action)Delegate.Combine(obj.onLeave, new Action(_OnLeaveButtonClicked));
		InitUI(leaveButton);
		InitUI(crystalUpgradePanel);
		UIBase[] array = childrenUI;
		foreach (UIBase child in array)
		{
			InitUI(child);
		}
		animator.keepAnimatorStateOnDisable = true;
	}

	public override void Setup(WorldEvent worldEvent)
	{
		base.Setup(worldEvent);
		crystalUpgradeWorldEvent = worldEvent as CrystalUpgradeWorldEvent;
		animator.speed = UIBase.applicationState.settingManager.GetUIScale();
		animator.SetTrigger(animatorParameter_Reset);
		animator.SetInteger(animatorParameter_State, 1);
		animState = AnimState.Idle;
		crystalUpgradePanel.Setup(crystalUpgradeWorldEvent);
		leaveButton.Show();
		SetDirty();
	}

	public override void Unset()
	{
		base.Unset();
		crystalUpgradePanel.Unset();
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		worldEvent.UpdateWorldEventDisplayInfo();
		UpdateChoiceButton();
		animator.speed = UIBase.applicationState.settingManager.GetUIScale();
		animator.SetInteger(animatorParameter_State, (int)animState);
	}

	public void TriggerUpgradeChoice(CrystalUpgrade upgrade)
	{
		Action successFallback = delegate
		{
			if (crystalUpgradeWorldEvent.EndlessCrystalUpgrade)
			{
				crystalUpgradeWorldEvent.ReRollUpgrades(withCost: false);
			}
			ThinkingAnalyticsExtends.TrackCrystalUpgrade(UIBase.gameState, upgrade);
		};
		upgrade.Trigger(UIBase.gameState, successFallback);
		crystalUpgradeWorldEvent.upgraded.Add(upgrade);
		animState = AnimState.Idle;
		SetDirty();
	}

	public override bool HasAnim()
	{
		return leaveButton.HasAnim();
	}

	private void UpdateChoiceButton()
	{
		choicePool.DespawnAll();
		foreach (WorldEventChoice item in worldEvent.currentDisplayInfo.choices.OrEmptyIfNull())
		{
			choicePool.Spawn(item).GetComponent<CanvasGroup>().alpha = 0f;
		}
		if (!crystalUpgraded || crystalUpgradeWorldEvent.EndlessCrystalUpgrade)
		{
			choicePool.Spawn(new WorldEventChoice
			{
				buttonText = crystalUpgradeWorldEvent.confirmText,
				descriptionText = crystalUpgradeWorldEvent.confirmDescriptionText,
				executeAction = _OnOpenSelectAreaButtonClicked
			});
		}
		ClavisDef clavis = crystalUpgradeWorldEvent.crystalUpgradeConfig.clavis;
		if (!UIBase.applicationState.clavisManager.IsActivated(clavis))
		{
			choicePool.Spawn(new WorldEventChoice
			{
				buttonText = clavis.GetMajorText(UIBase.applicationState),
				descriptionText = clavis.GetMinorText(UIBase.applicationState),
				executeAbleChecker = () => UIBase.applicationState.clavisManager.IsUnlocked(clavis),
				executeAction = _OnClavisButtonClicked
			});
		}
	}

	private void _OnClavisButtonClicked()
	{
		UIBase.applicationState.clavisManager.ActivatedClavis(crystalUpgradeWorldEvent.crystalUpgradeConfig.clavis, silent: false, SetDirty);
	}

	private void _OnOpenSelectAreaButtonClicked()
	{
		animState = AnimState.Crystal;
		SetDirty();
	}

	public void _OnBackButtonClicked()
	{
		animState = AnimState.Idle;
		SetDirty();
	}

	private void _OnLeaveButtonClicked()
	{
		crystalUpgradeWorldEvent.FinishedWorldEvent();
	}
}
