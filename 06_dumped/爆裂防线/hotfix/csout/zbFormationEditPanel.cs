using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFormationEditPanel
{
	private zbFormationWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbSkillType;

	private Button m_btnFollow;

	private VisualElement m_spriteSkillType;

	private Label m_lbDamage;

	private VisualElement m_notifyFollow;

	private Button m_btnClose;

	private ScrollView m_scrollView;

	private List<zbFormationEditItem> m_items = new List<zbFormationEditItem>();

	private ScrollView m_scrollViewMecha;

	private List<zbFormationEditMechaItem> m_mechaItems = new List<zbFormationEditMechaItem>();

	private VisualElement m_nodeQiang;

	private zbFormationEditPanelQiang m_qiangPanel = new zbFormationEditPanelQiang();

	public void OnEnable(zbFormationWnd wndParent, VisualElement root)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbSkillType = UQueryExtensions.Q<Label>(m_RootElement, "lb_skillType", (string)null);
		m_btnFollow = UQueryExtensions.Q<Button>(m_RootElement, "btn_follow", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFollow, (Action)OnFollow, "", (TrickleDown)0);
		m_spriteSkillType = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnFollow, "bg_skillType", (string)null);
		m_lbDamage = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnFollow, "lb_damage", (string)null);
		m_notifyFollow = zbUtil.CreateNotifyTag((VisualElement)(object)m_lbDamage, (Vector2?)new Vector2(-10f, -5f), 1f, true);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		m_nodeQiang = UQueryExtensions.Q<VisualElement>(m_RootElement, "qiang", (string)null);
		m_qiangPanel.OnEnable(wndParent, m_nodeQiang);
		m_scrollViewMecha = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollViewMecha", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_x", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_skillType == 0)
		{
			return;
		}
		m_nodeQiang.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_wndParent.m_skillType != 1));
		((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_wndParent.m_skillType == 1 || m_wndParent.m_skillType == 99) ? 1 : 0));
		((VisualElement)m_scrollViewMecha).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_wndParent.m_skillType != 99));
		((VisualElement)m_btnFollow).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_wndParent.m_skillType == 1 || m_wndParent.m_skillType == 99) ? 1 : 0));
		if (m_wndParent.m_skillType == 99)
		{
			((TextElement)m_lbSkillType).text = zbUtil.Language("lang.10791");
			RefreshMechaScrollView();
			return;
		}
		zbSkillTypeCsvRecord recordByKey = zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey(m_wndParent.m_skillType);
		string resName = m_wndParent.m_record.rarity switch
		{
			2 => recordByKey.icon2, 
			3 => recordByKey.icon3, 
			4 => recordByKey.icon4, 
			_ => recordByKey.icon1, 
		};
		if (m_wndParent.m_skillType == 1)
		{
			resName = recordByKey.icon4;
			m_qiangPanel.Refresh();
		}
		else
		{
			RefreshScrollView();
		}
		((TextElement)m_lbSkillType).text = recordByKey.name;
		m_spriteSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
		WarRoomDetail warRoomDetail = ((IEnumerable<WarRoomDetail>)zbServerData.sGameData.WarRoomInfo.RoomList).FirstOrDefault((WarRoomDetail x) => x.SkillType == (ESkillType)m_wndParent.m_skillType);
		HeroDetail heroDetail = zbSelf.Instance.heroHandler.GetHeroDetail(warRoomDetail.HeroTid);
		if (zbSelf.Instance.heroHandler.CanFollow((ESkillType)m_wndParent.m_skillType))
		{
			float num = zbSelf.Instance.heroHandler.GetDamage(heroDetail.Quality, heroDetail.Rarity, heroDetail.QualityStar) + zbSelf.Instance.heroHandler.GetFollowDamages((ESkillType)m_wndParent.m_skillType);
			((TextElement)m_lbDamage).text = Mathf.RoundToInt(num * 100f) + "%";
			((VisualElement)m_lbDamage).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnFollow).SetEnabled(true);
		}
		else
		{
			((VisualElement)m_lbDamage).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnFollow).SetEnabled(false);
			((VisualElement)m_btnFollow).style.opacity = StyleFloat.op_Implicit(1f);
		}
		m_notifyFollow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroFollowNotify((ESkillType)m_wndParent.m_skillType)));
	}

	private void RefreshMechaScrollView()
	{
		zbMechaCsvRecord[] allMechaRecords = zbSelf.Instance.mechaHandler.GetAllMechaRecords();
		if (allMechaRecords.Length != m_mechaItems.Count)
		{
			m_mechaItems.Clear();
			((VisualElement)m_scrollViewMecha).Clear();
			zbMechaCsvRecord[] array = allMechaRecords;
			for (int i = 0; i < array.Length; i++)
			{
				_ = array[i];
				zbFormationEditMechaItem zbFormationEditMechaItem2 = new zbFormationEditMechaItem(m_wndParent);
				m_mechaItems.Add(zbFormationEditMechaItem2);
				((VisualElement)m_scrollViewMecha).Add((VisualElement)(object)zbFormationEditMechaItem2);
			}
		}
		for (int j = 0; j < m_mechaItems.Count; j++)
		{
			m_mechaItems[j].Refresh(allMechaRecords[j]);
		}
	}

	private void RefreshScrollView()
	{
		List<zbHeroCsvRecord> heroRecordsBySkillType = zbSelf.Instance.heroHandler.GetHeroRecordsBySkillType(m_wndParent.m_skillType);
		if (heroRecordsBySkillType.Count != m_items.Count)
		{
			m_items.Clear();
			((VisualElement)m_scrollView).Clear();
			foreach (zbHeroCsvRecord item in heroRecordsBySkillType)
			{
				_ = item;
				zbFormationEditItem zbFormationEditItem2 = new zbFormationEditItem(m_wndParent);
				m_items.Add(zbFormationEditItem2);
				((VisualElement)m_scrollView).Add((VisualElement)(object)zbFormationEditItem2);
			}
		}
		for (int i = 0; i < m_items.Count; i++)
		{
			m_items[i].Refresh(heroRecordsBySkillType[i]);
		}
	}

	private void OnFollow()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.HeroFollow))
		{
			zbUIMgr.Instance.ShowHeroFollow((ESkillType)m_wndParent.m_skillType);
		}
	}

	public void OnClose()
	{
		m_wndParent.SetEditPanelVisible(vis: false);
	}
}
