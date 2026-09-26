using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBarOperateItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbBarOperateItem>
	{
	}

	private zbLotteryWnd m_wndParent;

	private zbVipTabBarOperate m_parent;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private VisualElement m_spriteRarity;

	private VisualElement m_spriteSkillType;

	private VisualElement m_spriteSelect;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private LimitTimeHeroDetail m_timeHeroDetail;

	public zbBarOperateItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/vip/tb_barOperate.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbLotteryWnd wndParent, zbVipTabBarOperate parent)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_parent = parent;
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_spriteRarity = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_rarity", (string)null);
		m_spriteSkillType = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_skillType", (string)null);
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "select", (string)null);
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(LimitTimeHeroDetail timeHeroDetail)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		m_timeHeroDetail = timeHeroDetail;
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Hero, m_timeHeroDetail.BaseData.HeroTid);
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex(zbResourceHelper.GetResourceQuality(EResourceType.Hero, m_timeHeroDetail.BaseData.HeroTid));
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resourceFrameTex));
		string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath((ERarityType)zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_timeHeroDetail.BaseData.HeroTid).rarity);
		m_spriteRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
		string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_timeHeroDetail.BaseData.HeroTid));
		m_spriteSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.operateHandler.HasTimeHeroBarGift(m_timeHeroDetail.BaseInfo.UUID) && !zbSelf.Instance.operateHandler.HasTimeHeroScoreGift(m_timeHeroDetail.BaseInfo.UUID)) ? 1 : 0));
	}

	public void RefreshSelect(string uuid)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (m_timeHeroDetail != null)
		{
			bool flag = m_timeHeroDetail.BaseInfo.UUID == uuid;
			m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		m_parent.RefreshSelectList(m_timeHeroDetail.BaseInfo.UUID);
	}
}
