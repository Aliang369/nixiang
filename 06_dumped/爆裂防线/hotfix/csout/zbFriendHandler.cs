using System.Collections.Generic;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbFriendHandler
{
	public RepeatedField<FriendsDetail> friendsList = new RepeatedField<FriendsDetail>();

	public RepeatedField<long> friendsIdList = new RepeatedField<long>();

	public RepeatedField<FriendsApplyDetail> applyList = new RepeatedField<FriendsApplyDetail>();

	public RepeatedField<FriendsCommonDetail> recommendList = new RepeatedField<FriendsCommonDetail>();

	public RepeatedField<FriendsCommonDetail> searchList = new RepeatedField<FriendsCommonDetail>();

	public static HashSet<int> AppliedPlayerIds = new HashSet<int>();

	public int Max_Friends_Collect_Number;

	public int Max_Friends_Number;

	public void Init()
	{
		JObject record = zbDataConfig.Instance.friendsBaseJsonTable.GetRecord();
		Max_Friends_Collect_Number = (int)((record != null) ? record["actionDaily"] : null);
		Max_Friends_Number = (int)((record != null) ? record["maxFriendsNum"] : null);
	}

	public void UpdateFriendsList(RepeatedField<FriendsDetail> list)
	{
		friendsList = list;
	}

	public void UpdateFriendIdList(RepeatedField<long> list)
	{
		friendsIdList = list;
	}

	public bool CheckIsFriend(long playerId)
	{
		foreach (long friendsId in friendsIdList)
		{
			if ((int)friendsId == playerId)
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateApplyList(RepeatedField<FriendsApplyDetail> list)
	{
		applyList = list;
	}

	public void UpdateRecommendList(RepeatedField<FriendsCommonDetail> list)
	{
		recommendList = list;
	}

	public void UpdateSearchList(RepeatedField<FriendsCommonDetail> list)
	{
		searchList = list;
	}

	public void GetSortFriendsList(List<FriendsDetail> result)
	{
		result.Clear();
		foreach (FriendsDetail friends in friendsList)
		{
			result.Add(friends);
		}
		result.Sort((FriendsDetail v1, FriendsDetail v2) => (v1.RoleLoginInfo.State != ERoleState.Online && v2.RoleLoginInfo.State != ERoleState.Online) ? v2.RoleLoginInfo.LoginTm.CompareTo(v1.RoleLoginInfo.LoginTm) : v2.RoleLoginInfo.State.CompareTo(v1.RoleLoginInfo.State));
	}

	public void UpdateOneFriendDetail(FriendsDetail friend)
	{
		for (int i = 0; i < friendsList.Count; i++)
		{
			if (friendsList[i] != null && friendsList[i].RoleBaseInfo.RoleId == friend.RoleBaseInfo.RoleId)
			{
				friendsList[i] = friend;
				break;
			}
		}
	}

	public void AddOneFriendsDetail(FriendsDetail friend)
	{
		if (friend != null)
		{
			long roleId = friend.RoleBaseInfo.RoleId;
			if (!IsMyFriend(roleId))
			{
				friendsList.Add(friend);
			}
			DeleteFriendsApplyListById(roleId);
		}
	}

	public void DeleteFriendDetailById(long RoleId)
	{
		foreach (FriendsDetail friends in friendsList)
		{
			if (friends.RoleBaseInfo.RoleId == RoleId)
			{
				friendsList.Remove(friends);
				break;
			}
		}
	}

	public void DeleteFriendsApplyListById(long RoleId)
	{
		foreach (FriendsApplyDetail apply in applyList)
		{
			if (apply.RoleBaseInfo.RoleId == RoleId)
			{
				applyList.Remove(apply);
				break;
			}
		}
	}

	public bool IsMyFriend(long RoleId)
	{
		foreach (FriendsDetail friends in friendsList)
		{
			if (friends.RoleBaseInfo.RoleId == RoleId)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFriendsApply()
	{
		if (friendsList.Count < Max_Friends_Number)
		{
			return applyList.Count > 0;
		}
		return false;
	}

	public bool HasFriendListRedPoint()
	{
		foreach (FriendsDetail friends in friendsList)
		{
			if (friends.ActionHim == EBoolState.False || HasFriendTiliRedPoint())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFriendTiliRedPoint()
	{
		if (zbServerData.sGameData.FriendsInfo != null)
		{
			foreach (FriendsDetail friends in friendsList)
			{
				if (friends.ActionMe == EFriendActionCollectState.Pending && zbServerData.sGameData.FriendsInfo.CollectCount < Max_Friends_Collect_Number)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Friends))
		{
			return false;
		}
		if (!HasFriendsApply())
		{
			return HasFriendListRedPoint();
		}
		return true;
	}

	public void ShowFriendAlreadyAppliedTip()
	{
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10187"));
	}

	public string GetPassTimeShort(long offlineSeconds)
	{
		if (offlineSeconds < 0)
		{
			offlineSeconds = 0L;
		}
		int num = Mathf.FloorToInt((float)offlineSeconds);
		int num2 = num / 3600;
		int num3 = num % 3600 / 60;
		if (num2 >= 72)
		{
			return string.Format(zbUtil.Language("lang.10442"), 3);
		}
		if (num2 >= 24)
		{
			int num4 = num2 / 24;
			return string.Format(zbUtil.Language("lang.10442"), num4);
		}
		if (num2 > 0)
		{
			return string.Format(zbUtil.Language("lang.10443"), num2);
		}
		if (num3 > 0)
		{
			return string.Format(zbUtil.Language("lang.10444"), num3);
		}
		return zbUtil.Language("lang.10445");
	}

	public string GetThankLetterShort(long offlineSeconds)
	{
		if (offlineSeconds < 0)
		{
			offlineSeconds = 0L;
		}
		int num = Mathf.FloorToInt((float)offlineSeconds);
		int num2 = num / 3600;
		int num3 = num % 3600 / 60;
		if (num2 >= 72)
		{
			return string.Format(zbUtil.Language("lang.10442"), 3);
		}
		if (num2 >= 24)
		{
			int num4 = num2 / 24;
			return string.Format(zbUtil.Language("lang.10442"), num4);
		}
		if (num2 > 0)
		{
			return string.Format(zbUtil.Language("lang.10443"), num2);
		}
		if (num3 > 0)
		{
			return string.Format(zbUtil.Language("lang.10444"), num3);
		}
		return zbUtil.Language("lang.10772");
	}
}
