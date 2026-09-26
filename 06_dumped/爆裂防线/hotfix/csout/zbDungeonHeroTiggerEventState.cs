using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbDungeonHeroTiggerEventState(zbDungeonHeroStateMachine fsm, EDungeonHeroState eState) : zbDungeonHeroState(fsm, eState)
{
	private DungeonBaseInfo dungeonBaseInfo;

	private SkeletonAnimation skeletonAnimation;

	private Coroutine m_curCoroutine;

	private float moveSpeed = 3f;

	public override void OnEnter()
	{
		base.OnEnter();
		m_fsm.hero.SetAnimation("idle", loop: true);
		zbDungeonMgr.Instance.dungeonWnd.joyStick.ResetJoystick();
		StopAction();
		dungeonBaseInfo = zbDungeonMgr.Instance.dungeonHandler.dungeonBaseInfo;
		if (dungeonBaseInfo == null)
		{
			return;
		}
		zbDungeonRoom roomByCellId = zbDungeonMgr.Instance.GetRoomByCellId(dungeonBaseInfo.TriggerEvent.CellId);
		skeletonAnimation = roomByCellId.GetSkeletonAnimationInGrid(dungeonBaseInfo.TriggerEvent.GridId);
		if (zbDungeonMgr.Instance.dungeonHandler.IsFightEvent(dungeonBaseInfo.TriggerEvent.EventType))
		{
			zbAudioMgr.Instance.PlayAudio("audio/combat/monster_bellow.ogg");
			if (dungeonBaseInfo.TriggerEvent.EventType == EDungeonEventType.RiotFight)
			{
				m_fsm.SetState(EDungeonHeroState.RiotEvent);
				return;
			}
			if (dungeonBaseInfo.TriggerEvent.EventType == EDungeonEventType.BossFight)
			{
				zbDungeonMgr.Instance.ActionBossComing();
			}
			if (dungeonBaseInfo.TriggerEvent.Trigger == EDungeonEventTrigger.Force)
			{
				m_curCoroutine = ((MonoBehaviour)m_fsm.hero).StartCoroutine(StartFightAction());
			}
			else
			{
				StartFight();
			}
		}
		else
		{
			m_curCoroutine = ((MonoBehaviour)m_fsm.hero).StartCoroutine(StartOtherAction());
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		StopAction();
	}

	private void StopAction()
	{
		if (m_curCoroutine != null)
		{
			((MonoBehaviour)m_fsm.hero).StopCoroutine(m_curCoroutine);
		}
	}

	private IEnumerator StartFightAction()
	{
		if ((Object)(object)skeletonAnimation != (Object)null && ((SkeletonRenderer)skeletonAnimation).skeleton != null && ((SkeletonRenderer)skeletonAnimation).skeleton.Data != null)
		{
			if (((Component)skeletonAnimation).transform.position.x < ((Component)m_fsm.hero).transform.position.x)
			{
				((SkeletonRenderer)skeletonAnimation).skeleton.ScaleX = -1f;
			}
			else
			{
				((SkeletonRenderer)skeletonAnimation).skeleton.ScaleX = 1f;
			}
			if (zbUtil.CheckAnimationNameExist(skeletonAnimation, "run"))
			{
				skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
			}
			zbDungeonMgr.Instance.GetMapCamera().SmoothTransitionToTarget(((Component)skeletonAnimation).transform, 1f);
			yield return (object)new WaitForSeconds(1f);
			zbDungeonMgr.Instance.ActionBossEnd();
			while (Vector2.Distance(Vector2.op_Implicit(((Component)skeletonAnimation).transform.position), Vector2.op_Implicit(((Component)m_fsm.hero).transform.position)) > 0.1f)
			{
				((Component)skeletonAnimation).transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(((Component)skeletonAnimation).transform.position), Vector2.op_Implicit(((Component)m_fsm.hero).transform.position), moveSpeed * Time.unscaledDeltaTime));
				yield return null;
			}
			zbDungeonMgr.Instance.GetMapCamera().SmoothTransitionToTarget(((Component)m_fsm.hero).transform, 0.1f);
			if (zbUtil.CheckAnimationNameExist(skeletonAnimation, "attack"))
			{
				skeletonAnimation.AnimationState.SetAnimation(0, "attack", true);
			}
			yield return (object)new WaitForSeconds(0.1f);
		}
		StartFight();
	}

	private void StartFight()
	{
		zbUIMgr.Instance.ShowDungeonFightPop(dungeonBaseInfo.TriggerEvent);
		m_fsm.SetState(EDungeonHeroState.Idle);
	}

	private IEnumerator StartOtherAction()
	{
		if ((Object)(object)skeletonAnimation != (Object)null && ((SkeletonRenderer)skeletonAnimation).skeleton != null && ((SkeletonRenderer)skeletonAnimation).skeleton.Data != null && zbUtil.CheckAnimationNameExist(skeletonAnimation, "2"))
		{
			skeletonAnimation.AnimationState.SetAnimation(0, "2", false);
		}
		yield return (object)new WaitForSeconds(0.6f);
		m_fsm.SetState(EDungeonHeroState.Idle);
		zbAudioMgr.Instance.PlayAudio("audio/ui/open_box.ogg");
		if (dungeonBaseInfo.TriggerEvent.EventType == EDungeonEventType.Popup)
		{
			zbUIMgr.Instance.ShowDungeonEventPopUp(dungeonBaseInfo.TriggerEvent);
		}
		else if (dungeonBaseInfo.TriggerEvent.EventType == EDungeonEventType.ModRand)
		{
			string title = "";
			string message = "";
			zbDungeonEventCsvRecord recordByKey = zbDataConfig.Instance.dungeonEventCsvTable.GetRecordByKey(dungeonBaseInfo.TriggerEvent.EventId);
			if (recordByKey != null)
			{
				title = recordByKey.name;
				message = recordByKey.desc;
			}
			zbUIMgr.Instance.ShowMessageBox(title, message, delegate
			{
				zbUIMgr.Instance.ShowDungeonModPopUp(dungeonBaseInfo.TriggerEvent, dungeonBaseInfo);
			}, zbUtil.Language("lang.10206"), "");
		}
	}
}
