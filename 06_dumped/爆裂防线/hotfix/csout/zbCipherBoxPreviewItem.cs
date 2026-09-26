using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbCipherBoxPreviewItem : zbViewBaseItem
{
	private zbCipherBoxPreViewWnd m_wndParent;

	private Label m_lbIndex;

	private zbItem m_award;

	private VisualElement[] m_cards = (VisualElement[])(object)new VisualElement[4];

	private VisualElement m_iconDone;

	public zbCipherBoxPreviewItem(VisualTreeAsset tAsset, zbCipherBoxPreViewWnd wndParent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_wndParent = wndParent;
		m_lbIndex = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_index", (string)null);
		m_award = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "award", (string)null);
		m_award.OnEnable(wndParent);
		for (int i = 0; i < 4; i++)
		{
			m_cards[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "card" + i, (string)null);
		}
		m_iconDone = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_done", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(CipherBoxTargetDetail detail, int index)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbIndex).text = index.ToString();
		zbItemData data = new zbItemData(detail.Award.Type, detail.Award.Tid, detail.Award.Count);
		m_award.Refresh(data);
		bool flag = detail.State == ECommonState.Yes;
		m_iconDone.visible = flag;
		if (flag)
		{
			int num = 0;
			string cipher = detail.Cipher;
			for (int i = 0; i < cipher.Length; i++)
			{
				int num2 = cipher[i] - 48;
				m_cards[num].style.backgroundImage = new StyleBackground(m_wndParent.bg_nums[num2 - 1]);
				num++;
			}
		}
	}
}
