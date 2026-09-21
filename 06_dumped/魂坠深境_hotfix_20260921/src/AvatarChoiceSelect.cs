using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AvatarChoiceSelect : AvatarDialogueStep
{
	public class AvatarChoiceInfo
	{
		[LocalizeText("立绘对话选项")]
		public string choiceText;

		public List<AvatarSpeakText> branchDialogue;
	}

	public List<AvatarChoiceInfo> avatarChoices;

	public override IEnumerator ExecuteCor(AvatarDialoguePanel panel)
	{
		AvatarChoiceInfo selectedChoice = null;
		panel.dialogueChoicePrefabPool.DespawnAll();
		panel.dialogueArea.gameObject.SetActive(value: false);
		panel.dialogueChoiceArea.gameObject.SetActive(value: true);
		for (int i = 0; i < avatarChoices.Count; i++)
		{
			AvatarChoiceInfo choice = avatarChoices[i];
			Transform transform = panel.dialogueChoicePrefabPool.Spawn();
			transform.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;
			int i_closure = i;
			transform.GetComponentInChildren<GameButton>().onClick.RemoveAllListeners();
			transform.GetComponentInChildren<GameButton>().onClick.AddListener(OnChoiceSelect);
			void OnChoiceSelect()
			{
				selectedChoice = choice;
				panel.onChoiceSelected?.Invoke(i_closure);
			}
		}
		yield return new WaitUntil(() => selectedChoice != null);
		panel.dialogueChoicePrefabPool.DespawnAll();
		panel.dialogueChoiceArea.gameObject.SetActive(value: false);
		foreach (AvatarSpeakText item in selectedChoice.branchDialogue.OrEmptyIfNull())
		{
			yield return item.ExecuteCor(panel);
		}
	}
}
