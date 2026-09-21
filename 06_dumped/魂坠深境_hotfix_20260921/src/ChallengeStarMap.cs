using DG.Tweening;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class ChallengeStarMap : CTUIBaseAsset
{
	public class StarMapDisplayConfig
	{
		public EffectLineRenderer lineRendererPrefab;

		public ChallengeStar challengeStarPrefab;

		public Color lockedStarColor;

		public Color unlockedStarColor;

		public Color finishedStarColor;

		public Color clearedStarColor;

		public Color hardChallengeStarGlowColor;

		public Color storyStarGlowColor;

		public float focusStarScale;

		public Color unlockedConstellationColor;

		public Color focusConstellationColor;

		public Color activePathLineColor;

		public Color inactivePathLineColor;

		public float focusDuration;

		public Color dailyChallengeConstellationTextColor;

		public Color seasonChallengeConstellationTextColor;
	}

	public override string CTAssetIDPrepend => "STARMAP";

	public MapPreviewConstellation[] ChildrenConstellations => GetComponentsInChildren<MapPreviewConstellation>(includeInactive: true);

	protected override void Init()
	{
		MapPreviewConstellation[] childrenConstellations = ChildrenConstellations;
		foreach (MapPreviewConstellation child in childrenConstellations)
		{
			InitUI(child);
		}
		SetDirty();
	}

	public void MoveCenterLocalToPos(Vector3 localPos)
	{
		DOTweenModuleUI.DOAnchorPos(GetComponent<RectTransform>(), (Vector2)(-localPos), ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.focusDuration, false).SetEase(Ease.InOutCubic);
	}
}
