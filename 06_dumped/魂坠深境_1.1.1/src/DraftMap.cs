using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DraftMap : CTPrefabAsset
{
	[LocalizeText("地图名字")]
	public string mapName;

	[LocalizeText("地图描述")]
	[TextArea]
	public string mapDescription;

	public List<DraftPivot> pivots;

	public bool isExtraMap;

	public DifficultyLevel minimumDifficultyLevel = DifficultyLevel.Not;

	[TextArea(4, 8)]
	public string mapDesignNote;

	public override string CTAssetIDPrepend => "DRMAP";

	public void SetRenderOrder()
	{
		List<Renderer> list = GetComponentsInChildren<Renderer>(includeInactive: true).ToList();
		list.Sort((Renderer t2, Renderer t1) => base.transform.InverseTransformPoint(t1.transform.position).z.CompareTo(base.transform.InverseTransformPoint(t2.transform.position).z));
		int num = 1;
		foreach (Renderer item in list)
		{
			item.sortingOrder = num;
			num++;
		}
	}

	public List<DraftPivot> GetAllVisiblePivots()
	{
		return pivots.Where((DraftPivot p) => p.ShouldGenerate()).ToList();
	}

	public int GetPivotIndex(DraftPivot pivot)
	{
		return pivots.IndexOf(pivot);
	}

	public DraftPivot GetIndexPivot(int index)
	{
		return pivots.ElementAtOrDefault(index);
	}

	public List<RoomRef> GetAllValidRoomRefs(ApplicationStateInfo applicationState)
	{
		return (from r in GetAllVisiblePivots().SelectMany((DraftPivot p) => p.randomRoomRefs)
			where r?.IsValidRef(applicationState.assetCatalog) ?? false
			select r).ToList();
	}
}
