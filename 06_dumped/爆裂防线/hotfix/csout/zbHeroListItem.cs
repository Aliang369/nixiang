using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroListItem : zbViewBaseItem
{
	private zbHeroListWnd m_wndParent;

	private VisualElement m_spriteLine;

	private Button m_btnFollow;

	private VisualElement m_spriteSkillType;

	private Label m_lbDamage;

	private ScrollView m_scrollView;

	private VisualElement m_notifyFollow;

	private ESkillType m_skillType;

	private List<zbHeroCsvRecord> m_records;

	private List<zbHeroListSubItem> m_heroItems;

	public zbHeroListItem(zbHeroListWnd wndParent)
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		m_heroItems = new List<zbHeroListSubItem>();
		base._002Ector();
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/hero/tb_heroList.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteLine = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_line", (string)null);
		m_btnFollow = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_follow", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFollow, (Action)OnFollow, "", (TrickleDown)0);
		m_spriteSkillType = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_skillType", (string)null);
		m_lbDamage = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_damage", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)this, "scrollView", (string)null);
		m_notifyFollow = zbUtil.CreateNotifyTag((VisualElement)(object)m_lbDamage, (Vector2?)new Vector2(-10f, -5f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbSkillTypeCsvRecord skillTypeRecord, int index)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		m_skillType = (ESkillType)skillTypeRecord.id;
		m_spriteLine.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(index == 0));
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)skillTypeRecord.id);
		HeroDetail heroDetail = zbSelf.Instance.heroHandler.GetHeroDetail(warRoomDetailBySkillType.HeroTid);
		((VisualElement)m_lbDamage).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.HeroFollow) || heroDetail == null || m_skillType == ESkillType.Qiang) ? 1 : 0));
		m_notifyFollow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroFollowNotify(m_skillType)));
		if (heroDetail != null)
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroDetail.HeroTid);
			if (recordByKey != null)
			{
				string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(recordByKey, heroDetail.Rarity);
				m_spriteSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
				if (zbSelf.Instance.heroHandler.CanFollow(m_skillType))
				{
					float num = zbSelf.Instance.heroHandler.GetDamage(heroDetail.Quality, heroDetail.Rarity, heroDetail.QualityStar) + zbSelf.Instance.heroHandler.GetFollowDamages(m_skillType);
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
			}
		}
		else
		{
			m_spriteSkillType.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeRecord.icon1));
			((VisualElement)m_lbDamage).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnFollow).SetEnabled(false);
			((VisualElement)m_btnFollow).style.opacity = StyleFloat.op_Implicit(1f);
		}
		RefreshList(skillTypeRecord);
	}

	private void RefreshList(zbSkillTypeCsvRecord skillTypeRecord)
	{
		m_records = zbSelf.Instance.heroHandler.GetHeroRecordsBySkillType(skillTypeRecord.id);
		if (m_records.Count != m_heroItems.Count)
		{
			m_heroItems.Clear();
			((VisualElement)m_scrollView).Clear();
			foreach (zbHeroCsvRecord record in m_records)
			{
				_ = record;
				zbHeroListSubItem zbHeroListSubItem2 = new zbHeroListSubItem(m_wndParent);
				m_heroItems.Add(zbHeroListSubItem2);
				((VisualElement)m_scrollView).Add((VisualElement)(object)zbHeroListSubItem2);
			}
		}
		for (int i = 0; i < m_heroItems.Count; i++)
		{
			m_heroItems[i].Refresh(m_records[i]);
		}
	}

	private void OnFollow()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.HeroFollow))
		{
			zbUIMgr.Instance.ShowHeroFollow(m_skillType);
		}
	}

	public void Update()
	{
		foreach (zbHeroListSubItem heroItem in m_heroItems)
		{
			heroItem.update();
		}
	}
}
