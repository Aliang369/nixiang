using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbHeroPreview : MonoBehaviour
{
	private SkeletonAnimation m_spineAnimation;

	public zbHeroCsvRecord record;

	protected void OnEnable()
	{
		m_spineAnimation = ((Component)((Component)this).transform.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
	}

	public void Refresh(int skillType)
	{
		((Component)this).gameObject.SetActive(false);
		if (skillType == 1)
		{
			record = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(10100);
			int avatarId = zbSelf.Instance.GetAvatarId();
			zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(avatarId);
			zbUtil.RefreshSpine(m_spineAnimation, recordByKey.fightBone + "_SkeletonData.asset", "idle", loop: true);
			((Component)this).gameObject.SetActive(true);
			return;
		}
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)skillType);
		if (warRoomDetailBySkillType.HeroTid <= 0)
		{
			return;
		}
		record = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(warRoomDetailBySkillType.HeroTid);
		if (record != null)
		{
			zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(record.avatarId);
			if (recordByKey2 != null)
			{
				zbUtil.RefreshSpine(m_spineAnimation, recordByKey2.fightBone + "_SkeletonData.asset", "idle", loop: true);
				((Component)this).gameObject.SetActive(true);
			}
		}
	}

	public zbHeroCsvRecord GetHeroRecord()
	{
		return record;
	}

	public void SetHeroActive(bool active)
	{
		((Component)this).gameObject.SetActive(active);
		if (!active)
		{
			record = null;
		}
	}
}
