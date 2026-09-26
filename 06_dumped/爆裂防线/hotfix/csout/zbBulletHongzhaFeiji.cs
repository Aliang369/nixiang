using System.Collections;
using UnityEngine;

public class zbBulletHongzhaFeiji : zbSkillBase
{
	private GameObject feijiGameObject;

	private Vector3 targetP;

	private float feijiSpeed = 12f;

	protected override void OnInited()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		triggerEndMod = false;
		feijiGameObject = zbUtil.FindGameObjectRecursive(((Component)this).transform, "SkeletonFeiji");
		feijiGameObject.SetActive(false);
		zbAudioMgr.Instance.PlayAudio("audio/combat/kongtouhongzha_fire.ogg");
		targetP = ((Component)this).transform.position;
		((MonoBehaviour)this).StartCoroutine(EnableColliderAfterDelay());
		ReturnToPool(5f);
	}

	protected override void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		((Component)this).transform.Translate(Vector3.up * feijiSpeed * Time.deltaTime, (Space)0);
	}

	private IEnumerator EnableColliderAfterDelay()
	{
		yield return null;
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, -9f, ((Component)this).transform.position.z);
		feijiGameObject.SetActive(true);
		yield return (object)new WaitForSeconds(16f / feijiSpeed);
		zbSkillSystem.Instance.CreateSkill("prefab/combat/hero/hongzha/bulletHongzha.prefab", hero, targetP, Vector2.up, mainMonsterUUID);
		if (zbUtil.Roll0to1(hero.team.modIndex2061.roll0to1))
		{
			for (int i = 0; i < hero.team.modIndex2061.skillCount; i++)
			{
				yield return (object)new WaitForSeconds(0.1f);
				zbSkillSystem.Instance.CreateSkill("prefab/combat/hero/hongzha/bulletHongzha.prefab", hero, targetP, Vector2.up, mainMonsterUUID);
			}
		}
	}
}
