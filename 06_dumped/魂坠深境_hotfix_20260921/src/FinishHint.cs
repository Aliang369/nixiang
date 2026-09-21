using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class FinishHint : CellUI<FinishHint.Info>
{
	public class Info
	{
		public string s;

		public float timeScale;
	}

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	private Sequence sequence;

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnSetup(Info info)
	{
		text.text = info.s;
		base.gameObject.SetActive(value: true);
		animator.SetTrigger(AnimKeywords.show);
	}

	private void _OnAnimEnd()
	{
		base.gameObject.SetActive(value: false);
		(parent as FinishHintPanel).Despawn(this);
	}
}
