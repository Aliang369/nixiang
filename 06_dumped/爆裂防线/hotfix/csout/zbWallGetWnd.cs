using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallGetWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private zbWallAvatar wallAvatar;

	private Label m_lbTitle;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private VisualElement m_spriteSkill;

	private Label m_lbDesc;

	private Button m_btnTouchTest;

	private int m_wallId;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		wallAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("wallAvatar")).GetComponent<zbWallAvatar>();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(int wallId)
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		m_wallId = wallId;
		zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wallId);
		if (recordByKey != null)
		{
			((TextElement)m_lbName).text = recordByKey.name;
			if (recordByKey.backGround != "")
			{
				m_spineScene.skeletonObject.SetActive(true);
				m_spineScene.SetSceneBg(recordByKey.backGround);
				m_spineScene.SetSkeletonDataAsset(recordByKey.backEffect + "_SkeletonData.asset", defaultZ: false);
				m_spineScene.SetAnimation("animation", isLoop: true);
			}
			else
			{
				m_spineScene.skeletonObject.SetActive(false);
				m_spineScene.SetSceneBg("ui/image/wall/wall_bg.jpg");
			}
			wallAvatar.RefreshCustom(recordByKey.id);
			m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon));
			((TextElement)m_lbDesc).text = zbSelf.Instance.wallHandler.GetSkillDesc(wallId, 1);
			if (recordByKey.quality >= 6)
			{
				wallAvatar.SetSuperWallInAnimation(isFirst: true);
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
		zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.10841"), delegate(bool isOK)
		{
			if (isOK)
			{
				zbWsMsgHandler.Instance.Wall_Set_Req(m_wallId);
			}
		});
	}
}
