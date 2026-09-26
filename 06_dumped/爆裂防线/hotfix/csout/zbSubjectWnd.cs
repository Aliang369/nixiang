using System;
using Spine.Unity;
using UnityEngine.UIElements;
using zbType;

public class zbSubjectWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Label m_lbTitle;

	private Label m_lbDesc;

	private Button m_btnGoto;

	private VisualElement m_btnTip;

	private VisualElement m_spriteOk;

	private ESubjectType m_subjectType;

	private Action<bool> m_callback;

	private Action<bool> m_callgoto;

	private SpineVisualElement m_tierSpine;

	private SpineVisualElement m_passPortSpine;

	private Label m_lbPassportTitle;

	private Label m_lbPassportDesc;

	private Label[] m_lbSubscriptions = (Label[])(object)new Label[5];

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_tierSpine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spineBg", (string)null);
		m_passPortSpine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spinePassportBg", (string)null);
		m_lbPassportTitle = UQueryExtensions.Q<Label>((VisualElement)(object)m_passPortSpine, "lb_title", (string)null);
		m_lbPassportDesc = UQueryExtensions.Q<Label>((VisualElement)(object)m_passPortSpine, "lb_desc", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "btn_goto", (string)null);
		m_btnTip = UQueryExtensions.Q<VisualElement>(m_RootElement, "tip", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		for (int i = 0; i < m_lbSubscriptions.Length; i++)
		{
			m_lbSubscriptions[i] = UQueryExtensions.Q<Label>(m_RootElement, $"lb_desc{i}", (string)null);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action<ClickEvent>)OnGoto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnTip, (Action<ClickEvent>)OnTip, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(ESubjectType type, Action<bool> callback, Action<bool> callgoto)
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		m_subjectType = type;
		m_callback = callback;
		m_callgoto = callgoto;
		zbSubjectData subjectData = GetSubjectData(type);
		if (subjectData != null)
		{
			bool flag = type == ESubjectType.Passport;
			((VisualElement)m_tierSpine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
			((VisualElement)m_passPortSpine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (!flag)
			{
				zbUtil.RefreshSpineUI(m_tierSpine, subjectData.iconPath, "animation", loop: true);
				((TextElement)m_lbTitle).text = subjectData.strTitle;
				((TextElement)m_lbDesc).text = subjectData.strDesc;
			}
			else
			{
				zbUtil.RefreshSpineUI(m_passPortSpine, subjectData.iconPath, "animation", loop: true);
				((TextElement)m_lbPassportTitle).text = subjectData.strTitle;
				((TextElement)m_lbPassportDesc).text = subjectData.strDesc;
			}
		}
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnGoto(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			zbGameSave.Instance.SetString("SubjectDate" + zbSelf.Instance.GetRoleId(), zbSelf.Instance.timeHandler.GetServerDateStr());
		}
		Hide();
		m_callgoto?.Invoke(obj: true);
	}

	private void OnClose(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			zbGameSave.Instance.SetString("SubjectDate" + zbSelf.Instance.GetRoleId(), zbSelf.Instance.timeHandler.GetServerDateStr());
		}
		Hide();
		m_callback?.Invoke(obj: true);
	}

	private void OnTip(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
	}

	private zbSubjectData GetSubjectData(ESubjectType type)
	{
		return type switch
		{
			ESubjectType.MonthCard => new zbSubjectData("animation/ui/ui_banner/ui_yueka3/ui_yueka3_SkeletonData.asset", zbUtil.Language("lang.10687"), zbUtil.Language("lang.10688")), 
			ESubjectType.Passport => new zbSubjectData("animation/ui/ui_banner/ui_tongxingzheng/ui_xiandingshizhuang_SkeletonData.asset", zbUtil.Language("lang.10689"), zbUtil.Language("lang.10690"), 800f, 900f, 750f, 790f), 
			ESubjectType.Roulette => new zbSubjectData("animation/ui/ui_banner/ui_zhuanpan/ui_zhuanpan_SkeletonData.asset", zbDataConfig.Instance.operateCsvTable.GetRecordByKey(25).name, zbUtil.Language("lang.10691"), 720f, 1000f, 715f, 760f, -150f), 
			ESubjectType.Treasure => new zbSubjectData("animation/ui/ui_banner/ui_duobao/ui_duobao_SkeletonData.asset", zbDataConfig.Instance.operateCsvTable.GetRecordByKey(9).name, zbUtil.Language("lang.10691"), 800f, 900f, 690f, 735f, -50f), 
			ESubjectType.Vendor => new zbSubjectData("animation/ui/ui_banner/ui_fanmai/ui_fanmai_SkeletonData.asset", zbDataConfig.Instance.operateCsvTable.GetRecordByKey(36).name, zbUtil.Language("lang.10691"), 800f, 1000f, 740f, 785f), 
			_ => new zbSubjectData("", "", ""), 
		};
	}
}
