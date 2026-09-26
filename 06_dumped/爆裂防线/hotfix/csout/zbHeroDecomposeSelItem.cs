using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroDecomposeSelItem : zbViewBaseItem
{
	private zbItem m_item = new zbItem();

	private Label m_lbNum;

	private Label m_lbCount;

	private VisualElement m_spriteSkillType;

	private Button m_btnTouch;

	private VisualElement m_spriteOk;

	private zbHeroDecomposeSelWnd m_wndParent;

	private ResourceDetail m_data;

	private zbHeroRarityCsvRecord m_rarityRecord;

	public zbHeroDecomposeSelItem(zbHeroDecomposeSelWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/hero/tb_heroDecomposeSel.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_lbNum = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_num", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_spriteSkillType = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_skillType", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "sprite_ok", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(ResourceDetail data)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_item.Refresh(new zbItemData(EResourceType.HeroFrag, data.Tid, data.Count), isNameVis: false, isCountVis: false);
		long selCount = m_wndParent.GetSelCount(m_data.Tid);
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((selCount <= 0) ? 1 : 0));
		((TextElement)m_lbNum).text = selCount + "/" + m_data.Count;
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_data.Tid);
		if (recordByKey != null)
		{
			m_rarityRecord = zbDataConfig.Instance.heroRarityCsvTable.GetRecordByKey(recordByKey.rarity);
			if (m_rarityRecord != null)
			{
				((TextElement)m_lbCount).text = (selCount * m_rarityRecord.soul).ToString();
			}
			string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(recordByKey);
			m_spriteSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_wndParent.RefreshDecomposeList(1, m_data);
			((TextElement)m_lbNum).text = "0/" + m_data.Count;
			((TextElement)m_lbCount).text = "0";
			return;
		}
		if (m_wndParent.decomposeList.Count >= 6)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10259"));
			return;
		}
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (m_data.Count > 1)
		{
			zbUIMgr.Instance.ShowHeroDecomposeSelCount(m_data, delegate(ResourceDetail data, long count)
			{
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				((TextElement)m_lbNum).text = count + "/" + m_data.Count;
				((TextElement)m_lbCount).text = (count * m_rarityRecord.soul).ToString();
				if (count == 0L)
				{
					m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
				else
				{
					m_wndParent.AddToDecomposeList(m_data, count);
				}
			});
		}
		else
		{
			((TextElement)m_lbNum).text = "1/" + m_data.Count;
			((TextElement)m_lbCount).text = m_rarityRecord.soul.ToString();
			m_wndParent.RefreshDecomposeList(2, m_data);
		}
	}
}
