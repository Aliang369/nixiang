using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallSubjectHandler
{
	public string currUUID;

	public int currLevelId;

	public int rouletteCost = 3;

	public List<zbWallSubjectZhanLingCsvRecord> m_zhanlingRecords = new List<zbWallSubjectZhanLingCsvRecord>();

	public zbWallSubjectLevelCsvRecord[] m_levelRecords;

	public zbWallSubjectRelicCsvRecord[] m_relicRecords;

	private Dictionary<int, int> m_maxLevelDic = new Dictionary<int, int>();

	public int m_barPropId;

	public int[][][] m_targetAwards;

	public string pathNamePre { get; set; } = "ui/image/wallSubject/";

	public string skeletonPathNamePre { get; set; } = "animation/ui/wallSubject/";

	public string pathName { get; set; }

	public void Init()
	{
		pathName = "wallSubject_1/";
		int count = zbDataConfig.Instance.wallSubjectZhanLingCsvTable.GetRecords().records.Count;
		for (int i = 0; i < count; i++)
		{
			zbWallSubjectZhanLingCsvRecord recordByKey = zbDataConfig.Instance.wallSubjectZhanLingCsvTable.GetRecordByKey(i + 1);
			m_zhanlingRecords.Add(recordByKey);
		}
		m_levelRecords = zbDataConfig.Instance.wallSubjectLevelCsvTable.GetRecords().records;
		zbWallSubjectLevelCsvRecord[] levelRecords = m_levelRecords;
		foreach (zbWallSubjectLevelCsvRecord zbWallSubjectLevelCsvRecord2 in levelRecords)
		{
			if (!m_maxLevelDic.TryGetValue(zbWallSubjectLevelCsvRecord2.subjectId, out var value) || zbWallSubjectLevelCsvRecord2.level > value)
			{
				m_maxLevelDic[zbWallSubjectLevelCsvRecord2.subjectId] = zbWallSubjectLevelCsvRecord2.level;
			}
		}
		m_relicRecords = zbDataConfig.Instance.wallSubjectRelicCsvTable.GetRecords().records;
		m_barPropId = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.wallSubjectBaseJsonTable.GetRecord()["propId"]);
		m_targetAwards = zbDataConfig.Instance.wallSubjectBaseJsonTable.GetRecord()["chapter"][(object)"awardList"].ToObject<int[][][]>();
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
		zbWallSubjectLevelCsvRecord[] levelRecords = m_levelRecords;
		foreach (zbWallSubjectLevelCsvRecord zbWallSubjectLevelCsvRecord2 in levelRecords)
		{
			if (zbWallSubjectLevelCsvRecord2.subjectId == subjectId && zbWallSubjectLevelCsvRecord2.level == level)
			{
				return zbWallSubjectLevelCsvRecord2.sceneId;
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

	public WallSubjectDetail GetWallSubjectDetail()
	{
		if (zbServerData.sGameData.OperateInfo.WallSubjectList != null)
		{
			foreach (WallSubjectDetail wallSubject in zbServerData.sGameData.OperateInfo.WallSubjectList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(wallSubject.BaseInfo) && wallSubject.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					return wallSubject;
				}
			}
		}
		return null;
	}

	public (zbWallSubjectZhanLingCsvRecord prev, zbWallSubjectZhanLingCsvRecord next) GetZhanlingRecord(int count)
	{
		List<zbWallSubjectZhanLingCsvRecord> zhanlingRecords = m_zhanlingRecords;
		int num = 0;
		int num2 = zhanlingRecords.Count - 1;
		zbWallSubjectZhanLingCsvRecord item = null;
		zbWallSubjectZhanLingCsvRecord item2 = null;
		while (num <= num2)
		{
			int num3 = num + num2 >> 1;
			zbWallSubjectZhanLingCsvRecord zbWallSubjectZhanLingCsvRecord2 = zhanlingRecords[num3];
			if (zbWallSubjectZhanLingCsvRecord2.count <= count)
			{
				item = zbWallSubjectZhanLingCsvRecord2;
				num = num3 + 1;
			}
			else
			{
				item2 = zbWallSubjectZhanLingCsvRecord2;
				num2 = num3 - 1;
			}
		}
		return (prev: item, next: item2);
	}

	public bool HasNextCanReceiveAward(int mainLevel, int receivedLevel)
	{
		foreach (zbWallSubjectZhanLingCsvRecord zhanlingRecord in m_zhanlingRecords)
		{
			if (zhanlingRecord.count > receivedLevel && zhanlingRecord.count <= mainLevel)
			{
				return true;
			}
		}
		return false;
	}

	public ECommonState GetLevelTargeState(WallSubjectDetail wallSubjectDetail, int chapterId)
	{
		OperateSubjectChapterInfo chapterInfo = wallSubjectDetail.ChapterInfo;
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

	public bool HasLevelNotify(WallSubjectDetail wallSubjectDetail)
	{
		OperateSubjectChapterInfo chapterInfo = wallSubjectDetail.ChapterInfo;
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
		if (!HasBarNotify(wallSubjectDetail))
		{
			return HasRelicNew();
		}
		return true;
	}

	public bool HasBarNotify(WallSubjectDetail wallSubjectDetail)
	{
		if (wallSubjectDetail.BarInfo == null)
		{
			return false;
		}
		if (zbSelf.Instance.itemHandler.HasEnoughProp(m_barPropId, 10L))
		{
			return true;
		}
		return false;
	}

	public bool HasZhanLingNotify(WallSubjectDetail wallSubjectDetail)
	{
		ZhanLingCommonInfo zhanLingInfo = wallSubjectDetail.ZhanLingInfo;
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

	public bool HasRouletteNotify(WallSubjectDetail wallSubjectDetail)
	{
		OperateSubjectRouletteInfo rouletteInfo = wallSubjectDetail.RouletteInfo;
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
		WallSubjectDetail wallSubjectDetail = GetWallSubjectDetail();
		if (wallSubjectDetail != null)
		{
			if (!HasLevelNotify(wallSubjectDetail) && !HasZhanLingNotify(wallSubjectDetail))
			{
				return HasRouletteNotify(wallSubjectDetail);
			}
			return true;
		}
		return false;
	}

	public string GetWallSubjectUUID()
	{
		WallSubjectDetail wallSubjectDetail = GetWallSubjectDetail();
		if (wallSubjectDetail != null)
		{
			return wallSubjectDetail.BaseInfo.UUID;
		}
		return "";
	}

	public bool HasRelicNew()
	{
		zbWallSubjectRelicCsvRecord[] relicRecords = m_relicRecords;
		foreach (zbWallSubjectRelicCsvRecord zbWallSubjectRelicCsvRecord2 in relicRecords)
		{
			if (zbSelf.Instance.newTagHandler.IsNewRelic(zbWallSubjectRelicCsvRecord2.id))
			{
				return true;
			}
		}
		return false;
	}
}
