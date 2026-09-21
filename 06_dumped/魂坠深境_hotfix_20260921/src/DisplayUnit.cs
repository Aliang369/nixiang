using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;

public class DisplayUnit : MonoBehaviour
{
	public static class EventName
	{
		public const string talk = "#talk#";

		public const string move = "#move#";

		public const string death_fade = "#death_fade#";

		public const string shadow_fade = "#shadow_fade#";

		public const string playSound = "#playsound#";

		public const string shoot = "#shoot#";

		public const string sound = "game_enemy_sound_";

		public const string skin = "#skin_{(?<str>.*)}#";
	}

	public class AnimationMoveInfo
	{
		public float distance;

		public Ease ease;

		public float transitTime;
	}

	public class EnemyPerformanceState
	{
		public EnemyAnimationType animationType;

		public Action effectAction;

		private bool effected;

		public bool isFinished;

		public Item sourceItem;

		public bool talked;

		public void TriggerEffectAction()
		{
			if (effected || effectAction == null)
			{
				effected = true;
				effectAction = null;
				return;
			}
			effected = true;
			Action action = effectAction;
			effectAction = null;
			action?.Invoke();
		}
	}

	[Serializable]
	public class SpineLoadInfo
	{
		public float initScale = 0.003f;

		public SkeletonDataAsset asset;

		public Texture normalTexture;
	}

	public class AnimConfig
	{
		public float time;

		public AnimationCurve animationCurve;

		public Color32 color;
	}

	private static readonly Regex skinEventRegex = new Regex("#skin_{(?<str>.*)}#", RegexOptions.Compiled);

	[SerializeField]
	private Renderer shadow;

	[SerializeField]
	private Renderer outline;

	public SpineLoadInfo spineLoadInfo;

	[NonSerialized]
	public DisplayAnimator animator;

	[NonSerialized]
	private TrackEntry currentTrackEntry;

	[NonSerialized]
	private DG.Tweening.Sequence fadeSequence;

	[NonSerialized]
	private DG.Tweening.Sequence fallbackDeathSequence;

	[NonSerialized]
	public bool hasDefeatDialogTrigger;

	[NonSerialized]
	public bool hasHitDialogTrigger;

	[NonSerialized]
	public bool hasIdleDialogTrigger;

	[NonSerialized]
	private Vector3 initLocalPosition;

	[NonSerialized]
	private Color initShadowColor;

	[NonSerialized]
	private DG.Tweening.Sequence moveSequence;

	[NonSerialized]
	public OutlineController outlineController;

	[NonSerialized]
	private EnemyPerformanceState performance;

	[NonSerialized]
	private DG.Tweening.Sequence shadowFadeSequence;

	[NonSerialized]
	private bool withAnimator;

	[NonSerialized]
	public List<DG.Tweening.Sequence> vfxSequences;

	[NonSerialized]
	public DG.Tweening.Sequence hitSequence;

	[NonSerialized]
	public DG.Tweening.Sequence noSpineAnimSequence;

	public SkeletonAnimation spineAnimator => GetComponent<SkeletonAnimation>();

	public BattleFieldTransformDisplay bft => GetComponentInParent<BattleFieldTransformDisplay>();

	private float alpha
	{
		get
		{
			if ((UnityEngine.Object)(object)spineAnimator != null)
			{
				return ((SkeletonRenderer)spineAnimator).skeleton.A;
			}
			if (GetComponent<Renderer>() != null)
			{
				return GetComponent<Renderer>().material.color.a;
			}
			return -1f;
		}
		set
		{
			if ((UnityEngine.Object)(object)spineAnimator != null)
			{
				((SkeletonRenderer)spineAnimator).skeleton.A = value;
			}
			else if (GetComponent<Renderer>() != null)
			{
				Color color = GetComponent<Renderer>().material.color;
				color.a = value;
				GetComponent<Renderer>().material.color = color;
			}
		}
	}

	private float BattleTimeScale => SceneInfoSingleton.instance.gameState.actionManager.GetBattleScale();

