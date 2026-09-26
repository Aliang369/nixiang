using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbSpineScene : MonoBehaviour
{
	private int runSceneId = -1;

	[NonSerialized]
	public SpriteRenderer sceneBg;

	[NonSerialized]
	public SkeletonAnimation skeletonAnimation;

	[NonSerialized]
	public GameObject skeletonObject;

	private Sprite m_sceneSprite;

	private void OnEnable()
	{
		sceneBg = ((Component)((Component)this).transform.Find("SceneBg")).GetComponent<SpriteRenderer>();
		skeletonObject = ((Component)((Component)this).transform.Find("SkeletonAnimation")).gameObject;
		skeletonAnimation = skeletonObject.GetComponent<SkeletonAnimation>();
	}

	public void Load(int id)
	{
		runSceneId = id;
		zbSceneCsvRecord recordByKey = zbDataConfig.Instance.sceneCsvTable.GetRecordByKey(runSceneId);
		string text = ((recordByKey.sceneType == 1) ? ("animation/scene/scene_fight/" + recordByKey.path + "/bg.jpg") : ("animation/scene/scene_fight/" + recordByKey.path + "/bg1.jpg"));
		if (zbCombatSystem.Instance != null)
		{
			zbCombatSystem.Instance.bMonsterNeedWaterEffect = recordByKey.needWaterEffect == 1;
		}
		SetSceneBg(text);
		((Component)skeletonAnimation).gameObject.SetActive(false);
	}

	public void SetSceneBg(string spritePath)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		OnEnable();
		if ((Object)(object)sceneBg != (Object)null)
		{
			ReleaseSprites();
			Sprite val = zbAssetLoader.Instance.LoadSpriteAsset(spritePath);
			if ((Object)(object)val != (Object)null)
			{
				m_sceneSprite = val;
				sceneBg.sprite = m_sceneSprite;
				((Component)sceneBg).transform.position = new Vector3(((Component)sceneBg).transform.position.x, ((Component)sceneBg).transform.position.y, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z + zbCameraHelper.Instance.mainCamera.farClipPlane - 100f);
			}
		}
	}

	public void SetSkeletonDataAsset(string spinePath, bool defaultZ = true, float positionZ = 0f)
	{
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		OnEnable();
		if (!((Object)(object)skeletonAnimation != (Object)null))
		{
			return;
		}
		((Component)skeletonAnimation).gameObject.SetActive(true);
		if (string.IsNullOrEmpty(spinePath))
		{
			((Component)skeletonAnimation).gameObject.SetActive(false);
			return;
		}
		SkeletonDataAsset val = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(spinePath);
		if ((Object)(object)val != (Object)null)
		{
			((SkeletonRenderer)skeletonAnimation).skeletonDataAsset = val;
			((SkeletonRenderer)skeletonAnimation).Initialize(true, false);
			if (defaultZ)
			{
				((Component)skeletonAnimation).transform.position = new Vector3(((Component)skeletonAnimation).transform.position.x, ((Component)skeletonAnimation).transform.position.y, ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z + zbCameraHelper.Instance.mainCamera.nearClipPlane + 0.1f);
			}
			else
			{
				((Component)skeletonAnimation).transform.position = new Vector3(((Component)skeletonAnimation).transform.position.x, ((Component)skeletonAnimation).transform.position.y, positionZ);
			}
		}
	}

	public TrackEntry SetAnimation(string animationName, bool isLoop)
	{
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			return skeletonAnimation.AnimationState.SetAnimation(0, animationName, isLoop);
		}
		return null;
	}

	public void PlaySequenceAnimation(string enterAnimationNale, string loopAnimationNale)
	{
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			skeletonAnimation.AnimationState.ClearTracks();
			skeletonAnimation.AnimationState.SetAnimation(0, enterAnimationNale, false);
			skeletonAnimation.AnimationState.AddAnimation(0, loopAnimationNale, true, 0f);
		}
	}

	public void RefreshSceneBgGray(bool isGray)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (isGray)
		{
			sceneBg.color = Color.gray;
		}
		else
		{
			sceneBg.color = Color.white;
		}
	}

	public void ClearSpine()
	{
		if ((Object)(object)skeletonAnimation != (Object)null)
		{
			if (skeletonAnimation.AnimationState != null)
			{
				skeletonAnimation.AnimationState.ClearTracks();
			}
			((SkeletonRenderer)skeletonAnimation).skeletonDataAsset = null;
		}
		if ((Object)(object)sceneBg != (Object)null)
		{
			sceneBg.sprite = null;
		}
		if ((Object)(object)m_sceneSprite != (Object)null)
		{
			Object.Destroy((Object)(object)m_sceneSprite);
			m_sceneSprite = null;
		}
	}

	private void ReleaseSprites()
	{
		if ((Object)(object)m_sceneSprite != (Object)null)
		{
			Object.Destroy((Object)(object)m_sceneSprite);
			m_sceneSprite = null;
		}
	}

	private void OnDestroy()
	{
		ReleaseSprites();
	}
}
