using UnityEngine;

public class zbFacilityPlayer : MonoBehaviour
{
	private Transform m_root;

	private zbPlayerAvatar m_playerAvatar;

	private Transform m_nodeSelf;

	private int m_cachedAvatarId = -1;

	private int m_cachedGunId = -1;

	private bool m_isCurrentRoom;

	private void Awake()
	{
		m_root = ((Component)this).transform.Find("root");
		m_playerAvatar = ((Component)m_root.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		m_nodeSelf = m_root.Find("playerAvatar").Find("playerAvatarJiantou");
	}

	public void Refresh(zbFacilityRoom room, bool isCurrentRoom = false)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		m_isCurrentRoom = isCurrentRoom;
		((Component)m_nodeSelf).gameObject.SetActive(isCurrentRoom);
		if (Object.op_Implicit((Object)(object)m_playerAvatar))
		{
			int avatarId = zbSelf.Instance.GetAvatarId();
			int gunId = zbSelf.Instance.GetGunId();
			if (!m_isCurrentRoom)
			{
				avatarId = room.roomData.RoomRobot.AvatarId;
				gunId = room.roomData.RoomRobot.GunId;
			}
			if (avatarId != m_cachedAvatarId || gunId != m_cachedGunId)
			{
				m_cachedAvatarId = avatarId;
				m_cachedGunId = gunId;
				m_playerAvatar.RefreshCustom(avatarId, gunId);
			}
		}
		if (m_isCurrentRoom)
		{
			((Component)this).transform.position = room.GetLeftPos();
		}
		else
		{
			((Component)this).transform.position = room.GetCenterPos();
		}
	}

	public void PlayAnimation(string animName, bool loop = true)
	{
		if (Object.op_Implicit((Object)(object)m_playerAvatar))
		{
			m_playerAvatar.SetAnimation(0, animName, loop);
		}
	}

	public void PlayExploreAnimation()
	{
		if (Object.op_Implicit((Object)(object)m_playerAvatar))
		{
			m_playerAvatar.SetAnimation(0, "action", loop: false);
			m_playerAvatar.AddAnimation(0, "idle", loop: true);
		}
	}
}
