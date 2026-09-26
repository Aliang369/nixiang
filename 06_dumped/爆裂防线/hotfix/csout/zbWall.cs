using System.Collections;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbWall : MonoBehaviour
{
	private SkeletonAnimation m_spineDoor;

	private SkeletonAnimation m_spineDengpai;

	private SpriteRenderer m_spriteWall;

	protected ESortMainLayerType m_sortType = ESortMainLayerType.Door;

	private bool m_isDengpaiPlaying;

	private void Awake()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		m_spineDoor = ((Component)((Component)this).transform.Find("wall_door")).GetComponent<SkeletonAnimation>();
		m_spineDengpai = ((Component)((Component)this).transform.Find("wall_dengpai")).GetComponent<SkeletonAnimation>();
		((SkeletonRenderer)m_spineDoor).updateWhenInvisible = (UpdateMode)0;
		m_spriteWall = ((Component)((Component)this).transform.Find("icon_wall")).GetComponent<SpriteRenderer>();
		zbSortMainMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
	}

	public void Refresh()
	{
		((Component)m_spriteWall).gameObject.SetActive(!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.WallDefense));
	}

	public Vector3 GetLeftDoorPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3(-3.3f, -1.75f, 0f);
	}

	public Vector3 GetRightDoorPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3(-2.8f, -1.75f, 0f);
	}

	public IEnumerator OpenDoor()
	{
		m_spineDoor.AnimationState.SetAnimation(0, "left_anim_open", false);
		yield return (object)new WaitForSeconds(0.5f);
	}

	public IEnumerator CloseDoor(float delayTime = 0f)
	{
		TrackEntry val = m_spineDoor.AnimationState.SetAnimation(0, "left_anim_close", false);
		if (val != null && delayTime > 0f)
		{
			val.Delay = delayTime;
		}
		yield return null;
	}

	public void PlayDengpaiAction()
	{
		if (!m_isDengpaiPlaying)
		{
			((MonoBehaviour)this).StartCoroutine(PlayDengpaiActionCoroutine());
		}
	}

	private IEnumerator PlayDengpaiActionCoroutine()
	{
		m_isDengpaiPlaying = true;
		m_spineDengpai.AnimationState.SetAnimation(0, "action", false);
		yield return (object)new WaitForSeconds(4.25f);
		m_spineDengpai.AnimationState.SetAnimation(0, "off", true);
		m_isDengpaiPlaying = false;
	}
}
