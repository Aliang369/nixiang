using System;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class ForgetShrineModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private float moveSpeed;

	[SerializeField]
	private float dragScale;

	[SerializeField]
	private RectTransform infoPassRect;

	[SerializeField]
	private Image forgetShrineIcon;

	[NonSerialized]
	private ICommonInfoPanel infoPanel;

	[NonSerialized]
	private ForgetShrinePanel forgetShrinePanel;

	[NonSerialized]
	private ForgetShrineItemSlot.EType type;

	[NonSerialized]
	private int index;

	[NonSerialized]
	private Vector2? target;

	[NonSerialized]
	private Vector3 initScale;

	[NonSerialized]
	private bool isVerticalDrag;

	public ForgetShrineItemSlot currentSlot => forgetShrinePanel.GetSlot(type, index);

	private ScrollRect scrollRect => GetComponentInParent<ScrollRect>();

	public override void Init()
	{
		forgetShrinePanel = GetComponentInParent<ForgetShrinePanel>(includeInactive: true);
		InputModule module = parent.GetModule<InputModule>();
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, new Action<InputModuleBase>(_HandleInput));
		initScale = base.rectTransform.localScale;
		infoPanel = UIHelper.GetDefaultCommonInfoPanel();
	}

	public override void Setup()
	{
		base.rectTransform.localScale = initScale;
		target = null;
		UpdateForgetShrineIcon(show: false);
	}

	public void OnSet()
	{
		ComputeIndex();
		base.rectTransform.anchoredPosition = Vector2.zero;
	}

	public void OnSequenceChange()
	{
		ComputeIndex();
		target = Vector2.zero;
	}

	public void OnPostSequenceChange()
	{
		if (currentSlot == null)
		{
			forgetShrinePanel.itemDisplayPool.pool.Despawn(base.slotDisplay);
		}
	}

	private void Update()
	{
		if (target.HasValue)
		{
			base.rectTransform.anchoredPosition = Vector2.Lerp(base.rectTransform.anchoredPosition, target.Value, moveSpeed);
			if (Vector2.Distance(base.rectTransform.anchoredPosition, target.Value) < 0.1f)
			{
				base.rectTransform.anchoredPosition = target.Value;
				target = null;
			}
		}
	}

	private void ComputeIndex()
	{
		index = forgetShrinePanel.deckItems.IndexOf(base.item);
		type = ForgetShrineItemSlot.EType.Deck;
		if (index == -1)
		{
			index = forgetShrinePanel.shrineItems.IndexOf(base.item);
			type = ForgetShrineItemSlot.EType.ForgetShrine;
		}
		ForgetShrineItemSlot forgetShrineItemSlot = currentSlot;
		UpdateForgetShrineIcon(forgetShrineItemSlot != null && type == ForgetShrineItemSlot.EType.ForgetShrine);
		if (!(forgetShrineItemSlot == null))
		{
			SetParent(forgetShrineItemSlot.pivot.transform);
		}
	}

	private void UpdateForgetShrineIcon(bool show)
	{
		if (!(forgetShrineIcon == null))
		{
			CharacterClassDef characterClassDef = forgetShrinePanel?.setupData?.classDef;
			if (characterClassDef != null)
			{
				forgetShrineIcon.sprite = ((characterClassDef.forgetShrinePointIcon != null) ? characterClassDef.forgetShrinePointIcon : ScriptableObjectSingleton<GameDataManager>.instance.forgetShrinePointIcon);
			}
			forgetShrineIcon.gameObject.SetActive(show && characterClassDef != null);
		}
	}

	private void _HandleInput(InputModuleBase inputModule)
	{
		if (inputModule.pointerClicked)
		{
			_HandleClick();
		}
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
			SetParent(forgetShrinePanel.dragPivot);
			target = ScreenToLocal(inputModule.eventData.position);
			base.rectTransform.localScale = dragScale * initScale;
			if (type == ForgetShrineItemSlot.EType.Deck)
			{
				forgetShrinePanel.SetForgetShineRectImage(0.5f);
			}
			else
			{
				forgetShrinePanel.SetDeckRectImage(0.5f);
			}
		}
		void _HandleClick()
		{
			if (!(inputModule.lastPointerDownTime < inputModule.lastDragTime))
			{
				infoPanel?.ShowCommonInfo(new CommonInfoData(parent._setupData, CommonInfoData.ControlType.Click, infoPassRect, false));
			}
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
			target = ScreenToLocal(inputModule.eventData.position);
			if (type == ForgetShrineItemSlot.EType.Deck)
			{
				if (InShrineRect(inputModule.eventData.position))
				{
					forgetShrinePanel.SetForgetShineRectImage(1f);
				}
				else
				{
					forgetShrinePanel.SetForgetShineRectImage(0.5f);
				}
			}
			else if (InDeckRect(inputModule.eventData.position))
			{
				forgetShrinePanel.SetDeckRectImage(1f);
			}
			else
			{
				forgetShrinePanel.SetDeckRectImage(0.5f);
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
			ComputeIndex();
			if (type == ForgetShrineItemSlot.EType.Deck)
			{
				if (InShrineRect(inputModule.eventData.position))
				{
					if (forgetShrinePanel.CanMoveToShrine(base.item))
					{
						forgetShrinePanel.MoveToShrine(base.item);
						ComputeIndex();
					}
					else
					{
						forgetShrinePanel.ShowQuickHint(ScriptableObjectSingleton<TextDataPreset>.instance.term_forget_shrine_limit);
					}
				}
			}
			else if (InDeckRect(inputModule.eventData.position) && forgetShrinePanel.CanMoveToDeck(base.item))
			{
				forgetShrinePanel.MoveToDeck(base.item);
				ComputeIndex();
			}
			if (currentSlot != null)
			{
				target = Vector3.zero;
				base.rectTransform.localScale = initScale;
			}
			else
			{
				forgetShrinePanel.itemDisplayPool.pool.Despawn(parent as ItemDisplayBase);
			}
			forgetShrinePanel.SetDeckRectImage(0f);
			forgetShrinePanel.SetForgetShineRectImage(0f);
		}
	}

	private Vector2 ScreenToLocal(Vector2 screenPoint)
	{
		Vector2 result = default(Vector2);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform.parent as RectTransform, screenPoint, (Camera)null, ref result);
		return result;
	}

	private bool InDeckRect(Vector2 screenPoint)
	{
		return RectTransformUtility.RectangleContainsScreenPoint(forgetShrinePanel.deckRect, screenPoint);
	}

	private bool InShrineRect(Vector2 screenPoint)
	{
		return RectTransformUtility.RectangleContainsScreenPoint(forgetShrinePanel.forgetShineRect, screenPoint);
	}

	private void SetParent(Transform target)
	{
		parent.transform.SetParent(target, worldPositionStays: true);
	}
}
