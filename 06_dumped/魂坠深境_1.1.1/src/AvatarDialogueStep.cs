using System;
using System.Collections;
using UnityEngine;

public abstract class AvatarDialogueStep
{
	public enum AvatarSide
	{
		Left,
		Right
	}

	public abstract IEnumerator ExecuteCor(AvatarDialoguePanel panel);

	protected IEnumerator WaitForEmptyAreaClicked(AvatarDialoguePanel panel)
	{
		bool clicked = false;
		AvatarDialoguePanel avatarDialoguePanel = panel;
		avatarDialoguePanel.onEmptyAreaClicked = (Action)Delegate.Combine(avatarDialoguePanel.onEmptyAreaClicked, new Action(Wait));
		yield return new WaitUntil(() => clicked);
		void Wait()
		{
			AvatarDialoguePanel avatarDialoguePanel2 = panel;
			avatarDialoguePanel2.onEmptyAreaClicked = (Action)Delegate.Remove(avatarDialoguePanel2.onEmptyAreaClicked, new Action(Wait));
			clicked = true;
		}
	}

	protected IEnumerator ProgressDialogueText(AvatarDialoguePanel panel, string text)
	{
		bool skipFlag = false;
		AvatarDialoguePanel avatarDialoguePanel = panel;
		avatarDialoguePanel.onEmptyAreaClicked = (Action)Delegate.Combine(avatarDialoguePanel.onEmptyAreaClicked, new Action(Skip));
		yield return panel.dialogueText.ScrollTextCor(text, ScriptableObjectSingleton<VFXDataPreset>.instance.dialogueBaseReadSpeed, () => skipFlag);
		skipFlag = false;
		void Skip()
		{
			skipFlag = true;
			panel.dialogueText.text = text;
			AvatarDialoguePanel avatarDialoguePanel2 = panel;
			avatarDialoguePanel2.onEmptyAreaClicked = (Action)Delegate.Remove(avatarDialoguePanel2.onEmptyAreaClicked, new Action(Skip));
		}
	}

	protected void SetAndActiveSideAvatar(AvatarDialoguePanel panel, AvatarSide side, Sprite avatar = null)
	{
		switch (side)
		{
		case AvatarSide.Left:
			if (avatar != null)
			{
				panel.leftAvatar.sprite = avatar;
			}
			panel.leftAvatar.color = Color.white;
			panel.rightAvatar.color = ScriptableObjectSingleton<VFXDataPreset>.instance.inactiveDialogueAvatarColor;
			break;
		case AvatarSide.Right:
			if (avatar != null)
			{
				panel.rightAvatar.sprite = avatar;
			}
			panel.rightAvatar.color = Color.white;
			panel.leftAvatar.color = ScriptableObjectSingleton<VFXDataPreset>.instance.inactiveDialogueAvatarColor;
			break;
		}
		panel.rightAvatar.gameObject.SetActive(panel.rightAvatar.sprite != null);
		panel.leftAvatar.gameObject.SetActive(panel.leftAvatar.sprite != null);
	}
}
