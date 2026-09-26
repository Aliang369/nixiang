using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEquipUpgradeWnd : zbTabWnd
{
	private zbSpineScene m_spineScene;

	private Button m_btnClose;

	private Label m_lbEquip;

	private VisualElement m_equipFrame;

	private VisualElement m_equipIcon;

	private Button m_btnEquip;

	private SpineVisualElement m_spineEquip;

	private zbEquipSlotItem[] m_itemEquipSlots = new zbEquipSlotItem[3];

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private VisualElement m_spriteTabSel;

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private VisualElement[] m_notifys = (VisualElement[])(object)new VisualElement[2];

	private zbEquipUpgradeTabLevel m_moduleLevel;

	private zbEquipUpgradeTabEquip m_moduleEquip;

	public EEquipType m_curEquipType = EEquipType.Neuron;

	private EquipSlotDetail m_curEquipSlotDetail;

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < 3; i++)
		{
			m_itemEquipSlots[i] = new zbEquipSlotItem();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "root_top", (string)null);
		m_lbEquip = UQueryExtensions.Q<Label>(val, "lb_equip", (string)null);
		m_equipFrame = UQueryExtensions.Q<VisualElement>(val, "frame_bg", (string)null);
		m_equipIcon = UQueryExtensions.Q<VisualElement>(m_equipFrame, "icon_equip", (string)null);
		m_btnEquip = UQueryExtensions.Q<Button>(m_equipFrame, "btn_equip", (string)null);
		m_spineEquip = UQueryExtensions.Q<SpineVisualElement>(val, "spine", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_itemEquipSlots[i] = UQueryExtensions.Q<zbEquipSlotItem>(m_RootElement, "equipSlot" + i, (string)null);
			m_itemEquipSlots[i].OnEnable(this, (EEquipType)(i + 1));
		}
		m_tabLayers[0] = UQueryExtensions.Q<VisualElement>(m_RootElement, "tab0", (string)null);
		m_tabLayers[1] = UQueryExtensions.Q<VisualElement>(m_RootElement, "tab1", (string)null);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		m_tabButtons[0] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn0", (string)null);
		m_tabButtons[1] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn1", (string)null);
		m_notifys[0] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[0]);
		m_notifys[1] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[1]);
		InitTab(m_tabButtons, m_tabLayers, null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "button_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnEquip, (Action)OnEquipTip, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.EquipSoul
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_EquipSlotInfo, OnEventUpdateEquipSlot);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdateEquipSlot);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdateEquipSlot);
		m_gameEevent.RegisterEvent(EGameEvent.Event_EquipSlotUpgrade_Ack, OnEventUpgradeAck);
	}

	public void RefreshCustom(EEquipType equipTyp, int tabIndex)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		m_curEquipType = equipTyp;
		for (int i = 0; i < 3; i++)
		{
			m_itemEquipSlots[i].Refresh();
		}
		RefreshSelectList(m_curEquipType);
		m_notifys[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.equipHandler.HasEquipSlotUpgrade()));
		m_notifys[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.equipHandler.HasEquipLevelUpgrade()));
		zbSelf.Instance.equipHandler.SetTodayRedPointUse();
		SelectTab(tabIndex);
	}

	private void RefreshDetail()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		string resName = "ui/image/frame/frame_1.png";
		string resName2 = "ui/image/frame/frame_add.png";
		m_curEquipSlotDetail = zbSelf.Instance.equipHandler.GetEquipSlotInfo(m_curEquipType);
		if (m_curEquipSlotDetail.Equip != null)
		{
			resName = zbResourceHelper.GetResourceFrameTex(m_curEquipSlotDetail.Equip.Quality);
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Equip, m_curEquipSlotDetail.Equip.EquipTid);
			resName2 = resorceIconPath.strIconPath;
			((TextElement)m_lbEquip).text = resorceIconPath.strName;
			((VisualElement)m_lbEquip).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((VisualElement)m_lbEquip).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resName);
		if ((Object)(object)val != (Object)null)
		{
			m_equipFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_equipFrame.MarkDirtyRepaint();
		}
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resName2);
		if ((Object)(object)val2 != (Object)null)
		{
			m_equipIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_equipIcon.MarkDirtyRepaint();
		}
	}

	public override void DoSelectTab()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		switch (m_currTab)
		{
		case 0:
			m_spineScene.SetSceneBg("ui/image/bg/bg_upgrade.png");
			m_spineScene.SetSkeletonDataAsset("animation/ui/baoshi_bg/baoshi_bg_SkeletonData.asset");
			if (m_moduleLevel == null)
			{
				m_moduleLevel = new zbEquipUpgradeTabLevel();
				m_moduleLevel.OnEnable(this, m_tabLayers[0]);
			}
			m_moduleLevel.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			m_spineScene.SetSceneBg("ui/image/bg/bg_equip.png");
			m_spineScene.SetSkeletonDataAsset("animation/ui/zhujueqianghua_bg/zhujueqianghua_bg_SkeletonData.asset");
			if (m_moduleEquip == null)
			{
				m_moduleEquip = new zbEquipUpgradeTabEquip();
				m_moduleEquip.OnEnable(this, m_tabLayers[1]);
			}
			m_moduleEquip.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			break;
		}
		for (int i = 0; i < 3; i++)
		{
			m_itemEquipSlots[i].RefreshNotify();
		}
	}

	public void RefreshSelectList(EEquipType equipType)
	{
		m_curEquipType = equipType;
		RefreshDetail();
		DoSelectTab();
		for (int i = 0; i < 3; i++)
		{
			m_itemEquipSlots[i].RefreshSelect(equipType);
		}
	}

	private void OnEquipTip()
	{
		if (m_curEquipSlotDetail.Equip != null)
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Equip, m_curEquipSlotDetail.Equip.EquipTid, 0L);
		}
	}

	private void OnClose()
	{
		Hide();
		if ((Object)(object)m_spineScene != (Object)null)
		{
			m_spineScene.ClearSpine();
		}
	}

	private void OnEventUpdateEquipSlot(object data)
	{
		RefreshCustom(m_curEquipType, m_currTab);
	}

	private void OnEventUpgradeAck(object data)
	{
		if (m_curEquipSlotDetail?.Equip != null)
		{
			m_spineEquip.AnimationState.SetAnimation(0, "animation", false);
		}
	}
}
