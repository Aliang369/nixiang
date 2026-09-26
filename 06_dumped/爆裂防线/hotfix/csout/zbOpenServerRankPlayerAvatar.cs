using Spine.Unity;
using UnityEngine;

public class zbOpenServerRankPlayerAvatar : MonoBehaviour
{
	private SpriteRenderer m_taizi;

	private SpriteRenderer m_taiziTop;

	private SpriteRenderer m_noPlayer;

	private GameObject m_playerAvatar;

	private GameObject m_yingzi;

	private SkeletonAnimation m_spineAvatarBody;

	private SkeletonAnimation m_spineGun;

	private Transform m_nodeMecha;

	private SkeletonAnimation m_spineMecha;

	private GameObject skeletonObject;

	private GameObject gunObject;

	private void OnEnable()
	{
		m_playerAvatar = ((Component)((Component)this).transform.Find("playerAvatar")).gameObject;
		m_yingzi = ((Component)m_playerAvatar.transform.Find("yingzi")).gameObject;
		skeletonObject = ((Component)m_playerAvatar.transform.Find("avatarBody")).gameObject;
		m_spineAvatarBody = skeletonObject.GetComponent<SkeletonAnimation>();
		gunObject = ((Component)m_playerAvatar.transform.Find("gun")).gameObject;
		m_spineGun = gunObject.GetComponent<SkeletonAnimation>();
		m_nodeMecha = ((Component)this).transform.Find("mecha");
		m_spineMecha = ((Component)m_nodeMecha.Find("mechaAvatar")).GetComponent<SkeletonAnimation>();
		m_taizi = ((Component)((Component)this).transform.Find("taizi")).GetComponent<SpriteRenderer>();
		m_taiziTop = ((Component)((Component)this).transform.Find("taiziTop")).GetComponent<SpriteRenderer>();
		m_noPlayer = ((Component)((Component)this).transform.Find("noPlayer")).GetComponent<SpriteRenderer>();
	}

	public void RefreshCustom(int rank, bool visible, int avatarId, int gunAvatarId, int mechaId)
	{
		SetTaizi(rank);
		SetAvatarVisible(visible);
		SetMechaVisible(visible && mechaId != 0);
		LoadAvatar(avatarId);
		LoadGun(gunAvatarId);
		LoadMecha(mechaId);
		SetFaceByRank(rank, mechaId != 0);
		SetPosition(rank, mechaId != 0, visible);
	}

	private void LoadAvatar(int avatarId)
	{
		if (avatarId != 0)
		{
			zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(avatarId);
			if (recordByKey != null)
			{
				zbUtil.RefreshSpine(m_spineAvatarBody, recordByKey.frontBone + "_SkeletonData.asset", "idle", loop: true);
			}
		}
	}

	private void LoadGun(int gunAvatarId)
	{
		if (gunAvatarId != 0)
		{
			zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(gunAvatarId);
			if (recordByKey != null)
			{
				zbUtil.RefreshSpine(m_spineGun, recordByKey.frontBone + "_SkeletonData.asset", "animation", loop: true);
			}
		}
	}

	private void LoadMecha(int mechaId)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (mechaId == 0)
		{
			return;
		}
		zbMechaCsvRecord recordByKey = zbDataConfig.Instance.mechaCsvTable.GetRecordByKey(mechaId);
		if (recordByKey != null)
		{
			zbMechaAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			if (recordByKey2 != null)
			{
				zbUtil.RefreshSpine(m_spineMecha, recordByKey2.fightBone + "_SkeletonData.asset", "idle", loop: true);
				((Component)m_nodeMecha).transform.localScale = Vector2.op_Implicit(new Vector2(0f - recordByKey.scale1, recordByKey.scale1));
			}
		}
	}

	private void SetMechaVisible(bool visible)
	{
		((Component)m_nodeMecha).gameObject.SetActive(visible);
	}

	private void SetTaizi(int rank)
	{
		bool flag = rank == 1;
		((Component)m_taizi).gameObject.SetActive(!flag);
		((Component)m_taiziTop).gameObject.SetActive(flag);
		if (!flag)
		{
			m_taizi.flipX = rank % 2 == 0;
		}
	}

	private void SetFaceByRank(int rank, bool hasMecha)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		int num = ((rank % 2 != 0) ? 1 : (-1));
		((SkeletonRenderer)m_spineAvatarBody).skeleton.ScaleX = num;
		((SkeletonRenderer)m_spineMecha).skeleton.ScaleX = (hasMecha ? ((float)(-num)) : ((SkeletonRenderer)m_spineMecha).skeleton.ScaleX);
		m_noPlayer.flipX = num < 0;
		Vector3 localPosition = ((Component)m_noPlayer).transform.localPosition;
		localPosition.x = ((num < 0) ? 0.27f : (-0.22f));
		((Component)m_noPlayer).transform.localPosition = localPosition;
	}

	private void SetPosition(int rank, bool hasMecha, bool hasPlayer)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (!hasMecha)
		{
			m_playerAvatar.transform.localPosition = new Vector3(0f, m_playerAvatar.transform.localPosition.y, m_playerAvatar.transform.localPosition.z);
		}
		else if (hasPlayer)
		{
			if (((rank % 2 != 0 || true) ? 1 : 0) > (false ? 1 : 0))
			{
				m_playerAvatar.transform.localPosition = new Vector3(0.75f, m_playerAvatar.transform.localPosition.y, m_playerAvatar.transform.localPosition.z);
				((Component)m_nodeMecha).transform.localPosition = new Vector3(-0.45f, ((Component)m_nodeMecha).transform.localPosition.y, ((Component)m_nodeMecha).transform.localPosition.z);
			}
			else
			{
				m_playerAvatar.transform.localPosition = new Vector3(-0.5f, m_playerAvatar.transform.localPosition.y, m_playerAvatar.transform.localPosition.z);
				((Component)m_nodeMecha).transform.localPosition = new Vector3(0.55f, ((Component)m_nodeMecha).transform.localPosition.y, ((Component)m_nodeMecha).transform.localPosition.z);
			}
		}
	}

	private void SetAvatarVisible(bool hasPlayer)
	{
		((Component)m_spineAvatarBody).gameObject.SetActive(hasPlayer);
		((Component)m_spineGun).gameObject.SetActive(hasPlayer);
		m_yingzi.gameObject.SetActive(hasPlayer);
		((Component)m_noPlayer).gameObject.SetActive(!hasPlayer);
	}
}
