using System.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSnowVeilMap
{
	private zbSnowVeilWnd m_wndParent;

	private zbPlayerAvatar m_playerAvatar;

	public SpriteRenderer[] m_spriteRenderers = (SpriteRenderer[])(object)new SpriteRenderer[28];

	private zbSnowVeilGetItem m_getPropItem;

	private int m_avatarId;

	private int m_gunId;

	private int m_curIndex;

	private int m_targetIndex;

	private bool m_isFirstIn = true;

	private Coroutine m_coroutine;

	private float m_runSpeed;

	public void OnEnable(zbSnowVeilWnd wndParent, zbPlayerAvatar playerAvatar, Transform landsObject, VisualElement root)
	{
		m_wndParent = wndParent;
		m_playerAvatar = playerAvatar;
		for (int i = 0; i < 28; i++)
		{
			m_spriteRenderers[i] = ((Component)landsObject.Find("land" + i)).GetComponent<SpriteRenderer>();
		}
		m_getPropItem = UQueryExtensions.Q<zbSnowVeilGetItem>(root, "getItem", (string)null);
		m_getPropItem.OnEnable(m_wndParent, this);
	}

	public void Refresh()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		if (m_avatarId != zbSelf.Instance.GetAvatarId() || m_gunId != zbSelf.Instance.GetGunId())
		{
			m_avatarId = zbSelf.Instance.GetAvatarId();
			m_gunId = zbSelf.Instance.GetGunId();
			m_playerAvatar.RefreshCustom(m_avatarId, m_gunId);
		}
		m_targetIndex = m_wndParent.m_data.RoleState.Index - 1;
		if (m_isFirstIn)
		{
			m_curIndex = m_wndParent.m_data.RoleState.Index - 1;
			Vector3 position = ((Component)m_spriteRenderers[m_curIndex]).transform.position;
			position.y -= 0.2f;
			((Component)m_playerAvatar).transform.position = position;
			Vector3 position2 = ((Component)m_spriteRenderers[0]).transform.position;
			if (m_curIndex < 27)
			{
				position2 = ((Component)m_spriteRenderers[m_curIndex + 1]).transform.position;
			}
			SetFacing(((Component)m_playerAvatar).transform.position.x > position2.x);
			RefreshLands();
			if (m_curIndex != 0)
			{
				((Component)m_spriteRenderers[m_curIndex]).gameObject.SetActive(false);
			}
			m_isFirstIn = false;
		}
		else if (m_curIndex != m_targetIndex)
		{
			m_runSpeed = zbRoomDefine.Instance.runSpeed * m_wndParent.m_speed * 1.5f;
			if (m_coroutine != null)
			{
				((MonoBehaviour)m_wndParent).StopCoroutine(m_coroutine);
				m_coroutine = null;
			}
			m_coroutine = ((MonoBehaviour)m_wndParent).StartCoroutine(PlayHeroAvatarWalk());
		}
	}

	private void RefreshLands()
	{
		for (int i = 0; i < 28; i++)
		{
			SnowVeilLandDetail snowVeilLandDetail = null;
			foreach (SnowVeilLandDetail land in m_wndParent.m_data.LandList)
			{
				if (land.Index == i + 1)
				{
					snowVeilLandDetail = land;
					break;
				}
			}
			if (snowVeilLandDetail.LandType == ESnowVeilLandType.EmptyValue)
			{
				((Component)m_spriteRenderers[i]).gameObject.SetActive(false);
				continue;
			}
			((Component)m_spriteRenderers[i]).gameObject.SetActive(true);
			string landIconPath = GetLandIconPath(snowVeilLandDetail);
			m_spriteRenderers[i].sprite = zbAssetLoader.Instance.LoadSpriteAsset(landIconPath);
		}
	}

	private IEnumerator PlayHeroAvatarWalk()
	{
		zbUIMgr.Instance.ShowModel();
		if (m_targetIndex < m_curIndex)
		{
			for (int i = m_curIndex + 1; i < 28; i++)
			{
				Vector3 position = ((Component)m_spriteRenderers[i]).transform.position;
				yield return MoveTo(position);
			}
			for (int i = 0; i < m_targetIndex + 1; i++)
			{
				Vector3 position2 = ((Component)m_spriteRenderers[i]).transform.position;
				yield return MoveTo(position2);
				if (i == 0)
				{
					RefreshLands();
				}
			}
		}
		else
		{
			for (int i = m_curIndex + 1; i < m_targetIndex + 1; i++)
			{
				Vector3 position3 = ((Component)m_spriteRenderers[i]).transform.position;
				yield return MoveTo(position3);
			}
		}
		ShowGetProp();
		if (m_targetIndex != 0)
		{
			((Component)m_spriteRenderers[m_targetIndex]).gameObject.SetActive(false);
		}
		m_curIndex = m_targetIndex;
		zbUIMgr.Instance.HideModel();
	}

	private IEnumerator MoveTo(Vector3 targetPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		targetPos.y -= 0.2f;
		m_playerAvatar.SetAnimation(0, "run", loop: true);
		SetFacing(((Component)m_playerAvatar).transform.position.x > targetPos.x);
		while (Vector3.Distance(((Component)m_playerAvatar).transform.position, targetPos) > 0.01f)
		{
			((Component)m_playerAvatar).transform.position = Vector3.MoveTowards(((Component)m_playerAvatar).transform.position, targetPos, m_runSpeed * Time.deltaTime);
			yield return null;
		}
		m_playerAvatar.SetAnimation(0, "idle", loop: true);
	}

	private void SetFacing(bool left)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localScale = ((Component)m_playerAvatar).transform.localScale;
		localScale.x = (left ? Mathf.Abs(localScale.x) : (0f - Mathf.Abs(localScale.x)));
		((Component)m_playerAvatar).transform.localScale = localScale;
	}

	private string GetLandIconPath(SnowVeilLandDetail landDetail)
	{
		if (landDetail.LandType == ESnowVeilLandType.Item)
		{
			return zbResourceHelper.GetResorceIconPath(landDetail.Item.Type, landDetail.Item.Tid).strIconPath;
		}
		if (landDetail.LandType == ESnowVeilLandType.Turntable)
		{
			return "ui/image/operate/snowVeil/snowVeil_type_turntable.png";
		}
		if (landDetail.LandType == ESnowVeilLandType.Rolling)
		{
			return "ui/image/operate/snowVeil/snowVeil_type_rolling.png";
		}
		if (landDetail.LandType == ESnowVeilLandType.Score)
		{
			return "ui/image/operate/snowVeil/snowVeil_type_score.png";
		}
		if (landDetail.LandType == ESnowVeilLandType.HighScore)
		{
			return "ui/image/operate/snowVeil/snowVeil_type_highScore.png";
		}
		return "";
	}

	private void ShowGetProp()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_wndParent.m_spineBig).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		foreach (SnowVeilLandDetail land in m_wndParent.m_data.LandList)
		{
			if (land.Index == m_targetIndex + 1)
			{
				m_getPropItem.Refresh(land);
				break;
			}
		}
	}
}
