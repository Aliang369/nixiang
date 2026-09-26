using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaSubjectHandler
{
	public string currUUID;

	public int currLevelId;

	public int rouletteCost = 3;

	public List<zbMechaSubjectZhanLingCsvRecord> m_zhanlingRecords = new List<zbMechaSubjectZhanLingCsvRecord>();

	public zbMechaSubjectLevelCsvRecord[] m_levelRecords;

	public zbMechaSubjectRelicCsvRecord[] m_relicRecords;

	private Dictionary<int, int> m_maxLevelDic = new Dictionary<int, int>();

	public int m_barPropId;

	public int[][][] m_targetAwards;

	public int[] m_skillUnlock;

	public string pathNamePre { get; set; } = "ui/image/mechaSubject/";

	public string skeletonPathNamePre { get; set; } = "animation/ui/mechaSubject/";

	public string pathName { get; set; }

	public void Init()
	{
		pathName = "mechaSubject_1/";
		int count = zbDataConfig.Instance.mechaSubjectZhanLingCsvTable.GetRecords().records.Count;
		for (int i = 0; i < count; i++)
		{
			zbMechaSubjectZhanLingCsvRecord recordByKey = zbDataConfig.Instance.mechaSubjectZhanLingCsvTable.GetRecordByKey(i + 1);
			m_zhanlingRecords.Add(recordByKey);
		}
		m_levelRecords = zbDataConfig.Instance.mechaSubjectLevelCsvTable.GetRecords().records;
		zbMechaSubjectLevelCsvRecord[] levelRecords = m_levelRecords;
		foreach (zbMechaSubjectLevelCsvRecord zbMechaSubjectLevelCsvRecord2 in levelRecords)
		{
			if (!m_maxLevelDic.TryGetValue(zbMechaSubjectLevelCsvRecord2.subjectId, out var value) || zbMechaSubjectLevelCsvRecord2.level > value)
			{
				m_maxLevelDic[zbMechaSubjectLevelCsvRecord2.subjectId] = zbMechaSubjectLevelCsvRecord2.level;
			}
		}
		m_relicRecords = zbDataConfig.Instance.mechaSubjectRelicCsvTable.GetRecords().records;
		m_barPropId = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.mechaSubjectBaseJsonTable.GetRecord()["propId"]);
		m_targetAwards = zbDataConfig.Instance.mechaSubjectBaseJsonTable.GetRecord()["chapter"][(object)"awardList"].ToObject<int[][][]>();
		m_skillUnlock = zbDataConfig.Instance.mechaSubjectBaseJsonTable.GetRecord()["relic"][(object)"skillUnlock"].ToObject<int[]>();
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
		if ((Object)(object)sceneBg != (Object)null && (Object)(object)val != (Object)null)
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

	public int GetSceneId(int subjectId, int level)
	{
		zbMechaSubjectLevelCsvRecord[] levelRecords = m_levelRecords;
		foreach (zbMechaSubjectLevelCsvRecord zbMechaSubjectLevelCsvRecord2 in levelRecords)
		{
			if (zbMechaSubjectLevelCsvRecord2.subjectId == subjectId && zbMechaSubjectLevelCsvRecord2.level == level)
			{
				return zbMechaSubjectLevelCsvRecord2.sceneId;
			}
		}
		return 1;
	}

	public int GetMaxLevel(int subjectId)
	{
		if (!m_maxLevelDic.TryGetValue(subjectId, out var value))
		{
			return 0;
		}
		return value;
	}

	public MechaSubjectDetail GetMechaSubjectDetail()
	{
		if (zbServerData.sGameData.OperateInfo.MechaSubjectList != null)
		{
			foreach (MechaSubjectDetail mechaSubject in zbServerData.sGameData.OperateInfo.MechaSubjectList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(mechaSubject.BaseInfo) && mechaSubject.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					return mechaSubject;
				}
			}
		}
		return null;
	}

	public (zbMechaSubjectZhanLingCsvRecord prev, zbMechaSubjectZhanLingCsvRecord next) GetZhanlingRecord(int count)
	{
		List<zbMechaSubjectZhanLingCsvRecord> zhanlingRecords = m_zhanlingRecords;
		int num = 0;
		int num2 = zhanlingRecords.Count - 1;
		zbMechaSubjectZhanLingCsvRecord item = null;
		zbMechaSubjectZhanLingCsvRecord item2 = null;
		while (num <= num2)
		{
			int num3 = num + num2 >> 1;
			zbMechaSubjectZhanLingCsvRecord zbMechaSubjectZhanLingCsvRecord2 = zhanlingRecords[num3];
			if (zbMechaSubjectZhanLingCsvRecord2.count <= count)
			{
				item = zbMechaSubjectZhanLingCsvRecord2;
				num = num3 + 1;
			}
			else
			{
				item2 = zbMechaSubjectZhanLingCsvRecord2;
				num2 = num3 - 1;
			}
		}
		return (prev: item, next: item2);
	}

	public bool HasNextCanReceiveAward(int mainLevel, int receivedLevel)
	{
		foreach (zbMechaSubjectZhanLingCsvRecord zhanlingRecord in m_zhanlingRecords)
		{
			if (zhanlingRecord.count > receivedLevel && zhanlingRecord.count <= mainLevel)
			{
				return true;
			}
		}
		return false;
	}

	public ECommonState GetLevelTargeState(MechaSubjectDetail mechaSubjectDetail, int chapterId)
	{
		OperateSubjectChapterInfo chapterInfo = mechaSubjectDetail.ChapterInfo;
		if (chapterInfo != null)
		{
			foreach (OperateSubjectChapterDetail chapter in chapterInfo.ChapterList)
			{
				if (chapter.ChapterId == chapterId)
				{
					return chapter.ChapterAward;
				}
			}
		}
		return ECommonState.No;
	}

	public bool HasLevelNotify(MechaSubjectDetail mechaSubjectDetail)
	{
		OperateSubjectChapterInfo chapterInfo = mechaSubjectDetail.ChapterInfo;
		if (chapterInfo == null)
		{
			return false;
		}
		foreach (OperateSubjectChapterDetail chapter in chapterInfo.ChapterList)
		{
			if (chapter.ChapterAward == ECommonState.Yes)
			{
				return true;
			}
		}
		if (!HasBarNotify(mechaSubjectDetail))
		{
			return HasRelicNew();
		}
		return true;
	}

	public bool HasBarNotify(MechaSubjectDetail mechaSubjectDetail)
	{
		if (mechaSubjectDetail.BarInfo == null)
		{
			return false;
		}
		if (zbSelf.Instance.itemHandler.HasEnoughProp(m_barPropId, 10L))
		{
			return true;
		}
		return false;
	}

	public bool HasZhanLingNotify(MechaSubjectDetail mechaSubjectDetail)
	{
		ZhanLingCommonInfo zhanLingInfo = mechaSubjectDetail.ZhanLingInfo;
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

	public bool HasRouletteNotify(MechaSubjectDetail mechaSubjectDetail)
	{
		OperateSubjectRouletteInfo rouletteInfo = mechaSubjectDetail.RouletteInfo;
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

	public bool HasRedPoint()
	{
		MechaSubjectDetail mechaSubjectDetail = GetMechaSubjectDetail();
		if (mechaSubjectDetail != null)
		{
			if (!HasLevelNotify(mechaSubjectDetail) && !HasZhanLingNotify(mechaSubjectDetail))
			{
				return HasRouletteNotify(mechaSubjectDetail);
			}
			return true;
		}
		return false;
	}

	public string GetMechaSubjectUUID()
	{
		MechaSubjectDetail mechaSubjectDetail = GetMechaSubjectDetail();
		if (mechaSubjectDetail != null)
		{
			return mechaSubjectDetail.BaseInfo.UUID;
		}
		return "";
	}

	public bool HasRelicNew()
	{
		zbMechaSubjectRelicCsvRecord[] relicRecords = m_relicRecords;
		foreach (zbMechaSubjectRelicCsvRecord zbMechaSubjectRelicCsvRecord2 in relicRecords)
		{
			if (zbSelf.Instance.newTagHandler.IsNewMechaRelic(zbMechaSubjectRelicCsvRecord2.id))
			{
				return true;
			}
		}
		return false;
	}
}
