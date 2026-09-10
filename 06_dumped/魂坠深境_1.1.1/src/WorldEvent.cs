using System;
using System.Collections.Generic;
using OdinEx;
using UnityEngine;

public abstract class WorldEvent : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	[Serializable]
	public class WorldEventChoiceText
	{
		[LocalizeText("事件选项名称")]
		[TextAreaProp(1, 5, false, 0)]
		public string choiceName;

		[LocalizeText("事件选项描述")]
		[TextAreaProp(1, 5, false, 0)]
		public string choiceDescription;

		[LocalizeText("事件选项后描述")]
		[TextAreaProp(1, 5, false, 0)]
		public string chosenDescription;
	}

	[LocalizeText("事件名字")]
	[CTSOAssetName]
	public string EventName;

	[TextArea]
	[LocalizeText("事件描述")]
	public string EventDescription;

	[SerializeField]
	public RoomRef eventRoomRef;

	public RoomObjectDisplay eventDisplayPrefab;

	public RoomObjectDisplay secondaryEventDisplayPrefab;

	public Sprite backgroundImage;

	public bool toGenerate;

	public bool characterConstraint;

	public List<CharacterClassType> attachClasses;

	public bool globalGenerate;

	[NonSerialized]
	public WorldEventDisplayInfo currentDisplayInfo;

	[NonSerialized]
	public int lastSelectChoiceIndex;

	[NonSerialized]
	public WorldEventChoice lastSelectedChoice;

	[NonSerialized]
	public Action onTextShown;

	[NonSerialized]
	public GameStateInfo gameState;

	[NonSerialized]
	public bool finishing;

	[NonSerialized]
	public bool finished;

	[NonSerialized]
	public long initTime;

	[NonSerialized]
	public Dictionary<string, object> tempValues = new Dictionary<string, object>();

	public override string CTAssetIDPrepend => "WE";

	public override Type CTAssetType => typeof(WorldEvent);

	public virtual WorldEventDisplayType displayType => WorldEventDisplayType.Default;

	public virtual bool KeepTitleAfterChoice => false;

	protected PathLocation EventCurrentLocation => gameState.dungeonMapManager.currentLocation;

	public virtual WorldEventPanel GetDisplayWorldEventPanel(GameStateInfo gameState)
	{
		return gameState.UIManager.worldEventPanel_Normal;
	}

	protected virtual void Init()
	{
		tempValues.Clear();
		currentDisplayInfo = GenerateDefaultWorldEventDisplayInfo();
		lastSelectChoiceIndex = -1;
		lastSelectedChoice = null;
	}

	public void SetupAndShow(GameStateInfo gameState)
	{
		this.gameState = gameState;
		Init();
		initTime = gameState.gameTimer.GetTime();
		UpdateWorldEventDisplayInfo();
		WorldEventPanel displayWorldEventPanel = GetDisplayWorldEventPanel(gameState);
		gameState.UIManager.currentEventPanel = displayWorldEventPanel;
		if (gameState.UIManager.currentEventPanel != null)
		{
			gameState.UIManager.currentEventPanel.Setup(this);
		}
		if (eventDisplayPrefab != null)
		{
			gameState.currentEncounter.SpawnRoomObjectDisplay(eventDisplayPrefab, major: true);
		}
		if ((bool)secondaryEventDisplayPrefab)
		{
			gameState.currentEncounter.SpawnRoomObjectDisplay(secondaryEventDisplayPrefab, major: false);
		}
		if (toGenerate)
		{
			gameState.recordManager.TrackJournalRecord(new EncounterWorldEventRecord(this));
		}
	}

	public virtual void FinishedWorldEvent()
	{
		FinishedWorldEvent(true);
	}

	public void FinishedWorldEvent(bool exitRoom = true)
	{
		if (!finishing)
		{
			finishing = true;
			UIHelper.HideAllInfo(null);
			EnqueueAction(new FinishedWorldEventAction(this, exitRoom));
		}
	}

	public void NotifyEvent()
	{
		Notify();
	}

	protected virtual void Notify()
	{
	}

	public virtual void UpdateWorldEventDisplayInfo()
	{
		UpdateDisplayInfo(currentDisplayInfo);
	}

	private WorldEventDisplayInfo GenerateDefaultWorldEventDisplayInfo()
	{
		return new WorldEventDisplayInfo
		{
			title = EventName,
			description = EventDescription
		};
	}

	protected virtual void UpdateDisplayInfo(WorldEventDisplayInfo displayInfo)
	{
	}

	public virtual bool CheckWorldEventGenerateAble(GameStateInfo gameState)
	{
		if (!toGenerate)
		{
			return false;
		}
		if (obsolate)
		{
			return false;
		}
		if (characterConstraint && !attachClasses.Contains(gameState.playerState.CharacterClass))
		{
			return false;
		}
		if (!gameState.contractState.currentLevel.worldEventPool.Contains(this) && !globalGenerate)
		{
			return false;
		}
		return true;
	}

	protected void EnqueueAction(GameAction action)
	{
		gameState?.actionManager?.EnqueueAction(action);
	}

	public void EnqueueGetMultiElementAction(MultiInfoDisplayPanel.SetupInfo setupInfo)
	{
		EnqueueAction(new GatherMultiElementsAction(setupInfo));
	}

	protected void SetPanelDirty()
	{
		WorldEventPanel currentEventPanel = gameState.UIManager.currentEventPanel;
		if (currentEventPanel != null)
		{
			currentEventPanel.SetDirty();
		}
	}

	public virtual void RefreshAfterChoose(string afterChooseDescription, List<WorldEventChoice> newChoices = null, bool showLeaveButton = true)
	{
		currentDisplayInfo.ClearChoice();
		if (newChoices != null)
		{
			currentDisplayInfo.choices = newChoices;
		}
		currentDisplayInfo.description = afterChooseDescription;
		currentDisplayInfo.showLeaveButton = showLeaveButton;
		SetPanelDirty();
	}

	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public new virtual List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
