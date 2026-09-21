using System;
using Common.UI;
using ContainerOfSoul.Display.Game.PathLocationModules;

public class PathLocationDisplay : CellUI<PathLocation>, IDisplayModulesController
{
	public enum State
	{
		None,
		Disabled,
		Idle,
		Current_Unfinished,
		Current_Finished,
		Finished,
		Selectable,
		Focused
	}

	public abstract class DisplayModule : UIDisplayModule
	{
		protected PathLocationDisplay pathLocationDisplay => parent as PathLocationDisplay;
	}

	[NonSerialized]
	public State currentState;

	[NonSerialized]
	public PathLocation location;

	[NonSerialized]
	public SelectImageModule selectImageModule;

	[NonSerialized]
	public CurrentIconModule currentIconModule;

	[NonSerialized]
	public DungeonMapDisplay dungeonMapDisplay;

	[NonSerialized]
	private InputModule inputModule;

	private DungeonMap map => CellUI<PathLocation>.gameState.dungeonMapManager.currentDungeonMap;

	protected override bool shouldUpdate
	{
		get
		{
			if (dungeonMapDisplay.updateLocations)
			{
				return base.shouldUpdate;
			}
			return false;
		}
	}

	protected override void Init()
	{
		selectImageModule = GetModule<SelectImageModule>();
		currentIconModule = GetModule<CurrentIconModule>();
		inputModule = GetModule<InputModule>();
		InputModule obj = inputModule;
		obj.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(obj.onHandleInput, new Action<InputModuleBase>(_OnHandleInput));
	}

	protected override void OnSetup(PathLocation dailyContract)
	{
		location = dailyContract;
		currentState = State.None;
		dungeonMapDisplay = CellUI<PathLocation>.gameState.UIManager.dungeonMapPanel.dungeonMapDisplay;
		CellUI<PathLocation>.gameState.gameEventManager.onTriggerGameEvent -= _OnGameEventTrigger;
		CellUI<PathLocation>.gameState.gameEventManager.onTriggerGameEvent += _OnGameEventTrigger;
		ForceUpdate();
	}

	protected override void OnUnset()
	{
		CellUI<PathLocation>.gameState.gameEventManager.onTriggerGameEvent -= _OnGameEventTrigger;
	}

	protected override void UpdateDisplay()
	{
		RefreshState();
	}

	private void RefreshState()
	{
		if (CellUI<PathLocation>.gameState.recordManager.currentLevelVisitedLocations.Contains(location))
		{
			currentState = State.Finished;
			return;
		}
		if (map.currentLocation == location && location.finished)
		{
			currentState = State.Current_Finished;
			return;
		}
		if (map.currentLocation == location && !location.finished)
		{
			currentState = State.Current_Unfinished;
			return;
		}
		if (!location.reachable || location.finished)
		{
			currentState = State.Disabled;
			return;
		}
		if (dungeonMapDisplay.focusedLocationDisplay == this)
		{
			currentState = State.Focused;
			return;
		}
		PathLocation currentLocation = map.currentLocation;
		if ((currentLocation == null || currentLocation.finished) && (location.jumpAble || CSCommon.cheatJumpMap))
		{
			currentState = State.Selectable;
		}
		else
		{
			currentState = State.Idle;
		}
	}

	private void _OnGameEventTrigger(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.ExitLocation || eve.type == GameEvent.Type.ExitCity || eve.type == GameEvent.Type.EnterLocation)
		{
			SetDirty();
		}
	}

	private void _OnHandleInput(InputModuleBase _)
	{
		if (inputModule.pointerClicked && !inputModule.endDrag)
		{
			if (!CellUI<PathLocation>.gameState.actionManager.isEmpty || (map.currentLocation != null && !map.currentLocation.finished))
			{
				return;
			}
			if (dungeonMapDisplay.focusedLocationDisplay == this && map.currentLocation != location && dungeonMapDisplay.selectedLocationDisplay == null && location.selectable)
			{
				dungeonMapDisplay.SelectLocation(this);
			}
			else
			{
				dungeonMapDisplay.FocusLocation(this);
			}
		}
		if (PlatformHelper.GetPlatform() == PlatformHelper.Type.PC)
		{
			if (inputModule.pointerEnter)
			{
				dungeonMapDisplay.FocusLocation(this);
			}
			if (inputModule.pointerExit && dungeonMapDisplay.focusedLocationDisplay == this)
			{
				dungeonMapDisplay.UnfocusLocation();
			}
		}
	}
}
