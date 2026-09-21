using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using DG.Tweening;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBftHealthDisplay : BftHealthDisplay
{
	private enum HealthType
	{
		Normal,
		Crystal
	}

	private const string animStr_Idle = "#idle#";

	private const string animStr_Hit = "#hit#";

	[SerializeField]
	private GameObject normalHealthBarDecoration;

	[SerializeField]
	private RectTransform crystalHealthBarPanel;

	[SerializeField]
	private SkeletonGraphic crystalHealthEffect;

	[SerializeField]
	private TextMeshProUGUI crystalHealthText;

	[SerializeField]
	private Image extraHealthPointEffect;

	[SerializeField]
	private RectTransform extraHealthPointPivot;

	[SerializeField]
	private RectTransform extraHealthPointPrefab;

	[NonSerialized]
	private EnemyCrystalShield enemyCrystalShield;

	[NonSerialized]
	private bool dead;

	[NonSerialized]
	private PrefabPool<RectTransform> extraHealthPointPool;

	[NonSerialized]
	private HealthType healthType;

	private EnemyCrystalShield.VFXConfig[] vfxConfigs => enemyCrystalShield.vfxConfigs;

	protected override bool shouldUpdate
	{
		get
		{
			if (base.shouldUpdate)
			{
				return bft != null;
			}
			return false;
		}
	}

	protected override BattleFieldTransform bft => UIBase.gameState.battleState.enemy?.bft;

	protected override void Init()
	{
		extraHealthPointPrefab.gameObject.SetActive(value: false);
		extraHealthPointPool = new PrefabPool<RectTransform>(extraHealthPointPrefab, extraHealthPointPivot);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterDamage, _OnAfterDamage);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterDeathblow, _OnAfterDeathblow);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, _OnBattleEnd);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.EnterCrystalFreeze, _OnEnterCrystalFreeze);
		UIBase.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.ExitCrystalFreeze, _OnExitCrystalFreeze);
		enemyCrystalShield = ((Component)(object)UIBase.gameState.UIManager.enemyCrystalAnim).GetComponent<EnemyCrystalShield>();
		ResetUI();
		base.Init();
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterDamage, _OnAfterDamage);
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterDeathblow, _OnAfterDeathblow);
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, _OnBattleEnd);
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.EnterCrystalFreeze, _OnEnterCrystalFreeze);
		UIBase.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.ExitCrystalFreeze, _OnExitCrystalFreeze);
	}

	protected override void UpdateDisplay()
	{
		switch (healthType)
		{
		case HealthType.Normal:
			base.UpdateDisplay();
			break;
		case HealthType.Crystal:
		{
			int num = bft.health;
			crystalHealthText.text = TextDisplayHelper.ToNumSpriteString(num.Clamp(0, num)) ?? "";
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		UpdateExtraHealthPoint();
	}

	private TrackEntry PlayAnim(string animName, bool loop)
	{
		return crystalHealthEffect.PlayAnimation(animName, loop);
	}

	private TrackEntry PlayCrystalShieldAnim(string animName, bool loop)
	{
		SkeletonAnimation enemyCrystalAnim = UIBase.gameState.UIManager.enemyCrystalAnim;
		EnemyCrystalShield.VFXConfig vfxConfig = vfxConfigs.Find((EnemyCrystalShield.VFXConfig c) => c.name == animName);
		if (vfxConfig?.nfxConfig != null)
		{
			UIBase.gameState.StartCoroutine(() => Cor(vfxConfig));
		}
		return enemyCrystalAnim.PlayAnimation(animName, loop);
		static IEnumerator Cor(EnemyCrystalShield.VFXConfig c)
		{
			yield return new WaitForSeconds(c.time);
			if (c.pivot == null)
			{
				UIBase.applicationState.nfxManager.SpawnAndPlay(c.nfxConfig, ((Component)(object)UIBase.gameState.UIManager.enemyCrystalAnim).gameObject.transform);
			}
			else
			{
				UIBase.applicationState.nfxManager.SpawnAndPlay(c.nfxConfig, c.pivot);
			}
		}
	}

	private void UpdateExtraHealthPoint()
	{
		int num = bft.extraHealthPoint;
		int count = extraHealthPointPool.GetActiveElements().Count;
		if (num > count)
		{
			for (int i = 0; i < num - count; i++)
			{
				SpawnExtraHealthPoint();
			}
		}
		else if (num < count)
		{
			for (int j = 0; j < count - num; j++)
			{
				UIBase.gameState.StartCoroutine(DespawnExtraHealthPoint);
			}
		}
	}

	private void SpawnExtraHealthPoint()
	{
		extraHealthPointPool.Spawn();
	}

	private void ReturnToNormal()
	{
		if (healthType == HealthType.Normal)
		{
			return;
		}
		TrackEntry trackEntry = PlayAnim("#absorb#", loop: false);
		trackEntry.Event += delegate(TrackEntry _, Event e)
		{
			if (e.Data.Name.Contains("Number Out"))
			{
				crystalHealthText.gameObject.SetActive(value: false);
				SetDirty();
			}
		};
		trackEntry.Complete += delegate
		{
			SetNormalHealthBarState(healthBarActive: true, decorationActive: false);
			crystalHealthBarPanel.gameObject.SetActive(value: false);
			healthType = HealthType.Normal;
		};
		UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
		TrackEntry trackEntry2 = PlayCrystalShieldAnim("#absorb#", loop: false);
		trackEntry2.Event += delegate(TrackEntry _, Event e)
		{
			if (e.Data.Name.Contains("ACT") && UIBase.gameState.battleState?.enemy != null)
			{
				UIBase.gameState.battleState.enemy.BftDisplay.UnfreezeAnim();
			}
		};
		trackEntry2.Complete += delegate
		{
			((Component)(object)UIBase.gameState.UIManager.enemyCrystalAnim).gameObject.SetActive(value: false);
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
		};
	}

	protected override void ResetUI()
	{
		base.ResetUI();
		extraHealthPointEffect.gameObject.SetActive(value: false);
		SetNormalHealthBarState(healthBarActive: true, ShouldShowCrystalDecoration());
		crystalHealthBarPanel.gameObject.SetActive(value: false);
		healthType = HealthType.Normal;
		((Component)(object)UIBase.gameState.UIManager.enemyCrystalAnim).gameObject.SetActive(value: false);
		dead = false;
	}

	private bool ShouldShowCrystalDecoration()
	{
		EnemyEncounterType? enemyEncounterType = bft?.characterAI?.enemyEncounterType;
		if (bft != null && (int)bft.extraHealthPoint > 0)
		{
			if (enemyEncounterType != EnemyEncounterType.Elite)
			{
				return enemyEncounterType == EnemyEncounterType.Boss;
			}
			return true;
		}
		return false;
	}

	private void SetNormalHealthBarState(bool healthBarActive, bool decorationActive)
	{
		maxHealthBar.gameObject.SetActive(healthBarActive);
		normalHealthBarDecoration.SetActive(decorationActive);
	}

	private IEnumerator ShowCrystalShield()
	{
		UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
		SetNormalHealthBarState(healthBarActive: false, decorationActive: false);
		crystalHealthBarPanel.gameObject.SetActive(value: true);
		crystalHealthText.gameObject.SetActive(value: false);
		healthType = HealthType.Crystal;
		bool finish = false;
		TrackEntry trackEntry = PlayAnim("#in#", loop: false);
		trackEntry.Event += delegate(TrackEntry trackEntry2, Event e)
		{
			if (e.Data.Name.Contains("Number In"))
			{
				crystalHealthText.gameObject.SetActive(value: true);
				((Component)(object)UIBase.gameState.UIManager.enemyCrystalAnim).gameObject.SetActive(value: true);
				PlayCrystalShieldAnim("#in#", loop: false);
				UIBase.gameState.battleState.enemy.bft.attachDisplay.FreezeAnim();
			}
		};
		trackEntry.Complete += delegate
		{
			finish = true;
			PlayAnim("#idle#", loop: true);
			PlayCrystalShieldAnim("#idle#", loop: true);
		};
		yield return new WaitUntil(() => finish);
		UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
	}

	private IEnumerator DespawnExtraHealthPoint()
	{
		RectTransform rectTransform = extraHealthPointPool.GetActiveElements()[extraHealthPointPool.GetActiveElements().Count - 1];
		extraHealthPointPool.Despawn(rectTransform);
		List<Image> images = new List<Image>();
		DG.Tweening.Sequence sequence = DOTween.Sequence();
		sequence.timeScale = UIBase.gameState.actionManager.GetBattleScale();
		for (int i = 0; i < 2; i++)
		{
			Image image = UnityEngine.Object.Instantiate(extraHealthPointEffect, extraHealthPointEffect.transform.parent);
			image.gameObject.SetActive(value: true);
			image.rectTransform.position = rectTransform.position;
			image.rectTransform.localScale = Vector3.one;
			images.Add(image);
			sequence.Insert((float)i * 0.25f, DOTweenModuleUI.DOFade(image, 0f, 1f));
			sequence.Insert((float)i * 0.25f, image.rectTransform.DOScale(new Vector3(2f, 2f, 1f), 1f));
		}
		sequence.Play();
		yield return sequence.WaitForKill();
		for (int j = 0; j < 2; j++)
		{
			if (images[j]?.gameObject != null)
			{
				UnityEngine.Object.Destroy(images[j].gameObject);
			}
		}
	}

	private IEnumerator CrystalShieldDeath()
	{
		if (dead)
		{
			yield break;
		}
		UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
		dead = true;
		bool finish = false;
		bool act = false;
		PlayAnim("#death#", loop: false);
		TrackEntry trackEntry = PlayCrystalShieldAnim("#death#", loop: false);
		trackEntry.Event += delegate(TrackEntry trackEntry2, Event e)
		{
			if (e.Data.Name.Contains("ACT"))
			{
				UIBase.gameState.battleState.enemy.BftDisplay.UnfreezeAnim();
				act = true;
			}
		};
		trackEntry.Complete += delegate
		{
			finish = true;
		};
		yield return new WaitUntil(() => finish | act);
		UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
	}

	private IEnumerator PlayHitAnim()
	{
		BattleFieldTransform battleFieldTransform = bft;
		bool dead = (int)battleFieldTransform.health <= 0;
		if (dead)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
		}
		TrackEntry trackEntry = PlayAnim("#hit#", loop: false);
		PlayCrystalShieldAnim("#hit#", loop: false);
		bool finish = false;
		trackEntry.Complete += delegate
		{
			finish = true;
		};
		yield return new WaitUntil(() => finish);
		if (dead)
		{
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
		}
		if ((int)battleFieldTransform.health > 0)
		{
			PlayAnim("#idle#", loop: true);
			PlayCrystalShieldAnim("#idle#", loop: true);
		}
		else
		{
			UIBase.gameState.StartCoroutine(CrystalShieldDeath);
		}
	}

	private void _OnAfterDamage(GameEvent.Type _, GameEvent eve)
	{
		if (UIBase.gameState.battleState.enemy != null && eve.TargetOwner == UIBase.gameState.battleState.enemy.bft && healthType == HealthType.Crystal && !dead)
		{
			UIBase.gameState.StartCoroutine(PlayHitAnim);
		}
	}

	private void _OnAfterDeathblow(GameEvent.Type _, GameEvent eve)
	{
		if (eve.TargetBft == bft)
		{
			normalHealthBarDecoration.SetActive(healthType == HealthType.Normal && ShouldShowCrystalDecoration());
			SetDirty();
		}
	}

	private void _OnEnterCrystalFreeze(GameEvent.Type _, GameEvent eve)
	{
		UIBase.gameState.StartCoroutine(ShowCrystalShield);
	}

	private void _OnExitCrystalFreeze(GameEvent.Type _, GameEvent eve)
	{
		if (healthType != HealthType.Normal)
		{
			ReturnToNormal();
		}
	}

	protected override void _OnBattleStart()
	{
		ResetUI();
	}

	private void _OnBattleEnd(GameEvent.Type _, GameEvent eve)
	{
		if (healthType != HealthType.Normal && !bft.characterAI.dead)
		{
			ReturnToNormal();
		}
	}
}
