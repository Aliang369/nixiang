using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPlayerSuitSkillItem : zbViewBaseItem
{
	private zbPlayerSuitSkillWnd m_Parent;

	private VisualElement m_spriteCell;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbLevel;

	private Label m_lbDesc;

	private Button m_btnDown;

	private Button m_btnSelect;

	private Button m_btnPreview;

	private int suitId;

	private int selectIndex;

	private bool m_isSuper;

	public zbPlayerSuitSkillItem(zbPlayerSuitSkillWnd parent)
	{
		m_Parent = parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_suitDetail.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteCell = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "skill_detail", (string)null);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "temp", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "detail_icon", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_btnDown = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_down", (string)null);
		m_btnSelect = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_select", (string)null);
		m_btnPreview = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_preview", (string)null);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDown, (Action)OnDown, "", (TrickleDown)0);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSelect, (Action)OnTouch, "", (TrickleDown)0);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPreview, (Action)OnPreview, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(int _suitId, int _selectIndex)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		suitId = _suitId;
		selectIndex = _selectIndex;
		zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suitId);
		if (recordByKey != null)
		{
			m_isSuper = recordByKey.grade == 2;
			string resName = (m_isSuper ? "ui/image/suitGun/skill_itembg1.png" : "ui/image/suitGun/skill_itembg.png");
			m_spriteCell.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
			string resName2 = (m_isSuper ? "ui/image/suitGun/skill_empty1.png" : "ui/image/suitGun/skill_empty.png");
			float num = (m_isSuper ? 1f : 0.8f);
			m_spriteIcon.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(num, num)));
			m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName2));
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon));
		}
		PlayerSuitDetail playerSuitDetail = zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(suitId);
		((TextElement)m_lbDesc).text = zbSelf.Instance.suitGunHandler.GetSkillDesc(suitId, playerSuitDetail.Star);
		((TextElement)m_lbLevel).text = "Lv." + playerSuitDetail.Star;
		int num2 = ((selectIndex >= 3) ? zbServerData.sGameData.SuitInfo.SuperUseList[selectIndex - 3] : zbServerData.sGameData.SuitInfo.UseList[selectIndex]);
		((VisualElement)m_btnDown).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((num2 != suitId) ? 1 : 0));
		((VisualElement)m_btnSelect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((num2 == suitId) ? 1 : 0));
	}

	private void OnDown()
	{
		if (m_isSuper)
		{
			zbWsMsgHandler.Instance.Suit_SuperSkillRemove_Req(selectIndex - 3);
		}
		else
		{
			zbWsMsgHandler.Instance.Suit_SkillRemove_Req(selectIndex);
		}
	}

	private void OnTouch()
	{
		if (m_isSuper)
		{
			zbWsMsgHandler.Instance.Suit_SuperSkillSet_Req(selectIndex - 3, suitId);
			return;
		}
		zbPlayerSuitCsvRecord recordByKey = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suitId);
		if (zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)recordByKey.skillType).HeroTid <= 0)
		{
			string name = zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey(recordByKey.skillType).name;
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10611"), name));
		}
		else
		{
			zbWsMsgHandler.Instance.Suit_SkillSet_Req(selectIndex, suitId);
		}
	}

	private void OnPreview()
	{
		if (suitId != 0)
		{
			zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(suitId, EPreviewType.Suit);
		}
	}
}
