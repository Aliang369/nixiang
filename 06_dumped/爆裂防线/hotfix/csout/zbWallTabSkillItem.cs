using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbWallTabSkillItem : zbViewBaseItem
{
	private zbWallWnd m_parentWnd;

	private VisualElement m_spriteIcon;

	private Label m_lbLevel;

	private Label m_lbDesc;

	private Button m_btnDown;

	private Button m_btnSelect;

	private Button m_btnPreview;

	private int m_wallId;

	private int m_selectIndex;

	public zbWallTabSkillItem(zbWallWnd parentWnd)
	{
		m_parentWnd = parentWnd;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/wall/tb_wallSkill.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "detail_icon", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_btnDown = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_down", (string)null);
		m_btnSelect = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_select", (string)null);
		m_btnPreview = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_preview", (string)null);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDown, (Action)OnDown, "", (TrickleDown)0);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSelect, (Action)OnTouch, "", (TrickleDown)0);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPreview, (Action)OnPreview, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(int wallId, int selectIndex)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		m_wallId = wallId;
		m_selectIndex = selectIndex;
		zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(m_wallId);
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon));
		WallDetail wallDetail = zbSelf.Instance.wallHandler.GetWallDetail(m_wallId);
		((TextElement)m_lbDesc).text = zbSelf.Instance.wallHandler.GetSkillDesc(m_wallId, wallDetail.Star);
		((TextElement)m_lbLevel).text = "Lv." + wallDetail.Star;
		int num = zbServerData.sGameData.WallInfo.UseList[m_selectIndex];
		((VisualElement)m_btnDown).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((num != m_wallId) ? 1 : 0));
		((VisualElement)m_btnSelect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((num == m_wallId) ? 1 : 0));
	}

	private void OnDown()
	{
		zbWsMsgHandler.Instance.Wall_SkillRemove_Req(m_selectIndex);
	}

	private void OnTouch()
	{
		zbWsMsgHandler.Instance.Wall_SkillSet_Req(m_selectIndex, m_wallId);
	}

	private void OnPreview()
	{
		zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(m_wallId, EPreviewType.Wall);
	}
}
