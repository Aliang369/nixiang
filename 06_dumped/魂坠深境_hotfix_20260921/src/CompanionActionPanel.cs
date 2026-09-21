using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CompanionActionPanel : CellUI<ItemDefinition>
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	private Sequence sequence;

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnSetup(ItemDefinition item)
	{
		base.gameObject.SetActive(value: true);
		icon.sprite = item.Icon;
		animator.SetTrigger("Play");
	}

	public void _OnAnimEnd()
	{
		base.gameObject.SetActive(value: false);
	}
}
