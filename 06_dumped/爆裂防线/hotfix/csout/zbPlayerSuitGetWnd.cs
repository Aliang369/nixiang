using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerSuitGetWnd : zbBaseWnd
{
	private zbPlayerAvatar m_spineAvatar;

	private Label m_lbTitle;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private VisualElement m_skillNormal;

	private VisualElement m_skillHigh;

	private Label m_lbSkill;

	private Label m_lbDesc;

	private Button m_btnTouchTest;

	private Button m_btnShare;

	private int m_id;

	protected override void OnEnable()
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_skillNormal = UQueryExtensions.Q<VisualElement>(m_RootElement, "normal_skill", (string)null);
		m_skillHigh = UQueryExtensions.Q<VisualElement>(m_RootElement, "high_skill", (string)null);
		m_lbSkill = UQueryExtensions.Q<Label>(m_RootElement, "lb_skill", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnShare = UQueryExtensions.Q<Button>(m_RootElement, "btn_share", (string)null);
		((VisualElement)m_btnShare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbChannelUtil.IsOpenShare()));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShare, (Action)OnShare, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(int id)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		m_id = id;
		zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			m_spineAvatar.RefreshCustom(recordByKey.avatarId, zbSelf.Instance.GetGunId());
			((TextElement)m_lbName).text = zbUtil.Language(recordByKey.name);
			((TextElement)m_lbSkill).text = zbUtil.Language(recordByKey.skillName);
			((TextElement)m_lbDesc).text = zbUtil.Language(recordByKey.skillDesc1);
			bool flag = recordByKey.grade == 2;
			m_skillNormal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			m_skillHigh.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			(flag ? UQueryExtensions.Q<VisualElement>(m_skillHigh, "bg_skill", (string)null) : UQueryExtensions.Q<VisualElement>(m_skillNormal, "bg_skill", (string)null)).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon));
			if (flag)
			{
				zbSelf.Instance.suitGunHandler.SetSuperSuitEnterAnimation(m_spineAvatar);
			}
			m_spriteBg.style.opacity = StyleFloat.op_Implicit(0f);
			((MonoBehaviour)this).StartCoroutine(PlayAnimationSequence());
		}
	}

	private IEnumerator PlayAnimationSequence()
	{
		PlayAnimation();
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

	public void PlayAnimation()
	{
		if ((Object)(object)m_spineAvatar != (Object)null && !(m_spineAvatar.GetAnimationName(0) == "show"))
		{
			m_spineAvatar.SetAnimation(0, "show", loop: false);
			m_spineAvatar.AddAnimation(0, "idle", loop: true);
		}
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
		zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.10596"), delegate(bool isOK)
		{
			if (isOK)
			{
				zbWsMsgHandler.Instance.Suit_Set_Req(m_id);
			}
		});
	}

	private void OnShare()
	{
		zbSdkHelper.DoShare();
	}
}
