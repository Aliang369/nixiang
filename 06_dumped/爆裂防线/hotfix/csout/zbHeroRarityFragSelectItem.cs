using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroRarityFragSelectItem : zbViewBaseItem
{
	private zbItem m_item = new zbItem();

	private Label m_lbNum;

	private VisualElement m_spriteSkillType;

	private Button m_btnTouch;

	private VisualElement m_spriteOk;

	private zbHeroRarityFragSelectWnd m_wndParent;

	private ResourceDetail m_data;

	public zbHeroRarityFragSelectItem(zbHeroRarityFragSelectWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/hero/tb_heroRarityFragSelect.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_lbNum = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_num", (string)null);
		m_spriteSkillType = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_skillType", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "sprite_ok", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(ResourceDetail data)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_item.Refresh(new zbItemData(EResourceType.HeroFrag, data.Tid, data.Count), isNameVis: false, isCountVis: false);
		ERarityType rarity = zbSelf.Instance.heroHandler.GetHeroDetail(data.Tid).Rarity;
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex((EQuality)(rarity + 2));
		m_item.m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resourceFrameTex));
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_data.Tid);
		if (recordByKey != null)
		{
			string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(recordByKey, rarity);
			m_spriteSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
		}
		long num = m_wndParent.m_selectList.FirstOrDefault((BagSelectDetail x) => x.Tid == m_data.Tid)?.Count ?? 0;
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((num <= 0) ? 1 : 0));
		((TextElement)m_lbNum).text = num + "/" + m_data.Count;
	}

	public void OnTouch(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			m_wndParent.SetSelect(m_data.Tid, 0L);
			Refresh(m_data);
			return;
		}
		long num = m_wndParent.m_needCount - m_wndParent.m_selectList.Sum((BagSelectDetail x) => x.Count);
		if (num <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11074"));
		}
		else if (m_wndParent.m_selectList.Count >= 6)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10259"));
		}
		else if (m_data.Count > 1 && num > 1)
		{
			ResourceDetail data = new ResourceDetail
			{
				Tid = m_data.Tid,
				Count = Math.Min(num, m_data.Count)
			};
			zbUIMgr.Instance.ShowHeroDecomposeSelCount(data, delegate(ResourceDetail resourceDetail, long count)
			{
				if (count > 0)
				{
					m_wndParent.SetSelect(resourceDetail.Tid, count);
					Refresh(m_data);
				}
			});
		}
		else
		{
			m_wndParent.SetSelect(m_data.Tid, 1L);
			Refresh(m_data);
		}
	}
}
