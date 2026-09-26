using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class zbCrossPeakPlayerAvatar : MonoBehaviour
{
	private zbPlayerAvatar m_spineAvatar;

	[SerializeField]
	private SkeletonAnimation m_taiziAnimation;

	private List<string> pathList = new List<string> { "animation/ui/dianfengsai/dianfengsai_taizi1/dianfengsai_taizi1_SkeletonData.asset", "animation/ui/dianfengsai/dianfengsai_taizi2/dianfengsai_taizi2_SkeletonData.asset", "animation/ui/dianfengsai/dianfengsai_taizi3/dianfengsai_taizi3_SkeletonData.asset", "animation/ui/dianfengsai/dianfengsai_taizi4/dianfengsai_taizi4_SkeletonData.asset" };

	private void OnEnable()
	{
		m_spineAvatar = ((Component)((Component)this).transform.Find("avatar")).GetComponent<zbPlayerAvatar>();
	}

	public void RefreshCustom(int rank, int avatarId, int gunAvatarId)
	{
		m_spineAvatar.RefreshCustom(avatarId, gunAvatarId);
		SetPosition(rank);
		switch (rank)
		{
		case 1:
			zbUtil.RefreshSpine(m_taiziAnimation, pathList[0], "animation", loop: true);
			return;
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
			zbUtil.RefreshSpine(m_taiziAnimation, pathList[1], "animation", loop: true);
			return;
		}
		if (rank > 10 && rank <= 30)
		{
			zbUtil.RefreshSpine(m_taiziAnimation, pathList[2], "animation", loop: true);
		}
		else
		{
			zbUtil.RefreshSpine(m_taiziAnimation, pathList[3], "animation", loop: true);
		}
	}

	private void SetPosition(int rank)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (rank % 2 != 0)
		{
			((Component)m_spineAvatar).transform.localScale = new Vector3(0f - ((Component)m_spineAvatar).transform.localScale.y, ((Component)m_spineAvatar).transform.localScale.y, ((Component)m_spineAvatar).transform.localScale.z);
		}
		else
		{
			((Component)m_spineAvatar).transform.localScale = new Vector3(((Component)m_spineAvatar).transform.localScale.y, ((Component)m_spineAvatar).transform.localScale.y, ((Component)m_spineAvatar).transform.localScale.z);
		}
	}
}
