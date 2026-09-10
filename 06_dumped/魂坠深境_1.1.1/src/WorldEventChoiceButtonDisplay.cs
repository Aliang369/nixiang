using System;
using TMPro;
using UnityEngine;

public class WorldEventChoiceButtonDisplay : CellUI<WorldEventChoice>
{
	[SerializeField]
	private GameButton button;

	[SerializeField]
	private TextMeshProUGUI eventNameText;

	[SerializeField]
	private TextMeshProUGUI eventDescriptionText;

	[NonSerialized]
	public WorldEventChoice choice;

	private bool Interactable
	{
		get
		{
			if (CellUI<WorldEventChoice>.gameState.actionManager.isEmpty)
			{
				if (choice.executeAbleChecker != null)
				{
					return choice.executeAbleChecker();
				}
				return true;
			}
			return false;
		}
	}

	protected override void Init()
	{
		button.onClick.AddListener(_OnPointerClick);
	}

	protected override void OnSetup(WorldEventChoice choice)
	{
		this.choice = choice;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		eventNameText.text = choice.buttonText;
		eventDescriptionText.text = choice.descriptionText;
		button.interactable = choice.executeAbleChecker?.Invoke() ?? true;
	}

	private void _OnPointerClick()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (Interactable)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_event_choice_confirm);
			choice.executeAction?.Invoke();
		}
	}
}
