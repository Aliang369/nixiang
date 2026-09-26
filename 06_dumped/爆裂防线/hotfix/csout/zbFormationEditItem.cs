using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFormationEditItem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private VisualElement m_spriteRarity;

	private VisualElement m_spriteSkill;

	private Label m_lbName;

	private Label m_lbLevel;

	private VisualElement m_spriteMask;

	private VisualElement m_spriteSelect;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private VisualElement m_new;

	private zbFormationWnd m_wndParent;

	private zbHeroCsvRecord m_record;

	private HeroDetail m_data;

	public zbFormationEditItem(zbFormationWnd wndParent)
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/formation/tb_formationEdit.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_spriteRarity = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_rarity", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_skill", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_spriteMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_mask", (string)null);
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_select", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-3f, 0f), 1f, true);
		m_new = zbUtil.CreateNewTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-10f, -15f), 0.9f);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbHeroCsvRecord record)
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		((TextElement)m_lbName).text = m_record.name;
		int avatarId = m_record.avatarId;
		EQuality quality = zbResourceHelper.GetResourceQuality(EResourceType.Hero, m_record.id);
		ERarityType rarity = (ERarityType)m_record.rarity;
		m_data = zbSelf.Instance.heroHandler.GetHeroDetail(m_record.id);
		if (m_data != null)
		{
			avatarId = m_data.AvatarId;
			quality = m_data.Quality;
			rarity = m_data.Rarity;
		}
		zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(avatarId);
		if (recordByKey != null)
		{
			m_spriteHero.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.heroIcon));
		}
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex(quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath(rarity);
		m_spriteRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
		string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(m_record, rarity);
		m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)m_record.type);
		if (warRoomDetailBySkillType != null)
		{
			HomelandRoomDetail homelandRoomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(warRoomDetailBySkillType.RoomId);
			((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbLevel).text = "Lv." + homelandRoomDetail.Level;
		}
		else
		{
			((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbUtil.SetElementGray(m_spriteRarity, m_data == null);
		zbUtil.SetElementGray(m_spriteSkill, m_data == null);
		m_spriteMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data != null));
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.heroHandler.GetWarRoomDetailByHeroTid(m_record.id) == null));
		bool flag = (m_data != null && zbSelf.Instance.heroHandler.HasHeroUpgradeNotify(m_record.id)) || zbSelf.Instance.heroHandler.HasHeroAwakenNotify(m_record.id) || zbSelf.Instance.heroHandler.HasHeroStarNotify(m_record.id) || zbSelf.Instance.heroHandler.HasHeroRarityNotify(m_record.id) || zbSelf.Instance.geneHandler.HasHeroTabGeneNotify((ESkillType)m_record.type, m_record.id);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_new.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((flag || !zbSelf.Instance.newTagHandler.IsNewHero(m_record.id)) ? 1 : 0));
	}

	public void OnTouch(ClickEvent evt)
	{
		zbSelf.Instance.newTagHandler.RemoveNewHero(m_record.id);
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Hero);
		zbUIMgr.Instance.ShowHeroDetail(m_record);
	}
}
