using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;

public class zbSuitGunHandler
{
	public SuitConvertAck suitConvertAck;

	public GunConvertAck gunConvertAck;

	private List<int> m_listReleasedSuitIds;

	private List<int> m_listReleasedGunIds;

	public int maxGunStar = 10;

	public void Init()
	{
		GetReleasedSuitIds();
		GetReleasedGunIds();
	}

	private List<int> GetReleasedSuitIds()
	{
		if (m_listReleasedSuitIds == null)
		{
			m_listReleasedSuitIds = new List<int>();
			foreach (KeyValuePair<int, zbPlayerSuitCsvRecord> record in zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records)
			{
				if (record.Value.id != 1 && zbVersionMgr.IsReleased(record.Value.preShow))
				{
					m_listReleasedSuitIds.Add(record.Value.id);
				}
			}
		}
		return m_listReleasedSuitIds;
	}

	private List<int> GetReleasedGunIds()
	{
		if (m_listReleasedGunIds == null)
		{
			m_listReleasedGunIds = new List<int>();
			foreach (KeyValuePair<int, zbPlayerGunCsvRecord> record in zbDataConfig.Instance.playerGunCsvTable.GetRecords().records)
			{
				if (record.Value.id != 2 && zbVersionMgr.IsReleased(record.Value.preShow))
				{
					m_listReleasedGunIds.Add(record.Value.id);
				}
			}
		}
		return m_listReleasedGunIds;
	}

	public PlayerSuitDetail GetPlayerSuitDetail(int suitId)
	{
		foreach (PlayerSuitDetail suit in zbServerData.sGameData.SuitInfo.SuitList)
		{
			if (suit.SuitId == suitId)
			{
				return suit;
			}
		}
		return null;
	}

