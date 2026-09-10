using System;
using ContainerOfSoul.Display.Fortress.DotManagers;
using UnityEngine;
using UnityEngine.Events;

public abstract class SwitchTag : CellUI<SwitchTag.Data>
{
	public class Data
	{
		public int i;

		public Func<SwitchTag, bool> shouldShowRedDot;

		public object appendData;
	}

	[SerializeField]
	public bool locked;

	[SerializeField]
	public HintRedDot redDot;

	[NonSerialized]
	private SwitchPanel switchPanel;

	[NonSerialized]
	private bool selected;

	[NonSerialized]
	private GameButton[] buttons;

	[NonSerialized]
	public Func<SwitchTag, bool> canSelectByClick;

	[NonSerialized]
	public Action<SwitchTag> onSelectBlockedByClick;

	protected virtual bool ControlButtonInteractable => true;

	protected override void Init()
	{
		if (redDot != null)
		{
			InitUI(redDot);
			redDot.Setup(new CustomModule
			{
				shouldShow = ShouldShow
			});
		}
		buttons = GetComponentsInChildren<GameButton>(includeInactive: true);
		selected = false;
		GameButton[] array = buttons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].onClick.AddListener(_OnButtonClicked);
		}
		bool? ShouldShow()
		{
			if (base.setupData == null || base.setupData.shouldShowRedDot == null)
			{
				return false;
			}
			return base.setupData.shouldShowRedDot(this);
		}
	}

	public void SetPanel(SwitchPanel panel)
	{
		switchPanel = panel;
	}

	public void SetInteractable(bool interactable)
	{
		if (buttons != null)
		{
			GameButton[] array = buttons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].interactable = interactable;
			}
		}
	}

	public void AddNotInteractableClickListener(UnityAction listener)
	{
		if (listener != null && buttons != null)
		{
			GameButton[] array = buttons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].onNotInteractableClick.AddListener(listener);
			}
		}
	}

	protected override void UpdateDisplay()
	{
		if (switchPanel == null)
		{
			return;
		}
		if (ControlButtonInteractable)
		{
			if (locked)
			{
				GameButton[] array = buttons;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].interactable = false;
				}
			}
			else
			{
				GameButton[] array = buttons;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].interactable = true;
				}
			}
		}
		bool flag = switchPanel.targetShowTag == this;
		if (flag != selected)
		{
			selected = flag;
			if (selected && !locked)
			{
				OnSelect();
			}
			else
			{
				OnUnselect();
			}
		}
	}

	protected override void OnSetup(Data d)
	{
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
	}

	public virtual void SetText(string text)
	{
	}

	protected abstract void OnSelect();

	protected abstract void OnUnselect();

	private void _OnButtonClicked()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Func<SwitchTag, bool> func = canSelectByClick;
		if (func != null && !func(this))
		{
			onSelectBlockedByClick?.Invoke(this);
			return;
		}
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_tab);
		switchPanel.Select(this);
	}
}