	public void Init()
	{
		currentTrackEntry = null;
		performance = null;
		initShadowColor = shadow.material.color;
		initLocalPosition = base.transform.localPosition;
		vfxSequences = new List<DG.Tweening.Sequence>();
		outlineController = new OutlineController(this, outline);
		if ((UnityEngine.Object)(object)spineAnimator != null)
		{
			withAnimator = true;
			animator = GetComponent<DisplayAnimator>();
			animator?.Init();
			shadow?.gameObject.SetActive(value: true);
			spineAnimator.AnimationState.Event += _OnEvent;
			spineAnimator.AnimationState.Start += _OnStart;
			spineAnimator.AnimationState.Complete += _OnComplete;
			Bootstrap.applicationState?.eventManager?.GlobalConfigDataChanged.AddListener(_OnGlobalConfigDataChanged);
			InitTriggers();
			SetOutlineAlpha(0f, 0f);
			RefreshAnimationTimeScale();
			animator?.PlaySpineAnimation(animator?.GetAnimationPair(spineAnimator.AnimationName));
		}
		else
		{
			withAnimator = false;
		}
	}

	private void OnDestroy()
	{
		fallbackDeathSequence?.Kill();
		if ((UnityEngine.Object)(object)spineAnimator != null)
		{
			spineAnimator.AnimationState.Event -= _OnEvent;
			spineAnimator.AnimationState.Start -= _OnStart;
			spineAnimator.AnimationState.Complete -= _OnComplete;
		}
		Bootstrap.applicationState?.eventManager?.GlobalConfigDataChanged.RemoveListener(_OnGlobalConfigDataChanged);
	}

	public void PlayPerformance(EnemyPerformanceState performance)
	{
		if (!withAnimator)
		{
			PlayNoSpinePerformanceEffect(performance);
			return;
		}
		this.performance = performance;
		if (performance.animationType == EnemyAnimationType.hit)
		{
			PlayHitEffect();
		}
		if (performance.animationType != EnemyAnimationType.none)
		{
			currentTrackEntry = animator?.PlaySpineAnimation(performance.animationType);
		}
		else
		{
			currentTrackEntry = animator?.PlaySpineAnimation(performance.sourceItem.GetDef());
		}
		if (currentTrackEntry != null)
		{
			if (GetEvent(currentTrackEntry, "#talk#") == null)
			{
				performance.talked = true;
				ShowDialog(currentTrackEntry.Animation.Name, performance.sourceItem?.GetDef());
			}
			if (performance.animationType == EnemyAnimationType.death)
			{
				SetOutlineAlpha(0f, 0.2f);
			}
		}
		else if (performance.animationType == EnemyAnimationType.death)
		{
			PlayFallbackDeathPerformance(performance);
		}
		else
		{
			performance.TriggerEffectAction();
			performance.isFinished = true;
		}
	}

	public void FreezeAnim()
	{
		if (withAnimator && !((UnityEngine.Object)(object)spineAnimator == null))
		{
			spineAnimator.state.TimeScale = 0f;
		}
	}

	public void UnfreezeAnim()
	{
		if (withAnimator && !((UnityEngine.Object)(object)spineAnimator == null))
		{
			spineAnimator.state.TimeScale = 1f;
		}
	}

	public bool IsPushingForward()
	{
		if (!withAnimator)
		{
			return false;
		}
		return moveSequence != null;
	}

	public bool HasAnimationPlaying()
	{
		DG.Tweening.Sequence sequence = fallbackDeathSequence;
		if (sequence != null && sequence.IsActive())
		{
			return true;
		}
		if (!withAnimator && noSpineAnimSequence == null)
		{
			return false;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			return false;
		}
		return currentTrackEntry != null;
	}

	public void SetOutlineAlpha(float value, float time)
	{
		outlineController.DoFade(value, time);
	}

	private void InitTriggers()
	{
		if (!((UnityEngine.Object)(object)spineAnimator != null))
		{
			Animation spineAnimation = animator.GetSpineAnimation(animator.GetAnimationPair(EnemyAnimationType.idle));
			Event obj = GetEvent(spineAnimation, "#talk#");
			hasIdleDialogTrigger = obj != null;
		}
	}

	private void ShowDialog(string eventName, ItemDefinition itemDefinition)
	{
		CharacterAI characterAI = GetComponentInParent<CharacterAIComponent>().BftDisplay.attachBft.characterAI;
		if (characterAI != null)
		{
			if (eventName == animator.GetAnimationPair(EnemyAnimationType.idle).animationName)
			{
				characterAI.TriggerIdleDialog();
			}
			else
			{
				characterAI.TriggerDialogue(eventName, itemDefinition);
			}
		}
	}

	private void PlaySound()
	{
	}

