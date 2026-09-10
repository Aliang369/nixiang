using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class AfterBossUpgradeEventPanel : WorldEventPanel
{
	private enum AnimState
	{
		Null,
		Idle,
		AfterBoss
	}

	private static readonly int animatorParameter_State = Animator.StringToHash("State");

	private static readonly int animatorParameter_Reset = Animator.StringToHash("Reset");

	private const float RootBlessLeaveButtonScale = 0.75f;

	private static readonly Vector2 RootBlessLeaveButtonOffset = new Vector2(54f, -42f);

	[SerializeField]
	public TextMeshProUGUI titleText;

	[SerializeField]
	public TextMeshProUGUI descriptionText;

	[SerializeField]
	private WorldEventItemChoiceDisplay choicePrefab;

	[SerializeField]
	public AfterBossUpgradePanel afterBossUpgradePanel;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private LeaveButton leaveButton;

	[SerializeField]
	private GameButton showHintPanelButton;

	[SerializeField]
	private UIBase[] childrenUI;

	[NonSerialized]
	private CellUIPrefabPool<WorldEventItemChoiceDisplay, WorldEventChoice> choicePool;

	[NonSerialized]
	private AnimState animState;

	[NonSerialized]
	private IAfterBossUpgradeSource upgradeSource;

	[NonSerialized]
	public bool upgraded;

	[NonSerialized]
	private bool viewed;

	[NonSerialized]
	private RectTransform leaveButtonRect;

	[NonSerialized]
	private Vector2 leaveButtonDefaultAnchoredPosition;

	[NonSerialized]
	private Vector3 leaveButtonDefaultLocalScale;

	[NonSerialized]
	private bool leaveButtonLayoutCached;

	private bool UsesDirectLeaveUpgradeLayout
	{
		get
		{
			WorldEvent worldEvent = base.worldEvent;
			if (worldEvent is RootBlessUpgradeEvent || worldEvent is AfterBossWorldEvent)
			{
				return animState == AnimState.AfterBoss;
			}
			return false;
		}
	}

	protected override void Init()
	{
		base.Init();
		choicePrefab.gameObject.SetActive(value: false);
		GetPrefabPool(ref choicePool, choicePrefab, choicePrefab.transform.parent);
		LeaveButton obj = leaveButton;
		obj.onLeave = (Action)Delegate.Combine(obj.onLeave, new Action(_OnLeaveButtonClicked));
		InitUI(leaveButton);
		InitUI(afterBossUpgradePanel);
		CacheLeaveButtonLayout();
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
		upgradeSource = worldEvent as IAfterBossUpgradeSource;
		ApplyRootBlessLeaveButtonLayout(enabled: false);
		animator.SetTrigger(animatorParameter_Reset);
		animator.SetInteger(animatorParameter_State, 1);
		animState = AnimState.Idle;
		afterBossUpgradePanel.Setup(upgradeSource);
		SetDirty();
		viewed = false;
		upgraded = false;
	}

	public override void Unset()
	{
		ApplyRootBlessLeaveButtonLayout(enabled: false);
		base.Unset();
		afterBossUpgradePanel.Unset();
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		worldEvent.UpdateWorldEventDisplayInfo();
		titleText.text = worldEvent.currentDisplayInfo.title;
		descriptionText.text = worldEvent.currentDisplayInfo.description;
		choicePool.Sync(worldEvent.currentDisplayInfo.choices);
		animator.SetInteger(animatorParameter_State, (int)animState);
		if (worldEvent.currentDisplayInfo.showLeaveButton || UsesDirectLeaveUpgradeLayout)
		{
			leaveButton.Show();
			if (UsesDirectLeaveUpgradeLayout)
			{
				leaveButton.transform.SetAsLastSibling();
			}
		}
		else
		{
			leaveButton.Hide();
		}
		ApplyRootBlessLeaveButtonLayout(UsesDirectLeaveUpgradeLayout);
		RecordVisibleBossBlessOptions();
	}

	public void RecordVisibleBossBlessOptions()
	{
		if (animState == AnimState.AfterBoss && worldEvent is AfterBossWorldEvent afterBossWorldEvent)
		{
			afterBossWorldEvent.RecordDisplayedBossBlessOptions();
		}
	}

	public override bool HasAnim()
	{
		return leaveButton.HasAnim();
	}

	public void TriggerUpgradeChoice(AfterBossUpgrade upgrade)
	{
		if (upgrade == null)
		{
			TraceLog.Warn("[AfterBossUpgradeEventPanel] Ignored null after-boss upgrade choice.");
		}
		else if (!upgraded)
		{
			upgraded = true;
			upgrade.Trigger(UIBase.gameState);
			upgradeSource?.OnUpgradeTriggered(upgrade);
			ThinkingAnalyticsExtends.TrackAfterBossUpgrade(UIBase.gameState, upgrade);
			if (worldEvent is RootBlessUpgradeEvent { UpgradeFinished: not false })
			{
				worldEvent.FinishedWorldEvent();
				return;
			}
			animState = AnimState.Idle;
			SetDirty();
		}
	}

	public void ToBossUpgrade()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Bootstrap.instance.audioManager.PlaySustained(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_enter_blessed_root, "BgmTag");
		viewed = true;
		animState = AnimState.AfterBoss;
		SetDirty();
	}

	public void _OnBackButtonClicked()
	{
		animState = AnimState.Idle;
		SetDirty();
	}

	private void _OnLeaveButtonClicked()
	{
		if (UIBase.gameState.showConfirm && ShouldConfirmLeave())
		{
			UIBase.gameState.UIManager.leaveConfirmPanel.Setup(new LeaveConfirmPanel.Info
			{
				text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unview_influence,
				onConfirm = worldEvent.FinishedWorldEvent,
				onCancel = delegate
				{
					leaveButton.Show();
				}
			});
		}
		else
		{
			worldEvent.FinishedWorldEvent();
		}
	}

	private bool ShouldConfirmLeave()
	{
		if (animState != AnimState.AfterBoss)
		{
			return !viewed;
		}
		if (worldEvent is RootBlessUpgradeEvent rootBlessUpgradeEvent)
		{
			return !rootBlessUpgradeEvent.UpgradeFinished;
		}
		if (worldEvent is AfterBossWorldEvent afterBossWorldEvent)
		{
			return !afterBossWorldEvent.upgradeFinished;
		}
		return !viewed;
	}

	private void CacheLeaveButtonLayout()
	{
		if (!leaveButtonLayoutCached && !(leaveButton == null))
		{
			leaveButtonRect = leaveButton.transform as RectTransform;
			if (!(leaveButtonRect == null))
			{
				leaveButtonDefaultAnchoredPosition = leaveButtonRect.anchoredPosition;
				leaveButtonDefaultLocalScale = leaveButtonRect.localScale;
				leaveButtonLayoutCached = true;
			}
		}
	}

	private void ApplyRootBlessLeaveButtonLayout(bool enabled)
	{
		CacheLeaveButtonLayout();
		if (leaveButtonLayoutCached)
		{
			leaveButtonRect.anchoredPosition = (enabled ? (leaveButtonDefaultAnchoredPosition + RootBlessLeaveButtonOffset) : leaveButtonDefaultAnchoredPosition);
			leaveButtonRect.localScale = (enabled ? (leaveButtonDefaultLocalScale * 0.75f) : leaveButtonDefaultLocalScale);
		}
	}
}
