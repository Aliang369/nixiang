using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEncyclopediaWnd : zbTabWnd
{
	private zbSpineScene m_spineScene;

	private Button m_btnClose;

	private VisualElement[] m_panels = (VisualElement[])(object)new VisualElement[2];

	private Button[] m_tabs = (Button[])(object)new Button[2];

	private VisualElement[] m_notifys = (VisualElement[])(object)new VisualElement[2];

	private zbMonsterWnd m_moduleMonster;

	public zbModBook m_moduleModBook;

	private Sprite[] m_unSelect = (Sprite[])(object)new Sprite[2];

	private Sprite[] m_select = (Sprite[])(object)new Sprite[2];

	private bool monsterHasRed;

	private bool skillHasRed;

	public bool SkillHasRed => skillHasRed;

	protected override void OnEnable()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_panels[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabs[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab" + i, (string)null);
			m_notifys[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabs[i], (Vector2?)new Vector2(10f, 7f), 1f, true);
		}
		m_unSelect[0] = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/encyclopedia/tab1_0.png");
		m_select[0] = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/encyclopedia/tab1_1.png");
		m_unSelect[1] = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/encyclopedia/tab0_0.png");
		m_select[1] = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/encyclopedia/tab0_1.png");
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Handbook, RefreshUI);
		m_gameEevent.RegisterEvent(EGameEvent.Event_MonsterActive_Ack, RefreshUI);
		m_gameEevent.RegisterEvent(EGameEvent.Event_ModActive_Ack, RefreshUI);
		InitTab(m_tabs, m_panels, OnSelect);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
	}

	private void RefreshUI(object obj)
	{
		UpdateNotify();
		SelectTab(m_currTab);
	}

	public void RefreshCustom(int tab)
	{
		base.Refresh();
		UpdateNotify();
		SelectTab(tab);
	}

	private void UpdateNotify()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		monsterHasRed = zbSelf.Instance.handbookHandler.HasRedPoint();
		m_notifys[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!monsterHasRed));
		skillHasRed = zbSelf.Instance.handbookHandler.HasModRedPoint();
		m_notifys[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!skillHasRed));
		((VisualElement)m_tabs[0]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.HandbookMod) ? 1 : 0));
		((VisualElement)m_tabs[1]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.HandbookMod) ? 1 : 0));
	}

	public override void DoSelectTab()
	{
		switch (m_currTab)
		{
		case 0:
			if (m_moduleMonster == null)
			{
				m_moduleMonster = new zbMonsterWnd();
				m_moduleMonster.OnEnable(this, m_panels[m_currTab]);
			}
			m_spineScene.SetSceneBg("ui/image/encyclopedia/monsterbook/monsterBook_banner.jpg");
			m_spineScene.SetSkeletonDataAsset("animation/ui/bosstujian_bg/bosstujian_bg_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			m_moduleMonster.Refresh();
			break;
		case 1:
			if (m_moduleModBook == null)
			{
				m_moduleModBook = new zbModBook();
				m_moduleModBook.OnEnable(this, m_panels[m_currTab]);
			}
			m_spineScene.SetSceneBg("ui/image/encyclopedia/banner/skillType_banner.jpg");
			m_spineScene.SetSkeletonDataAsset("animation/ui/guaiwutujian_bg/guaiwutujian_bg_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			m_moduleModBook.Refresh();
			break;
		}
	}

	private void OnSelect(int index)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 2; i++)
		{
			Sprite val = ((index == i) ? m_select[i] : m_unSelect[i]);
			((VisualElement)m_tabs[i]).style.backgroundImage = new StyleBackground(val);
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
