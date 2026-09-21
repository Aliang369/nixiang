using System;
using System.Collections;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarDialoguePanel : UIBase
{
	public Image leftAvatar;

	public Image rightAvatar;

	public RectTransform dialogueArea;

	public TextMeshProUGUI dialogueText;

	public Transform dialogueChoiceArea;

	public GameObject dialogueChoicePrefab;

	public PrefabPool<Transform> dialogueChoicePrefabPool;

	public Action onEmptyAreaClicked;

	public Action<int> onChoiceSelected;

	public Action onDialogueEnd;

	public AvatarDialogueSection currentSection;

	protected override void Init()
	{
		dialogueChoicePrefabPool = new PrefabPool<Transform>(dialogueChoicePrefab.transform, dialogueChoiceArea);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(OnEmptyAreaClicked);
		Reset();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(OnEmptyAreaClicked);
	}

	public void Reset()
	{
		currentSection = null;
		base.gameObject.SetActive(value: false);
		leftAvatar.gameObject.SetActive(value: false);
		leftAvatar.sprite = null;
		rightAvatar.gameObject.SetActive(value: false);
		rightAvatar.sprite = null;
		dialogueText.text = "";
		dialogueArea.gameObject.SetActive(value: false);
		dialogueChoicePrefabPool.DespawnAll();
	}

	public IEnumerator PlayCurrentDialogueSectionCor(AvatarDialogueSection section)
	{
		if (section == null || currentSection != null)
		{
			yield break;
		}
		currentSection = section;
		base.gameObject.SetActive(value: true);
		foreach (AvatarDialogueStep step in currentSection.steps)
		{
			yield return step.ExecuteCor(this);
		}
		onDialogueEnd?.Invoke();
		Reset();
	}

	private void OnEmptyAreaClicked()
	{
		onEmptyAreaClicked?.Invoke();
	}
}
