using System;
using DG.Tweening;

public class RuneDisplay_MintagePreview : RuneDisplay.DisplayModule
{
	[NonSerialized]
	private bool isPreview;

	[NonSerialized]
	private Sequence sequence;

	public override void Setup()
	{
		base.display.icon.color = base.display.icon.color.SetA(1f);
		isPreview = false;
	}

	public override void Unset()
	{
		sequence?.Kill();
		sequence = null;
		base.display.icon.color = base.display.icon.color.SetA(1f);
	}

	public void OnMintage()
	{
		if (isPreview)
		{
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.Append(DOTweenModuleUI.DOFade(base.display.icon, 1f, 0f));
			sequence.Append(base.display.icon.transform.DOLocalMoveY(30f, 0.2f));
			sequence.Append(base.display.icon.transform.DOLocalMoveY(0f, 0.2f));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
		}
	}

	public void SetAsPreview()
	{
		isPreview = true;
		StartFade();
	}

	private void StartFade()
	{
		sequence?.Kill();
		sequence = DOTween.Sequence();
		sequence.SetLoops(-1);
		sequence.Append(DOTweenModuleUI.DOFade(base.display.icon, 0f, 0.5f));
		sequence.Append(DOTweenModuleUI.DOFade(base.display.icon, 1f, 0.5f));
		sequence.Play();
	}
}
