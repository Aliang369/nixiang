using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinUnlockHint : UIBase
{
	public Image skinAvatar;

	public TextMeshProUGUI skinTitle;

	public TextMeshProUGUI skinDescription;

	public CanvasGroup canvasGroup;

	private Coroutine hintCor;

	private PlayerCharacterDef character;

	public void Setup(PlayerCharacterDef t)
	{
		character = t;
		Hint();
	}

	public void Unset()
	{
		base.gameObject.SetActive(value: false);
		hintCor = null;
	}

	protected override void Init()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	public void Hint()
	{
		if (hintCor == null)
		{
			base.gameObject.SetActive(value: true);
			hintCor = StartCoroutine(HintCor());
		}
	}

	private IEnumerator HintCor()
	{
		Sequence seq = DOTween.Sequence();
		seq.Append(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0f));
		skinAvatar.sprite = character.fullBody;
		skinTitle.text = character.characterName;
		skinDescription.text = character.description;
		seq.Append(DOTweenModuleUI.DOFade(canvasGroup, 1f, 0.5f));
		seq.Play();
		yield return new WaitUntil(() => seq.IsComplete() || !base.gameObject.activeSelf);
		hintCor = null;
	}

	private void _OnEmptyAreaClicked()
	{
		Unset();
	}
}
