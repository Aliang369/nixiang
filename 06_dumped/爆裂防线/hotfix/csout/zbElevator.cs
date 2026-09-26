using System;
using Spine.Unity;
using UnityEngine;

public class zbElevator : MonoBehaviour
{
	[NonSerialized]
	public zbFloor floor;

	private SpriteRenderer m_spriteRenderer;

	private SkeletonAnimation m_spineAnimation;

	private SkeletonAnimation m_spineDoor;

	private float m_WalkPosGapY = 0.1f;

	protected ESortMainLayerType m_sortType = ESortMainLayerType.Elevator;

	private void Awake()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		Transform val = ((Component)this).transform.Find("visibleObject");
		floor = ((Component)((Component)this).transform.parent).GetComponent<zbFloor>();
		m_spriteRenderer = ((Component)val.Find("elevator")).GetComponent<SpriteRenderer>();
		m_spineAnimation = ((Component)val.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		m_spineDoor = ((Component)val.Find("spineDoor")).GetComponent<SkeletonAnimation>();
		((SkeletonRenderer)m_spineAnimation).updateWhenInvisible = (UpdateMode)0;
		((SkeletonRenderer)m_spineDoor).updateWhenInvisible = (UpdateMode)0;
		zbSortMainMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
	}

	public void Build()
	{
		if (floor.floorIndex >= 0)
		{
			m_spriteRenderer.sprite = zbAssetLoader.Instance.LoadSpriteAsset("ui/image/home/home_elevator1.jpg");
			zbUtil.RefreshSpine(m_spineDoor, "animation/home/room_elevator_1/room_elevator_1_middle_door/room_elevator_1_middle_door_SkeletonData.asset");
		}
		else
		{
			m_spriteRenderer.sprite = zbAssetLoader.Instance.LoadSpriteAsset("ui/image/home/home_elevator2.jpg");
			zbUtil.RefreshSpine(m_spineDoor, "animation/home/room_elevator_2/room_elevator_2_middle_door/room_elevator_2_middle_door_SkeletonData.asset");
		}
		RefreshElevateSpine();
	}

	public void RefreshElevateSpine()
	{
		if (floor.floorIndex >= 0)
		{
			zbUtil.RefreshSpine(m_spineAnimation, "animation/home/room_elevator_1/room_elevator_1/room_elevator_1_SkeletonData.asset", "ambient", loop: true);
		}
		else
		{
			zbUtil.RefreshSpine(m_spineAnimation, "animation/home/room_elevator_2/room_elevator_2/room_elevator_2_SkeletonData.asset", "ambient", loop: true);
		}
	}

	public void OpenDoor()
	{
		if ((Object)(object)m_spineDoor != (Object)null && (Object)(object)((Component)m_spineDoor).gameObject != (Object)null && zbUtil.CheckAnimationNameExist(m_spineDoor, "middle_anim_open"))
		{
			m_spineDoor.AnimationState.SetAnimation(0, "middle_anim_open", false);
		}
	}

	public void CloseDoor()
	{
		if ((Object)(object)m_spineDoor != (Object)null && (Object)(object)((Component)m_spineDoor).gameObject != (Object)null && zbUtil.CheckAnimationNameExist(m_spineDoor, "middle_anim_close"))
		{
			m_spineDoor.AnimationState.SetAnimation(0, "middle_anim_close", false);
		}
	}

	public Vector3 GetCenterPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3(0f, -0.9f + m_WalkPosGapY, 0f);
	}

	public Vector3 GetInsidePosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3(0f, -0.9f + m_WalkPosGapY + 0.15f, 0f);
	}

	private void Update()
	{
	}
}