	private bool IsIdleTrack(TrackEntry entry)
	{
		DisplayAnimator.AnimationPair animationPair = animator?.GetAnimationPair(EnemyAnimationType.idle);
		if (entry != null && animationPair != null)
		{
			return entry.Animation.Name == animationPair.animationName;
		}
		return false;
	}

	private void RefreshAnimationTimeScale()
	{
		SkeletonAnimation obj = spineAnimator;
		TrackEntry entry = ((obj != null) ? obj.AnimationState.GetCurrent(0) : null);
		float timeScale = (IsIdleTrack(entry) ? 1f : BattleTimeScale);
		if ((UnityEngine.Object)(object)spineAnimator != null)
		{
			spineAnimator.timeScale = timeScale;
		}
		if (moveSequence != null)
		{
			moveSequence.timeScale = timeScale;
		}
		if (fadeSequence != null)
		{
			fadeSequence.timeScale = timeScale;
		}
		if (fallbackDeathSequence != null)
		{
			fallbackDeathSequence.timeScale = timeScale;
		}
		if (shadowFadeSequence != null)
		{
			shadowFadeSequence.timeScale = timeScale;
		}
		if (hitSequence != null)
		{
			hitSequence.timeScale = timeScale;
		}
		if (noSpineAnimSequence != null)
		{
			noSpineAnimSequence.timeScale = timeScale;
		}
		if (vfxSequences == null)
		{
			return;
		}
		foreach (DG.Tweening.Sequence vfxSequence in vfxSequences)
		{
			vfxSequence.timeScale = timeScale;
		}
	}

	private void _OnGlobalConfigDataChanged(SavingUtil.GlobalConfigSaveData _)
	{
		RefreshAnimationTimeScale();
	}

	private Event GetEvent(TrackEntry entry, string eventName)
	{
		return GetEvent(entry.Animation, eventName);
	}

	private Event GetEvent(Animation animation, string eventName)
	{
		if (!(animation.Timelines.Find((Timeline t) => t is EventTimeline) is EventTimeline eventTimeline))
		{
			return null;
		}
		return eventTimeline.Events.Find((Event eve) => eve.Data.Name.Contains(eventName));
	}

