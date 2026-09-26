using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

public class zbPlayerMechaAvatar : MonoBehaviour
{
	private GameObject m_nodeMecha;

	private SkeletonAnimation m_spineMecha;

	[NonSerialized]
	public zbPlayerAvatar m_spinePlayer;

	private bool hasMecha;

	private bool isInitialized;

	private void Awake()
	{
		InitializeAvatar();
	}

	private void InitializeAvatar()
	{
		if (!isInitialized)
		{
			m_nodeMecha = ((Component)((Component)this).transform.Find("mecha")).gameObject;
			m_spineMecha = ((Component)m_nodeMecha.transform.Find("mechaAvatar")).GetComponent<SkeletonAnimation>();
			m_spinePlayer = ((Component)((Component)this).transform.Find("player")).GetComponent<zbPlayerAvatar>();
			isInitialized = true;
		}
	}

	public void RefreshCustom(int avatarId, int gunAvatarId, int MechaId)
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		InitializeAvatar();
		m_spinePlayer.RefreshCustom(avatarId, gunAvatarId);
		m_spinePlayer.RandomAnimation();
		hasMecha = MechaId > 0;
		if (hasMecha)
		{
			zbMechaCsvRecord recordByKey = zbDataConfig.Instance.mechaCsvTable.GetRecordByKey(MechaId);
			if (recordByKey != null)
			{
				zbMechaAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
				if (recordByKey2 != null)
				{
					zbUtil.RefreshSpine(m_spineMecha, recordByKey2.fightBone + "_SkeletonData.asset", "idle", loop: true);
				}
				((Component)m_spineMecha).transform.localScale = new Vector3(0.75f * recordByKey.scale1, 0.75f * recordByKey.scale1, 0.75f * recordByKey.scale1);
			}
			((Component)m_spinePlayer).transform.position = m_nodeMecha.transform.position + new Vector3(1f, -0.2f, 0f);
			m_nodeMecha.SetActive(true);
		}
		else
		{
			((Component)m_spinePlayer).transform.position = m_nodeMecha.transform.position + new Vector3(0.4f, -0.2f, 0f);
			m_nodeMecha.SetActive(false);
		}
		if (zbSelf.Instance.suitGunHandler.SetSuperSuitEnterAnimation(m_spinePlayer) > 0f)
		{
			HideGunThenShow(5f);
		}
	}

	public void HideGunThenShow(float delay)
	{
		((MonoBehaviour)this).StopCoroutine("CoHideGunThenShow");
		((MonoBehaviour)this).StartCoroutine(CoHideGunThenShow(delay));
	}

	private IEnumerator CoHideGunThenShow(float delay)
	{
		m_nodeMecha.SetActive(false);
		yield return (object)new WaitForSeconds(delay);
		if (hasMecha)
		{
			m_nodeMecha.SetActive(true);
		}
	}

	public void RefreshRankPosition(int rankIndex, int MechaId)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (rankIndex == 2)
		{
			if (MechaId > 0)
			{
				((Component)m_spinePlayer).transform.position = m_nodeMecha.transform.position + new Vector3(-1f, -0.2f, 0f);
			}
			else
			{
				((Component)m_spinePlayer).transform.position = m_nodeMecha.transform.position + new Vector3(-0.4f, -0.2f, 0f);
			}
		}
	}

	public void SetScale(Vector2 vector2)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = Vector2.op_Implicit(vector2);
	}
}
