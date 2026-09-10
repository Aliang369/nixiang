using System;
using UnityEngine;
using UnityEngine.UI;

public class Mounting_AllGemDisplayModule : UIDisplayModule
{
	[NonSerialized]
	private GemMountingPanel gemMountingPanel;

	[NonSerialized]
	private ScrollRect scrollRect;

	[NonSerialized]
	private bool isVerticalDrag;

	[NonSerialized]
	private GemMountedDisplay target;

	[NonSerialized]
	private bool isDragging;

	public SkillGemDisplay skillGemDisplay => parent as SkillGemDisplay;

	public SkillGem skillGem => parent._setupData as SkillGem;

	public override void Init()
	{
		gemMountingPanel = GetComponentInParent<GemMountingPanel>(includeInactive: true);
		scrollRect = GetComponentInParent<ScrollRect>(includeInactive: true);
		SetScrollRectExtendEventEnabled(enabled: false);
		if (gemMountingPanel == null)
		{
			TraceLog.Error("[Mounting_AllGemDisplayModule] Init() - gemMountingPanel == null");
		}
		if (scrollRect == null)
		{
			TraceLog.Error("[Mounting_AllGemDisplayModule] Init() - scrollRect == null");
		}
		InputModule module = parent.GetModule<InputModule>();
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, new Action<InputModuleBase>(_HandleInput));
	}

	public override void Setup()
	{
		isDragging = false;
		target = null;
		SetScrollRectExtendEventEnabled(enabled: false);
	}

	private void SetScrollRectExtendEventEnabled(bool enabled)
	{
		ScrollRectExtendEvent component = GetComponent<ScrollRectExtendEvent>();
		if (component != null)
		{
			component.enabled = enabled;
		}
	}

	private void _HandleInput(InputModuleBase inputModule)
	{
		if (inputModule.beginDrag)
		{
			_HandleBeginDrag();
		}
		if (inputModule.drag)
		{
			_HandleDrag();
		}
		if (inputModule.endDrag)
		{
			_HandleEndDrag();
		}
		void _HandleBeginDrag()
		{
			Vector2 delta = inputModule.eventData.delta;
			if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
			{
				isVerticalDrag = true;
				if (scrollRect != null)
				{
					scrollRect.OnBeginDrag(inputModule.eventData);
				}
			}
			else
			{
				_HandleBeginDrag_Horizontal();
			}
		}
		void _HandleBeginDrag_Horizontal()
		{
			target = gemMountingPanel?.BeginDragInventoryGem(skillGem, inputModule.eventData.position);
			isDragging = target != null;
		}
		void _HandleDrag()
		{
			if (isVerticalDrag)
			{
				if (scrollRect != null)
				{
					scrollRect.OnDrag(inputModule.eventData);
				}
			}
			else
			{
				_HandleDrag_Horizontal();
			}
		}
		void _HandleDrag_Horizontal()
		{
			if (isDragging && !(target == null))
			{
				gemMountingPanel?.DragInventoryGem(target, inputModule.eventData.position);
			}
		}
		void _HandleEndDrag()
		{
			if (isVerticalDrag)
			{
				if (scrollRect != null)
				{
					scrollRect.OnEndDrag(inputModule.eventData);
				}
			}
			else
			{
				_HandleEndDrag_Horizontal();
			}
			isVerticalDrag = false;
		}
		void _HandleEndDrag_Horizontal()
		{
			if (isDragging && !(target == null))
			{
				gemMountingPanel?.EndDragInventoryGem(skillGem, target, inputModule.eventData.position);
				target = null;
				isDragging = false;
			}
		}
	}
}
