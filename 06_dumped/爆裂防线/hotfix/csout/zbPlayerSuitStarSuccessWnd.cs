using System;
using System.Collections;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerSuitStarSuccessWnd : zbBaseWnd
{
	private zbPlayerAvatar m_spineAvatar;

	private Label m_lbTitle;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[5];

	private VisualElement m_skillNormal0;

	private VisualElement m_skillNormal1;

	private VisualElement m_skillHigh0;

	private VisualElement m_skillHigh1;

	private Label m_lbAttr;

	private Label m_lbLastValue;

	private Label m_lbCurValue;

	private Button m_btnTouchTest;

	private Button m_btnShare;

	protected override void OnEnable()
	{
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "star" + i, (string)null);
		}
		m_skillNormal0 = UQueryExtensions.Q<VisualElement>(m_RootElement, "normal_skill0", (string)null);
		m_skillNormal1 = UQueryExtensions.Q<VisualElement>(m_RootElement, "normal_skill1", (string)null);
		m_skillHigh0 = UQueryExtensions.Q<VisualElement>(m_RootElement, "high_skill0", (string)null);
		m_skillHigh1 = UQueryExtensions.Q<VisualElement>(m_RootElement, "high_skill1", (string)null);
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
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(id);
		PlayerSuitDetail playerSuitDetail = zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(id);
		if (recordByKey != null && playerSuitDetail != null)
		{
			((TextElement)m_lbName).text = recordByKey.name;
			m_spineAvatar.RefreshCustom(recordByKey.avatarId, zbSelf.Instance.GetGunId());
			bool flag = recordByKey.grade == 2;
			m_skillNormal0.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			m_skillNormal1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			m_skillHigh0.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			m_skillHigh1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			(flag ? UQueryExtensions.Q<VisualElement>(m_skillHigh0, "bg_skill0", (string)null) : UQueryExtensions.Q<VisualElement>(m_skillNormal0, "bg_skill0", (string)null)).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon));
			(flag ? UQueryExtensions.Q<VisualElement>(m_skillHigh1, "bg_skill1", (string)null) : UQueryExtensions.Q<VisualElement>(m_skillNormal1, "bg_skill1", (string)null)).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon));
			if (flag)
			{
				zbSelf.Instance.suitGunHandler.SetSuperSuitEnterAnimation(m_spineAvatar);
			}
			zbUIEffect.SetStars(m_stars, playerSuitDetail.Star);
			zbPlayerSuitUpgradeCsvRecord suitStarUpgrade = zbSelf.Instance.suitGunHandler.GetSuitStarUpgrade(id, playerSuitDetail.Star - 1);
			zbPlayerSuitUpgradeCsvRecord suitStarUpgrade2 = zbSelf.Instance.suitGunHandler.GetSuitStarUpgrade(id, playerSuitDetail.Star);
			((TextElement)m_lbAttr).text = zbUtil.Language("lang.10798");
			((TextElement)m_lbLastValue).text = "+" + suitStarUpgrade.atk;
			((TextElement)m_lbCurValue).text = "+" + suitStarUpgrade2.atk;
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
