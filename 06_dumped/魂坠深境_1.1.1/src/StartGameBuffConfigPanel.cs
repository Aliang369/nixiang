using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using GameRandom;
using TMPro;
using UnityEngine;

public class StartGameBuffConfigPanel : UIBase, IShowAsync, IHideAsync, IFrameUpdate
{
	public GameButton confirmButton;

	public GameButton cancelButton;

	public StartGameBuffCell configSlot;

	public StartGameBuffCell buffPrefab;

	public TextMeshProUGUI buffGroupNameText;

	public bool taskPanding;

	public Animator animator;

	public DialoguePlayer dialoguePlayer;

	public GameObject[] confirmHideArea;

	public GameObject[] cancelHideArea;

	private CellUIPrefabPool<StartGameBuffCell, StartGameBuff> pool;

	private CellUIPrefabPool<MatterCell, MatterInfo> matterPool;

	private float idleDialogueTimer;

	private StartGameBuff currentAllUsingBuffs => configSlot.setupData;

	private StartGameBuffConfigPanelConfig config => ScriptableObjectSingleton<GameDataManager>.instance.startGameBuffConfigPanelConfig;

	protected override void Init()
	{
		InitUI(configSlot);
		configSlot.gameObject.SetActive(value: false);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
		pool = GetPrefabPool<StartGameBuffCell, StartGameBuff>(buffPrefab);
		if (buffGroupNameText != null)
		{
			buffGroupNameText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_start_game_buff_group_name;
		}
		taskPanding = false;
		((Behaviour)(object)animator).enabled = false;
	}

	protected override void UpdateDisplay()
	{
		List<StartGameBuff> currentStartGameBuffs = UIBase.applicationState.startGameBuffManager.currentStartGameBuffs;
		UpdateCell(pool, buffPrefab.transform.parent.gameObject, currentStartGameBuffs.Where(BuffCheck).ToList());
		confirmButton.gameObject.SetActive(configSlot.gameObject.activeInHierarchy);
		confirmButton.interactable = configSlot.gameObject.activeInHierarchy && configSlot.buff.IsActivatable(UIBase.applicationState);
		static bool BuffCheck(StartGameBuff b)
		{
			return b.IsActivatable(UIBase.applicationState);
		}
		static int Sort(StartGameBuff a, StartGameBuff b)
		{
			int num = (a.IsActivatable(UIBase.applicationState) ? 1 : 0);
			int num2 = (b.IsActivatable(UIBase.applicationState) ? 1 : 0);
			if (num != num2)
			{
				return num2 - num;
			}
			int num3 = a.expireDayPeriod.CompareTo(b.expireDayPeriod);
			if (num3 != 0)
			{
				return num3;
			}
			int num4 = b.def.quality.CompareTo(a.def.quality);
			if (num4 != 0)
			{
				return num4;
			}
			int num5 = a.def.className.CompareTo(b.def.className);
			if (num5 != 0)
			{
				return num5;
			}
			return a.def.quality.CompareTo(b.def.quality);
		}
		static void UpdateCell(CellUIPrefabPool<StartGameBuffCell, StartGameBuff> cellPool, GameObject parent, List<StartGameBuff> list)
		{
			list.Sort(Sort);
			parent.SetActive(list.Count > 0);
			cellPool.Sync(list);
		}
	}

	public void Setup()
	{
		manager.ShowUI(this);
	}

	IEnumerator IShowAsync.ShowUIAsync()
	{
		confirmButton.interactable = false;
		cancelButton.interactable = false;
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.Play("Show");
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		UIBase.gameState.UIManager.dialoguePanel.TriggerDialogue(config.enterDialogue, dialoguePlayer, null, setTransform: true);
		UIBase.applicationState.eventManager.storyEvent.TriggerEvent(StoryEventObserver.Type.SCJ_002_n);
		confirmButton.interactable = true;
		cancelButton.interactable = true;
	}

	IEnumerator IHideAsync.HideUIAsync()
	{
		((Behaviour)(object)animator).enabled = true;
		animator.Play("Hide");
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		base.gameObject.SetActive(value: false);
		UIBase.gameState.UIManager.dialoguePanel.StopAllDialogueOfPlayer(dialoguePlayer);
	}

	void IFrameUpdate.FrameUpdate()
	{
		if (base.gameObject.activeInHierarchy && !dialoguePlayer.IsPlaying(UIBase.gameState))
		{
			idleDialogueTimer += Time.deltaTime;
			if (idleDialogueTimer > config.idleDialogueConfig.interval)
			{
				idleDialogueTimer = 0f;
				DialogueSequence sequence = config.idleDialogueConfig.idleDialogues.SelectRandom();
				UIBase.gameState.UIManager.dialoguePanel.TriggerDialogue(sequence, dialoguePlayer, null, setTransform: true);
			}
		}
	}

	public void SetStartGameBuffCell(StartGameBuff buff)
	{
		configSlot.Setup(buff);
		configSlot.gameObject.SetActive(value: true);
		SetDirty();
	}

	private void _OnConfirmButtonClicked()
	{
		if (configSlot.gameObject.activeInHierarchy)
		{
			confirmButton.interactable = false;
			cancelButton.interactable = false;
			UIBase.gameState.UIManager.dialoguePanel.TriggerDialogue(config.confirmDialogue, dialoguePlayer, null, setTransform: true);
			UIBase.gameState.StartCoroutine(Confirm);
		}
		IEnumerator Confirm()
		{
			GameObject[] array = confirmHideArea;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
			UIBase.gameState.actionManager.EnqueueAction(new ActivateStartGameBuffAction(currentAllUsingBuffs));
			ThinkingAnalyticsExtends.TrackActivateStartGameBuff(UIBase.gameState, new List<StartGameBuff> { currentAllUsingBuffs });
			yield return new WaitUntil(() => UIBase.gameState.actionManager.isEmpty);
			UIBase.applicationState.savingManager.SetProfileDirtyToSave();
			yield return End();
		}
	}

	private void _OnCancelButtonClicked()
	{
		GameObject[] array = cancelHideArea;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		confirmButton.interactable = false;
		cancelButton.interactable = false;
		UIBase.gameState.UIManager.dialoguePanel.TriggerDialogue(config.cancelDialogue, dialoguePlayer, null, setTransform: true);
		UIBase.gameState.StartCoroutine(End);
	}

	private IEnumerator End()
	{
		yield return new WaitUntil(() => !TaskRunning());
		taskPanding = false;
		manager.HideUI(this);
		UIHelper.EnableAllButton();
		confirmButton.interactable = true;
		cancelButton.interactable = true;
		bool TaskRunning()
		{
			if (!UIBase.gameState.actionManager.isEmpty)
			{
				return true;
			}
			if (dialoguePlayer.IsPlaying(UIBase.gameState))
			{
				return true;
			}
			if (UIBase.gameState.UIManager.mintageRunePanel_Select.gameObject.activeInHierarchy)
			{
				return true;
			}
			if (UIBase.gameState.UIManager.multiInfoDisplayPanel.gameObject.activeInHierarchy)
			{
				return true;
			}
			return false;
		}
	}
}
