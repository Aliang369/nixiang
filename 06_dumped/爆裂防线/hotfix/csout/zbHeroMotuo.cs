using System.Collections;
using Proto.Types;
using UnityEngine;

public class zbHeroMotuo : zbHeroMain
{
	private static readonly WaitForSeconds s_wait02;

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team, BattleSkillDetail _skill)
	{
		base.InitData(_combatData, _team, _skill);
		extraBulletInterval = 2f;
	}

	public override IEnumerator ShootMonster()
	{
		if (skillRecord.id == 90003)
		{
			zbAudioMgr.Instance.PlayAudio("audio/combat/motuo_2.mp3");
		}
		else
		{
			zbAudioMgr.Instance.PlayAudio("audio/combat/motuo.ogg");
		}
		foreach (stModIndex2093 item in team.modIndex2093)
		{
			if (item.skillType == GetSkillType())
			{
				item.tickNumber++;
				if (item.tickNumber >= item.releaseNumber)
				{
					item.canAction = true;
				}
			}
		}
		if (team.ExtraProperties[ETeamExtra.MotuoMod2091] <= 0f)
		{
			((MonoBehaviour)this).StartCoroutine(base.ShootMonster());
			((MonoBehaviour)this).StartCoroutine(ActionModIndex2093());
			yield break;
		}
		isCDFlag = false;
		nextFireTime = 0f;
		int i = 0;
		Vector3 targetPosition = default(Vector3);
		while (i < 5)
		{
			((Vector3)(ref targetPosition))._002Ector(((float)i - 2f) * 1.4f + combatData.nodeRootPositonX, -5f, 0f);
			FireToTargetPosition(0, targetPosition);
			yield return s_wait02;
			int num = i + 1;
			i = num;
		}
		((MonoBehaviour)this).StartCoroutine(ActionModIndex2093());
	}

	public IEnumerator ActionModIndex2093()
	{
		Vector3 targetPosition = default(Vector3);
		foreach (stModIndex2093 temp in team.modIndex2093)
		{
			if (temp.skillType == GetSkillType() && temp.canAction)
			{
				temp.canAction = false;
				yield return s_wait02;
				for (int i = 0; i < temp.extraNumber; i++)
				{
					yield return s_wait02;
					int num = temp.extraNumber / 2;
					((Vector3)(ref targetPosition))._002Ector((float)(i - num) * 1.4f, -5f, 0f);
					FireToTargetPosition(0, targetPosition);
				}
				temp.tickNumber = 0;
			}
		}
	}

	public override void FireToTargetPosition(int targetMonsterId, Vector3 targetPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		zbSkillSystem.Instance.CreateSkill(this, targetPosition, Vector2.up, targetMonsterId);
	}

	static zbHeroMotuo()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		s_wait02 = new WaitForSeconds(0.2f);
	}
}
