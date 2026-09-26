using System;
using System.Collections;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerGunStarSuccessWnd : zbBaseWnd
{
	private SkeletonAnimation m_spineGun;

	private Label m_lbTitle;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[5];

	private Label m_lbAttr;

	private Label m_lbLastValue;

	private Label m_lbCurValue;

	private Button m_btnTouchTest;

	private Button m_btnShare;

	protected override void OnEnable()
	{
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineGun = ((Component)m_GameObjectSceneRoot.transform.Find("gunAnimation")).GetComponent<SkeletonAnimation>();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "star" + i, (string)null);
		}
		m_lbAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr", (string)null);
		m_lbLastValue = UQueryExtensions.Q<Label>(m_RootElement, "lb_lastValue", (string)null);
		m_lbCurValue = UQueryExtensions.Q<Label>(m_RootElement, "lb_curValue", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnShare = UQueryExtensions.Q<Button>(m_RootElement, "btn_share", (string)null);
		((VisualElement)m_btnShare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbChannelUtil.IsOpenShare()));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShare, (Action)OnShare, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(int id)
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(id);
		PlayerGunDetail playerGunDetail = zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(id);
		if (recordByKey != null && playerGunDetail != null)
		{
			((TextElement)m_lbName).text = recordByKey.name;
			zbUtil.RefreshSpine(m_spineGun, recordByKey.frontBone + "_SkeletonData.asset", "animation", loop: true);
			zbUIEffect.SetStars(m_stars, playerGunDetail.Star);
			zbPlayerGunUpgradeCsvRecord gunStarUpgrade = zbSelf.Instance.suitGunHandler.GetGunStarUpgrade(id, playerGunDetail.Star - 1);
			zbPlayerGunUpgradeCsvRecord gunStarUpgrade2 = zbSelf.Instance.suitGunHandler.GetGunStarUpgrade(id, playerGunDetail.Star);
			((TextElement)m_lbAttr).text = zbUtil.Language("lang.10798");
			((TextElement)m_lbLastValue).text = "+" + gunStarUpgrade.dmg * 100f + "%";
			((TextElement)m_lbCurValue).text = "+" + gunStarUpgrade2.dmg * 100f + "%";
			m_spriteBg.style.opacity = StyleFloat.op_Implicit(0f);
			((MonoBehaviour)this).StartCoroutine(PlayAnimationSequence());
		}
	}

	private IEnumerator PlayAnimationSequence()
	{
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		float duration = 1f / 6f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			m_spriteBg.style.opacity = StyleFloat.op_Implicit(num);
			yield return null;
		}
		m_spriteBg.style.opacity = StyleFloat.op_Implicit(1f);
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}

	private void OnShare()
	{
		zbSdkHelper.DoShare();
	}
}
