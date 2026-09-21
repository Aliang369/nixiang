using System;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmItemSelector : CellUI<Item>, IDisplayModulesController
{
	[SerializeField]
	public Image selectedFrame;

	[SerializeField]
	public ItemDisplayBase itemDisplay;

	[NonSerialized]
	public ConfirmItemSelectionPanel attachPanel;

	[NonSerialized]
	public Item item;

	[NonSerialized]
	private RewardUncollectedTag uncollectedTag;

	protected override void Init()
	{
		attachPanel = parent as ConfirmItemSelectionPanel;
		InputModule module = GetModule<InputModule>();
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, (Action<InputModuleBase>)delegate(InputModuleBase input)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			if (attachPanel != null && input.pointerClicked)
			{
				attachPanel.Select(this);
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_select);
			}
		});
		InitUI(itemDisplay);
		uncollectedTag = RewardUncollectedTag.AttachTo(itemDisplay, RewardUncollectedTagLayout.ConfirmItemSelectionCard);
	}

	protected override void OnSetup(Item dailyContract)
	{
		item = dailyContract;
		itemDisplay.Setup(dailyContract);
		SetDirty();
	}

	protected override void OnUnset()
	{
		uncollectedTag?.Hide();
		itemDisplay.Unset();
	}

	protected override void UpdateDisplay()
	{
		uncollectedTag?.Setup(item?.GetDef(), CellUI<Item>.applicationState);
		if (attachPanel.selected.Contains(this))
		{
			selectedFrame.gameObject.SetActive(value: true);
			base.gameObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f) * 1.1f;
		}
		else
		{
			selectedFrame.gameObject.SetActive(value: false);
			base.gameObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
		}
	}
}
