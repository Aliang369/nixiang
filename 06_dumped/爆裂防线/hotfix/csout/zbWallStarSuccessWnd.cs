using System;
using System.Collections;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallStarSuccessWnd : zbBaseWnd
{
	private zbSpineScene m_spineScene;

	private zbWallAvatar wallAvatar;

	private Label m_lbTitle;

	private VisualElement m_spriteBg;

	private Label m_lbName;

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[5];

	private VisualElement m_curSkill;

	private VisualElement m_nextSkill;

	private Label m_lbAttr;

	private Label m_lbLastValue;

	private Label m_lbCurValue;

	private Button m_btnTouchTest;

	private Button m_btnShare;

	protected override void OnEnable()
	{
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		wallAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("wallAvatar")).GetComponent<zbWallAvatar>();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_spriteBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "star" + i, (string)null);
		}
		m_curSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill0", (string)null);
		m_nextSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_skill1", (string)null);
		m_lbAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr", (string)null);
		m_lbLastValue = UQueryExtensions.Q<Label>(m_RootElement, "lb_lastValue", (string)null);
		m_lbCurValue = UQueryExtensions.Q<Label>(m_RootElement, "lb_curValue", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnShare = UQueryExtensions.Q<Button>(m_RootElement, "btn_share", (string)null);
		((VisualElement)m_btnShare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbChannelUtil.IsOpenShare()));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShare, (Action)OnShare, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(int wallId)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		zbWallCsvRecord record = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wallId);
		WallDetail detail = zbSelf.Instance.wallHandler.GetWallDetail(wallId);
		if (record != null && detail != null)
		{
			((TextElement)m_lbName).text = record.name;
			m_curSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(record.skillIcon));
			m_nextSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(record.skillIcon));
			if (record.backGround != "")
			{
				m_spineScene.skeletonObject.SetActive(true);
				m_spineScene.SetSceneBg(record.backGround);
				m_spineScene.SetSkeletonDataAsset(record.backEffect + "_SkeletonData.asset", defaultZ: false);
				m_spineScene.SetAnimation("animation", isLoop: true);
			}
			else
			{
				m_spineScene.skeletonObject.SetActive(false);
				m_spineScene.SetSceneBg("ui/image/wall/wall_bg.jpg");
			}
			wallAvatar.RefreshCustom(record.id);
			zbUIEffect.SetStars(m_stars, detail.Star);
			zbWallStarUpgradeCsvRecord zbWallStarUpgradeCsvRecord2 = zbDataConfig.Instance.wallStarUpgradeCsvTable.GetRecords().records.FirstOrDefault((zbWallStarUpgradeCsvRecord r) => r.id == record.starUpgradeId && r.star == detail.Star - 1);
			zbWallStarUpgradeCsvRecord zbWallStarUpgradeCsvRecord3 = zbDataConfig.Instance.wallStarUpgradeCsvTable.GetRecords().records.FirstOrDefault((zbWallStarUpgradeCsvRecord r) => r.id == record.starUpgradeId && r.star == detail.Star);
			((TextElement)m_lbAttr).text = zbUtil.Language("lang.10839");
			((TextElement)m_lbLastValue).text = "+" + zbWallStarUpgradeCsvRecord2.damageImmunity;
			((TextElement)m_lbCurValue).text = "+" + zbWallStarUpgradeCsvRecord3.damageImmunity;
			if (record.quality >= 6)
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
	}

	private void OnShare()
	{
		zbSdkHelper.DoShare();
	}
}
