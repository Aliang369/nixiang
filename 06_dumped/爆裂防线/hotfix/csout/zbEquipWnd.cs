using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEquipWnd : zbBaseWnd
{
	private Button m_btnTop;

	private Button m_btnDown;

	private VisualElement m_bgLeft;

	private VisualElement m_bgFrameLeft;

	private Label m_lbNameLeft;

	private Button m_btnLockLeft;

	private VisualElement m_spriteFrameLeft;

	private VisualElement m_spriteIconLeft;

	private Button m_btnIconLeft;

	private Label m_lbLevelLeft;

	private Label m_lbMainAttrValueLeft;

	private VisualElement m_nodeSubAttrLeft;

	private VisualElement[] m_nodeAttrsLeft = (VisualElement[])(object)new VisualElement[6];

	private Label[] m_lbSubAttrsLeft = (Label[])(object)new Label[6];

	private Label[] m_lbSubAttrValuesLeft = (Label[])(object)new Label[6];

	private Button m_btnDecompose;

	private Button m_btnSet;

	private TextElement m_btnSetText;

	private VisualElement m_bgRight;

	private VisualElement m_bgFrameRight;

	private Label m_lbNameRight;

	private Button m_btnLockRight;

	private VisualElement m_spriteFrameRight;

	private VisualElement m_spriteIconRight;

	private Button m_btnIconRight;

	private Label m_lbLevelRight;

	private Label m_lbMainAttrValueRight;

	private VisualElement m_nodeSubAttrRight;

	private VisualElement[] m_nodeSubAttrsRight = (VisualElement[])(object)new VisualElement[6];

	private Label[] m_lbSubAttrsRight = (Label[])(object)new Label[6];

	private Label[] m_lbSubAttrValuesRight = (Label[])(object)new Label[6];

	private Button m_btnUpgrade;

	private EquipDetail m_data;

	private zbEquipCsvRecord m_record;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTop = UQueryExtensions.Q<Button>(m_RootElement, "btn_top", (string)null);
		m_btnDown = UQueryExtensions.Q<Button>(m_RootElement, "btn_down", (string)null);
		m_bgLeft = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_left", (string)null);
		m_bgRight = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_right", (string)null);
		m_bgFrameLeft = UQueryExtensions.Q<VisualElement>(m_bgLeft, "bg_frame", (string)null);
		m_lbNameLeft = UQueryExtensions.Q<Label>(m_bgLeft, "lb_name", (string)null);
		m_btnLockLeft = UQueryExtensions.Q<Button>(m_bgLeft, "btn_lock", (string)null);
		m_spriteFrameLeft = UQueryExtensions.Q<VisualElement>(m_bgLeft, "frame", (string)null);
		m_spriteIconLeft = UQueryExtensions.Q<VisualElement>(m_bgLeft, "icon", (string)null);
		m_btnIconLeft = UQueryExtensions.Q<Button>(m_bgLeft, "btn_icon", (string)null);
		m_lbLevelLeft = UQueryExtensions.Q<Label>(m_bgLeft, "lb_subLevel", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_bgLeft, "title_main", (string)null);
		m_lbMainAttrValueLeft = UQueryExtensions.Q<Label>(val, "lb_value", (string)null);
		m_nodeSubAttrLeft = UQueryExtensions.Q<VisualElement>(m_bgLeft, "title_sub", (string)null);
		m_btnDecompose = UQueryExtensions.Q<Button>(m_bgLeft, "btn_decompose", (string)null);
		m_btnSet = UQueryExtensions.Q<Button>(m_bgLeft, "btn_replace", (string)null);
		m_btnSetText = UQueryExtensions.Q<TextElement>((VisualElement)(object)m_btnSet, (string)null, (string)null);
		m_bgFrameRight = UQueryExtensions.Q<VisualElement>(m_bgRight, "bg_frame", (string)null);
		m_lbNameRight = UQueryExtensions.Q<Label>(m_bgRight, "lb_name", (string)null);
		m_btnLockRight = UQueryExtensions.Q<Button>(m_bgRight, "btn_lock", (string)null);
		m_spriteFrameRight = UQueryExtensions.Q<VisualElement>(m_bgRight, "frame", (string)null);
		m_spriteIconRight = UQueryExtensions.Q<VisualElement>(m_bgRight, "icon", (string)null);
		m_btnIconRight = UQueryExtensions.Q<Button>(m_bgRight, "btn_icon", (string)null);
		m_lbLevelRight = UQueryExtensions.Q<Label>(m_bgRight, "lb_subLevel", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_bgRight, "title_main", (string)null);
		m_lbMainAttrValueRight = UQueryExtensions.Q<Label>(val2, "lb_value", (string)null);
		m_nodeSubAttrRight = UQueryExtensions.Q<VisualElement>(m_bgRight, "title_sub", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_bgRight, "btn_upgrade", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_nodeAttrsLeft[i] = UQueryExtensions.Q<VisualElement>(m_nodeSubAttrLeft, "attr" + i, (string)null);
			m_lbSubAttrsLeft[i] = UQueryExtensions.Q<Label>(m_nodeAttrsLeft[i], "lb_attr", (string)null);
			m_lbSubAttrValuesLeft[i] = UQueryExtensions.Q<Label>(m_nodeAttrsLeft[i], "lb_value", (string)null);
			m_nodeSubAttrsRight[i] = UQueryExtensions.Q<VisualElement>(m_nodeSubAttrRight, "attr" + i, (string)null);
			m_lbSubAttrsRight[i] = UQueryExtensions.Q<Label>(m_nodeSubAttrsRight[i], "lb_attr", (string)null);
			m_lbSubAttrValuesRight[i] = UQueryExtensions.Q<Label>(m_nodeSubAttrsRight[i], "lb_value", (string)null);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTop, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDown, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLockLeft, (Action)OnLockLeft, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnIconLeft, (Action)OnIconLeft, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLockRight, (Action)OnLockRight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnIconRight, (Action)OnIconRight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSet, (Action)OnSet, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDecompose, (Action)OnDecompose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_EquipSlotInfo, OnEventUpdateEquipSlot);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Equip, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_EquipLock_Ack, OnEventUpdate);
	}

	public void RefreshCustom(EquipDetail data)
	{
		m_data = data;
		m_record = zbDataConfig.Instance.equipCsvTable.GetRecordByKey(m_data.EquipTid);
		RefreshLeft();
		RefreshRight();
	}

	private void RefreshLeft()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		if (m_data.Lock == EBoolState.True)
		{
			((VisualElement)m_btnLockLeft).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_lock.png"));
		}
		else
		{
			((VisualElement)m_btnLockLeft).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_unLock.png"));
		}
		string resourceBgTex = zbResourceHelper.GetResourceBgTex(m_data.Quality);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceBgTex);
		if ((Object)(object)val != (Object)null)
		{
			m_bgFrameLeft.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_bgFrameLeft.MarkDirtyRepaint();
		}
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex(m_data.Quality);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteFrameLeft.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteFrameLeft.MarkDirtyRepaint();
		}
		Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(m_record.icon);
		if ((Object)(object)val3 != (Object)null)
		{
			m_spriteIconLeft.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val3));
			m_spriteIconLeft.MarkDirtyRepaint();
		}
		((TextElement)m_lbNameLeft).text = m_record.name;
		((TextElement)m_lbLevelLeft).text = m_data.SubLevel.ToString();
		((TextElement)m_lbMainAttrValueLeft).text = zbSelf.Instance.equipHandler.GetEquipMainAttrValue(m_data.EquipTid, m_data.Quality).ToString();
		for (int i = 0; i < 6; i++)
		{
			m_nodeAttrsLeft[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (EquipSubAttrDetail subAttr in m_data.SubAttrList)
		{
			if (num < m_nodeAttrsLeft.Length)
			{
				m_nodeAttrsLeft[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_lbSubAttrsLeft[num]).text = zbAttrHelper.GetAttrName(subAttr.AttrType, subAttr.FuncId, subAttr.AttrId);
				if (subAttr.ShowPct == EBoolState.False)
				{
					((TextElement)m_lbSubAttrValuesLeft[num]).text = (subAttr.Value * (float)m_data.SubLevel).ToString("F2");
				}
				else
				{
					((TextElement)m_lbSubAttrValuesLeft[num]).text = (subAttr.Value * (float)m_data.SubLevel * 100f).ToString("F2") + "%";
				}
			}
			num++;
		}
		m_nodeSubAttrLeft.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(num <= 0));
	}

	private void RefreshRight()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		EquipDetail equipSlotEquip = zbSelf.Instance.equipHandler.GetEquipSlotEquip((EEquipType)m_record.type);
		if (equipSlotEquip == null)
		{
			m_btnSetText.text = zbUtil.Language("lang.10040");
			m_bgRight.visible = false;
			return;
		}
		if (equipSlotEquip.Lock == EBoolState.True)
		{
			((VisualElement)m_btnLockRight).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_lock.png"));
		}
		else
		{
			((VisualElement)m_btnLockRight).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_unLock.png"));
		}
		m_btnSetText.text = zbUtil.Language("lang.10041");
		m_bgRight.visible = true;
		zbEquipCsvRecord recordByKey = zbDataConfig.Instance.equipCsvTable.GetRecordByKey(equipSlotEquip.EquipTid);
		string resourceBgTex = zbResourceHelper.GetResourceBgTex(equipSlotEquip.Quality);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceBgTex);
		if ((Object)(object)val != (Object)null)
		{
			m_bgFrameRight.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_bgFrameRight.MarkDirtyRepaint();
		}
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex(equipSlotEquip.Quality);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteFrameRight.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteFrameRight.MarkDirtyRepaint();
		}
		Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
		if ((Object)(object)val3 != (Object)null)
		{
			m_spriteIconRight.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val3));
			m_spriteIconRight.MarkDirtyRepaint();
		}
		((TextElement)m_lbNameRight).text = recordByKey.name;
		((TextElement)m_lbLevelRight).text = equipSlotEquip.SubLevel.ToString();
		((TextElement)m_lbMainAttrValueRight).text = zbSelf.Instance.equipHandler.GetEquipMainAttrValue(equipSlotEquip.EquipTid, equipSlotEquip.Quality).ToString();
		for (int i = 0; i < 6; i++)
		{
			m_nodeSubAttrsRight[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (EquipSubAttrDetail subAttr in equipSlotEquip.SubAttrList)
		{
			if (num < m_nodeSubAttrsRight.Length)
			{
				m_nodeSubAttrsRight[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_lbSubAttrsRight[num]).text = zbAttrHelper.GetAttrName(subAttr.AttrType, subAttr.FuncId, subAttr.AttrId);
				if (subAttr.ShowPct == EBoolState.False)
				{
					((TextElement)m_lbSubAttrValuesRight[num]).text = (subAttr.Value * (float)equipSlotEquip.SubLevel).ToString("F2");
				}
				else
				{
					((TextElement)m_lbSubAttrValuesRight[num]).text = (subAttr.Value * (float)equipSlotEquip.SubLevel * 100f).ToString("F2") + "%";
				}
			}
			num++;
		}
		m_nodeSubAttrRight.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(num <= 0));
	}

	private void OnLockLeft()
	{
		if (m_data.Lock == EBoolState.False)
		{
			zbWsMsgHandler.Instance.Equip_Lock_Req(m_data.EquipId, EBoolState.True);
		}
		else
		{
			zbWsMsgHandler.Instance.Equip_Lock_Req(m_data.EquipId, EBoolState.False);
		}
	}

	private void OnIconLeft()
	{
		zbUIMgr.Instance.ShowTip(EResourceType.Equip, m_data.EquipTid, 0L);
	}

	private void OnLockRight()
	{
		EquipDetail equipSlotEquip = zbSelf.Instance.equipHandler.GetEquipSlotEquip((EEquipType)m_record.type);
		if (equipSlotEquip != null)
		{
			if (equipSlotEquip.Lock == EBoolState.False)
			{
				zbWsMsgHandler.Instance.EquipSlot_EquipLock_Req((EEquipType)m_record.type, EBoolState.True);
			}
			else
			{
				zbWsMsgHandler.Instance.EquipSlot_EquipLock_Req((EEquipType)m_record.type, EBoolState.False);
			}
		}
	}

	private void OnIconRight()
	{
		EquipDetail equipSlotEquip = zbSelf.Instance.equipHandler.GetEquipSlotEquip((EEquipType)m_record.type);
		if (equipSlotEquip != null)
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Equip, equipSlotEquip.EquipTid, 0L);
		}
	}

	private void OnSet()
	{
		zbWsMsgHandler.Instance.EquipSlot_EquipSet_Req((EEquipType)m_record.type, m_data.EquipId);
	}

	private void OnDecompose()
	{
		EquipDetail equipSlotEquip = zbSelf.Instance.equipHandler.GetEquipSlotEquip((EEquipType)m_record.type);
		if (m_data.Lock == EBoolState.True)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10501"));
			return;
		}
		if (m_data.Quality <= equipSlotEquip.Quality)
		{
			zbWsMsgHandler.Instance.Equip_Decompose_Req(new string[1] { m_data.EquipId });
			return;
		}
		zbUIMgr.Instance.ShowVerificationCode(zbUtil.Language("lang.10763"), zbUtil.Language("lang.10566"), delegate(bool isOK)
		{
			if (isOK)
			{
				zbWsMsgHandler.Instance.Equip_Decompose_Req(new string[1] { m_data.EquipId });
			}
		});
	}

	private void OnUpgrade()
	{
		Hide();
		zbUIMgr.Instance.ShowEquipUpgrade((EEquipType)m_record.type);
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
		zbLeaderWnd zbLeaderWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Leader) as zbLeaderWnd;
		if ((Object)(object)zbLeaderWnd2 != (Object)null)
		{
			zbLeaderWnd2.m_moduleYiti.RefreshSelect("");
		}
	}

	private void OnEventUpdateEquipSlot(object data)
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		EquipDetail equipDetail = zbSelf.Instance.equipHandler.GetEquipDetail(m_data.EquipId);
		if (equipDetail != null)
		{
			RefreshCustom(equipDetail);
		}
		else
		{
			Hide();
		}
	}
}
