using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbArenaGiftItem : zbViewBaseItem
{
	private zbArenaGiftWnd m_wndParent;

	private Label m_lbRank;

	private VisualElement m_spriteLock;

	public VisualElement[] m_spriteBgs = (VisualElement[])(object)new VisualElement[3];

	private SpineVisualElement[] m_spines = (SpineVisualElement[])(object)new SpineVisualElement[3];

	private VisualElement[] m_spriteIcons = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_soldOuts = (VisualElement[])(object)new VisualElement[3];

	private Label[] m_lbNames = (Label[])(object)new Label[3];

	private Label[] m_lbLimteds = (Label[])(object)new Label[3];

	private Label[] m_lbCounts = (Label[])(object)new Label[3];

	private VisualElement[] m_iconBonus = (VisualElement[])(object)new VisualElement[3];

	private Label[] m_lbBonus = (Label[])(object)new Label[3];

	private Button[] m_btnBuys = (Button[])(object)new Button[3];

	private Label[] m_lbCosts = (Label[])(object)new Label[3];

	private Label[] m_lbFrees = (Label[])(object)new Label[3];

	private Button[] m_btnItemTips = (Button[])(object)new Button[3];

	private VisualElement[] m_notifys = (VisualElement[])(object)new VisualElement[3];

	private int m_layer;

	public zbArenaGiftCsvRecord[] m_records;

	public zbArenaGiftItem(zbArenaGiftWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/arena/tb_arenaGift.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		m_spriteLock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "lock", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_spriteLock, (Action<ClickEvent>)OnLock, "", (TrickleDown)0);
		for (int i = 0; i < 3; i++)
		{
			m_spriteBgs[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg" + i, (string)null);
			m_spines[i] = UQueryExtensions.Q<SpineVisualElement>(m_spriteBgs[i], "spine", (string)null);
			m_spriteIcons[i] = UQueryExtensions.Q<VisualElement>(m_spriteBgs[i], "icon", (string)null);
			m_lbNames[i] = UQueryExtensions.Q<Label>(m_spriteBgs[i], "lb_name", (string)null);
			m_lbLimteds[i] = UQueryExtensions.Q<Label>(m_spriteBgs[i], "lb_limited", (string)null);
			m_lbCounts[i] = UQueryExtensions.Q<Label>(m_spriteBgs[i], "lb_count", (string)null);
			m_soldOuts[i] = UQueryExtensions.Q<VisualElement>(m_spriteBgs[i], "soldOut", (string)null);
			m_iconBonus[i] = UQueryExtensions.Q<VisualElement>(m_spriteBgs[i], "icon_bonus", (string)null);
			m_lbBonus[i] = UQueryExtensions.Q<Label>(m_spriteBgs[i], "lb_bonus", (string)null);
			m_btnBuys[i] = UQueryExtensions.Q<Button>(m_spriteBgs[i], "btn_buy", (string)null);
			m_lbCosts[i] = UQueryExtensions.Q<Label>(m_spriteBgs[i], "lb_cost", (string)null);
			m_lbFrees[i] = UQueryExtensions.Q<Label>(m_spriteBgs[i], "lb_free", (string)null);
			m_btnItemTips[i] = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_item" + i, (string)null);
			m_notifys[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnBuys[i]);
			int tempIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnItemTips[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnItemTips(evt, tempIndex);
			}, "", (TrickleDown)0);
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuys[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnBuys(evt, tempIndex);
			}, "", (TrickleDown)0);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int layer)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		m_layer = layer;
		m_records = zbSelf.Instance.arenaHandler.GetArenaGiftRecordsByLayer(layer);
		((TextElement)m_lbRank).text = layer.ToString();
		m_spriteLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbServerData.sGameData.ArenaInfo.BestRank <= layer) ? 1 : 0));
		for (int i = 0; i < 3; i++)
		{
			m_spriteBgs[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnItemTips[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		zbArenaGiftCsvRecord[] records = m_records;
		foreach (zbArenaGiftCsvRecord zbArenaGiftCsvRecord2 in records)
		{
			if (num < m_records.Length)
			{
				RefreshItem(num, zbArenaGiftCsvRecord2);
				m_spriteBgs[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_btnItemTips[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_notifys[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.arenaHandler.HasGiftNotifyById(zbArenaGiftCsvRecord2.id)));
			}
			num++;
		}
	}

	public void RefreshItem(int index, zbArenaGiftCsvRecord record)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		string resName = ((record.price == 0) ? "ui/image/button/btn_260_88_green.png" : "ui/image/button/btn_260_88_yellow.png");
		((VisualElement)m_btnBuys[index]).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
		m_iconBonus[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(record.bonus != "")));
		((TextElement)m_lbBonus[index]).text = record.bonus;
		string shopFrameTex = zbResourceHelper.GetShopFrameTex(zbResourceHelper.GetResourceQuality((EResourceType)record.award[0], record.award[1]));
		m_spriteBgs[index].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(shopFrameTex));
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)record.award[0], record.award[1]);
		m_spriteIcons[index].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		((TextElement)m_lbNames[index]).text = resorceIconPath.strName;
		((TextElement)m_lbCounts[index]).text = "x" + record.award[2];
		string text = zbUtil.Language("lang.10879");
		if (record.refresh == 1)
		{
			text = zbUtil.Language("lang.10123");
		}
		else if (record.refresh == 2)
		{
			text = zbUtil.Language("lang.10124");
		}
		else if (record.refresh == 3)
		{
			text = zbUtil.Language("lang.10125");
		}
		bool num = record.limitNum <= 0;
		int arenaGiftSaleCount = zbSelf.Instance.arenaHandler.GetArenaGiftSaleCount(record.id);
		if (num)
		{
			((VisualElement)m_lbLimteds[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_soldOuts[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_lbLimteds[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbLimteds[index]).text = text + ":" + (record.limitNum - arenaGiftSaleCount) + "/" + record.limitNum;
			m_soldOuts[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((arenaGiftSaleCount < record.limitNum) ? 1 : 0));
			if (arenaGiftSaleCount >= record.limitNum)
			{
				((VisualElement)m_btnBuys[index]).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_260x88.png"));
			}
		}
		if (record.moneyType > 0)
		{
			((VisualElement)m_lbCosts[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbFrees[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			stResorceIconInfo resorceIconPath2 = zbResourceHelper.GetResorceIconPath(EResourceType.Money, record.moneyType);
			((VisualElement)m_lbCosts[index]).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath2.strIconPath));
			((TextElement)m_lbCosts[index]).text = record.price.ToString();
		}
		else
		{
			((VisualElement)m_lbCosts[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbFrees[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		((VisualElement)m_spines[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(record.effect != 1));
	}

	private void OnItemTips(ClickEvent evt, int index)
	{
		if (m_records[index] != null)
		{
			zbUIMgr.Instance.ShowTip((EResourceType)m_records[index].award[0], m_records[index].award[1], 0L);
		}
	}

	private void OnBuys(ClickEvent evt, int index)
	{
		zbArenaGiftCsvRecord zbArenaGiftCsvRecord2 = m_records[index];
		if (zbArenaGiftCsvRecord2 != null)
		{
			if (zbArenaGiftCsvRecord2.limitNum > 0 && zbSelf.Instance.arenaHandler.GetArenaGiftSaleCount(zbArenaGiftCsvRecord2.id) >= zbArenaGiftCsvRecord2.limitNum)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10393"));
			}
			else if (zbSelf.Instance.moneyHandler.HasEnoughMoney(zbArenaGiftCsvRecord2.moneyType, zbArenaGiftCsvRecord2.price))
			{
				zbWsMsgHandler.Instance.Arena_GiftBuy_Req(zbArenaGiftCsvRecord2.id);
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, zbArenaGiftCsvRecord2.moneyType, zbArenaGiftCsvRecord2.price);
			}
		}
	}

	private void OnLock(ClickEvent evt)
	{
		zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10880"), m_layer));
	}
}
