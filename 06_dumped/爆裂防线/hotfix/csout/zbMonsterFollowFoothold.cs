using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class zbMonsterFollowFoothold : MonoBehaviour
{
	private SkeletonAnimation m_spineDikuai;

	private zbMonsterPreview[] m_monsters = new zbMonsterPreview[4];

	private int footholdId;

	private List<int> monsters = new List<int>();

	protected void OnEnable()
	{
		GameObject gameObject = ((Component)((Component)this).transform.Find("spine_dikuai")).gameObject;
		m_spineDikuai = gameObject.GetComponent<SkeletonAnimation>();
		for (int i = 0; i < 4; i++)
		{
			m_monsters[i] = zbUtil.FindGameObjectRecursive(((Component)this).transform, "monsterPreview" + i).GetComponent<zbMonsterPreview>();
		}
	}

	public void Refresh(int FootholdId, List<int> monsterIdList)
	{
		footholdId = FootholdId;
		monsters = monsterIdList;
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(footholdId);
		if (recordByKey != null)
		{
			zbUtil.RefreshSpine(m_spineDikuai, recordByKey.mainScene + "_SkeletonData.asset", "in");
			m_spineDikuai.AnimationState.AddAnimation(0, "loop", true, 0f);
		}
		for (int i = 0; i < 4; i++)
		{
			m_monsters[i].Refresh(monsters[i]);
		}
	}
}
