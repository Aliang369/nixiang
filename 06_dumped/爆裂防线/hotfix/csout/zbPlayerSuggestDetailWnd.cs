using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerSuggestDetailWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private zbPlayerAvatar m_spineAvatar;

	private zbPlayerSuggestDetailItem[] m_equipSlots = new zbPlayerSuggestDetailItem[3];

	private Button m_close;

	private VisualElement m_headIcon;

	private VisualElement m_headFrame;

	private Label m_lbName;

	private Label m_lbLevel;

	private Label m_lbId;

	private Label m_lbServer;

	private Button m_btnCopy;

	private Button m_btnHead;

	private RoleTipsInfo m_roleTipsInfo;

	private ListView m_listView;

	private List<ModSkillInfo> m_modSkillInfos;

	private Button m_btnGun;

	private Label m_lbGunDesc;

	private Button[] m_suits = (Button[])(object)new Button[3];

	private Button[] m_highSuits = (Button[])(object)new Button[2];

	private Label[] m_lbSuitsDesc = (Label[])(object)new Label[3];

	private Label[] m_lbHighSuitsDesc = (Label[])(object)new Label[2];

	private VisualElement m_btnMask;

	private VisualElement m_leftIcons;

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < 3; i++)
		{
			m_equipSlots[i] = new zbPlayerSuggestDetailItem();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/combat/tb_fightMod.uxml");
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		for (int i = 0; i < 3; i++)
		{
			VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "equipSlot" + i, (string)null);
			m_suits[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_suit" + i, (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_suits[i], (Action)delegate
			{
				OnMask(tempIndex + 1);
			}, "", (TrickleDown)0);
			m_lbSuitsDesc[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_descSuit" + i, (string)null);
			m_equipSlots[i].OnEnable(this, root);
		}
		for (int num = 0; num < 2; num++)
		{
			m_highSuits[num] = UQueryExtensions.Q<Button>(m_RootElement, "btn_highSuit" + num, (string)null);
			m_lbHighSuitsDesc[num] = UQueryExtensions.Q<Label>(m_RootElement, "lb_descHighSuit" + num, (string)null);
			int tempIndex2 = num;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_highSuits[num], (Action)delegate
			{
				OnMask(tempIndex2 + 1 + 3);
			}, "", (TrickleDown)0);
		}
		m_leftIcons = UQueryExtensions.Q<VisualElement>(m_RootElement, "leftIcons", (string)null);
		m_close = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_headIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "head", (string)null);
		m_headFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "headFrame", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>(m_RootElement, "Button_Head", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_lbId = UQueryExtensions.Q<Label>(m_RootElement, "lb_id", (string)null);
		m_lbServer = UQueryExtensions.Q<Label>(m_RootElement, "lb_server", (string)null);
		m_btnCopy = UQueryExtensions.Q<Button>(m_RootElement, "btn_copy", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_btnGun = UQueryExtensions.Q<Button>(m_RootElement, "btn_gun", (string)null);
		m_lbGunDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_gunDesc", (string)null);
		m_btnMask = UQueryExtensions.Q<VisualElement>(m_RootElement, "btn_mask", (string)null);
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 160f;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_close, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCopy, (Action<ClickEvent>)OnCopyRoleID, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnHead, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGun, (Action)delegate
		{
			OnMask();
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnMask, (Action)OnCloseMask, "", (TrickleDown)0);
	}

	public void RefreshCustom(RoleTipsInfo _roleTipsInfo, List<ModSkillInfo> modSkillInfos)
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (_roleTipsInfo != null)
		{
			m_modSkillInfos = modSkillInfos;
			m_roleTipsInfo = _roleTipsInfo;
			bool flag = m_roleTipsInfo.RoleBaseInfo.GunId > 2 && m_roleTipsInfo.RoleSuitInfo.SuitList.Count == 3 && ((IEnumerable<PlayerSuitDetail>)m_roleTipsInfo.RoleSuitInfo.SuitList).All((PlayerSuitDetail u) => u.SuitId > 1) && m_roleTipsInfo.RoleSuitInfo.SuperSuitList.Count == 2 && ((IEnumerable<PlayerSuitDetail>)m_roleTipsInfo.RoleSuitInfo.SuperSuitList).All((PlayerSuitDetail s) => s.SuitId > 1);
			m_leftIcons.style.justifyContent = StyleEnum<Justify>.op_Implicit((Justify)(flag ? 2 : 0));
			RefreshView();
			UpdateHeadAndHeadFrameDisplay();
			UpdateListView();
			InitGunAndSuit();
			OnCloseMask();
		}
	}

	private void InitGunAndSuit()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_roleTipsInfo.RoleBaseInfo.GunId);
		((VisualElement)m_btnGun).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_roleTipsInfo.RoleBaseInfo.GunId <= 2));
		if (recordByKey != null)
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon);
			((VisualElement)m_btnGun).style.backgroundImage = new StyleBackground(val);
			((TextElement)m_lbGunDesc).text = zbUtil.Language(recordByKey.desc);
		}
		RepeatedField<PlayerSuitDetail> suitList = m_roleTipsInfo.RoleSuitInfo.SuitList;
		RepeatedField<PlayerSuitDetail> superSuitList = m_roleTipsInfo.RoleSuitInfo.SuperSuitList;
		for (int i = 0; i < 3; i++)
		{
			if (suitList == null || i >= suitList.Count || suitList[i].SuitId <= 1)
			{
				((VisualElement)m_suits[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				continue;
			}
			zbPlayerSuitCsvRecord recordByKey2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suitList[i].SuitId);
			bool flag = recordByKey2 != null && recordByKey2.grade < 2;
			((VisualElement)m_suits[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (!flag)
			{
				continue;
			}
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.skillIcon);
			((VisualElement)m_suits[i]).style.backgroundImage = new StyleBackground(val2);
			int suitId = suitList[i].SuitId;
			int num = 0;
			foreach (PlayerSuitDetail item in suitList)
			{
				if (item.SuitId == suitId)
				{
					num = item.Star;
					break;
				}
			}
			if (num > 0)
			{
				((TextElement)m_lbSuitsDesc[i]).text = zbUtil.Language(zbSelf.Instance.suitGunHandler.GetSkillDesc(suitId, num));
			}
		}
		for (int j = 0; j < 2; j++)
		{
			if (j >= superSuitList.Count)
			{
				((VisualElement)m_highSuits[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				continue;
			}
			zbPlayerSuitCsvRecord recordByKey3 = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(superSuitList[j].SuitId);
			if (recordByKey3 == null)
			{
				continue;
			}
			bool flag2 = j < superSuitList.Count && superSuitList[j].SuitId > 1 && recordByKey3.grade >= 2;
			((VisualElement)m_highSuits[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (!flag2)
			{
				continue;
			}
			Sprite val3 = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey3.skillIcon);
			((VisualElement)m_highSuits[j]).style.backgroundImage = new StyleBackground(val3);
			int suitId2 = superSuitList[j].SuitId;
			int num2 = 0;
			foreach (PlayerSuitDetail item2 in superSuitList)
			{
				if (item2.SuitId == suitId2)
				{
					num2 = item2.Star;
				}
			}
			if (suitId2 > 0 && num2 > 0)
			{
				((TextElement)m_lbHighSuitsDesc[j]).text = zbUtil.Language(zbSelf.Instance.suitGunHandler.GetSkillDesc(suitId2, num2));
			}
		}
	}

	private void UpdateListView()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_listView).Clear();
		((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_modSkillInfos.Count <= 0));
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		m_listView.makeItem = () => (VisualElement)(object)new zbSkillFightModItem(m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			zbSkillFightModItem obj = element as zbSkillFightModItem;
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(m_modSkillInfos[index].ModId);
			((VisualElement)obj).style.marginTop = StyleLength.op_Implicit(-10f);
			obj.Init(recordByKey, m_modSkillInfos[index].SkillId);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_modSkillInfos;
	}

	public bool IsSelf()
	{
		if (m_roleTipsInfo != null && m_roleTipsInfo.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId())
		{
			return true;
		}
		return false;
	}

	private void RefreshView()
	{
		if (m_roleTipsInfo != null)
		{
			((Component)m_spineAvatar).gameObject.SetActive(m_roleTipsInfo != null);
			m_spineAvatar.RefreshCustom(m_roleTipsInfo.RoleBaseInfo.AvatarId, m_roleTipsInfo.RoleBaseInfo.GunId);
		}
		for (int i = 0; i < 3; i++)
		{
			EquipSlotInfo equipSlotInfo = new EquipSlotInfo();
			equipSlotInfo.EquipSlotList.Add(m_roleTipsInfo.EquipSlotInfo.EquipSlotList[i]);
			GemPlanDetail gemPlanDetail = new GemPlanDetail();
			gemPlanDetail.GemPlanSlotList.AddRange((IEnumerable<GemPlanSlotDetail>)m_roleTipsInfo.EquipSlotInfo.GemSlotList);
			equipSlotInfo.GemPlanList.Add(gemPlanDetail);
			m_equipSlots[i].Refresh(equipSlotInfo, (EEquipType)(i + 1));
		}
		((TextElement)m_lbName).text = m_roleTipsInfo.RoleBaseInfo.RoleName;
		((TextElement)m_lbLevel).text = m_roleTipsInfo.RoleBaseInfo.Level.ToString();
		((TextElement)m_lbId).text = m_roleTipsInfo.RoleBaseInfo.RoleId.ToString();
		((TextElement)m_lbServer).text = "Server: " + zbUtil.SetServerString(m_roleTipsInfo.RoleBaseInfo.ServerId);
	}

	private void UpdateHeadAndHeadFrameDisplay()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(m_roleTipsInfo.RoleBaseInfo.HeadId);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex);
		m_headIcon.style.backgroundImage = new StyleBackground(val);
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(m_roleTipsInfo.RoleBaseInfo.HeadFrameId);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex);
		m_headFrame.style.backgroundImage = new StyleBackground(val2);
	}

	private void OnMask(int index = 0)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		m_btnMask.SetEnabled(true);
		m_btnMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_btnMask.pickingMode = (PickingMode)0;
		((VisualElement)m_lbGunDesc).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((index != 0) ? 1 : 0));
		((VisualElement)m_lbSuitsDesc[0]).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(index != 1));
		((VisualElement)m_lbSuitsDesc[1]).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(index != 2));
		((VisualElement)m_lbSuitsDesc[2]).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(index != 3));
		((VisualElement)m_lbHighSuitsDesc[0]).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(index != 4));
		((VisualElement)m_lbHighSuitsDesc[1]).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(index != 5));
	}

	private void OnCloseMask()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		m_btnMask.SetEnabled(false);
		m_btnMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btnMask.pickingMode = (PickingMode)1;
	}

	private void OnCopyRoleID(ClickEvent evt)
	{
		zbUtil.CopyString(m_roleTipsInfo.RoleBaseInfo.RoleId.ToString());
	}

	private void OnHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_roleTipsInfo.RoleBaseInfo.RoleId);
	}

	private void OnClose()
	{
		Hide();
	}
}