	private void ConfigInfo(TrackEntry entry)
	{
		moveSequence?.Kill();
		fadeSequence?.Kill();
		shadowFadeSequence?.Kill();
		List<(float, float)> list = new List<(float, float)>();
		List<(float, float)> list2 = new List<(float, float)>();
		List<(float, float)> list3 = new List<(float, float)>();
		foreach (Timeline timeline in entry.Animation.Timelines)
		{
			if (!(timeline is EventTimeline eventTimeline))
			{
				continue;
			}
			for (int i = 0; i < eventTimeline.Events.Length; i++)
			{
				if (eventTimeline.Events[i].Data.Name == "#move#")
				{
					list.Add((eventTimeline.Events[i].Time, eventTimeline.Events[i].Float));
				}
				else if (eventTimeline.Events[i].Data.Name == "#death_fade#")
				{
					list2.Add((eventTimeline.Events[i].Time, eventTimeline.Events[i].Float));
				}
				else if (eventTimeline.Events[i].Data.Name == "#shadow_fade#")
				{
					list3.Add((eventTimeline.Events[i].Time, eventTimeline.Events[i].Float));
				}
			}
		}
		if (list.Count != 0)
		{
			moveSequence = DOTween.Sequence();
			moveSequence.timeScale = BattleTimeScale;
			moveSequence.SetEase(ScriptableObjectSingleton<VFXDataPreset>.instance.enemyPullMoveInfo.ease);
			float num = 0f;
			for (int j = 0; j < list.Count; j++)
			{
				float num2 = 0f - list[j].Item2;
				if (num2 >= 0f)
				{
					moveSequence.Append(base.transform.DOLocalMove(initLocalPosition + ScriptableObjectSingleton<VFXDataPreset>.instance.enemyPullMoveInfo.distance * Vector3.forward * num2, list[j].Item1 - num));
				}
				else
				{
					moveSequence.Append(base.transform.DOLocalMove(initLocalPosition + ScriptableObjectSingleton<VFXDataPreset>.instance.enemyPushMoveInfo.distance * Vector3.forward * num2 * list[j].Item2, list[j].Item1 - num));
				}
				num = list[j].Item1;
			}
			moveSequence.AppendInterval(entry.Animation.Duration - num);
			DG.Tweening.Sequence sequence = moveSequence;
			sequence.onKill = (TweenCallback)Delegate.Combine(sequence.onKill, (TweenCallback)delegate
			{
				base.transform.localPosition = initLocalPosition;
				moveSequence = null;
			});
			moveSequence.Play();
		}
		if (list2.Count != 0)
		{
			fadeSequence = DOTween.Sequence();
			fadeSequence.timeScale = BattleTimeScale;
			fadeSequence.SetEase(ScriptableObjectSingleton<VFXDataPreset>.instance.enemyPullMoveInfo.ease);
			float num3 = 0f;
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				fadeSequence.Append(DOTween.To(() => alpha, delegate(float x)
				{
					alpha = x;
				}, list2[num4].Item2, list2[num4].Item1 - num3));
				num3 = list2[num4].Item1;
			}
			fadeSequence.AppendInterval(entry.Animation.Duration - num3);
			DG.Tweening.Sequence sequence2 = fadeSequence;
			sequence2.onKill = (TweenCallback)Delegate.Combine(sequence2.onKill, (TweenCallback)delegate
			{
				fadeSequence = null;
			});
			fadeSequence.Play();
		}
		if (list3.Count != 0)
		{
			shadowFadeSequence = DOTween.Sequence();
			shadowFadeSequence.timeScale = BattleTimeScale;
			shadowFadeSequence.SetEase(ScriptableObjectSingleton<VFXDataPreset>.instance.enemyPullMoveInfo.ease);
			float num5 = 0f;
			Material material = shadow.material;
			for (int num6 = 0; num6 < list3.Count; num6++)
			{
				shadowFadeSequence.Append(material.DOFade(list3[num6].Item2, list3[num6].Item1 - num5));
				num5 = list3[num6].Item1;
			}
			shadowFadeSequence.AppendInterval(entry.Animation.Duration - num5);
			DG.Tweening.Sequence sequence3 = shadowFadeSequence;
			sequence3.onKill = (TweenCallback)Delegate.Combine(sequence3.onKill, (TweenCallback)delegate
			{
				shadowFadeSequence = null;
			});
			shadowFadeSequence.Play();
		}
	}

	private void ResetConfig()
	{
		alpha = 1f;
		shadow.material.color = initShadowColor;
	}

	private void PlayAnimVFX(TrackEntry entry)
	{
		PlayAnimVFX(entry.Animation.Name);
	}

	private void PlayAnimVFX(string animationName)
	{
		foreach (DG.Tweening.Sequence vfxSequence in vfxSequences)
		{
			vfxSequence.Kill();
		}
		vfxSequences.Clear();
		List<DisplayAnimator.AnimationVFX> list = animator.animationVfxes.FindAll((DisplayAnimator.AnimationVFX animationVFX) => animationVFX.animationName == animationName);
		if (list.Count == 0)
		{
			return;
		}
		foreach (DisplayAnimator.AnimationVFX vfx in list)
		{
			DG.Tweening.Sequence sequence = DOTween.Sequence();
			sequence.timeScale = BattleTimeScale;
			sequence.AppendInterval(vfx.time);
			sequence.AppendCallback(delegate
			{
				SceneInfoSingleton.instance.gameState.applicationState.nfxManager.SpawnAndPlay(vfx.vfxSetup, vfx.vfxSetup.transform.parent, vfx.vfxSetup.transform.position);
			});
			sequence.Play();
			vfxSequences.Add(sequence);
		}
	}

	private void PlayFallbackDeathPerformance(EnemyPerformanceState deathPerformance)
	{
		fallbackDeathSequence?.Kill();
		spineAnimator.timeScale = 0f;
		PlayAnimVFX("#death#");
		SetOutlineAlpha(0f, 0.2f);
		Vector3 localPosition = base.transform.localPosition;
		Vector3 localScale = base.transform.localScale;
		Material material = shadow.material;
		fallbackDeathSequence = DOTween.Sequence();
		fallbackDeathSequence.timeScale = BattleTimeScale;
		fallbackDeathSequence.Append(base.transform.DOLocalMoveX(localPosition.x + 0.04f, 0.04f).SetLoops(4, LoopType.Yoyo).SetEase(Ease.Linear));
		fallbackDeathSequence.Append(DOTween.To(() => alpha, delegate(float value)
		{
			alpha = value;
		}, 0f, 0.5f).SetEase(Ease.InQuad));
		fallbackDeathSequence.Join(material.DOFade(0f, 0.4f));
		fallbackDeathSequence.Join(base.transform.DOLocalMoveY(localPosition.y + 0.08f, 0.5f).SetEase(Ease.InQuad));
		fallbackDeathSequence.Join(base.transform.DOScale(localScale * 0.92f, 0.5f).SetEase(Ease.InQuad));
		fallbackDeathSequence.OnComplete(delegate
		{
			deathPerformance.TriggerEffectAction();
			deathPerformance.isFinished = true;
			performance = null;
			fallbackDeathSequence = null;
			base.gameObject.SetActive(value: false);
		});
		fallbackDeathSequence.Play();
	}

	private void PlayHitEffect()
	{
		hitSequence?.Kill();
		hitSequence = DOTween.Sequence();
		hitSequence.timeScale = BattleTimeScale;
		AnimConfig displayUnitAnimConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.displayUnitAnimConfig;
		Material[] updatedSharedMaterialsArray = ((SkeletonRenderer)spineAnimator).rendererBuffers.GetUpdatedSharedMaterialsArray();
		foreach (Material mat in updatedSharedMaterialsArray)
		{
			hitSequence.Join(mat.DOShaderColor(ShaderFields._AddColor, displayUnitAnimConfig.animationCurve, displayUnitAnimConfig.color, displayUnitAnimConfig.time));
		}
		hitSequence.Play();
	}

	private void PlayNoSpinePerformanceEffect(EnemyPerformanceState performance)
	{
		noSpineAnimSequence?.Kill();
		noSpineAnimSequence = DOTween.Sequence();
		noSpineAnimSequence.timeScale = BattleTimeScale;
		Vector3 position = base.transform.position;
		if (performance.animationType == EnemyAnimationType.hit)
		{
			noSpineAnimSequence.Append(base.transform.DOMove(position + Vector3.forward, 0.1f));
			noSpineAnimSequence.Append(base.transform.DOMove(position, 0.2f));
		}
		else
		{
			noSpineAnimSequence.Append(base.transform.DOMove(position - Vector3.forward, 0.1f));
			noSpineAnimSequence.Append(base.transform.DOMove(position, 0.2f));
		}
		noSpineAnimSequence.AppendInterval(0.1f);
		noSpineAnimSequence.AppendCallback(delegate
		{
			performance.TriggerEffectAction();
			performance.isFinished = true;
			noSpineAnimSequence?.Kill();
			noSpineAnimSequence = null;
		});
		noSpineAnimSequence.Play();
	}

	private void _OnEvent(TrackEntry entry, Event e)
	{
		string text = e.Data.Name;
		if (text.Contains("#playsound#"))
		{
			PlaySound();
			return;
		}
		if (text.Contains("game_enemy_sound_"))
		{
			Bootstrap.instance.audioManager.PlayOneShot("event:/Enemy/" + text);
			return;
		}
		if (text.Contains("#shoot#"))
		{
			if (performance != null)
			{
				performance.TriggerEffectAction();
				performance.isFinished = true;
			}
			return;
		}
		if (text.Contains("#talk#"))
		{
			if (performance != null && !performance.talked)
			{
				performance.talked = true;
			}
			ShowDialog(entry.Animation.Name, performance?.sourceItem?.GetDef());
			return;
		}
		MatchCollection matchCollection = skinEventRegex.Matches(text);
		if (matchCollection.Count == 1)
		{
			Skeleton skeleton = ((SkeletonRenderer)spineAnimator).skeleton;
			string value = matchCollection[0].Groups["str"].Value;
			Skin skin = skeleton.Data.FindSkin(value);
			animator.SetSkin(skin);
		}
	}

	private void _OnStart(TrackEntry entry)
	{
		ResetConfig();
		RefreshAnimationTimeScale();
		ConfigInfo(entry);
		PlayAnimVFX(entry);
	}

	private void _OnComplete(TrackEntry entry)
	{
		if (performance != null)
		{
			performance.TriggerEffectAction();
			if (!performance.talked)
			{
				ShowDialog(entry.Animation.Name, performance.sourceItem?.GetDef());
			}
			performance.isFinished = true;
			performance = null;
		}
		currentTrackEntry = null;
		if (entry.Loop)
		{
			_OnStart(entry);
		}
	}
}
