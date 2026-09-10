using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using DG.Tweening;
using UnityEngine;

public class SkillPanel : UIBase
{
	public class Config
	{
		public Vector2 center;

		public float radius;

		public float start;

		public float end;

		public float gap;
	}

	[SerializeField]
	private SkillDisplay skillDisplayPrefab;

	[SerializeField]
	private RectTransform skillDisplayParent;

	[NonSerialized]
	protected CellUIPrefabPool<SkillDisplay, GameSkill> pool;

	[NonSerialized]
	private Sequence sequence;

	[NonSerialized]
	private WrapperEnumerator triggerInfoEnumerator;

	protected override void Init()
	{
		pool = GetPrefabPool<SkillDisplay, GameSkill>(skillDisplayPrefab, skillDisplayParent);
		skillDisplayPrefab.gameObject.SetActive(value: false);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AddSkill, _OnSkillChange);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.RemoveSkill, _OnSkillChange);
		UIBase.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterSkillCast, _OnSkillCast);
		SetDirty();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AddSkill, _OnSkillChange);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.RemoveSkill, _OnSkillChange);
		UIBase.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterSkillCast, _OnSkillCast);
	}

	protected override void UpdateDisplay()
	{
		pool.Sync(UIBase.gameState.playerState.ActivatedSkills.ToList());
	}

	private void _OnSkillChange(GameEvent.Type type, GameEvent eve)
	{
		SetDirty();
	}

	private void _OnSkillCast(GameEvent.Type type, GameEvent eve)
	{
		SetDirty();
	}

	public SkillDisplay GetSkillDisplay(GameSkill skill)
	{
		return pool.GetActiveElements().Find((SkillDisplay d) => d.setupData == skill);
	}

	public void TriggerAbilityInfo(GameSkill skill, Ability.AbilityAnimPriority priority)
	{
		if (priority < Ability.AbilityAnimPriority.Medium || skill == null)
		{
			return;
		}
		SkillPanel_Battle battleSkillPanel = this as SkillPanel_Battle;
		SkillDisplay display = GetSkillDisplay(skill);
		if (!(battleSkillPanel?.infoPanel == null) && !(display == null))
		{
			TriggerTimedInfo(delegate
			{
				battleSkillPanel.infoPanel.ShowCommonInfo(new CommonInfoData(skill, CommonInfoData.ControlType.FixDisplay, display.rectTransform, false));
			}, battleSkillPanel.infoPanel.HideAllCommonInfo);
		}
	}

	public void TriggerAbilityInfo(SkillWord skillWord, Ability.AbilityAnimPriority priority)
	{
		if (priority < Ability.AbilityAnimPriority.Medium || skillWord?.gameSkill == null)
		{
			return;
		}
		SkillDisplay display = GetSkillDisplay(skillWord.gameSkill);
		if (!(display == null) && !(UIBase.gameState.UIManager?.universalCommonInfoPanel == null))
		{
			TriggerTimedInfo(delegate
			{
				UIBase.gameState.UIManager.universalCommonInfoPanel.ShowCommonInfo(new CommonInfoData(skillWord, CommonInfoData.ControlType.FixDisplay, display.rectTransform, false));
			}, UIBase.gameState.UIManager.universalCommonInfoPanel.HideAll);
		}
	}

	private void TriggerTimedInfo(Action show, Action hide)
	{
		triggerInfoEnumerator?.Stop();
		triggerInfoEnumerator = UIBase.gameState.StartCoroutine(Coroutine);
		IEnumerator Coroutine()
		{
			hide?.Invoke();
			show?.Invoke();
			yield return new WaitForSeconds(0.4f);
			hide?.Invoke();
			triggerInfoEnumerator = null;
		}
	}

	public static void SetSideSkillPosition<T>(IReadOnlyList<T> displays)
	{
		Config skillPaneConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.skillPaneConfig;
		for (int i = 0; i < displays.Count; i++)
		{
			if (displays[i] is MonoBehaviour monoBehaviour)
			{
				if (displays.Count >= 4)
				{
					float alpha = Mathf.Lerp(skillPaneConfig.start, skillPaneConfig.end, (float)(displays.Count - i - 1) / (float)(displays.Count - 1));
					RectTransform rectTransform = monoBehaviour.RectTransform();
					Vector2 circlePoint = GeometryMathHelper.GetCirclePoint(skillPaneConfig.center, skillPaneConfig.radius, alpha);
					circlePoint.x += rectTransform.GetWidth() * 0.5f;
					rectTransform.anchoredPosition = circlePoint;
				}
				else
				{
					float alpha2 = ((float)(displays.Count - 1) / 2f - (float)i) * skillPaneConfig.gap;
					RectTransform rectTransform2 = monoBehaviour.RectTransform();
					Vector2 circlePoint2 = GeometryMathHelper.GetCirclePoint(skillPaneConfig.center, skillPaneConfig.radius, alpha2);
					circlePoint2.x += rectTransform2.GetWidth() * 0.5f;
					rectTransform2.anchoredPosition = circlePoint2;
				}
			}
		}
	}
}
