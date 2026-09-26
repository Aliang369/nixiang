using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonRankWnd : zbTabWnd
{
	private zbSpineScene m_spineScene;

	private GameObject m_sceneBg;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private zbSeasonRankTabMember m_moduleMember;

	private zbSeasonRankTabForce m_moduleForce;

	private zbPlayerMechaAvatar[] m_avatars = new zbPlayerMechaAvatar[3];

	private Button m_btnClose;

	private Button m_btnReward;

	private Sprite m_cachedBtnSpriteSel;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_sceneBg = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene").Find("SceneBg")).gameObject;
		m_btnReward = UQueryExtensions.Q<Button>(m_RootElement, "btn_reward", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReward, (Action)OnReward, "", (TrickleDown)0);
		for (int i = 0; i < 3; i++)
		{
			m_avatars[i] = ((Component)m_GameObjectSceneRoot.transform.Find("top" + (i + 1))).GetComponent<zbPlayerMechaAvatar>();
		}
		for (int j = 0; j < 2; j++)
		{
			m_tabLayers[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + j, (string)null);
			m_tabButtons[j] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + j, (string)null);
		}
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_cachedBtnSpriteSel = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_tabse.png");
		InitTab(m_tabButtons, m_tabLayers, null);
	}

	public void RefreshCustom(int tabIndex = 0)
	{
	}

	public override void DoSelectTab()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		base.DoSelectTab();
		for (int i = 0; i < m_tabButtons.Length; i++)
		{
			((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground((i == m_currTab) ? m_cachedBtnSpriteSel : null);
		}
		for (int j = 0; j < 3; j++)
		{
			((Component)m_avatars[j]).gameObject.SetActive(false);
		}
		switch (m_currTab)
		{
		case 0:
			m_spineScene.SetSceneBg("ui/image/season/seasonRank_force.jpg");
			m_sceneBg.transform.position = new Vector3(0f, 0f, 100f);
			zbUtil.RefreshSpine(m_spineScene.skeletonAnimation, "animation/ui/paihangbang_bg3/paihangbang_bg3_SkeletonData.asset");
			if (m_moduleForce == null)
			{
				m_moduleForce = new zbSeasonRankTabForce();
				m_moduleForce.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleForce.Refresh();
			break;
		case 1:
		{
			m_spineScene.SetSceneBg("ui/image/season/season_selfRank_bg.png");
			m_sceneBg.transform.position = new Vector3(0f, 2.22f, 100f);
			zbUtil.RefreshSpine(m_spineScene.skeletonAnimation, "animation/ui/paihangbang_bg3/paihangbang_bg3_SkeletonData.asset");
			if (m_moduleMember == null)
			{
				m_moduleMember = new zbSeasonRankTabMember();
				m_moduleMember.OnEnable(this, m_tabLayers[m_currTab]);
			}
			for (int k = 0; k < 3; k++)
			{
				((Component)m_avatars[k]).gameObject.SetActive(true);
			}
			m_moduleMember.Refresh();
			break;
		}
		}
	}

	private void OnReward()
	{
		zbUIMgr.Instance.ShowSeasonRankReward();
	}

	private void OnClose()
	{
		Hide();
	}
}
