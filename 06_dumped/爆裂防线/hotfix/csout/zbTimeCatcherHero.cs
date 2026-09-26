using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTimeCatcherHero : zbViewBaseItem
{
	private VisualElement m_rootHero;

	private Label m_lblFrag;

	private VisualElement m_heroTag;

	private Label m_lblName;

	private SpineVisualElement m_spineHero;

	public VisualElement heroRect;

	public zbCatcherHeroInfo catcherHeroInfo;

	private int m_heroTid;

	private Button m_btnTouch;

	public zbTimeCatcherHero(zbTimeCatcher wndParent)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		((VisualElement)this).style.width = StyleLength.op_Implicit(124f);
		((VisualElement)this).style.height = StyleLength.op_Implicit(230f);
		((VisualElement)this).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)this).style.left = StyleLength.op_Implicit(0f);
		((VisualElement)this).style.bottom = StyleLength.op_Implicit(10f);
		VisualElement val = (VisualElement)(object)zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_timecatcherhero.uxml").Instantiate();
		m_rootHero = UQueryExtensions.Q<VisualElement>(val, "hero", (string)null);
		((VisualElement)this).Add(m_rootHero);
		m_lblFrag = UQueryExtensions.Q<Label>(m_rootHero, "lblnum", (string)null);
		m_lblName = UQueryExtensions.Q<Label>(m_rootHero, "lb_name", (string)null);
		m_heroTag = UQueryExtensions.Q<VisualElement>(m_rootHero, "tag", (string)null);
		m_spineHero = UQueryExtensions.Q<SpineVisualElement>(m_rootHero, "spine_hero", (string)null);
		heroRect = UQueryExtensions.Q<VisualElement>(m_rootHero, "herorect", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_rootHero, "btn_touch", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI(m_rootHero);
	}

	public void Init(zbCatcherHeroInfo heroInfo)
	{
		catcherHeroInfo = heroInfo;
		if (m_heroTid != catcherHeroInfo.heroId)
		{
			m_heroTid = catcherHeroInfo.heroId;
			RefreshHeroDetail();
		}
		((TextElement)m_lblFrag).text = $"{catcherHeroInfo.coinCount * catcherHeroInfo.fragPerCoin}";
		PlayFragNumAction();
		SetHeroVisible(visible: true);
	}

	public void RefreshHeroDetail()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_heroTid);
		if (recordByKey != null)
		{
			((TextElement)m_lblName).text = recordByKey.name;
			string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath((ERarityType)recordByKey.rarity);
			m_heroTag.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
			zbHeroAvatarCsvRecord recordByKey2 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey.avatarId);
			if (recordByKey2 != null)
			{
				zbUtil.RefreshSpineUI(m_spineHero, recordByKey2.frontBone + "_SkeletonData.asset", "run", loop: true);
			}
		}
	}

	private void OnTouch()
	{
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(m_heroTid);
		zbHeroRarityCsvRecord recordByKey2 = zbDataConfig.Instance.heroRarityCsvTable.GetRecordByKey(recordByKey.rarity);
		if (recordByKey2 != null)
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Hero, recordByKey.id, recordByKey2.frag);
		}
	}

	private void PlayFragNumAction()
	{
		if (m_lblFrag != null)
		{
			zbUIEffect.CreateScaleActionOnce((VisualElement)(object)m_lblFrag, 2f, 0.5f);
		}
		if (m_lblName != null)
		{
			zbUIEffect.CreateScaleActionOnce((VisualElement)(object)m_lblName, 2f, 0.5f);
		}
	}

	public void SetHeroVisible(bool visible)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_rootHero.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
	}
}
