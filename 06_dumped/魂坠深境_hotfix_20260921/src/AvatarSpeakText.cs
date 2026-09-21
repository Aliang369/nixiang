using System.Collections;
using UnityEngine;

public class AvatarSpeakText : AvatarDialogueStep
{
	public AvatarSide side;

	[LocalizeText("立绘对话文本")]
	[TextArea]
	public string text;

	public Sprite newAvatar;

	public override IEnumerator ExecuteCor(AvatarDialoguePanel panel)
	{
		SetAndActiveSideAvatar(panel, side, newAvatar);
		panel.dialogueArea.gameObject.SetActive(value: true);
		panel.dialogueText.text = "";
		yield return ProgressDialogueText(panel, text);
		yield return WaitForEmptyAreaClicked(panel);
	}
}
