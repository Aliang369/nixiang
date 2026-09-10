using System;
using System.Collections.Generic;
using FMODUnity;
using Spine;
using Spine.Unity;
using UnityEngine;

public class DisplayAnimator : MonoBehaviour
{
	[Serializable]
	public class AnimationPair
	{
		[SerializeField]
		public string animationName;

		[SerializeField]
		public EventReference sound;

		[SerializeField]
		public EnemyAnimationType animationType;

		[SerializeField]
		public List<ItemDefinition> itemDefinitions;
	}

	[Serializable]
	public class AnimationVFX
	{
		public string animationName;

		[SerializeField]
		public NFXSetup vfxSetup;

		[SerializeField]
		public float time;
	}

	[SerializeField]
	public List<AnimationPair> animationpairs;

	[SerializeField]
	public List<AnimationVFX> animationVfxes;

	[SerializeField]
	public string initSkinName;

	[SerializeField]
	public bool hideOnInit;

	[NonSerialized]
	private ExposedList<Animation> animations;

	[NonSerialized]
	public SkeletonDataAsset spineAsset;

	private DisplayUnit displayUnit => GetComponent<DisplayUnit>();

	private SkeletonAnimation spineAnimator => displayUnit.spineAnimator;

	private Skin initSkin => ((SkeletonRenderer)spineAnimator).skeleton.Data.FindSkin(initSkinName);

	private GameStateInfo gameState => SceneInfoSingleton.instance.gameState;

	private void OnDestroy()
	{
		spineAnimator.AnimationState.Complete -= CheckSpineAnimationTrackToIdle;
	}

	public void Init()
	{
		SetSkin(initSkin);
		spineAsset = ((SkeletonRenderer)spineAnimator).skeletonDataAsset;
		spineAnimator.AnimationState.Complete -= CheckSpineAnimationTrackToIdle;
		spineAnimator.AnimationState.Complete += CheckSpineAnimationTrackToIdle;
		animations = ((SkeletonRenderer)spineAnimator).skeletonDataAsset.GetSkeletonData(false).Animations;
		if (hideOnInit)
		{
			base.gameObject.SetActive(value: true);
			GetAnimationPair(EnemyAnimationType.idle);
		}
		base.gameObject.SetActive(!hideOnInit);
	}

	public void CheckSpineAnimationTrackToIdle(TrackEntry trackEntry)
	{
		GameStateInfo gameStateInfo = gameState;
		if (((gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true) || (!trackEntry.Animation.Name.Contains("death") && !trackEntry.Animation.Name.Contains("special"))) && !trackEntry.Loop && spineAnimator.AnimationState.GetCurrent(0) == trackEntry && !trackEntry.Animation.Name.Contains("death"))
		{
			PlaySpineAnimation(EnemyAnimationType.idle);
		}
	}

	public TrackEntry PlaySpineAnimation(ItemDefinition itemDefinition)
	{
		AnimationPair animationPair = GetAnimationPair(itemDefinition);
		if (animationPair == null && !itemDefinition.HasTag(ScriptableObjectSingleton<GameDataManager>.instance.attackTag))
		{
			animationPair = GetAnimationPair(EnemyAnimationType.action);
		}
		if (animationPair == null)
		{
			animationPair = GetBaseAttackIntentAnimationPair();
		}
		return PlaySpineAnimation(animationPair);
	}

	public TrackEntry PlaySpineAnimation(EnemyAnimationType type)
	{
		AnimationPair animationPair = GetAnimationPair(type);
		return PlaySpineAnimation(animationPair, type == EnemyAnimationType.idle);
	}

	public TrackEntry PlaySpineAnimation(AnimationPair animationPair, bool loop = false)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (animationPair == null || !base.gameObject.activeSelf)
		{
			return null;
		}
		spineAnimator.timeScale = ((animationPair == GetAnimationPair(EnemyAnimationType.idle)) ? 1f : gameState.actionManager.GetBattleScale());
		Bootstrap.instance.audioManager.PlayOneShot(animationPair.sound);
		return spineAnimator.PlayAnimation(animationPair.animationName, loop);
	}

	public void SetSkin(Skin skin)
	{
		((SkeletonRenderer)spineAnimator).skeleton.SetSkin(skin);
		((SkeletonRenderer)spineAnimator).skeleton.SetBonesToSetupPose();
		((SkeletonRenderer)spineAnimator).skeleton.SetSlotsToSetupPose();
		spineAnimator.Update(0f);
	}

	public AnimationPair GetAnimationPair(EnemyAnimationType type)
	{
		return animationpairs.Find((AnimationPair p) => p.animationType == type);
	}

	public AnimationPair GetAnimationPair(ItemDefinition itemDefinition)
	{
		return animationpairs.Find((AnimationPair p) => p.itemDefinitions.Contains(itemDefinition));
	}

	public AnimationPair GetAnimationPair(string name)
	{
		return animationpairs.Find((AnimationPair p) => p.animationName == name);
	}

	public AnimationPair GetBaseAttackIntentAnimationPair()
	{
		return animationpairs.Find((AnimationPair p) => p.itemDefinitions.Find((ItemDefinition it) => it.HasTag(ScriptableObjectSingleton<GameDataManager>.instance.enemyAttackIntentTag)) != null);
	}

	public Animation GetSpineAnimation(string name)
	{
		return animations.Find((Animation anim) => anim.Name == name);
	}

	public Animation GetSpineAnimation(AnimationPair pair)
	{
		return GetSpineAnimation(pair.animationName);
	}
}
