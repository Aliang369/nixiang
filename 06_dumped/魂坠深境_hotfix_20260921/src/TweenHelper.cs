using DG.Tweening;

public static class TweenHelper
{
	public static bool Playing(this Tween tween)
	{
		if (tween != null && tween.active)
		{
			return tween.IsPlaying();
		}
		return false;
	}
}
