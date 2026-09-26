using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonEventPopUp : zbBaseWnd
{
	private Label m_title;

	private Label m_lblDesc;

	private VisualElement m_wrapper;

	private VisualElement m_panel0;

	private VisualElement m_panel1;

	private Sprite m_propBg;

	private Sprite m_buffBg;

	private Sprite m_debuffBg;

	private Sprite m_iconHealthGreen;

	private Sprite m_iconHealthRed;

	private Sprite m_iconRiotGreen;

	private Sprite m_iconRiotRed;

	private Button m_btnOk;

	protected override void OnEnable()
	{
		base.OnEnable();
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_title = UQueryExtensions.Q<Label>(val, "title", (string)null);
		m_lblDesc = UQueryExtensions.Q<Label>(val, "desc", (string)null);
		m_wrapper = UQueryExtensions.Q<VisualElement>(val, "wrapper", (string)null);
		m_panel0 = UQueryExtensions.Q<VisualElement>(val, "panel0", (string)null);
		m_panel1 = UQueryExtensions.Q<VisualElement>(val, "panel1", (string)null);
		UQueryExtensions.Q<VisualElement>(val, "info", (string)null);
		m_btnOk = UQueryExtensions.Q<Button>(val, "btnok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOk, (Action<ClickEvent>)OnSure, "", (TrickleDown)0);
		m_propBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/dungeon/bg_prop.png");
		m_buffBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/dungeon/bg_buff.png");
		m_debuffBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/dungeon/bg_debuff.png");
		m_iconHealthGreen = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/dungeon/icon_healthGreen.png");
		m_iconHealthRed = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/dungeon/icon_healthRed.png");
		m_iconRiotGreen = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/dungeon/icon_riotGreen.png");
		m_iconRiotRed = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/dungeon/icon_riotRed.png");
		AddDark();
	}

	private void InitElement()
	{
	}

	public void RefreshCustom(DungeonTriggerEventInfo info)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		InitElement();
		if (info == null || info.EventPopup == null)
		{
			return;
		}
		bool flag = info.EventPopup.HP != 0f;
		m_panel0.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			bool flag2 = info.EventPopup.HP > 0f;
			((TextElement)UQueryExtensions.Q<Label>(m_panel0, "lb_name", (string)null)).text = "Hp";
			((TextElement)UQueryExtensions.Q<Label>(m_panel0, "lb_count", (string)null)).text = info.EventPopup.HP + "%";
			m_panel0.style.backgroundImage = new StyleBackground(flag2 ? m_buffBg : m_debuffBg);
			UQueryExtensions.Q<VisualElement>(m_panel0, "icon", (string)null).style.backgroundImage = new StyleBackground(flag2 ? m_iconHealthGreen : m_iconHealthRed);
		}
		bool flag3 = info.EventPopup.Riot != 0;
		m_panel1.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
		if (flag3)
		{
			bool flag4 = info.EventPopup.Riot < 0;
			((TextElement)UQueryExtensions.Q<Label>(m_panel1, "lb_name", (string)null)).text = zbUtil.Language("lang.10307");
			((TextElement)UQueryExtensions.Q<Label>(m_panel1, "lb_count", (string)null)).text = ((info.EventPopup.Riot > 0) ? $"+{info.EventPopup.Riot}" : info.EventPopup.Riot.ToString());
			m_panel1.style.backgroundImage = new StyleBackground(flag4 ? m_buffBg : m_debuffBg);
			UQueryExtensions.Q<VisualElement>(m_panel1, "icon", (string)null).style.backgroundImage = new StyleBackground(flag4 ? m_iconRiotGreen : m_iconRiotRed);
		}
		if (info.EventPopup.AwardList != null)
		{
			for (int i = 0; i < info.EventPopup.AwardList.Count; i++)
			{
				CreateAndAddAward(info.EventPopup.AwardList[i]);
			}
		}
		zbDungeonEventCsvRecord recordByKey = zbDataConfig.Instance.dungeonEventCsvTable.GetRecordByKey(info.EventId);
		if (recordByKey != null)
		{
			((VisualElement)m_lblDesc).visible = true;
			((TextElement)m_lblDesc).text = recordByKey.desc;
			((TextElement)m_title).text = recordByKey.name;
		}
	}

	private void CreateAndAddAward(AwardDetail awardDetail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Expected O, but got Unknown
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		VisualElement val = new VisualElement();
		val.name = "prop";
		val.style.width = StyleLength.op_Implicit(129f);
		val.style.height = StyleLength.op_Implicit(162f);
		val.style.backgroundImage = new StyleBackground(m_propBg);
		val.style.marginRight = StyleLength.op_Implicit(20f);
		Label val2 = new Label();
		float num = 22f;
		((TextElement)val2).text = zbUtil.Language("lang.10410");
		Vector2 val3 = ((TextElement)val2).MeasureTextSize(((TextElement)val2).text, 9999f, (MeasureMode)0, 0f, (MeasureMode)0);
		if (val3.x > 120f)
		{
			num = 14f;
		}
		((VisualElement)val2).style.fontSize = StyleLength.op_Implicit(num);
		((VisualElement)val2).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)val2).style.top = StyleLength.op_Implicit(0f);
		((VisualElement)val2).style.width = StyleLength.op_Implicit(128f);
		((VisualElement)val2).style.height = StyleLength.op_Implicit(32f);
		((VisualElement)val2).style.marginBottom = StyleLength.op_Implicit(0f);
		((VisualElement)val2).style.marginTop = StyleLength.op_Implicit(0f);
		((VisualElement)val2).style.marginLeft = StyleLength.op_Implicit(0f);
		((VisualElement)val2).style.marginRight = StyleLength.op_Implicit(0f);
		((VisualElement)val2).style.paddingBottom = StyleLength.op_Implicit(0f);
		((VisualElement)val2).style.paddingTop = StyleLength.op_Implicit(0f);
		((VisualElement)val2).style.paddingLeft = StyleLength.op_Implicit(0f);
		((VisualElement)val2).style.paddingRight = StyleLength.op_Implicit(0f);
		((VisualElement)val2).style.unityTextAlign = StyleEnum<TextAnchor>.op_Implicit((TextAnchor)4);
		((VisualElement)val2).style.color = StyleColor.op_Implicit(new Color(0.54509807f, 23f / 85f, 0.07450981f, 1f));
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)val2);
		val.Add((VisualElement)(object)val2);
		Label val4 = new Label();
		float num2 = 18f;
		((TextElement)val4).text = zbResourceHelper.GetResorceIconPath(awardDetail.Type, awardDetail.Tid).strName + "+" + awardDetail.Count;
		((TextElement)val4).MeasureTextSize(((TextElement)val4).text, 9999f, (MeasureMode)0, 0f, (MeasureMode)0);
		if (val3.x > 120f)
		{
			num = 14f;
		}
		((VisualElement)val4).style.fontSize = StyleLength.op_Implicit(num2);
		((VisualElement)val4).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)val4).style.whiteSpace = StyleEnum<WhiteSpace>.op_Implicit((WhiteSpace)0);
		((VisualElement)val4).style.bottom = StyleLength.op_Implicit(15f);
		((VisualElement)val4).style.width = StyleLength.op_Implicit(128f);
		((VisualElement)val4).style.marginBottom = StyleLength.op_Implicit(0f);
		((VisualElement)val4).style.marginTop = StyleLength.op_Implicit(0f);
		((VisualElement)val4).style.marginLeft = StyleLength.op_Implicit(0f);
		((VisualElement)val4).style.marginRight = StyleLength.op_Implicit(0f);
		((VisualElement)val4).style.paddingBottom = StyleLength.op_Implicit(0f);
		((VisualElement)val4).style.paddingTop = StyleLength.op_Implicit(0f);
		((VisualElement)val4).style.paddingLeft = StyleLength.op_Implicit(0f);
		((VisualElement)val4).style.paddingRight = StyleLength.op_Implicit(0f);
		((VisualElement)val4).style.unityTextAlign = StyleEnum<TextAnchor>.op_Implicit((TextAnchor)4);
		((VisualElement)val4).style.color = StyleColor.op_Implicit(new Color(0.54509807f, 23f / 85f, 0.07450981f, 1f));
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)val4);
		val.Add((VisualElement)(object)val4);
		zbItem zbItem2 = new zbItem();
		zbItem2.OnEnable(this);
		zbItem2.Refresh(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count));
		((VisualElement)zbItem2).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)zbItem2).transform.scale = new Vector3(0.6f, 0.6f, 1f);
		((VisualElement)zbItem2).style.top = StyleLength.op_Implicit(12f);
		((VisualElement)zbItem2).style.left = StyleLength.op_Implicit(5f);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)zbItem2);
		val.Add((VisualElement)(object)zbItem2);
		m_wrapper.Add(val);
	}

	private void OnSure(ClickEvent e)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Hide();
		List<VisualElement> list = UQueryExtensions.Query<VisualElement>(m_wrapper, "prop", (string)null).ToList();
		zbDungeonWnd zbDungeonWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Dungeon) as zbDungeonWnd;
		foreach (VisualElement item in list)
		{
			FlyToTarget(UQueryExtensions.Q<VisualElement>(item, "icon", (string)null), (VisualElement)(object)zbDungeonWnd2.m_btnLog, zbDungeonWnd2.GetRootElement());
			VisualElement parent = item.parent;
			if (parent != null)
			{
				parent.Remove(item);
			}
		}
		zbDungeonMgr.Instance.dungeonHandler.DungeonEventPopConfirm_Req();
	}

	private void FlyToTarget(VisualElement from, VisualElement to, VisualElement container)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		VisualElement flying = new VisualElement();
		flying.style.width = StyleLength.op_Implicit(from.resolvedStyle.width);
		flying.style.height = StyleLength.op_Implicit(from.resolvedStyle.height);
		flying.style.backgroundImage = StyleBackground.op_Implicit(from.resolvedStyle.backgroundImage);
		flying.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		container.Add(flying);
		Rect worldBound = from.worldBound;
		float x = ((Rect)(ref worldBound)).center.x;
		worldBound = from.worldBound;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(x, ((Rect)(ref worldBound)).center.y, 0f);
		worldBound = to.worldBound;
		float num = ((Rect)(ref worldBound)).center.x - 55f;
		worldBound = to.worldBound;
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(num, ((Rect)(ref worldBound)).center.y - 55f, 0f);
		Vector3 position = Vector2.op_Implicit(VisualElementExtensions.WorldToLocal(container, Vector2.op_Implicit(val)));
		Vector3 val3 = Vector2.op_Implicit(VisualElementExtensions.WorldToLocal(container, Vector2.op_Implicit(val2)));
		flying.transform.position = position;
		flying.experimental.animation.Position(val3, 1000);
		flying.schedule.Execute((Action)delegate
		{
			container.Remove(flying);
		}).ExecuteLater(1000L);
	}
}
