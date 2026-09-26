using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerGunSkillItem : zbViewBaseItem
{
	private zbPlayerGunWnd m_Parent;

	private VisualElement m_spriteCell;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbLevel;

	private Label m_lbDesc;

	private Button m_btnDown;

	private Button m_btnSelect;

	private int gunId;

	private int bulletId;

	private bool m_isSuper;

	public zbPlayerGunSkillItem(zbPlayerGunWnd parent)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_Parent = parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_gunSkill.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteCell = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "skill_detail", (string)null);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "temp", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "detail_icon", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_btnDown = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_down", (string)null);
		m_btnSelect = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_select", (string)null);
		((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDown, (Action)OnDown, "", (TrickleDown)0);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSelect, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(int GunId, int _bulletId)
	{
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		gunId = GunId;
		bulletId = _bulletId;
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(bulletId);
		if (recordByKey != null)
		{
			m_isSuper = false;
			string resName = (m_isSuper ? "ui/image/suitGun/skill_itembg1.png" : "ui/image/suitGun/skill_itembg.png");
			m_spriteCell.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
			string resName2 = (m_isSuper ? "ui/image/suitGun/skill_empty1.png" : "ui/image/suitGun/skill_empty.png");
			float num = (m_isSuper ? 1f : 0.8f);
			m_spriteIcon.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(num, num)));
			m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName2));
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon));
			((TextElement)m_lbDesc).text = recordByKey.desc;
		}
		PlayerGunDetail playerGunDetail = zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(gunId);
		((TextElement)m_lbLevel).text = "Lv." + playerGunDetail.Star;
		int num2 = playerGunDetail.BulletId;
		((VisualElement)m_btnDown).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((num2 != bulletId) ? 1 : 0));
		((VisualElement)m_btnSelect).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((num2 == bulletId) ? 1 : 0));
	}

	private void OnDown()
	{
		zbWsMsgHandler.Instance.Gun_SetBullet_Req(gunId, 0);
	}

	private void OnTouch()
	{
		zbWsMsgHandler.Instance.Gun_SetBullet_Req(gunId, bulletId);
	}
}
