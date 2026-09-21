using System;
using System.Collections.Generic;
using ContainerOfSoul.Display.Fortress;

public class EnterFortressMainPanelLayer : GlobalTriggerObserver
{
	public FortressMainUIPanel.MainPanelLayer targetLayer;

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public override List<IEffectNode> GetChildren()
	{
		return null;
	}

	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		applicationState.eventManager.fortressMainPanelLayerChangedEvent.AddListener(OnLayerChanged);
		void OnLayerChanged(FortressMainUIPanel.MainPanelLayer layer)
		{
			if (layer == targetLayer)
			{
				onTrigger();
			}
		}
	}
}