	public string GetSkillDesc(int suitId, int star)
	{
		zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suitId);
		if (recordByKey == null)
		{
			return "";
		}
		return star switch
		{
			1 => recordByKey.skillDesc1, 
			2 => recordByKey.skillDesc2, 
			3 => recordByKey.skillDesc3, 
			4 => recordByKey.skillDesc4, 
			5 => recordByKey.skillDesc5, 
			_ => "", 
		};
	}

	public zbPlayerSuitUpgradeCsvRecord GetSuitStarUpgrade(int suitId, int toStar)
	{
		zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suitId);
		if (recordByKey == null)
		{
			return null;
		}
		zbPlayerSuitUpgradeCsvRecord[] records = zbDataConfig.Instance.playerSuitUpgradeCsvTable.GetRecords().records;
		foreach (zbPlayerSuitUpgradeCsvRecord zbPlayerSuitUpgradeCsvRecord2 in records)
		{
			if (zbPlayerSuitUpgradeCsvRecord2.id == recordByKey.upgradeId && zbPlayerSuitUpgradeCsvRecord2.star == toStar)
			{
				return zbPlayerSuitUpgradeCsvRecord2;
			}
		}
		return null;
	}

	public bool CanSuitActiveOrUpgrade(int suitId)
	{
		int num = 1;
		PlayerSuitDetail playerSuitDetail = zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(suitId);
		if (playerSuitDetail != null)
		{
			num = playerSuitDetail.Star + 1;
		}
		if (num > 5)
		{
			return false;
		}
		zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suitId);
		if (recordByKey == null)
		{
			return false;
		}
		zbPlayerSuitUpgradeCsvRecord suitStarUpgrade = GetSuitStarUpgrade(suitId, num);
		if (suitStarUpgrade == null)
		{
			return false;
		}
		long num2 = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.propId);
		if (num != 1)
		{
			num2 += zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.convertPropId);
		}
		return num2 >= suitStarUpgrade.costProp;
	}

	public bool CanAllSuitActiveOrUpgrade()
	{
		foreach (int releasedSuitId in GetReleasedSuitIds())
		{
			if (CanSuitActiveOrUpgrade(releasedSuitId))
			{
				return true;
			}
		}
		return false;
	}

	public float SetSuperSuitEnterAnimation(zbPlayerAvatar m_spinePlayer, bool isFirstEnter = true)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		if (!zbUtil.CheckAnimationNameExist(m_spinePlayer.m_spineAvatarBody, "4_enter"))
		{
			return 0f;
		}
		Skeleton skeleton = ((SkeletonRenderer)m_spinePlayer.m_spineAvatarBody).Skeleton;
		AnimationState state = m_spinePlayer.m_spineAvatarBody.AnimationState;
		state.ClearTracks();
		skeleton.SetToSetupPose();
		state.Apply(skeleton);
		m_spinePlayer.SetAnimation(0, "idle", loop: true);
		m_spinePlayer.SetAnimation(2, "2_effect_body", loop: true);
		if (isFirstEnter)
		{
			TrackEntry val = m_spinePlayer.SetAnimation(4, "4_enter", loop: false);
			if (val != null)
			{
				val.Complete += (TrackEntryDelegate)delegate
				{
					state.SetEmptyAnimation(4, 0f);
					m_spinePlayer.SetAnimation(3, "3_effect_bg", loop: true);
				};
				return val.AnimationEnd;
			}
			return 0f;
		}
		m_spinePlayer.SetAnimation(3, "3_effect_bg", loop: true);
		return 0f;
	}

	public bool HasNewSuitPoint()
	{
		foreach (PlayerSuitDetail suit in zbServerData.sGameData.SuitInfo.SuitList)
		{
			if (zbSelf.Instance.newTagHandler.IsNewSuit(suit.SuitId))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPlayerSuitSkillUsed(int suitId)
	{
		foreach (int use in zbServerData.sGameData.SuitInfo.UseList)
		{
			if (use == suitId)
			{
				return true;
			}
		}
		foreach (int superUse in zbServerData.sGameData.SuitInfo.SuperUseList)
		{
			if (superUse == suitId)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSuperSuitOpen()
	{
		int num = 0;
		foreach (PlayerSuitDetail suit in zbServerData.sGameData.SuitInfo.SuitList)
		{
			zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suit.SuitId);
			if (recordByKey != null)
			{
				if (recordByKey.grade == 2)
				{
					return true;
				}
				num++;
			}
		}
		return num > 3;
	}

	public PlayerGunDetail GetPlayerGunDetail(int gunId)
	{
		foreach (PlayerGunDetail gun in zbServerData.sGameData.GunInfo.GunList)
		{
			if (gun.GunId == gunId)
			{
				return gun;
			}
		}
		return null;
	}

	public zbPlayerGunUpgradeCsvRecord GetGunStarUpgrade(int gunId, int toStar)
	{
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(gunId);
		if (recordByKey == null)
		{
			return null;
		}
		zbPlayerGunUpgradeCsvRecord[] records = zbDataConfig.Instance.playerGunUpgradeCsvTable.GetRecords().records;
		foreach (zbPlayerGunUpgradeCsvRecord zbPlayerGunUpgradeCsvRecord2 in records)
		{
			if (zbPlayerGunUpgradeCsvRecord2.id == recordByKey.upgradeId && zbPlayerGunUpgradeCsvRecord2.star == toStar)
			{
				return zbPlayerGunUpgradeCsvRecord2;
			}
		}
		return null;
	}

	public bool CanGunActiveOrUpgrade(int gunId)
	{
		int num = 1;
		PlayerGunDetail playerGunDetail = zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(gunId);
		if (playerGunDetail != null)
		{
			num = playerGunDetail.Star + 1;
		}
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(gunId);
		if (recordByKey == null)
		{
			return false;
		}
		zbPlayerGunUpgradeCsvRecord gunStarUpgrade = GetGunStarUpgrade(gunId, num);
		if (gunStarUpgrade == null)
		{
			return false;
		}
		long num2 = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.propId);
		if (num != 1)
		{
			num2 += zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.convertPropId);
		}
		return num2 >= gunStarUpgrade.costProp;
	}

	public bool CanAllGunActiveOrUpgrade()
	{
		foreach (int releasedGunId in GetReleasedGunIds())
		{
			if (CanGunActiveOrUpgrade(releasedGunId))
			{
				return true;
			}
		}
		return false;
	}

	public List<PlayerSuitDetail> GetSuitsShouldConvert()
	{
		List<PlayerSuitDetail> list = new List<PlayerSuitDetail>();
		foreach (PlayerSuitDetail suit in zbServerData.sGameData.SuitInfo.SuitList)
		{
			if (suit.SuitId != 1)
			{
				zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suit.SuitId);
				bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, recordByKey.propId, 1);
				if ((suit.Star == 5) & flag)
				{
					list.Add(suit);
				}
			}
		}
		return list;
	}

	public List<PlayerGunDetail> GetGunsShouldConvert()
	{
		List<PlayerGunDetail> list = new List<PlayerGunDetail>();
		foreach (PlayerGunDetail gun in zbServerData.sGameData.GunInfo.GunList)
		{
			if (gun.GunId != 2)
			{
				zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(gun.GunId);
				bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, recordByKey.propId, 1);
				if ((gun.Star == maxGunStar) & flag)
				{
					list.Add(gun);
				}
			}
		}
		return list;
	}
}
