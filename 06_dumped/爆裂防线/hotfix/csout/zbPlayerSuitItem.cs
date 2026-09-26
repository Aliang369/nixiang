using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerSuitItem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private VisualElement m_select;

	private VisualElement m_current;

	private VisualElement m_lock;

	private zbPlayerSuitWnd m_parent;

	public zbPlayerSuitCsvRecord m_record;

	public int indexSort;

	private VisualElement[] m_spriteStars = (VisualElement[])(object)new VisualElement[5];

	public zbPlayerSuitItem(zbPlayerSuitWnd parent, int _indexSort)
	{
		m_parent = parent;
		indexSort = _indexSort;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_suit.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_select = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_sel", (string)null);
		m_current = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_OK", (string)null);
		m_lock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_lock", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "iconstar" + i, (string)null);
		}
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void RefreshBase()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(m_record.avatarId);
		if (recordByKey != null)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.heroIcon);
			if ((Object)(object)val != (Object)null)
			{
				m_spriteHero.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_spriteHero.MarkDirtyRepaint();
			}
		}
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex((EQuality)m_record.quality);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(heroFrameTex);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteFrame.MarkDirtyRepaint();
		}
	}

	public void Refresh(zbPlayerSuitCsvRecord record)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		if (m_record == null || m_record.id != record.id)
		{
			m_record = record;
			RefreshBase();
		}
		m_current.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetSuitId() != m_record.id) ? 1 : 0));
		for (int i = 0; i < 5; i++)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/suitGun/star_0.png");
			m_spriteStars[i].style.backgroundImage = StyleBackground.op_Implicit(val);
		}
		PlayerSuitDetail playerSuitDetail = zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(m_record.id);
		if (playerSuitDetail == null)
		{
			m_spriteFrame.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(new Color(0.5f, 0.5f, 0.5f, 1f));
			m_spriteHero.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(new Color(0.5f, 0.5f, 0.5f, 1f));
			m_lock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			zbUIEffect.SetStars(m_spriteStars, playerSuitDetail.Star);
			m_spriteFrame.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(new Color(1f, 1f, 1f, 1f));
			m_spriteHero.style.unityBackgroundImageTintColor = StyleColor.op_Implicit(new Color(1f, 1f, 1f, 1f));
			m_lock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.suitGunHandler.CanSuitActiveOrUpgrade(m_record.id)));
	}

	public void RefreshSelect(bool flag)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_select.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	public void OnTouch()
	{
		m_parent.m_selectSuitId = m_record.id;
		m_parent.indexSort = indexSort;
		m_parent.RefreshCustom();
	}
}
