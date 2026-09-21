using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SelectableModuleBase : UIDisplayModule
{
	[SerializeField]
	private Image darkenImage;

	[SerializeField]
	private Vector3 scale = Vector3.one;

	[SerializeField]
	private GameObject outline;

	[NonSerialized]
	protected ISelectablePanel selectableArea;

	[NonSerialized]
	private bool selected;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	private Vector3 originScale;

	[NonSerialized]
	public Action onSelect;

	[NonSerialized]
	public Action onUnselect;

	public override void Init()
	{
		selectableArea = GetComponentInParent<ISelectablePanel>(includeInactive: true);
		originScale = base.transform.localScale;
	}

	public override void Setup()
	{
		sequence?.Kill();
		selected = false;
		base.transform.localScale = originScale;
		if (outline != null)
		{
			outline.gameObject.SetActive(value: false);
		}
	}

	public override void Unset()
	{
		sequence?.Kill();
	}

	public override void UpdateDisplay()
	{
		UpdateSelected();
		UpdateCanSelect();
	}

	private void UpdateCanSelect()
	{
		if (darkenImage != null)
		{
			darkenImage.gameObject.SetActive(selectableArea != null && selectableArea.IsSelectDark(this));
		}
	}

	private void UpdateSelected()
	{
		if (selectableArea == null)
		{
			if (selected)
			{
				selected = false;
				OnUnselect();
			}
			return;
		}
		bool flag = selectableArea.IsSelected(this);
		if (flag != selected)
		{
			selected = flag;
			if (selected)
			{
				OnSelect();
			}
			else
			{
				OnUnselect();
			}
		}
	}

	private void OnSelect()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		if (outline != null)
		{
			outline.gameObject.SetActive(value: true);
		}
		sequence.Append((base.transform as RectTransform).DOScale(Vector3.Scale(originScale, scale), 0.1f));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		onSelect?.Invoke();
	}

	private void OnUnselect()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		if (outline != null)
		{
			outline.gameObject.SetActive(value: false);
		}
		sequence.Append(base.transform.DOScale(originScale, 0.1f));
		Sequence obj = sequence;
		obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
		{
			sequence = null;
		});
		sequence.Play();
		onUnselect?.Invoke();
	}
}
