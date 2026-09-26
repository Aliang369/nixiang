using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBossUpWnd : zbBaseWnd
{
	private GameObject m_bossUp;

	private Label m_lbText;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bossUp = zbUtil.FindGameObjectNoRecursive(((Component)this).transform, "SpineBossUp");
		m_bossUp.SetActive(false);
		m_lbText = UQueryExtensions.Q<Label>(m_RootElement, "lb_text", (string)null);
	}

	public void RefreshCustom()
	{
		((MonoBehaviour)this).StartCoroutine(PlayBossUpAnim());
	}

	private IEnumerator PlayBossUpAnim()
	{
		if ((Object)(object)m_bossUp == (Object)null)
		{
			yield break;
		}
		yield return null;
		Vector3 position = ((Component)zbCameraHelper.Instance.mainCamera).transform.position;
		m_bossUp.transform.position = new Vector3(position.x, position.y, position.z + 1f);
		SkeletonAnimation bossUpAnimation = m_bossUp.GetComponent<SkeletonAnimation>();
		m_bossUp.SetActive(true);
		bossUpAnimation.AnimationState.SetAnimation(0, "animation", false);
		((TextElement)m_lbText).text = zbUtil.Language("lang.10823");
		while (true)
		{
			TrackEntry current = bossUpAnimation.state.GetCurrent(0);
			if (current == null || current.IsComplete)
			{
				break;
			}
			yield return null;
		}
		m_bossUp.SetActive(false);
		OnClose();
	}

	protected override void UpdateAtShow()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		Vector3 position = ((Component)zbCameraHelper.Instance.mainCamera).transform.position;
		m_bossUp.transform.position = new Vector3(position.x, position.y, position.z + 1f);
	}

	private void OnClose()
	{
		Hide();
	}
}
