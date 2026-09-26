using System.Collections;
using Spine;
using UnityEngine;

public class zbWall5021 : zbWallSkillBase
{
	protected override void OnInited()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		ignoreAnimation = true;
		base.OnInited();
		((Component)this).transform.position = new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY, ((Component)this).transform.position.z);
		ignoreMonsters.Clear();
		((MonoBehaviour)this).StartCoroutine(ActionEnter());
	}

	public IEnumerator ActionEnter()
	{
		zbAudioMgr.Instance.PlayAudio("audio/ui/chuchang.mp3");
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "enter", false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		val = m_skeletonAnimation.AnimationState.SetAnimation(0, "exit", false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		ReturnToPool();
	}
}
