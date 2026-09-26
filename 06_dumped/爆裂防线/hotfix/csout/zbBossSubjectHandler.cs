using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBossSubjectHandler
{
	public string currUUID;

	public int currBossLevel;

	public float currDamageAdd;

	public int maxLevel;

	public int getScore;

	public int rouletteCost = 3;

	public List<zbBossZhanlingCsvRecord> m_zhanlingRecords = new List<zbBossZhanlingCsvRecord>();

	public List<zbBossTaskTargetCsvRecord> m_bossTaskTargetRecords = new List<zbBossTaskTargetCsvRecord>();

	public bool shouldShowBossSubjectTip = true;

	public bool shouldShowBossSubjectDiffTip = true;

	public string pathNamePre { get; set; } = "ui/image/bossSubject/";

	public string skeletonPathNamePre { get; set; } = "animation/ui/bossSubject/";

	public string pathName { get; set; }

	public void Init()
	{
		pathName = "bossSubject_3011/";
		maxLevel = zbDataConfig.Instance.bossLevelCsvTable.GetRecords().records.Count;
		int count = zbDataConfig.Instance.bossZhanlingCsvTable.GetRecords().records.Count;
		for (int i = 0; i < count; i++)
		{
			zbBossZhanlingCsvRecord recordByKey = zbDataConfig.Instance.bossZhanlingCsvTable.GetRecordByKey(i + 1);
			m_zhanlingRecords.Add(recordByKey);
		}
		foreach (KeyValuePair<int, zbBossTaskTargetCsvRecord> record in zbDataConfig.Instance.bossTaskTargetCsvTable.GetRecords().records)
		{
			m_bossTaskTargetRecords.Add(record.Value);
		}
		m_bossTaskTargetRecords.Sort((zbBossTaskTargetCsvRecord a, zbBossTaskTargetCsvRecord b) => a.target.CompareTo(b.target));
	}

	public void SetVisualElementSprite(VisualElement visualElement, string fileName)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(pathNamePre + pathName + fileName);
		visualElement.style.backgroundImage = new StyleBackground(val);
	}

	public void SetSpineVisualElement(SpineVisualElement visualElement, string fileName)
	{
		string text = skeletonPathNamePre + pathName + fileName + "/" + fileName;
		zbUtil.RefreshSpineUI(visualElement, text + "_SkeletonData.asset");
	}

	public void SetGameObjectSprite(SpriteRenderer sceneBg, string fileName)
	{
		Sprite val = zbAssetLoader.Instance.LoadSpriteAsset(pathNamePre + pathName + fileName);
		if ((Object)(object)val != (Object)null)
		{
			sceneBg.sprite = val;
		}
	}

	public void SetSkeletonAnimation(SkeletonAnimation skeletonAnimation, string fileName)
	{
		SkeletonDataAsset val = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(skeletonPathNamePre + pathName + fileName + "/" + fileName + "_SkeletonData.asset");
		if ((Object)(object)val != (Object)null)
		{
			((SkeletonRenderer)skeletonAnimation).skeletonDataAsset = val;
			((SkeletonRenderer)skeletonAnimation).Initialize(true, false);
		}
	}

	public BossSubjectDetail GetBossSubjectDetail()
	{
		if (zbServerData.sGameData.OperateInfo.BossSubjectList != null)
		{
			foreach (BossSubjectDetail bossSubject in zbServerData.sGameData.OperateInfo.BossSubjectList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(bossSubject.BaseInfo) && bossSubject.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					return bossSubject;
				}
			}
		}
		return null;
	}

	public (zbBossTaskTargetCsvRecord prev, zbBossTaskTargetCsvRecord next) GetBossTaskTargetRecord(int indexScore)
	{
		List<zbBossTaskTargetCsvRecord> bossTaskTargetRecords = m_bossTaskTargetRecords;
		int num = 0;
		int num2 = bossTaskTargetRecords.Count - 1;
		zbBossTaskTargetCsvRecord item = null;
		zbBossTaskTargetCsvRecord item2 = null;
		while (num <= num2)
		{
			int num3 = num + num2 >> 1;
			zbBossTaskTargetCsvRecord zbBossTaskTargetCsvRecord2 = bossTaskTargetRecords[num3];
			if (zbBossTaskTargetCsvRecord2.target <= indexScore)
			{
				item = zbBossTaskTargetCsvRecord2;
				num = num3 + 1;
			}
			else
			{
				item2 = zbBossTaskTargetCsvRecord2;
				num2 = num3 - 1;
			}
		}
		return (prev: item, next: item2);
	}

	public (zbBossZhanlingCsvRecord prev, zbBossZhanlingCsvRecord next) GetZhanlingRecord(int indexScore)
	{
		List<zbBossZhanlingCsvRecord> zhanlingRecords = m_zhanlingRecords;
		int num = 0;
		int num2 = zhanlingRecords.Count - 1;
		zbBossZhanlingCsvRecord item = null;
		zbBossZhanlingCsvRecord item2 = null;
		while (num <= num2)
		{
			int num3 = num + num2 >> 1;
			zbBossZhanlingCsvRecord zbBossZhanlingCsvRecord2 = zhanlingRecords[num3];
			if (zbBossZhanlingCsvRecord2.score <= indexScore)
			{
				item = zbBossZhanlingCsvRecord2;
				num = num3 + 1;
			}
			else
			{
				item2 = zbBossZhanlingCsvRecord2;
				num2 = num3 - 1;
			}
		}
		return (prev: item, next: item2);
	}

	public bool HasNextCanReceiveAward(int mainLevel, int receivedLevel)
	{
		foreach (zbBossZhanlingCsvRecord zhanlingRecord in m_zhanlingRecords)
		{
			if (zhanlingRecord.lv > receivedLevel && zhanlingRecord.score <= mainLevel)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasZhanLingNotify(BossSubjectDetail bossSubjectDetail)
	{
		ZhanLingCommonInfo zhanLingInfo = bossSubjectDetail.ZhanLingInfo;
		if (zhanLingInfo == null)
		{
			return false;
		}
		if (HasNextCanReceiveAward(zhanLingInfo.Reach, zhanLingInfo.LowAward))
		{
			return true;
		}
		if (zhanLingInfo.Active == EBoolState.True && HasNextCanReceiveAward(zhanLingInfo.Reach, zhanLingInfo.HighAward))
		{
			return true;
		}
		return false;
	}

	public bool HasRouletteNotify(BossSubjectDetail bossSubjectDetail)
	{
		BossSubjectRouletteInfo rouletteInfo = bossSubjectDetail.RouletteInfo;
		if (rouletteInfo == null)
		{
			return false;
		}
		if (zbSelf.Instance.moneyHandler.HasEnoughMoney(35, rouletteCost))
		{
			return true;
		}
		foreach (OperateTargetDetail target in rouletteInfo.TargetList)
		{
			if (target.State == ECommonState.Yes)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTaskNotify(BossSubjectDetail bossSubjectDetail)
	{
		foreach (TaskDetail task in bossSubjectDetail.TaskList)
		{
			if (task.State == EActiveState.Active)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRedPoint()
	{
		BossSubjectDetail bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
		if (bossSubjectDetail != null)
		{
			if (!HasTaskNotify(bossSubjectDetail) && !HasZhanLingNotify(bossSubjectDetail))
			{
				return HasRouletteNotify(bossSubjectDetail);
			}
			return true;
		}
		return false;
	}
}
