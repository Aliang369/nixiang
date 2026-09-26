using System;
using Spine.Unity;
using UnityEngine;

public class zbScrollScene : MonoBehaviour
{
	private Transform bg1;

	private Transform bg2;

	private float bgHeight;

	private Sprite spritebg1;

	private Sprite spritebg2;

	public void Load(int sceneId)
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		bg1 = ((Component)((Component)this).transform.Find("bg1")).transform;
		bg2 = ((Component)((Component)this).transform.Find("bg2")).transform;
		zbSceneCsvRecord recordByKey = zbDataConfig.Instance.sceneCsvTable.GetRecordByKey(sceneId);
		if (recordByKey != null)
		{
			SpriteRenderer component = ((Component)bg1).GetComponent<SpriteRenderer>();
			SpriteRenderer component2 = ((Component)bg2).GetComponent<SpriteRenderer>();
			ReleaseSprites();
			spritebg1 = zbAssetLoader.Instance.LoadSpriteAsset("animation/scene/scene_fight/" + recordByKey.path + "/bg1.jpg");
			spritebg2 = zbAssetLoader.Instance.LoadSpriteAsset("animation/scene/scene_fight/" + recordByKey.path + "/bg2.jpg");
			SetSceneBg(component, spritebg1);
			SetSceneBg(component2, spritebg2);
			if (zbCombatSystem.Instance != null)
			{
				zbCombatSystem.Instance.bMonsterNeedWaterEffect = recordByKey.needWaterEffect == 1;
			}
		}
		Bounds bounds = ((Renderer)((Component)bg1).GetComponent<SpriteRenderer>()).bounds;
		bgHeight = ((Bounds)(ref bounds)).size.y;
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		bg1.Translate(Vector3.down * zbCombatSystem.Instance.scrollSpeed * Time.deltaTime);
		bg2.Translate(Vector3.down * zbCombatSystem.Instance.scrollSpeed * Time.deltaTime);
		zbCombatSystem.Instance.scrollTotalDistance += zbCombatSystem.Instance.scrollSpeed * Time.deltaTime * 100f;
		zbCombatSystem.Instance.scrollDistance += zbCombatSystem.Instance.scrollSpeed * Time.deltaTime * 100f;
		if (zbCombatSystem.Instance.m_isCelebrating)
		{
			zbCombatSystem.Instance.celebrateDistance = Math.Max(zbCombatSystem.Instance.celebrateDistance - zbCombatSystem.Instance.scrollSpeed * Time.deltaTime * 100f, 0f);
			if (zbCombatSystem.Instance.celebrateDistance <= 0f || (Object)(object)zbCombatSystem.Instance.combatData.monsters.GetClosestBossMonster(10f) != (Object)null)
			{
				zbCombatSystem.Instance.m_isCelebrating = false;
				((Component)(zbUIMgr.Instance.GetWndById(EWndId.Combat) as zbCombatWnd).m_endlessHordePanel.m_CelebrateAnim).gameObject.SetActive(false);
				zbCombatSystem.Instance.scrollSpeed = (float)zbCombatSystem.Instance.carMaxSpeed / 100f;
			}
		}
		else
		{
			zbCombatSystem.Instance.celebrateDistance = Math.Min(zbCombatSystem.Instance.celebrateDistance + Time.deltaTime * zbCombatSystem.Instance.celebrateTimeEff, zbCombatSystem.Instance.celebrateMaxDistance);
		}
		if (bg1.position.y <= 0f - bgHeight)
		{
			Transform obj = bg1;
			obj.position += new Vector3(0f, 2f * bgHeight, 0f);
		}
		if (bg2.position.y <= 0f - bgHeight)
		{
			Transform obj2 = bg2;
			obj2.position += new Vector3(0f, 2f * bgHeight, 0f);
		}
	}

	public void SetVisible(bool flag)
	{
		((Component)this).gameObject.SetActive(flag);
		zbCombatSystem.Instance.scrollSpeed = 0f;
	}

	public void SetSceneBg(SpriteRenderer spriteBg, Sprite sprite)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)spriteBg != (Object)null && (Object)(object)sprite != (Object)null)
		{
			spriteBg.sprite = sprite;
			((Component)spriteBg).transform.position = new Vector3(((Component)spriteBg).transform.position.x, ((Component)spriteBg).transform.position.y, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z + zbCameraHelper.Instance.mainCamera.farClipPlane - 100f);
		}
	}

	public void SetSkeletonDataAsset(SkeletonAnimation skeletonAnimation, string spinePath)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			SkeletonDataAsset val = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(spinePath);
			if ((Object)(object)val != (Object)null)
			{
				((SkeletonRenderer)skeletonAnimation).skeletonDataAsset = val;
				((SkeletonRenderer)skeletonAnimation).Initialize(true, false);
				skeletonAnimation.AnimationState.SetAnimation(0, "animation", true);
				((Component)skeletonAnimation).transform.position = new Vector3(((Component)skeletonAnimation).transform.position.x, ((Component)skeletonAnimation).transform.position.y, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z + zbCameraHelper.Instance.mainCamera.nearClipPlane + 0.1f);
			}
		}
	}

	public void ReleaseSprites()
	{
		if ((Object)(object)spritebg1 != (Object)null)
		{
			Object.Destroy((Object)(object)spritebg1);
			spritebg1 = null;
		}
		if ((Object)(object)spritebg2 != (Object)null)
		{
			Object.Destroy((Object)(object)spritebg2);
			spritebg2 = null;
		}
	}

	private void OnDestroy()
	{
		ReleaseSprites();
	}
}
