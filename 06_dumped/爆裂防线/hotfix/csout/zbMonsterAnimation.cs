using Spine.Unity;
using UnityEngine;

public class zbMonsterAnimation
{
	public zbMonster monster;

	public zbSpineAnimation spineAnimation;

	public zbSpineColorChange spineColorChange;

	public Vector2 boxColliderOffset;

	public Vector2 boxColliderSize;

	private zbMonsterSpineMaterialUtil m_monsterSpineMaterialUtil;

	public zbMonsterAnimation()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		boxColliderOffset = new Vector2(0f, 0.5f);
		boxColliderSize = new Vector2(0.7f, 1f);
		base._002Ector();
	}

	public void Init(zbMonster obj)
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		monster = obj;
		spineAnimation = new zbSpineAnimation();
		spineColorChange = new zbSpineColorChange();
		spineAnimation.Init((MonoBehaviour)(object)monster);
		string spinePath = monster.monsterAvatarRecord.fightBone + "_SkeletonData.asset";
		spineAnimation.RefreshSpine(spinePath);
		SkeletonAnimation skeletonAnimation = spineAnimation.skeletonAnimation;
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			((SkeletonRenderer)skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
			SkeletonDataAsset skeletonDataAsset = ((SkeletonRenderer)skeletonAnimation).skeletonDataAsset;
			bool singleSubmesh = (Object)(object)skeletonDataAsset != (Object)null && skeletonDataAsset.atlasAssets != null && skeletonDataAsset.atlasAssets.Length == 1 && (skeletonDataAsset.blendModeMaterials == null || !skeletonDataAsset.blendModeMaterials.RequiresBlendModeMaterials);
			((SkeletonRenderer)skeletonAnimation).singleSubmesh = singleSubmesh;
		}
		spineColorChange.Init(((Component)monster).GetComponentInChildren<SkeletonRenderer>());
		boxColliderSize.x = (float)monster.monsterAvatarRecord.boxWidth / 100f;
		boxColliderSize.y = (float)monster.monsterAvatarRecord.boxHeight / 100f;
		boxColliderOffset.y = boxColliderSize.y / 2f + (float)monster.monsterAvatarRecord.boxOffset / 100f;
		monster.SetBoxColliderSize(boxColliderSize, boxColliderOffset);
		InitMonsterWaterMaterial();
	}

	private void InitMonsterWaterMaterial()
	{
		bool num = zbCombatSystem.Instance.bMonsterNeedWaterEffect && monster.monsterRecord.moveType == 1;
		SkeletonAnimation componentInChildren = ((Component)monster).GetComponentInChildren<SkeletonAnimation>();
		if (num)
		{
			if (m_monsterSpineMaterialUtil == null)
			{
				m_monsterSpineMaterialUtil = new zbMonsterSpineMaterialUtil(componentInChildren);
				m_monsterSpineMaterialUtil.Initialize();
			}
			else
			{
				m_monsterSpineMaterialUtil.ApplyWaterEffect();
			}
		}
		else if (m_monsterSpineMaterialUtil != null)
		{
			m_monsterSpineMaterialUtil.RemoveWaterEffect();
		}
	}

	public void OnDestroy()
	{
		if (m_monsterSpineMaterialUtil != null)
		{
			m_monsterSpineMaterialUtil.Cleanup();
		}
	}

	public void Update()
	{
		spineColorChange.DeltaUpdate();
	}
}
