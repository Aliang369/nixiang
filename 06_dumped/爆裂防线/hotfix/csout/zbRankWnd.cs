using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRankWnd : zbTabWnd
{
	private zbSpineScene m_spineScene;

	private Transform[] m_nodeTops = (Transform[])(object)new Transform[3];

	private zbPlayerMechaAvatar[] m_spinePlayerMecha = new zbPlayerMechaAvatar[3];

	private VisualElement[] m_tops = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_noAvatars = (VisualElement[])(object)new VisualElement[3];

	private Label[] m_lbTopNames = (Label[])(object)new Label[3];

	private VisualElement m_nodeTab;

	private VisualElement m_spriteTabSel;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	public Button[] m_tabButtons = (Button[])(object)new Button[3];

	private zbRankTabMainQuest m_moduleMainQuest;

	public zbRankTabArena m_moduleArena;

	public zbRankTabEndlessHorde m_moduleEndlessHorde;

	private Button m_btnClose;

	private Button m_btnGift;

	public zbRankItemCopy m_copyRankItem;

	private VisualElement m_notifyMainQuest;

	private VisualElement m_notify;

	public Sprite m_rankBg;

	public Sprite m_rankBgMine;

	private RoleBaseInfo[] m_topRoleBaseInfos;

	public Action<bool> m_callback;

	public bool firstIn = true;

	protected override void Awake()
	{
		base.Awake();
		m_copyRankItem = new zbRankItemCopy();
	}

	protected override void OnEnable()
	{
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		for (int i = 0; i < 3; i++)
		{
			m_nodeTops[i] = ((Component)m_GameObjectSceneRoot.transform.Find($"top{i + 1}")).GetComponent<Transform>();
			m_spinePlayerMecha[i] = ((Component)m_nodeTops[i]).GetComponent<zbPlayerMechaAvatar>();
			m_tops[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"top{i + 1}", (string)null);
			m_noAvatars[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "no_avatar" + (i + 1), (string)null);
			m_lbTopNames[i] = UQueryExtensions.Q<Label>(m_tops[i], "lb_name", (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>(m_tops[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnHead(evt, tempIndex);
			}, "", (TrickleDown)0);
		}
		m_nodeTab = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTabbg", (string)null);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		for (int num = 0; num < 3; num++)
		{
			m_tabLayers[num] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + num, (string)null);
			m_tabButtons[num] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + num, (string)null);
		}
		((TextElement)m_tabButtons[0]).text = zbUtil.Language("lang.11054");
		((TextElement)m_tabButtons[1]).text = zbUtil.Language("lang.11055");
		((TextElement)m_tabButtons[2]).text = zbUtil.Language("lang.11056");
		m_btnGift = UQueryExtensions.Q<Button>(m_RootElement, "btn_gift", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGift, (Action)OnGift, "", (TrickleDown)0);
		m_notifyMainQuest = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[0]);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGift, (Vector2?)new Vector2(0f, 7f), 1f, true);
		m_rankBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legion/rank_bg.png");
		m_rankBgMine = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/worldBoss/rank_bg1.png");
		m_copyRankItem = UQueryExtensions.Q<zbRankItemCopy>(m_RootElement, "itemCopy", (string)null);
		m_copyRankItem.OnEnable(this);
		((VisualElement)m_copyRankItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		InitTab(m_tabButtons, m_tabLayers, null);
		m_gameEevent.RegisterEvent(EGameEvent.Event_FirstReachAward, RefreshNotify);
	}

	public void RefreshCustom(int tabIndex, Action<bool> callback)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		firstIn = false;
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena))
		{
			m_tabLayers[0].style.top = StyleLength.op_Implicit(740f);
			m_nodeTab.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			m_tabLayers[0].style.top = StyleLength.op_Implicit(700f);
			m_nodeTab.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.EndlessHorde))
			{
				((VisualElement)m_tabButtons[2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_tabButtons[0]).style.width = StyleLength.op_Implicit(360f);
				((VisualElement)m_tabButtons[1]).style.width = StyleLength.op_Implicit(360f);
				((VisualElement)m_tabButtons[1]).style.left = StyleLength.op_Implicit(360f);
				m_spriteTabSel.style.width = StyleLength.op_Implicit(362f);
			}
			else
			{
				((VisualElement)m_tabButtons[2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_tabButtons[0]).style.width = StyleLength.op_Implicit(240f);
				((VisualElement)m_tabButtons[1]).style.width = StyleLength.op_Implicit(240f);
				((VisualElement)m_tabButtons[1]).style.left = StyleLength.op_Implicit(240f);
				m_spriteTabSel.style.width = StyleLength.op_Implicit(242f);
			}
		}
		m_callback = callback;
		SelectTab(tabIndex);
		zbWsMsgHandler.Instance.Rank_Top_Req();
	}

	public override void DoSelectTab()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		switch (m_currTab)
		{
		case 0:
			m_spineScene.SetSceneBg("ui/image/bg2/bg_rankSub.jpg");
			zbUtil.RefreshSpine(m_spineScene.skeletonAnimation, "animation/ui/paihangbang_bg3/paihangbang_bg3_SkeletonData.asset");
			if (m_moduleMainQuest == null)
			{
				m_moduleMainQuest = new zbRankTabMainQuest();
				m_moduleMainQuest.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleMainQuest.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			m_spineScene.SetSceneBg("ui/image/arena/arena_rank.jpg");
			zbUtil.RefreshSpine(m_spineScene.skeletonAnimation, "animation/ui/paihangbang_bg3/paihangbang_bg3_SkeletonData.asset");
			if (m_moduleArena == null)
			{
				m_moduleArena = new zbRankTabArena();
				m_moduleArena.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleArena.Refresh();
			if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.EndlessHorde))
			{
				m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			}
			else
			{
				m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(240f, (LengthUnit)0));
			}
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.Diamond,
				EMoneyType.Gold
			});
			break;
		case 2:
			m_spineScene.SetSceneBg("ui/image/endlessHorde/endlessHorde_rank.jpg");
			zbUtil.RefreshSpine(m_spineScene.skeletonAnimation, "animation/ui/paihangbang_bg4/paihangbang_bg4_SkeletonData.asset");
			if (m_moduleEndlessHorde == null)
			{
				m_moduleEndlessHorde = new zbRankTabEndlessHorde();
				m_moduleEndlessHorde.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleEndlessHorde.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(480f, (LengthUnit)0));
			break;
		}
		((VisualElement)m_btnGift).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab != 0) ? 1 : 0));
		RefreshNotify();
	}

	private void RefreshNotify(object obj = null)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		m_notifyMainQuest.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.rankHandler.HasRankFirstReachRedPoint()));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.rankHandler.HasRankFirstReachRedPoint()));
	}

	public void RefreshTops(RoleBaseInfo[] list)
	{
		m_topRoleBaseInfos = list;
		for (int i = 0; i < 3; i++)
		{
			bool flag = m_topRoleBaseInfos.Length > i;
			((Component)m_nodeTops[i]).gameObject.SetActive(flag);
			m_noAvatars[i].visible = !flag;
			if (flag)
			{
				m_spinePlayerMecha[i].RefreshCustom(m_topRoleBaseInfos[i].AvatarId, m_topRoleBaseInfos[i].GunId, m_topRoleBaseInfos[i].MechaId);
				m_spinePlayerMecha[i].RefreshRankPosition(i, m_topRoleBaseInfos[i].MechaId);
				((TextElement)m_lbTopNames[i]).text = m_topRoleBaseInfos[i].RoleName;
			}
			else
			{
				((TextElement)m_lbTopNames[i]).text = "";
			}
		}
	}

	private void OnHead(ClickEvent evt, int index)
	{
		if (m_topRoleBaseInfos.Length > index)
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_topRoleBaseInfos[index].RoleId);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10499"));
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		m_copyRankItem.Update();
	}

	private void OnGift()
	{
		zbWsMsgHandler.Instance.Rank_FirstReach_Req(ERankType.MainQuest);
	}

	private void OnClose()
	{
		firstIn = true;
		m_moduleMainQuest?.BeforeClose();
		m_moduleArena?.BeforeClose();
		m_moduleEndlessHorde?.BeforeClose();
		Hide();
		m_callback?.Invoke(obj: true);
	}
}
