using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GemMountingDraggerModule : UIDisplayModule, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	[SerializeField]
	private GameObject outlineImg;

	private ScrollRectExtendEvent _scrollRectExtendEvent;

	private bool isSelected;

	private bool isDragging;

	private bool isMountedGem;

	public bool IsMountedGem
	{
		get
		{
			return isMountedGem;
		}
		set
		{
			isMountedGem = value;
			SetScrollRectForwardingEnabled();
		}
	}

	public SkillGem SkillGem { get; private set; }

	public GemMountingPanel gemMountingPanel => GetComponentInParent<GemMountingPanel>(includeInactive: true);

	public void Setup(SkillGem skillGem)
	{
		SkillGem = skillGem;
		isDragging = false;
		SetScrollRectForwardingEnabled();
	}

	private void Awake()
	{
		_scrollRectExtendEvent = GetComponent<ScrollRectExtendEvent>();
	}

	private void Start()
	{
		InputModule component = GetComponent<InputModule>();
		component.onPointerClick = (Action<InputModuleBase>)Delegate.Combine(component.onPointerClick, new Action<InputModuleBase>(OnPointerClick));
	}

	private void OnPointerClick(InputModuleBase obj)
	{
		if (UIDisplayModule.fortressState != null)
		{
			gemMountingPanel?.OnGemClicked(this);
		}
	}

	public void SetSelected(bool selected, bool playSound = true)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		isSelected = selected;
		SetScrollRectForwardingEnabled();
		parent.SetDirty();
		if (playSound)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gem_mount_move);
		}
	}

	private void SetScrollRectForwardingEnabled()
	{
		if (_scrollRectExtendEvent != null)
		{
			_scrollRectExtendEvent.enabled = !IsMountedGem && GetComponent<Mounting_AllGemDisplayModule>() == null;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (IsMountedGem)
		{
			isDragging = true;
			gemMountingPanel?.OnMountedGemBeginDrag(eventData.position, this);
			parent.SetDirty();
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (IsMountedGem && isDragging)
		{
			isDragging = true;
			gemMountingPanel?.OnMountedGemDrag(eventData.position);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (IsMountedGem && isDragging)
		{
			isDragging = false;
			gemMountingPanel?.OnMountedGemEndDrag(eventData.position);
			parent.SetDirty();
		}
	}

	public override void UpdateDisplay()
	{
		outlineImg.gameObject.SetActive(value: false);
		if (gemMountingPanel != null && gemMountingPanel.IsGemSelected(this))
		{
			outlineImg.gameObject.SetActive(value: true);
		}
	}
}
