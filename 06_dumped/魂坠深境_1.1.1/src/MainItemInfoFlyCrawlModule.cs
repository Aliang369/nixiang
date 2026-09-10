using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class MainItemInfoFlyCrawlModule : MainItemInfoDisplayBase.DisplayModule, IFrameUpdate
{
	private enum FlyState
	{
		Landing,
		Idle,
		Walk,
		FlyAway
	}

	private class FlyInstance
	{
		public RectTransform rectTransform;

		public Image image;

		public FlyState state;

		public Vector2 startPosition;

		public Vector2 targetPosition;

		public float elapsed;

		public float duration;

		public float speed;

		public float startAlpha;

		public float bodyRadius;
	}

	[Header("Target")]
	[SerializeField]
	private ItemDefinition targetItemDefinition;

	[SerializeField]
	private bool onlyMainBattleInfoPanel = true;

	[SerializeField]
	private RectTransform crawlArea;

	[Header("Sprites")]
	[SerializeField]
	private Sprite bigFlySprite;

	[SerializeField]
	private Sprite smallFlySprite;

	[Header("Count")]
	[SerializeField]
	private int minFlyCount = 1;

	[SerializeField]
	private int maxFlyCount = 2;

	[Header("Size")]
	[SerializeField]
	private Vector2 smallFlySizeRange = new Vector2(80f, 120f);

	[SerializeField]
	private Vector2 bigFlySizeRange = new Vector2(120f, 170f);

	[SerializeField]
	private bool fitFlyInsideCrawlArea = true;

	[SerializeField]
	private float fittedFlySizeMultiplier = 1f;

	[Header("Orientation")]
	[SerializeField]
	private float spriteHeadAngle = 180f;

	[Header("Movement")]
	[SerializeField]
	private Vector4 crawlPadding = new Vector4(24f, 24f, 24f, 24f);

	[SerializeField]
	private Vector2 landingTimeRange = new Vector2(0.35f, 0.7f);

	[SerializeField]
	private Vector2 idleTimeRange = new Vector2(0.35f, 1.05f);

	[SerializeField]
	private Vector2 slowSpeedRange = new Vector2(25f, 55f);

	[SerializeField]
	private Vector2 fastSpeedRange = new Vector2(70f, 130f);

	[SerializeField]
	private Vector2 walkDistanceRange = new Vector2(20f, 120f);

	[SerializeField]
	[Range(0f, 1f)]
	private float slowSpeedChance = 0.75f;

	[Header("Solid Image")]
	[SerializeField]
	private bool restrictToSolidPixels = true;

	[SerializeField]
	[Range(0f, 1f)]
	private float solidAlphaThreshold = 0.1f;

	[SerializeField]
	private int solidPointSearchAttempts = 64;

	[SerializeField]
	private int solidPathSampleCount = 8;

	[SerializeField]
	[Range(0f, 1f)]
	private float solidBodyRadiusScale = 0.5f;

	[Header("Fly Away")]
	[SerializeField]
	private Vector2 flyAwayTimeRange = new Vector2(0.45f, 0.75f);

	[SerializeField]
	private Vector2 flyAwayAngleRange = new Vector2(55f, 125f);

	[SerializeField]
	private Vector2 flyAwayDistanceRange = new Vector2(220f, 340f);

	[NonSerialized]
	private readonly List<FlyInstance> flies = new List<FlyInstance>();

	[NonSerialized]
	private RectTransform cachedCrawlArea;

	[NonSerialized]
	private Image cachedCrawlImage;

	[NonSerialized]
	private bool isTargetDisplay;

	[NonSerialized]
	private bool hasFlownAway;

	[NonSerialized]
	private bool solidSamplingUnavailable;

	public override void Setup()
	{
		ClearRuntimeObjects();
		cachedCrawlArea = null;
		cachedCrawlImage = null;
		solidSamplingUnavailable = false;
		hasFlownAway = false;
		isTargetDisplay = ShouldPlayForCurrentDisplay();
		if (isTargetDisplay)
		{
			SpawnFlies();
		}
	}

	public override void Unset()
	{
		isTargetDisplay = false;
		hasFlownAway = false;
		cachedCrawlArea = null;
		cachedCrawlImage = null;
		solidSamplingUnavailable = false;
		ClearRuntimeObjects();
	}

	public override void Dispose()
	{
		ClearRuntimeObjects();
	}

	void IFrameUpdate.FrameUpdate()
	{
		if (!isTargetDisplay || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		for (int num = flies.Count - 1; num >= 0; num--)
		{
			if (UpdateFly(flies[num], deltaTime))
			{
				RemoveFlyAt(num);
			}
		}
	}

	public void BeginExitFlyAway()
	{
		if (isTargetDisplay && !hasFlownAway)
		{
			hasFlownAway = true;
			for (int i = 0; i < flies.Count; i++)
			{
				BeginFlyAway(flies[i]);
			}
		}
	}

	private bool ShouldPlayForCurrentDisplay()
	{
		if (targetItemDefinition == null || base.item?.GetDef() != targetItemDefinition)
		{
			return false;
		}
		if (!onlyMainBattleInfoPanel && HasMainBattleOnlySibling())
		{
			return false;
		}
		if (!onlyMainBattleInfoPanel)
		{
			return true;
		}
		return UIDisplayModule.gameState?.UIManager?.battleItemInfoPanel?.battleItemInfoDisplay == base.mainItemInfoDisplay.itemInfoDisplay;
	}

	private bool HasMainBattleOnlySibling()
	{
		MainItemInfoFlyCrawlModule[] components = base.mainItemInfoDisplay.GetComponents<MainItemInfoFlyCrawlModule>();
		foreach (MainItemInfoFlyCrawlModule mainItemInfoFlyCrawlModule in components)
		{
			if (mainItemInfoFlyCrawlModule != null && mainItemInfoFlyCrawlModule != this && mainItemInfoFlyCrawlModule.onlyMainBattleInfoPanel)
			{
				return true;
			}
		}
		return false;
	}

	private void SpawnFlies()
	{
		RectTransform rectTransform = ResolveCrawlArea();
		if (rectTransform == null || !HasAnyFlySprite())
		{
			return;
		}
		int num = Mathf.Max(0, minFlyCount);
		int num2 = Mathf.Max(num, maxFlyCount);
		if (num2 > 0)
		{
			int num3 = UnityEngine.Random.Range(num, num2 + 1);
			for (int i = 0; i < num3; i++)
			{
				flies.Add(CreateFly(rectTransform));
			}
		}
	}

	private FlyInstance CreateFly(RectTransform area)
	{
		GameObject obj = new GameObject("MainItemInfoFlyCrawl", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		obj.transform.SetParent(area, worldPositionStays: false);
		obj.transform.SetAsLastSibling();
		RectTransform rectTransform = (RectTransform)obj.transform;
		rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		Image component = obj.GetComponent<Image>();
		component.raycastTarget = false;
		component.preserveAspect = true;
		bool flag = bigFlySprite != null && (smallFlySprite == null || UnityEngine.Random.value > 0.5f);
		component.sprite = (flag ? bigFlySprite : smallFlySprite);
		Vector2 vector = (flag ? bigFlySizeRange : smallFlySizeRange);
		float desiredSize = UnityEngine.Random.Range(Mathf.Min(vector.x, vector.y), Mathf.Max(vector.x, vector.y));
		desiredSize = GetFittedFlySize(desiredSize, area);
		rectTransform.sizeDelta = new Vector2(desiredSize, desiredSize);
		FlyInstance flyInstance = new FlyInstance
		{
			rectTransform = rectTransform,
			image = component,
			bodyRadius = desiredSize * Mathf.Clamp01(solidBodyRadiusScale)
		};
		BeginLanding(flyInstance);
		return flyInstance;
	}

	private bool UpdateFly(FlyInstance fly, float deltaTime)
	{
		if (fly.rectTransform == null)
		{
			return true;
		}
		switch (fly.state)
		{
		case FlyState.Landing:
			UpdateLanding(fly, deltaTime);
			return false;
		case FlyState.Idle:
			UpdateIdle(fly, deltaTime);
			return false;
		case FlyState.Walk:
			UpdateWalk(fly, deltaTime);
			return false;
		case FlyState.FlyAway:
			return UpdateFlyAway(fly, deltaTime);
		default:
			return true;
		}
	}

	private void BeginLanding(FlyInstance fly)
	{
		RectTransform rectTransform = ResolveCrawlArea();
		if (!(rectTransform == null))
		{
			Rect rect = rectTransform.rect;
			Vector2 vector = new Vector2(UnityEngine.Random.Range(rect.xMin, rect.xMax), rect.yMax + UnityEngine.Random.Range(35f, 90f));
			fly.state = FlyState.Landing;
			fly.startPosition = vector;
			fly.targetPosition = GetRandomPointInCrawlArea(fly.bodyRadius);
			fly.elapsed = 0f;
			fly.duration = RandomRange(landingTimeRange);
			fly.rectTransform.anchoredPosition = vector;
			RotateToward(fly, fly.targetPosition - vector);
		}
	}

	private void UpdateLanding(FlyInstance fly, float deltaTime)
	{
		fly.elapsed += deltaTime;
		float num = Mathf.Clamp01(fly.elapsed / Mathf.Max(0.01f, fly.duration));
		float t = Mathf.SmoothStep(0f, 1f, num);
		fly.rectTransform.anchoredPosition = Vector2.LerpUnclamped(fly.startPosition, fly.targetPosition, t);
		RotateToward(fly, fly.targetPosition - fly.startPosition);
		if (num >= 1f)
		{
			BeginIdle(fly);
		}
	}

	private void BeginIdle(FlyInstance fly)
	{
		fly.state = FlyState.Idle;
		fly.elapsed = 0f;
		fly.duration = RandomRange(idleTimeRange);
	}

	private void UpdateIdle(FlyInstance fly, float deltaTime)
	{
		fly.elapsed += deltaTime;
		if (fly.elapsed >= fly.duration)
		{
			BeginWalk(fly);
		}
	}

	private void BeginWalk(FlyInstance fly)
	{
		fly.state = FlyState.Walk;
		fly.speed = GetRandomWalkSpeed();
		fly.targetPosition = GetRandomWalkTarget(fly.rectTransform.anchoredPosition, fly.speed, fly.bodyRadius);
		if ((fly.targetPosition - fly.rectTransform.anchoredPosition).sqrMagnitude <= 0.1f)
		{
			BeginIdle(fly);
		}
		else
		{
			RotateToward(fly, fly.targetPosition - fly.rectTransform.anchoredPosition);
		}
	}

	private void UpdateWalk(FlyInstance fly, float deltaTime)
	{
		Vector2 anchoredPosition = fly.rectTransform.anchoredPosition;
		Vector2 vector = Vector2.MoveTowards(anchoredPosition, fly.targetPosition, fly.speed * deltaTime);
		if (!CanStandOnSolidPixel(vector, fly.bodyRadius))
		{
			BeginIdle(fly);
			return;
		}
		fly.rectTransform.anchoredPosition = vector;
		RotateToward(fly, fly.targetPosition - anchoredPosition);
		if (Vector2.Distance(vector, fly.targetPosition) <= 0.1f)
		{
			BeginIdle(fly);
		}
	}

	private void BeginFlyAway(FlyInstance fly)
	{
		if (!(fly.rectTransform == null))
		{
			float f = UnityEngine.Random.Range(flyAwayAngleRange.x, flyAwayAngleRange.y) * ((float)Math.PI / 180f);
			Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
			fly.state = FlyState.FlyAway;
			fly.startPosition = fly.rectTransform.anchoredPosition;
			fly.targetPosition = fly.startPosition + vector * RandomRange(flyAwayDistanceRange);
			fly.elapsed = 0f;
			fly.duration = RandomRange(flyAwayTimeRange);
			fly.startAlpha = ((fly.image != null) ? fly.image.color.a : 1f);
			RotateToward(fly, vector);
		}
	}

	private bool UpdateFlyAway(FlyInstance fly, float deltaTime)
	{
		fly.elapsed += deltaTime;
		float num = Mathf.Clamp01(fly.elapsed / Mathf.Max(0.01f, fly.duration));
		float t = Mathf.SmoothStep(0f, 1f, num);
		fly.rectTransform.anchoredPosition = Vector2.LerpUnclamped(fly.startPosition, fly.targetPosition, t);
		if (fly.image != null)
		{
			Color color = fly.image.color;
			color.a = Mathf.Lerp(fly.startAlpha, 0f, t);
			fly.image.color = color;
		}
		return num >= 1f;
	}

	private RectTransform ResolveCrawlArea()
	{
		if (crawlArea != null)
		{
			return crawlArea;
		}
		if (cachedCrawlArea != null)
		{
			return cachedCrawlArea;
		}
		Image[] componentsInChildren = base.mainItemInfoDisplay.GetComponentsInChildren<Image>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!(componentsInChildren[i].name != "ItemImage"))
			{
				cachedCrawlArea = componentsInChildren[i].rectTransform;
				return cachedCrawlArea;
			}
		}
		cachedCrawlArea = base.mainItemInfoDisplay.transform as RectTransform;
		return cachedCrawlArea;
	}

	private Image ResolveCrawlImage()
	{
		if (cachedCrawlImage != null)
		{
			return cachedCrawlImage;
		}
		RectTransform rectTransform = ResolveCrawlArea();
		if (rectTransform != null && rectTransform.TryGetComponent<Image>(out var component))
		{
			cachedCrawlImage = component;
			return cachedCrawlImage;
		}
		Image[] componentsInChildren = base.mainItemInfoDisplay.GetComponentsInChildren<Image>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!(componentsInChildren[i].name != "ItemImage"))
			{
				cachedCrawlImage = componentsInChildren[i];
				return cachedCrawlImage;
			}
		}
		return null;
	}

	private Vector2 GetRandomPointInCrawlArea(float bodyRadius)
	{
		if (!TryGetCrawlBounds(bodyRadius, out var minX, out var maxX, out var minY, out var maxY))
		{
			return Vector2.zero;
		}
		int num = Mathf.Max(1, solidPointSearchAttempts);
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
			if (CanStandOnSolidPixel(vector, bodyRadius))
			{
				return vector;
			}
		}
		if (TryFindNearestSolidPoint(Vector2.zero, bodyRadius, out var point))
		{
			return point;
		}
		return new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
	}

	private Vector2 GetRandomWalkTarget(Vector2 currentPosition, float speed, float bodyRadius)
	{
		float walkDistance = GetWalkDistance(speed);
		Vector2 result = currentPosition;
		float num = -1f;
		int num2 = Mathf.Max(8, solidPointSearchAttempts);
		for (int i = 0; i < num2; i++)
		{
			float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
			Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
			float num3 = walkDistance * UnityEngine.Random.Range(0.35f, 1f);
			Vector2 vector2 = ClampPointToCrawlArea(currentPosition + vector * num3, bodyRadius);
			if (CanStandOnSolidPixel(vector2, bodyRadius) && CanMoveAcrossSolidPixels(currentPosition, vector2, bodyRadius))
			{
				float sqrMagnitude = (vector2 - currentPosition).sqrMagnitude;
				if (!(sqrMagnitude <= num))
				{
					result = vector2;
					num = sqrMagnitude;
				}
			}
		}
		if (num > 0.1f)
		{
			return result;
		}
		if (!TryFindNearestSolidPoint(currentPosition, bodyRadius, out var point) || !CanMoveAcrossSolidPixels(currentPosition, point, bodyRadius))
		{
			return currentPosition;
		}
		return point;
	}

	private float GetWalkDistance(float speed)
	{
		float a = Mathf.Min(Mathf.Min(slowSpeedRange.x, slowSpeedRange.y), Mathf.Min(fastSpeedRange.x, fastSpeedRange.y));
		float b = Mathf.Max(Mathf.Max(slowSpeedRange.x, slowSpeedRange.y), Mathf.Max(fastSpeedRange.x, fastSpeedRange.y));
		float t = Mathf.InverseLerp(a, b, speed);
		return Mathf.Lerp(Mathf.Min(walkDistanceRange.x, walkDistanceRange.y), Mathf.Max(walkDistanceRange.x, walkDistanceRange.y), t);
	}

	private float GetFittedFlySize(float desiredSize, RectTransform area)
	{
		if (!fitFlyInsideCrawlArea || area == null)
		{
			return desiredSize;
		}
		Rect rect = area.rect;
		float a = rect.width - crawlPadding.x - crawlPadding.z;
		float num = rect.height - crawlPadding.y - crawlPadding.w;
		Image image = ResolveCrawlImage();
		if (image != null && image.rectTransform == area && image.sprite != null)
		{
			Rect imageDrawRect = GetImageDrawRect(image, image.sprite);
			a = Mathf.Min(a, imageDrawRect.width - crawlPadding.x - crawlPadding.z);
			num = Mathf.Min(num, imageDrawRect.height - crawlPadding.y - crawlPadding.w);
		}
		float b = Mathf.Max(1f, Mathf.Min(a, num)) * Mathf.Max(1f, fittedFlySizeMultiplier);
		return Mathf.Min(desiredSize, b);
	}

	private Vector2 ClampPointToCrawlArea(Vector2 point, float bodyRadius)
	{
		if (!TryGetCrawlBounds(bodyRadius, out var minX, out var maxX, out var minY, out var maxY))
		{
			return point;
		}
		return new Vector2(Mathf.Clamp(point.x, minX, maxX), Mathf.Clamp(point.y, minY, maxY));
	}

	private bool CanStandOnSolidPixel(Vector2 crawlLocalPoint, float bodyRadius = 0f)
	{
		if (!restrictToSolidPixels)
		{
			return true;
		}
		if (!IsInsideCrawlBounds(crawlLocalPoint, bodyRadius))
		{
			return false;
		}
		if (solidSamplingUnavailable)
		{
			return true;
		}
		Image image = ResolveCrawlImage();
		Sprite sprite = ((image != null) ? image.sprite : null);
		if (image == null || sprite == null || sprite.texture == null)
		{
			return true;
		}
		if (bodyRadius > 0.01f)
		{
			foreach (Vector2 bodySamplePoint in GetBodySamplePoints(crawlLocalPoint, bodyRadius))
			{
				if (!CanStandOnSolidPixel(bodySamplePoint))
				{
					return false;
				}
			}
			return true;
		}
		RectTransform rectTransform = ResolveCrawlArea();
		if (rectTransform == null)
		{
			return true;
		}
		Vector2 point = ((image.rectTransform == rectTransform) ? crawlLocalPoint : ((Vector2)image.rectTransform.InverseTransformPoint(rectTransform.TransformPoint(crawlLocalPoint))));
		Rect imageDrawRect = GetImageDrawRect(image, sprite);
		if (!imageDrawRect.Contains(point))
		{
			return false;
		}
		Vector2 vector = new Vector2(Mathf.InverseLerp(imageDrawRect.xMin, imageDrawRect.xMax, point.x), Mathf.InverseLerp(imageDrawRect.yMin, imageDrawRect.yMax, point.y));
		try
		{
			Rect textureRect = sprite.textureRect;
			Texture2D texture = sprite.texture;
			float u = Mathf.Clamp01((textureRect.x + vector.x * textureRect.width) / (float)texture.width);
			float v = Mathf.Clamp01((textureRect.y + vector.y * textureRect.height) / (float)texture.height);
			return texture.GetPixelBilinear(u, v).a >= solidAlphaThreshold;
		}
		catch (UnityException)
		{
			solidSamplingUnavailable = true;
			return true;
		}
	}

	private bool CanMoveAcrossSolidPixels(Vector2 start, Vector2 end, float bodyRadius)
	{
		if (!restrictToSolidPixels || solidSamplingUnavailable)
		{
			return true;
		}
		int num = Mathf.Max(1, solidPathSampleCount);
		for (int i = 0; i <= num; i++)
		{
			float t = (float)i / (float)num;
			if (!CanStandOnSolidPixel(Vector2.Lerp(start, end, t), bodyRadius))
			{
				return false;
			}
		}
		return true;
	}

	private bool TryFindNearestSolidPoint(Vector2 from, float bodyRadius, out Vector2 point)
	{
		point = from;
		if (!TryGetCrawlBounds(bodyRadius, out var minX, out var maxX, out var minY, out var maxY))
		{
			return false;
		}
		int num = Mathf.Max(3, Mathf.RoundToInt(Mathf.Sqrt(Mathf.Max(1, solidPointSearchAttempts))));
		float num2 = float.MaxValue;
		bool result = false;
		for (int i = 0; i < num; i++)
		{
			float x = Mathf.Lerp(minX, maxX, (num == 1) ? 0.5f : ((float)i / (float)(num - 1)));
			for (int j = 0; j < num; j++)
			{
				float y = Mathf.Lerp(minY, maxY, (num == 1) ? 0.5f : ((float)j / (float)(num - 1)));
				Vector2 vector = new Vector2(x, y);
				if (CanStandOnSolidPixel(vector, bodyRadius))
				{
					float sqrMagnitude = (vector - from).sqrMagnitude;
					if (!(sqrMagnitude >= num2))
					{
						point = vector;
						num2 = sqrMagnitude;
						result = true;
					}
				}
			}
		}
		return result;
	}

	private Rect GetImageDrawRect(Image image, Sprite sprite)
	{
		Rect rect = image.rectTransform.rect;
		if (!image.preserveAspect || sprite.rect.width <= 0f || sprite.rect.height <= 0f || rect.width <= 0f || rect.height <= 0f)
		{
			return rect;
		}
		float num = sprite.rect.width / sprite.rect.height;
		if (rect.width / rect.height > num)
		{
			float num2 = rect.height * num;
			return new Rect(rect.x + (rect.width - num2) * 0.5f, rect.y, num2, rect.height);
		}
		float num3 = rect.width / num;
		return new Rect(rect.x, rect.y + (rect.height - num3) * 0.5f, rect.width, num3);
	}

	private bool IsInsideCrawlBounds(Vector2 point, float bodyRadius)
	{
		if (!TryGetCrawlBounds(bodyRadius, out var minX, out var maxX, out var minY, out var maxY))
		{
			return true;
		}
		if (point.x >= minX && point.x <= maxX && point.y >= minY)
		{
			return point.y <= maxY;
		}
		return false;
	}

	private IEnumerable<Vector2> GetBodySamplePoints(Vector2 center, float bodyRadius)
	{
		float radius = Mathf.Max(0f, bodyRadius);
		yield return center;
		yield return center + Vector2.right * radius;
		yield return center + Vector2.left * radius;
		yield return center + Vector2.up * radius;
		yield return center + Vector2.down * radius;
		float diagonal = radius * 0.70710677f;
		yield return center + new Vector2(diagonal, diagonal);
		yield return center + new Vector2(0f - diagonal, diagonal);
		yield return center + new Vector2(diagonal, 0f - diagonal);
		yield return center + new Vector2(0f - diagonal, 0f - diagonal);
	}

	private bool TryGetCrawlBounds(float bodyRadius, out float minX, out float maxX, out float minY, out float maxY)
	{
		RectTransform rectTransform = ResolveCrawlArea();
		if (rectTransform == null)
		{
			minX = 0f;
			maxX = 0f;
			minY = 0f;
			maxY = 0f;
			return false;
		}
		Rect rect = rectTransform.rect;
		float num = Mathf.Max(0f, bodyRadius);
		minX = rect.xMin + crawlPadding.x + num;
		maxX = rect.xMax - crawlPadding.z - num;
		minY = rect.yMin + crawlPadding.y + num;
		maxY = rect.yMax - crawlPadding.w - num;
		if (minX > maxX)
		{
			maxX = (minX = rect.center.x);
		}
		if (minY > maxY)
		{
			maxY = (minY = rect.center.y);
		}
		return true;
	}

	private float GetRandomWalkSpeed()
	{
		Vector2 vector = ((UnityEngine.Random.value <= slowSpeedChance) ? slowSpeedRange : fastSpeedRange);
		return UnityEngine.Random.Range(Mathf.Min(vector.x, vector.y), Mathf.Max(vector.x, vector.y));
	}

	private bool HasAnyFlySprite()
	{
		if (!(bigFlySprite != null))
		{
			return smallFlySprite != null;
		}
		return true;
	}

	private float RandomRange(Vector2 range)
	{
		return UnityEngine.Random.Range(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y));
	}

	private void RotateToward(FlyInstance fly, Vector2 direction)
	{
		if (!(direction.sqrMagnitude < 0.001f) && !(fly.rectTransform == null))
		{
			float z = Mathf.Atan2(direction.y, direction.x) * 57.29578f - spriteHeadAngle;
			fly.rectTransform.localRotation = Quaternion.Euler(0f, 0f, z);
		}
	}

	private void ClearRuntimeObjects()
	{
		for (int num = flies.Count - 1; num >= 0; num--)
		{
			RemoveFlyAt(num);
		}
		flies.Clear();
	}

	private void RemoveFlyAt(int index)
	{
		if (index >= 0 && index < flies.Count)
		{
			FlyInstance flyInstance = flies[index];
			if (flyInstance.rectTransform != null)
			{
				UnityEngine.Object.Destroy(flyInstance.rectTransform.gameObject);
			}
			flies.RemoveAt(index);
		}
	}
}
