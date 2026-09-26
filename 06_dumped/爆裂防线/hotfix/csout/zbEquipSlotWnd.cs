using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEquipSlotWnd : zbTabWnd
{
	private Button m_btnTouchTest;

	private VisualElement m_bg;

	private VisualElement m_bgSub;

	private VisualElement m_bgFrame;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Button m_btnIcon;

	private Button m_btnLock;

	private Label m_lbName;

	private Label m_lbLevel;

	private Label m_lbSubLevel;

	private Button m_btnUpgrade;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private zbEquipSlotTabGem m_moduleGem;

	private zbEquipSlotTabEquip m_moduleEquip;

	private EquipSlotInfo slotInfo;

	private EquipSlotDetail m_data;

	private EEquipType m_equipType;

	public bool m_visUpgrade = true;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_bgSub = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_sub", (string)null);
		m_bgFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_frame", (string)null);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>(m_bgFrame, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_bgFrame, "icon", (string)null);
		m_btnIcon = UQueryExtensions.Q<Button>(m_bgFrame, "btn_icon", (string)null);
		m_btnLock = UQueryExtensions.Q<Button>(m_bgFrame, "btn_lock", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_bgFrame, "lb_name", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_bgFrame, "lb_level", (string)null);
		m_lbSubLevel = UQueryExtensions.Q<Label>(m_bgFrame, "lb_subLevel", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_detail", (string)null);
		m_tabButtons[0] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn0", (string)null);
		m_tabButtons[1] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn1", (string)null);
		m_tabLayers[0] = UQueryExtensions.Q<VisualElement>(m_RootElement, "tab0", (string)null);
		m_tabLayers[1] = UQueryExtensions.Q<VisualElement>(m_RootElement, "tab1", (string)null);
		InitTab(m_tabButtons, m_tabLayers, null);
		AddDark();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnIcon, (Action<ClickEvent>)OnIcon, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLock, (Action<ClickEvent>)OnLock, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action<ClickEvent>)OnUpgrade, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Equip, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Gem, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_EquipSlotInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_EquipLock_Ack, OnEventUpdate);
	}

	public void RefreshCustom(EquipSlotInfo _slotInfo, EEquipType equipType, bool visUpgrade = true)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		m_visUpgrade = visUpgrade;
		if (visUpgrade)
		{
			((VisualElement)m_btnLock).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_bg.style.height = StyleLength.op_Implicit(930f);
			m_bgSub.style.height = StyleLength.op_Implicit(750f);
		}
		else
		{
			((VisualElement)m_btnLock).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_bg.style.height = StyleLength.op_Implicit(860f);
			m_bgSub.style.height = StyleLength.op_Implicit(680f);
		}
		slotInfo = _slotInfo;
		m_equipType = equipType;
		m_data = zbSelf.Instance.equipHandler.GetEquipSlotInfo(equipType, _slotInfo);
		((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visUpgrade));
		zbEquipCsvRecord recordByKey = zbDataConfig.Instance.equipCsvTable.GetRecordByKey(m_data.Equip.EquipTid);
		string resourceBgTex = zbResourceHelper.GetResourceBgTex(m_data.Equip.Quality);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceBgTex);
		if ((Object)(object)val != (Object)null)
		{
			m_bgFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_bgFrame.MarkDirtyRepaint();
		}
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex(m_data.Equip.Quality);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteFrame.MarkDirtyRepaint();
		}
		Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
		if ((Object)(object)val3 != (Object)null)
		{
			m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val3));
			m_spriteIcon.MarkDirtyRepaint();
		}
		if (m_data.Equip.Lock == EBoolState.True)
		{
			((VisualElement)m_btnLock).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_lock.png"));
		}
		else
		{
			((VisualElement)m_btnLock).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_unLock.png"));
		}
		((TextElement)m_lbName).text = recordByKey.name;
		((TextElement)m_lbLevel).text = zbUtil.Language("lang.10037") + m_data.Level;
		((TextElement)m_lbSubLevel).text = m_data.Equip.SubLevel.ToString();
		DoSelectTab();
	}

	public override void DoSelectTab()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/leader/leader_tabbg.png");
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/leader/leader_tab.png");
		switch (m_currTab)
		{
		case 0:
			if (m_moduleGem == null)
			{
				m_moduleGem = new zbEquipSlotTabGem();
				m_moduleGem.OnEnable(this, m_tabLayers[0]);
			}
			m_moduleGem.Refresh(slotInfo, m_equipType);
			((VisualElement)m_tabButtons[0]).style.backgroundImage = new StyleBackground(val2);
			((VisualElement)m_tabButtons[1]).style.backgroundImage = new StyleBackground(val);
			break;
		case 1:
			if (m_moduleEquip == null)
			{
				m_moduleEquip = new zbEquipSlotTabEquip();
				m_moduleEquip.OnEnable(this, m_tabLayers[1]);
			}
			m_moduleEquip.Refresh(m_data);
			((VisualElement)m_tabButtons[0]).style.backgroundImage = new StyleBackground(val);
			((VisualElement)m_tabButtons[1]).style.backgroundImage = new StyleBackground(val2);
			break;
		}
	}

	public void OnIcon(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowTip(EResourceType.Equip, m_data.Equip.EquipTid, 0L);
	}

	public void OnLock(ClickEvent evt)
	{
		if (m_data.Equip.Lock == EBoolState.False)
		{
			zbWsMsgHandler.Instance.EquipSlot_EquipLock_Req(m_data.EquipType, EBoolState.True);
		}
		else
		{
			zbWsMsgHandler.Instance.EquipSlot_EquipLock_Req(m_data.EquipType, EBoolState.False);
		}
	}

	public void OnUpgrade(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowEquipUpgrade(m_data.EquipType);
	}

	private void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(zbServerData.sGameData.EquipSlotInfo, m_data.EquipType);
	}
}
