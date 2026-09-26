using System.Collections.Generic;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbLegionSceneMgr : MonoBehaviour
{
	public static zbLegionSceneMgr Instance;

	public Transform heroTF;

	public List<SkeletonAnimation> doorAnim { get; private set; }

	public List<zbLegionHero> heroList { get; private set; }

	private void Awake()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
		}
		heroList = new List<zbLegionHero>();
		doorAnim = new List<SkeletonAnimation>();
		Transform val = ((Component)this).transform.Find("Door");
		for (int i = 0; i < 4; i++)
		{
			doorAnim.Add(((Component)val.Find($"door{i}")).GetComponent<SkeletonAnimation>());
		}
		InitDoorAnimation();
	}

	public void InitHeroList()
	{
		for (int num = heroList.Count - 1; num >= 0; num--)
		{
			Object.Destroy((Object)(object)((Component)heroList[num]).gameObject);
		}
		heroList.Clear();
		long roleId = zbSelf.Instance.GetRoleId();
		RepeatedField<LegionMemberDetail> memberList = zbSelf.Instance.legionHandler.memberList;
		bool flag = false;
		if (memberList == null || memberList.Count == 0)
		{
			return;
		}
		int num2 = 0;
		for (int i = 0; i < memberList.Count; i++)
		{
			if (num2 >= 3)
			{
				break;
			}
			LegionMemberDetail legionMemberDetail = memberList[i];
			bool num3 = zbSelf.Instance.legionHandler.GetOfficialById((int)legionMemberDetail.RoleBaseInfo.RoleId) == ELegionOfficial.Leader;
			bool flag2 = zbSelf.Instance.legionHandler.GetOfficialById((int)legionMemberDetail.RoleBaseInfo.RoleId) == ELegionOfficial.Adviser;
			if (num3 | flag2)
			{
				if (roleId == legionMemberDetail.RoleBaseInfo.RoleId)
				{
					flag = true;
				}
				CreateHero(legionMemberDetail);
				num2++;
			}
		}
		int num4 = 0;
		for (int j = 0; j < memberList.Count; j++)
		{
			if (num4 >= 6)
			{
				break;
			}
			LegionMemberDetail legionMemberDetail2 = memberList[j];
			bool num5 = zbSelf.Instance.legionHandler.GetOfficialById((int)legionMemberDetail2.RoleBaseInfo.RoleId) == ELegionOfficial.Leader;
			bool flag3 = zbSelf.Instance.legionHandler.GetOfficialById((int)legionMemberDetail2.RoleBaseInfo.RoleId) == ELegionOfficial.Adviser;
			if (!num5 && !flag3)
			{
				if (roleId == legionMemberDetail2.RoleBaseInfo.RoleId)
				{
					flag = true;
				}
				CreateHero(legionMemberDetail2);
				num4++;
			}
		}
		if (flag)
		{
			return;
		}
		for (int k = 0; k < memberList.Count; k++)
		{
			LegionMemberDetail legionMemberDetail3 = memberList[k];
			if (legionMemberDetail3.RoleBaseInfo.RoleId == roleId)
			{
				CreateHero(legionMemberDetail3);
				break;
			}
		}
	}

	private void InitDoorAnimation()
	{
		for (int i = 0; i < 4; i++)
		{
			PlayDoorAnimation(i, "idle_close", loop: true);
		}
	}

	private void CreateHero(LegionMemberDetail legionMemberDetail)
	{
		zbLegionHero component = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/wnd/legion/legionhero/legionhero.prefab"), heroTF).GetComponent<zbLegionHero>();
		component.Init(legionMemberDetail);
		heroList.Add(component);
	}

	public void PlayDoorAnimation(int doorIndex, string animName, bool loop, bool addClose = false)
	{
		SkeletonAnimation val = doorAnim[doorIndex];
		if (Object.op_Implicit((Object)(object)val))
		{
			val.AnimationState.SetAnimation(0, animName, loop);
			if (addClose)
			{
				val.AnimationState.AddAnimation(0, "idle_close", true, 0.5f);
			}
		}
	}
}
