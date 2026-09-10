using System;
using UnityEngine.UI;

public class ScrollRectExtendModule : UIDisplayModule
{
	private ScrollRect scrollRect => GetComponentInParent<ScrollRect>(includeInactive: true);

	public override void Init()
	{
		if (scrollRect == null)
		{
			return;
		}
		InputModule module = parent.GetModule<InputModule>();
		if (module == null)
		{
			return;
		}
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, (Action<InputModuleBase>)delegate(InputModuleBase m)
		{
			if (!(scrollRect == null))
			{
				if (m.beginDrag)
				{
					scrollRect.OnBeginDrag(m.eventData);
				}
				if (m.drag)
				{
					scrollRect.OnDrag(m.eventData);
				}
				if (m.endDrag)
				{
					scrollRect.OnEndDrag(m.eventData);
				}
			}
		});
		module.SetEnable(InputModuleBase.EventType.BeginDrag, enable: true);
		module.SetEnable(InputModuleBase.EventType.Drag, enable: true);
		module.SetEnable(InputModuleBase.EventType.EndDrag, enable: true);
	}
}
