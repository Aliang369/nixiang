using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class CardFlyCrawlModule : ItemDisplayBase.DisplayModule, IFrameUpdate
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

	private class FlyAwayParticleInstance
	{
		private readonly ParticleSystem[] particleSystems;

		private ParticleScatterData[] scatterData;

		private bool isScattering;

		private float scatterDuration;

		private float scatterElapsed;

		public GameObject GameObject { get; }

		public FlyAwayParticleInstance(GameObject gameObject)
		{
			GameObject = gameObject;
			particleSystems = ((gameObject != null) ? gameObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true) : Array.Empty<ParticleSystem>());
		}

		public void BeginScatter(Transform scatterParent, float duration, Vector2 speedRange, float angleRange, float upwardBias)
		{
			if (!isScattering)
			{
				MoveToParent(scatterParent);
				isScattering = true;
				scatterElapsed = 0f;
				scatterDuration = Mathf.Max(0.01f, duration);
				scatterData = new ParticleScatterData[particleSystems.Length];
				for (int i = 0; i < particleSystems.Length; i++)
				{
					scatterData[i] = BeginScatterParticleSystem(particleSystems[i], speedRange, angleRange, upwardBias);
				}
			}
		}

		public bool UpdateScatter(float deltaTime)
		{
			if (!isScattering)
			{
				return false;
			}
			scatterElapsed += deltaTime;
			float num = Mathf.Clamp01(scatterElapsed / scatterDuration);
			float alphaMultiplier = 1f - Mathf.SmoothStep(0f, 1f, num);
			for (int i = 0; i < scatterData.Length; i++)
			{
				UpdateScatterParticleSystem(scatterData[i], alphaMultiplier);
			}
			return num >= 1f;
		}

		public void MoveToParent(Transform parent)
		{
			if (!(GameObject == null) && !(parent == null) && !(GameObject.transform.parent == parent))
			{
				GameObject.transform.SetParent(parent, worldPositionStays: true);
			}
		}

		public void SetWorldPositionAndDirection(Vector3 worldPosition, float positionLerp, Vector3 direction, float headAngle, float rotationLerpSpeed, float deltaTime)
		{
			if (!(GameObject == null) && !isScattering)
			{
				Transform transform = GameObject.transform;
				Vector3 position = transform.position;
				transform.position = ((positionLerp >= 1f) ? worldPosition : Vector3.Lerp(transform.position, worldPosition, Mathf.Clamp01(positionLerp)));
				Vector2 vector = new Vector2(direction.x, direction.y);
				if (vector.sqrMagnitude < 0.001f)
				{
					Vector3 vector2 = worldPosition - position;
					vector = new Vector2(vector2.x, vector2.y);
				}
				if (!(vector.sqrMagnitude < 0.001f))
				{
					float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f - headAngle;
					Quaternion b = Quaternion.Euler(0f, 0f, z);
					float value = ((rotationLerpSpeed <= 0f) ? 1f : (1f - Mathf.Exp((0f - rotationLerpSpeed) * deltaTime)));
					transform.rotation = Quaternion.Slerp(transform.rotation, b, Mathf.Clamp01(value));
				}
			}
		}

		private static ParticleScatterData BeginScatterParticleSystem(ParticleSystem particleSystem, Vector2 speedRange, float angleRange, float upwardBias)
		{
			if ((UnityEngine.Object)(object)particleSystem == null)
			{
				return null;
			}
			particleSystem.Stop(true, (ParticleSystemStopBehavior)1);
			Particle[] array = (Particle[])(object)new Particle[Mathf.Max(1, particleSystem.particleCount)];
			int particles = particleSystem.GetParticles(array);
			Color32[] array2 = new Color32[particles];
			for (int i = 0; i < particles; i++)
			{
				array2[i] = ((Particle)(ref array[i])).startColor;
				((Particle)(ref array[i])).velocity = GetScatterVelocity(((Particle)(ref array[i])).velocity, speedRange, angleRange, upwardBias);
			}
			if (particles > 0)
			{
				particleSystem.SetParticles(array, particles);
			}
			return new ParticleScatterData
			{
				ParticleSystem = particleSystem,
				Particles = array,
				StartColors = array2
			};
		}

		private static void UpdateScatterParticleSystem(ParticleScatterData scatterData, float alphaMultiplier)
		{
			if (!((UnityEngine.Object)(object)scatterData?.ParticleSystem == null) && scatterData.Particles != null)
			{
				int particles = scatterData.ParticleSystem.GetParticles(scatterData.Particles);
				for (int i = 0; i < particles; i++)
				{
					Color32 startColor = ((i < scatterData.StartColors.Length) ? scatterData.StartColors[i] : ((Particle)(ref scatterData.Particles[i])).startColor);
					startColor.a = (byte)Mathf.RoundToInt((float)(int)startColor.a * alphaMultiplier);
					((Particle)(ref scatterData.Particles[i])).startColor = startColor;
				}
				if (particles > 0)
				{
					scatterData.ParticleSystem.SetParticles(scatterData.Particles, particles);
				}
			}
		}

		private static Vector3 GetScatterVelocity(Vector3 currentVelocity, Vector2 speedRange, float angleRange, float upwardBias)
		{
			Vector2 vector = new Vector2(currentVelocity.x, currentVelocity.y);
			if (vector.sqrMagnitude < 0.001f)
			{
				vector = UnityEngine.Random.insideUnitCircle;
				if (vector.sqrMagnitude < 0.001f)
				{
					vector = Vector2.right;
				}
				vector = (vector.normalized + Vector2.up * Mathf.Max(0f, upwardBias)).normalized;
			}
			else
			{
				float angle = UnityEngine.Random.Range(0f - Mathf.Abs(angleRange), Mathf.Abs(angleRange));
				vector = Rotate(vector.normalized, angle);
			}
			float num = currentVelocity.magnitude + UnityEngine.Random.Range(Mathf.Min(speedRange.x, speedRange.y), Mathf.Max(speedRange.x, speedRange.y));
			return new Vector3(vector.x, vector.y, 0f) * num;
		}

		private static Vector2 Rotate(Vector2 vector, float angle)
		{
			float f = angle * ((float)Math.PI / 180f);
			float num = Mathf.Sin(f);
			float num2 = Mathf.Cos(f);
			return new Vector2(vector.x * num2 - vector.y * num, vector.x * num + vector.y * num2);
		}
	}

	private class ParticleScatterData
	{
		public ParticleSystem ParticleSystem;

		public Particle[] Particles;

		public Color32[] StartColors;
	}

	[Header("Target")]
	[SerializeField]
	private ItemDefinition targetItemDefinition;

	[SerializeField]
	private RectTransform crawlArea;

	[Header("Sprites")]
	[SerializeField]
	private Sprite bigFlySprite;

	[SerializeField]
	private Sprite smallFlySprite;

	[SerializeField]
	private GameObject flyAwayParticlePrefab;

	[Header("Count")]
	[SerializeField]
	private int minFlyCount = 1;

	[SerializeField]
	private int maxFlyCount = 2;

	[Header("Size")]
	[SerializeField]
	private Vector2 smallFlySizeRange = new Vector2(18f, 26f);

	[SerializeField]
	private Vector2 bigFlySizeRange = new Vector2(28f, 42f);

	[Header("Orientation")]
	[SerializeField]
	private float crawlSpriteHeadAngle = 180f;

	[SerializeField]
	[FormerlySerializedAs("spriteHeadAngle")]
	private float flyAwayParticleHeadAngle = 300f;

	[Header("Movement")]
	[SerializeField]
	private Vector4 crawlPadding = new Vector4(10f, 10f, 10f, 10f);

	[SerializeField]
	private Vector2 landingTimeRange = new Vector2(0.45f, 0.85f);

	[SerializeField]
	private Vector2 idleTimeRange = new Vector2(0.35f, 1.05f);

	[SerializeField]
	private Vector2 slowSpeedRange = new Vector2(18f, 35f);

	[SerializeField]
	private Vector2 fastSpeedRange = new Vector2(45f, 85f);

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
	private int solidPointSearchAttempts = 48;

	[SerializeField]
	private int solidPathSampleCount = 6;

	[SerializeField]
	[Range(0f, 1f)]
	private float solidBodyRadiusScale = 0.25f;

	[Header("Fly Away")]
	[SerializeField]
	private Vector2 flyAwayTimeRange = new Vector2(0.45f, 0.75f);

	[SerializeField]
	private Vector2 flyAwayAngleRange = new Vector2(60f, 120f);

	[SerializeField]
	private Vector2 flyAwayDistanceRange = new Vector2(160f, 240f);

	[SerializeField]
	private Vector2 respawnDelayRange = new Vector2(0.5f, 1f);

	[SerializeField]
	private float flyAwayParticleLifetime = 3.5f;

	[Header("Cancel Scatter")]
	[SerializeField]
	private RectTransform cancelScatterRoot;

	[SerializeField]
	private Vector2 cancelScatterDurationRange = new Vector2(0.35f, 0.65f);

	[SerializeField]
	private Vector2 cancelScatterSpeedRange = new Vector2(120f, 260f);

	[SerializeField]
	private float cancelScatterAngleRange = 25f;

	[SerializeField]
	private float cancelScatterUpwardBias = 0.25f;

	[Header("Drag Follow")]
	[SerializeField]
	private bool followDragPath = true;

	[SerializeField]
	private RectTransform dragFollowRoot;

	[SerializeField]
	private float dragFollowOffset = 80f;

	[SerializeField]
	private float dragFollowLerpSpeed = 18f;

	[SerializeField]
	private float dragFollowRotationLerpSpeed = 9f;

	[SerializeField]
	private float dragFollowMinMoveDistance = 3f;

	[NonSerialized]
	private readonly List<FlyInstance> flies = new List<FlyInstance>();

	[NonSerialized]
	private readonly List<FlyAwayParticleInstance> flyAwayParticles = new List<FlyAwayParticleInstance>();

	[NonSerialized]
	private InputModule inputModule;

	[NonSerialized]
	private PlayerTableDisplayStateModule stateModule;

	[NonSerialized]
	private Item trackedItem;

	[NonSerialized]
	private Item releasedSpellItem;

	[NonSerialized]
	private Item waitingCooldownItem;

	[NonSerialized]
	private RectTransform cachedCrawlArea;

	[NonSerialized]
	private Image cachedCrawlImage;

	[NonSerialized]
	private Transform dragFollowParent;

	[NonSerialized]
	private bool hasFlownAway;

	[NonSerialized]
	private bool isTargetCard;

	[NonSerialized]
	private bool waitingForReturnToRest;

	[NonSerialized]
	private bool wasReleasedAfterFlyAway;

	[NonSerialized]
	private bool releasedAndCleared;

	[NonSerialized]
	private bool clearedFlyAwayParticlesAfterReturn;

	[NonSerialized]
	private bool observedFlyAwaySelection;

	[NonSerialized]
	private bool solidSamplingUnavailable;

	[NonSerialized]
	private bool dragFollowActive;

	[NonSerialized]
	private bool hasDragFollowTarget;

	[NonSerialized]
	private bool hasLastDragFollowPointerPosition;

	[NonSerialized]
	private Vector3 dragFollowTargetWorldPosition;

	[NonSerialized]
	private Vector3 dragFollowDirection;

	[NonSerialized]
	private Vector3 lastDragFollowPointerWorldPosition;

	[NonSerialized]
	private float respawnDelay;

	public override void Init()
	{
		inputModule = base.slotDisplay.GetModule<InputModule>();
		if (inputModule != null)
		{
			InputModule obj = inputModule;
			obj.onPointerClick = (Action<InputModuleBase>)Delegate.Combine(obj.onPointerClick, new Action<InputModuleBase>(OnPointerClick));
			InputModule obj2 = inputModule;
			obj2.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(obj2.onHandleInput, new Action<InputModuleBase>(OnHandleInput));
		}
		stateModule = base.slotDisplay.GetModule<PlayerTableDisplayStateModule>();
		UIDisplayModule.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeSpellCast, OnBeforeSpellCast);
		UIDisplayModule.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterSpellCast, OnAfterSpellCast);
	}

	public override void Setup()
	{
		ClearFlyAwayParticles();
		ClearFlies();
		trackedItem = base.slotDisplay.item;
		releasedSpellItem = null;
		waitingCooldownItem = null;
		cachedCrawlArea = null;
		cachedCrawlImage = null;
		hasFlownAway = false;
		waitingForReturnToRest = false;
		wasReleasedAfterFlyAway = false;
		releasedAndCleared = false;
		clearedFlyAwayParticlesAfterReturn = false;
		observedFlyAwaySelection = false;
		solidSamplingUnavailable = false;
		ResetDragFollow();
		respawnDelay = 0f;
		isTargetCard = IsTargetCard();
		if (isTargetCard)
		{
			if (trackedItem.IsCooldownCastAble(UIDisplayModule.gameState))
			{
				SpawnFlies();
			}
			else
			{
				ClearAfterReleased();
			}
		}
	}

	public override void Unset()
	{
		isTargetCard = false;
		trackedItem = null;
		releasedSpellItem = null;
		waitingCooldownItem = null;
		hasFlownAway = false;
		waitingForReturnToRest = false;
		wasReleasedAfterFlyAway = false;
		releasedAndCleared = false;
		clearedFlyAwayParticlesAfterReturn = false;
		observedFlyAwaySelection = false;
		ResetDragFollow();
		respawnDelay = 0f;
		cachedCrawlArea = null;
		cachedCrawlImage = null;
		solidSamplingUnavailable = false;
		ClearFlyAwayParticles();
		ClearFlies();
	}

	public override void Dispose()
	{
		if (inputModule != null)
		{
			InputModule obj = inputModule;
			obj.onPointerClick = (Action<InputModuleBase>)Delegate.Remove(obj.onPointerClick, new Action<InputModuleBase>(OnPointerClick));
			InputModule obj2 = inputModule;
			obj2.onHandleInput = (Action<InputModuleBase>)Delegate.Remove(obj2.onHandleInput, new Action<InputModuleBase>(OnHandleInput));
		}
		inputModule = null;
		stateModule = null;
		trackedItem = null;
		releasedSpellItem = null;
		waitingCooldownItem = null;
		releasedAndCleared = false;
		cachedCrawlArea = null;
		cachedCrawlImage = null;
		solidSamplingUnavailable = false;
		UIDisplayModule.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeSpellCast, OnBeforeSpellCast);
		UIDisplayModule.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterSpellCast, OnAfterSpellCast);
		ClearFlyAwayParticles();
		ClearFlies();
	}

	void IFrameUpdate.FrameUpdate()
	{
		if (!isTargetCard)
		{
			return;
		}
		if (releasedAndCleared)
		{
			TryRespawnAfterCooldown();
			return;
		}
		float deltaTime = Time.deltaTime;
		UpdateDragFollow(deltaTime);
		UpdateFlyAwayParticles(deltaTime);
		for (int num = flies.Count - 1; num >= 0; num--)
		{
			if (UpdateFly(flies[num], deltaTime))
			{
				RemoveFlyAt(num);
			}
		}
		TryRespawnAfterReturnToRest();
	}

	private void OnPointerClick(InputModuleBase _)
	{
		TriggerFlyAwayFeedback(followDrag: false);
	}

	private void OnHandleInput(InputModuleBase input)
	{
		if ((object)input != null && input.beginDrag)
		{
			if (hasFlownAway)
			{
				BeginDragFollow();
			}
			else
			{
				TriggerFlyAwayFeedback(followDrag: true);
			}
		}
		if ((object)input != null && input.drag && input.eventData != null)
		{
			UpdateDragFollowTarget(input.eventData.position);
		}
		if ((object)input != null && input.endDrag)
		{
			dragFollowActive = false;
		}
	}

	private void TriggerFlyAwayFeedback(bool followDrag)
	{
		if (isTargetCard && !hasFlownAway && flies.Count != 0)
		{
			hasFlownAway = true;
			waitingForReturnToRest = true;
			wasReleasedAfterFlyAway = false;
			clearedFlyAwayParticlesAfterReturn = false;
			observedFlyAwaySelection = false;
			respawnDelay = -1f;
			PlayFlyAwayParticle();
			if (followDrag)
			{
				BeginDragFollow();
			}
			for (int i = 0; i < flies.Count; i++)
			{
				BeginFlyAway(flies[i]);
			}
		}
	}

	private void OnBeforeSpellCast(GameEvent.Type _, GameEvent gameEvent)
	{
		if (IsTrackedPlayerSpellCast(gameEvent))
		{
			releasedSpellItem = trackedItem;
			ClearAfterReleased();
		}
	}

	private void OnAfterSpellCast(GameEvent.Type _, GameEvent gameEvent)
	{
		if (IsTrackedPlayerSpellCast(gameEvent) || IsReleasedPlayerSpellCast(gameEvent))
		{
			if (!releasedAndCleared)
			{
				ClearAfterReleased();
			}
			releasedSpellItem = null;
		}
	}

	private bool IsTrackedPlayerSpellCast(GameEvent gameEvent)
	{
		if (releasedAndCleared || trackedItem == null)
		{
			return false;
		}
		if (gameEvent == null || gameEvent.SourceItem != trackedItem)
		{
			return false;
		}
		return IsPlayerOriginalSpellCast(gameEvent.spellInfo);
	}

	private bool IsReleasedPlayerSpellCast(GameEvent gameEvent)
	{
		if (releasedSpellItem == null)
		{
			return false;
		}
		if (gameEvent == null || gameEvent.SourceItem != releasedSpellItem)
		{
			return false;
		}
		return IsPlayerOriginalSpellCast(gameEvent.spellInfo);
	}

	private bool IsPlayerOriginalSpellCast(SpellCastInfo spellInfo)
	{
		if (spellInfo != null && spellInfo.casterType == SpellCastInfo.CasterType.Player)
		{
			return !spellInfo.isTriggerCast;
		}
		return false;
	}

	private void ClearAfterReleased()
	{
		releasedAndCleared = true;
		waitingCooldownItem = trackedItem ?? releasedSpellItem;
		trackedItem = waitingCooldownItem;
		hasFlownAway = true;
		waitingForReturnToRest = false;
		wasReleasedAfterFlyAway = true;
		clearedFlyAwayParticlesAfterReturn = false;
		observedFlyAwaySelection = false;
		respawnDelay = 0f;
		ResetDragFollow();
		ClearFlyAwayParticles();
		ClearFlies();
	}

	private void TryRespawnAfterCooldown()
	{
		if (releasedSpellItem == null && waitingCooldownItem != null && base.slotDisplay.item == waitingCooldownItem && IsStillInPlayerTable(waitingCooldownItem) && waitingCooldownItem.IsCooldownCastAble(UIDisplayModule.gameState))
		{
			releasedAndCleared = false;
			trackedItem = waitingCooldownItem;
			waitingCooldownItem = null;
			releasedSpellItem = null;
			hasFlownAway = false;
			waitingForReturnToRest = false;
			wasReleasedAfterFlyAway = false;
			clearedFlyAwayParticlesAfterReturn = false;
			observedFlyAwaySelection = false;
			respawnDelay = 0f;
			ResetDragFollow();
			ClearFlyAwayParticles();
			ClearFlies();
			SpawnFlies();
		}
	}

	private bool IsTargetCard()
	{
		if (targetItemDefinition != null)
		{
			return base.slotDisplay.item?.GetDef() == targetItemDefinition;
		}
		return false;
	}

	private void SpawnFlies()
	{
		RectTransform rectTransform = ResolveCrawlArea();
		if (!(rectTransform == null) && HasAnyFlySprite())
		{
			int num = UnityEngine.Random.Range(Mathf.Max(1, minFlyCount), Mathf.Max(minFlyCount, maxFlyCount) + 1);
			for (int i = 0; i < num; i++)
			{
				flies.Add(CreateFly(rectTransform));
			}
		}
	}

	private FlyInstance CreateFly(RectTransform area)
	{
		GameObject obj = new GameObject("CardFlyCrawl", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		obj.transform.SetParent(area, worldPositionStays: false);
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
		float num = UnityEngine.Random.Range(Mathf.Min(vector.x, vector.y), Mathf.Max(vector.x, vector.y));
		rectTransform.sizeDelta = new Vector2(num, num);
		FlyInstance flyInstance = new FlyInstance
		{
			rectTransform = rectTransform,
			image = component,
			bodyRadius = num * Mathf.Clamp01(solidBodyRadiusScale)
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
		Rect rect = ResolveCrawlArea().rect;
		Vector2 vector = new Vector2(UnityEngine.Random.Range(rect.xMin, rect.xMax), rect.yMax + UnityEngine.Random.Range(25f, 60f));
		fly.state = FlyState.Landing;
		fly.startPosition = vector;
		fly.targetPosition = GetRandomPointInCrawlArea(fly.bodyRadius);
		fly.elapsed = 0f;
		fly.duration = RandomRange(landingTimeRange);
		fly.rectTransform.anchoredPosition = vector;
		RotateToward(fly, fly.targetPosition - vector);
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

	private void TryRespawnAfterReturnToRest()
	{
		if (!waitingForReturnToRest || wasReleasedAfterFlyAway)
		{
			return;
		}
		PlayerControlInfo playerControlInfo = UIDisplayModule.gameState?.UIManager?.playerItemArea?.controlInfo;
		if (playerControlInfo != null && playerControlInfo.isCasting)
		{
			return;
		}
		if (IsFlyAwaySelectionActive(playerControlInfo))
		{
			observedFlyAwaySelection = true;
		}
		else
		{
			if (!observedFlyAwaySelection || !IsStillInPlayerTable())
			{
				return;
			}
			if (!clearedFlyAwayParticlesAfterReturn)
			{
				ScatterFlyAwayParticlesOnCancel();
				clearedFlyAwayParticlesAfterReturn = true;
			}
			if (IsRestState() && flies.Count <= 0)
			{
				if (respawnDelay < 0f)
				{
					respawnDelay = RandomRange(respawnDelayRange);
				}
				respawnDelay -= Time.deltaTime;
				if (!(respawnDelay > 0f))
				{
					hasFlownAway = false;
					waitingForReturnToRest = false;
					clearedFlyAwayParticlesAfterReturn = false;
					observedFlyAwaySelection = false;
					respawnDelay = 0f;
					SpawnFlies();
				}
			}
		}
	}

	private bool IsFlyAwaySelectionActive(PlayerControlInfo controlInfo)
	{
		Item item = base.slotDisplay.item;
		if (item == null)
		{
			return false;
		}
		if (controlInfo?.displaySpellInfo?.spellItem != item)
		{
			return UIDisplayModule.gameState?.playerState?.playerSpellInfo?.spellItem == item;
		}
		return true;
	}

	private bool IsStillInPlayerTable()
	{
		return IsStillInPlayerTable(base.slotDisplay.item);
	}

	private bool IsStillInPlayerTable(Item item)
	{
		if (item != null)
		{
			GameStateInfo gameStateInfo = UIDisplayModule.gameState;
			if (gameStateInfo == null)
			{
				return false;
			}
			PlayerGameStateInfo playerState = gameStateInfo.playerState;
			if (playerState == null)
			{
				return false;
			}
			return playerState.TableContainer?.GetItemIndex(item).HasValue ?? false;
		}
		return false;
	}

	private bool IsRestState()
	{
		if (stateModule == null)
		{
			return true;
		}
		if (stateModule.currentState != PlayerTableDisplayStateModule.State.Idle)
		{
			return stateModule.currentState == PlayerTableDisplayStateModule.State.Over;
		}
		return true;
	}

	private void BeginDragFollow()
	{
		if (!followDragPath)
		{
			return;
		}
		dragFollowActive = true;
		hasDragFollowTarget = false;
		hasLastDragFollowPointerPosition = false;
		dragFollowDirection = Vector3.zero;
		dragFollowParent = ResolveDragFollowParent();
		for (int num = flyAwayParticles.Count - 1; num >= 0; num--)
		{
			FlyAwayParticleInstance flyAwayParticleInstance = flyAwayParticles[num];
			if (flyAwayParticleInstance == null || flyAwayParticleInstance.GameObject == null)
			{
				flyAwayParticles.RemoveAt(num);
			}
			else
			{
				flyAwayParticleInstance.MoveToParent(dragFollowParent);
			}
		}
		if (inputModule?.eventData != null)
		{
			UpdateDragFollowTarget(inputModule.eventData.position);
		}
	}

	private void UpdateDragFollow(float deltaTime)
	{
		if (!dragFollowActive || !hasDragFollowTarget)
		{
			return;
		}
		float positionLerp = ((dragFollowLerpSpeed <= 0f) ? 1f : (1f - Mathf.Exp((0f - dragFollowLerpSpeed) * deltaTime)));
		for (int num = flyAwayParticles.Count - 1; num >= 0; num--)
		{
			FlyAwayParticleInstance flyAwayParticleInstance = flyAwayParticles[num];
			if (flyAwayParticleInstance == null || flyAwayParticleInstance.GameObject == null)
			{
				flyAwayParticles.RemoveAt(num);
			}
			else
			{
				flyAwayParticleInstance.SetWorldPositionAndDirection(dragFollowTargetWorldPosition, positionLerp, dragFollowDirection, flyAwayParticleHeadAngle, dragFollowRotationLerpSpeed, deltaTime);
			}
		}
	}

	private void UpdateDragFollowTarget(Vector2 screenPosition)
	{
		if (!dragFollowActive)
		{
			return;
		}
		dragFollowParent = ResolveDragFollowParent();
		for (int num = flyAwayParticles.Count - 1; num >= 0; num--)
		{
			FlyAwayParticleInstance flyAwayParticleInstance = flyAwayParticles[num];
			if (flyAwayParticleInstance == null || flyAwayParticleInstance.GameObject == null)
			{
				flyAwayParticles.RemoveAt(num);
			}
			else
			{
				flyAwayParticleInstance.MoveToParent(dragFollowParent);
			}
		}
		Vector3 dragFollowWorldPosition = GetDragFollowWorldPosition(screenPosition);
		if (hasLastDragFollowPointerPosition)
		{
			Vector3 vector = dragFollowWorldPosition - lastDragFollowPointerWorldPosition;
			if (vector.sqrMagnitude >= dragFollowMinMoveDistance * dragFollowMinMoveDistance)
			{
				dragFollowDirection = vector.normalized;
			}
		}
		lastDragFollowPointerWorldPosition = dragFollowWorldPosition;
		hasLastDragFollowPointerPosition = true;
		Vector3 vector2 = ((dragFollowDirection.sqrMagnitude > 0.001f) ? (dragFollowDirection * Mathf.Max(0f, dragFollowOffset)) : Vector3.zero);
		dragFollowTargetWorldPosition = dragFollowWorldPosition - vector2;
		hasDragFollowTarget = true;
	}

	private Transform ResolveDragFollowParent()
	{
		if (dragFollowRoot != null)
		{
			return dragFollowRoot;
		}
		return ResolveCancelScatterParent();
	}

	private Vector3 GetDragFollowWorldPosition(Vector2 screenPosition)
	{
		Canvas val = ((dragFollowParent != null) ? dragFollowParent.GetComponentInParent<Canvas>(includeInactive: true) : null);
		if ((UnityEngine.Object)(object)val != null && (UnityEngine.Object)(object)UIDisplayModule.gameState?.UIManager?.mainCanvas != null)
		{
			return RectTransformExtensions.WorldPositionFromCanvasToCanvas(screenPosition, UIDisplayModule.gameState.UIManager.mainCanvas, val);
		}
		return base.slotDisplay.transform.position;
	}

	private void ResetDragFollow()
	{
		dragFollowParent = null;
		dragFollowActive = false;
		hasDragFollowTarget = false;
		hasLastDragFollowPointerPosition = false;
		dragFollowTargetWorldPosition = Vector3.zero;
		dragFollowDirection = Vector3.zero;
		lastDragFollowPointerWorldPosition = Vector3.zero;
	}

	private void PlayFlyAwayParticle()
	{
		if (flyAwayParticlePrefab == null)
		{
			return;
		}
		RectTransform rectTransform = ResolveCrawlArea();
		if (!(rectTransform == null))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(flyAwayParticlePrefab, rectTransform);
			RectTransform rectTransform2 = gameObject.transform as RectTransform;
			if (rectTransform2 != null)
			{
				rectTransform2.anchorMin = new Vector2(0.5f, 0.5f);
				rectTransform2.anchorMax = new Vector2(0.5f, 0.5f);
				rectTransform2.pivot = new Vector2(0.5f, 0.5f);
				rectTransform2.anchoredPosition = Vector2.zero;
				rectTransform2.localScale = Vector3.one;
			}
			else
			{
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localScale = Vector3.one;
			}
			ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Play(true);
			}
			flyAwayParticles.Add(new FlyAwayParticleInstance(gameObject));
			UnityEngine.Object.Destroy(gameObject, flyAwayParticleLifetime);
		}
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
		cachedCrawlArea = base.slotDisplay.GetModule<IconModule>()?.ItemImageRectTransform;
		if (cachedCrawlArea == null)
		{
			cachedCrawlArea = base.slotDisplay.transform as RectTransform;
		}
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
		if (!image.preserveAspect || sprite == null)
		{
			return rect;
		}
		Vector2 size = sprite.rect.size;
		if (size.x <= 0f || size.y <= 0f || rect.width <= 0f || rect.height <= 0f)
		{
			return rect;
		}
		float num = size.x / size.y;
		if (rect.width / rect.height > num)
		{
			float num2 = rect.height * num;
			return new Rect(rect.center.x - num2 * 0.5f, rect.yMin, num2, rect.height);
		}
		float num3 = rect.width / num;
		return new Rect(rect.xMin, rect.center.y - num3 * 0.5f, rect.width, num3);
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
		minX = rect.xMin + crawlPadding.x + bodyRadius;
		maxX = rect.xMax - crawlPadding.z - bodyRadius;
		minY = rect.yMin + crawlPadding.y + bodyRadius;
		maxY = rect.yMax - crawlPadding.w - bodyRadius;
		Image image = ResolveCrawlImage();
		if (image != null && image.rectTransform == rectTransform && image.sprite != null)
		{
			Rect imageDrawRect = GetImageDrawRect(image, image.sprite);
			minX = Mathf.Max(minX, imageDrawRect.xMin + bodyRadius);
			maxX = Mathf.Min(maxX, imageDrawRect.xMax - bodyRadius);
			minY = Mathf.Max(minY, imageDrawRect.yMin + bodyRadius);
			maxY = Mathf.Min(maxY, imageDrawRect.yMax - bodyRadius);
		}
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
			float z = Mathf.Atan2(direction.y, direction.x) * 57.29578f - crawlSpriteHeadAngle;
			fly.rectTransform.localRotation = Quaternion.Euler(0f, 0f, z);
		}
	}

	private void ClearFlies()
	{
		for (int num = flies.Count - 1; num >= 0; num--)
		{
			RemoveFlyAt(num);
		}
		flies.Clear();
	}

	private void ClearFlyAwayParticles()
	{
		for (int num = flyAwayParticles.Count - 1; num >= 0; num--)
		{
			if (flyAwayParticles[num]?.GameObject != null)
			{
				UnityEngine.Object.Destroy(flyAwayParticles[num].GameObject);
			}
		}
		flyAwayParticles.Clear();
	}

	private void ScatterFlyAwayParticlesOnCancel()
	{
		dragFollowActive = false;
		Transform scatterParent = ResolveCancelScatterParent();
		for (int num = flyAwayParticles.Count - 1; num >= 0; num--)
		{
			FlyAwayParticleInstance flyAwayParticleInstance = flyAwayParticles[num];
			if (flyAwayParticleInstance == null || flyAwayParticleInstance.GameObject == null)
			{
				flyAwayParticles.RemoveAt(num);
			}
			else
			{
				flyAwayParticleInstance.BeginScatter(scatterParent, RandomRange(cancelScatterDurationRange), cancelScatterSpeedRange, cancelScatterAngleRange, cancelScatterUpwardBias);
			}
		}
	}

	private Transform ResolveCancelScatterParent()
	{
		if (cancelScatterRoot != null)
		{
			return cancelScatterRoot;
		}
		RectTransform rectTransform = ResolveCrawlArea();
		if (rectTransform != null && UIDisplayModule.gameState != null)
		{
			Transform vFXRoot = new ConvertablePivot(rectTransform.transform).GetVFXRoot(UIDisplayModule.gameState);
			if (vFXRoot != null)
			{
				return vFXRoot;
			}
		}
		if (UIDisplayModule.gameState?.UIManager?.uiVFXRoot != null)
		{
			return UIDisplayModule.gameState.UIManager.uiVFXRoot;
		}
		if ((UnityEngine.Object)(object)UIDisplayModule.gameState?.UIManager?.mainCanvas != null)
		{
			return ((Component)(object)UIDisplayModule.gameState.UIManager.mainCanvas).transform;
		}
		return null;
	}

	private void UpdateFlyAwayParticles(float deltaTime)
	{
		for (int num = flyAwayParticles.Count - 1; num >= 0; num--)
		{
			FlyAwayParticleInstance flyAwayParticleInstance = flyAwayParticles[num];
			if (flyAwayParticleInstance == null || flyAwayParticleInstance.GameObject == null)
			{
				flyAwayParticles.RemoveAt(num);
			}
			else if (flyAwayParticleInstance.UpdateScatter(deltaTime))
			{
				UnityEngine.Object.Destroy(flyAwayParticleInstance.GameObject);
				flyAwayParticles.RemoveAt(num);
			}
		}
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
