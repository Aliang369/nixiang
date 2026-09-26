using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;

public class zbWallHandler
{
	public int mechaSetId;

	public void Init()
	{
	}

	public void UpdateWallInfo(WallInfo wallInfo)
	{
		zbServerData.sGameData.WallInfo = wallInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_WallInfo);
	}

	public WallDetail GetWallDetail(int wallId)
	{
		return ((IEnumerable<WallDetail>)zbServerData.sGameData?.WallInfo?.WallList).FirstOrDefault((WallDetail v) => v.WallId == wallId);
	}

	public string GetSkillDesc(int wallId, int star)
	{
		zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wallId);
		if (recordByKey == null || star < 1)
		{
			return "";
		}
		int[] array = new int[5] { recordByKey.wallSkill1, recordByKey.wallSkill2, recordByKey.wallSkill3, recordByKey.wallSkill4, recordByKey.wallSkill5 };
		zbWallSkillCsvRecord recordByKey2 = zbDataConfig.Instance.wallSkillCsvTable.GetRecordByKey(array[star - 1]);
		if (recordByKey2 == null)
		{
			return "";
		}
		return string.Format(recordByKey2.desc, recordByKey2.param.Cast<object>().ToArray());
	}

	public bool HasActiveNotify(int wallId)
	{
		zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wallId);
		WallDetail wallDetail = GetWallDetail(wallId);
		if (recordByKey != null && wallDetail == null && zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, recordByKey.propId, 1))
		{
			return true;
		}
		return false;
	}

	public bool HasStarNotify(int wallId)
	{
		zbWallCsvRecord record = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wallId);
		WallDetail wallDetail = GetWallDetail(wallId);
		if (record != null && wallDetail != null)
		{
			zbWallStarUpgradeCsvRecord zbWallStarUpgradeCsvRecord2 = zbDataConfig.Instance.wallStarUpgradeCsvTable.GetRecords().records.FirstOrDefault((zbWallStarUpgradeCsvRecord r) => r.id == record.starUpgradeId && r.star == wallDetail.Star + 1);
			if (zbWallStarUpgradeCsvRecord2 != null && zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, record.propId) + zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, record.convertPropId) >= zbWallStarUpgradeCsvRecord2.costProp)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWallNotify(int wallId)
	{
		if (!HasActiveNotify(wallId))
		{
			return HasStarNotify(wallId);
		}
		return true;
	}

	public bool HasUpgradeNotify()
	{
		if (zbServerData.sGameData.WallInfo != null)
		{
			zbWallUpgradeCsvRecord recordByKey = zbDataConfig.Instance.wallUpgradeCsvTable.GetRecordByKey(zbServerData.sGameData.WallInfo.Level + 1);
			if (recordByKey != null && zbSelf.Instance.GetLevel() >= recordByKey.playerLv && zbSelf.Instance.dataHandler.GetCostListLackItem(recordByKey.costList) == null)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTabSetNotify()
	{
		foreach (zbWallCsvRecord value in zbDataConfig.Instance.wallCsvTable.GetRecords().records.Values)
		{
			if (HasWallNotify(value.id))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.WallDefense))
		{
			return false;
		}
		if (!HasUpgradeNotify())
		{
			return HasTabSetNotify();
		}
		return true;
	}

	public bool HasNewPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.WallDefense))
		{
			return false;
		}
		if (!zbGameSave.Instance.GetBool("NewFuncWallOpened" + zbSelf.Instance.GetRoleId()))
		{
			return true;
		}
		foreach (WallDetail wall in zbServerData.sGameData.WallInfo.WallList)
		{
			if (zbSelf.Instance.newTagHandler.IsNewWall(wall.WallId))
			{
				return true;
			}
		}
		return false;
	}

	public List<WallDetail> GetWallShouldConvert()
	{
		List<WallDetail> list = new List<WallDetail>();
		if (zbServerData.sGameData.WallInfo != null)
		{
			foreach (WallDetail wall in zbServerData.sGameData.WallInfo.WallList)
			{
				zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wall.WallId);
				bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, recordByKey.propId, 1);
				if ((wall.Star == 5) & flag)
				{
					list.Add(wall);
				}
			}
		}
		return list;
	}

	public void SetSuperWallInAnimation(SkeletonAnimation skeletonAnimation, bool isFirst = false)
	{
		if (isFirst && zbUtil.CheckAnimationNameExist(skeletonAnimation, "in"))
		{
			Skeleton skeleton = ((SkeletonRenderer)skeletonAnimation).Skeleton;
			AnimationState animationState = skeletonAnimation.AnimationState;
			animationState.ClearTracks();
			skeleton.SetToSetupPose();
			animationState.Apply(skeleton);
			skeletonAnimation.AnimationState.SetAnimation(0, "in", false);
			skeletonAnimation.AnimationState.AddAnimation(0, "1", true, 0f);
		}
	}

	public void DataChange(DefenseInfo oldDefenseInfo, DefenseInfo newDefenseInfo)
	{
		if (oldDefenseInfo == null || newDefenseInfo == null || oldDefenseInfo.DefenseList == null || oldDefenseInfo.DefenseList.Count == 0 || newDefenseInfo.DefenseList == null || newDefenseInfo.DefenseList.Count == 0)
		{
			return;
		}
		RepeatedField<BattleDefenseAttrDetail> attrList = oldDefenseInfo.DefenseList[0].AttrList;
		RepeatedField<BattleDefenseAttrDetail> attrList2 = newDefenseInfo.DefenseList[0].AttrList;
		if (attrList == null || attrList2 == null)
		{
			return;
		}
		foreach (BattleDefenseAttrDetail item in attrList2)
		{
			float diffAndUpdate = GetDiffAndUpdate(attrList, item.Type, item.Value);
			if (Math.Abs(diffAndUpdate) > 0.001f)
			{
				zbTeamAttrCsvRecord recordByKey = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)item.Type);
				if (recordByKey != null)
				{
					bool isPercentageDisplay = recordByKey.showPercent > 0;
					string msg = zbAttrHelper.GetAttrName(EAttrType.Team, 0, (int)item.Type) + " " + zbAttrHelper.FormatAttrValue(diffAndUpdate, isPercentageDisplay, isDisplayAdd: true);
					zbUIMgr.Instance.BubbleInfo(msg);
				}
			}
		}
	}

	private float GetDiffAndUpdate(RepeatedField<BattleDefenseAttrDetail> oldAttrList, EAttrDefenseType type, float value)
	{
		foreach (BattleDefenseAttrDetail oldAttr in oldAttrList)
		{
			if (oldAttr.Type == type)
			{
				return value - oldAttr.Value;
			}
		}
		return value;
	}
}
