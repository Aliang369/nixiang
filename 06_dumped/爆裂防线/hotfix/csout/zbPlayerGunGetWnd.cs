using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerGunGetWnd : zbBaseWnd
{
	private SkeletonAnimation m_spineGun;

	private Label m_lbTitle;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private VisualElement m_spriteSkilll;

	private Label m_lbDesc;

	private Button m_btnTouchTest;

	private Button m_btnShare;

	private int m_id;

	protected override void OnEnable()
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineGun = ((Component)m_GameObjectSceneRoot.transform.Find("gunAnimation")).GetComponent<SkeletonAnimation>();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteSkilll = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnShare = UQueryExtensions.Q<Button>(m_RootElement, "btn_share", (string)null);
		((VisualElement)m_btnShare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbChannelUtil.IsOpenShare()));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShare, (Action)OnShare, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(int id)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		m_id = id;
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			zbUtil.RefreshSpine(m_spineGun, recordByKey.frontBone + "_SkeletonData.asset", "animation", loop: true);
			((TextElement)m_lbName).text = recordByKey.name;
			m_spriteSkilll.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon));
			if (recordByKey.mod2 > 0)
			{
				string text = zbUtil.Language("lang.11155") + ": " + zbUtil.Language(recordByKey.desc2);
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.11154") + ": " + recordByKey.desc + "\n<color=#FF7173>" + text + "</color>";
			}
			else
			{
				((TextElement)m_lbDesc).text = recordByKey.desc;
			}
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
		zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.10596"), delegate(bool isOK)
		{
			if (isOK)
			{
				zbWsMsgHandler.Instance.Gun_Set_Req(m_id);
			}
		});
	}

	private void OnShare()
	{
		zbSdkHelper.DoShare();
	}
}
