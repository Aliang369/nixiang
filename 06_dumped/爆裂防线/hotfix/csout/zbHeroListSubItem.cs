using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroListSubItem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteHero;

	private VisualElement m_spriteRarity;

	private VisualElement m_spriteSkill;

	private Label m_lbName;

	private Label m_lbLevel;

	private VisualElement m_progress;

	private VisualElement m_progressBg;

	private Label m_lbProgress;

	private VisualElement m_spriteMask;

	private VisualElement m_spriteSelect;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private VisualElement m_new;

	private VisualElement m_starTag;

	private zbHeroListWnd m_wndParent;

	private zbHeroCsvRecord m_record;

	private HeroDetail m_data;

	private float maxWidth;

	private float offset;

	private long haveCount;

	private long needCount;

	private float updateTimer;

	public zbHeroListSubItem(zbHeroListWnd wndParent)
	{
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		maxWidth = 130f;
		base._002Ector();
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/hero/tb_heroListSub.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_frame", (string)null);
		m_spriteHero = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		m_spriteRarity = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_rarity", (string)null);
		m_spriteSkill = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_skill", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_progressBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progressBg", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_progress", (string)null);
		m_spriteMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_mask", (string)null);
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_select", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(3f, -2f), 0.9f, true);
		m_new = zbUtil.CreateNewTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-10f, -8f), 0.8f);
		m_starTag = zbUtil.CreateHeroStarTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-5f, -5f));
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbHeroCsvRecord record)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		((TextElement)m_lbName).text = m_record.name;
		offset = -2f;
		m_progressBg.style.left = StyleLength.op_Implicit(0f - offset);
		int avatarId = m_record.avatarId;
		EQuality quality = zbResourceHelper.GetResourceQuality(EResourceType.Hero, m_record.id);
		ERarityType rarity = (ERarityType)m_record.rarity;
		m_data = zbSelf.Instance.heroHandler.GetHeroDetail(m_record.id);
		if (m_data != null)
		{
			quality = m_data.Quality;
			avatarId = m_data.AvatarId;
			rarity = m_data.Rarity;
		}
		zbHeroAvatarCsvRecord recordByKey = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(avatarId);
		if (recordByKey != null)
		{
			m_spriteHero.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.heroIcon));
		}
		string heroFrameTex = zbResourceHelper.GetHeroFrameTex(quality);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroFrameTex));
		string heroRarityIconPath = zbResourceHelper.GetHeroRarityIconPath(rarity);
		m_spriteRarity.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(heroRarityIconPath));
		string skillTypeIconPath = zbResourceHelper.GetSkillTypeIconPath(record, rarity);
		m_spriteSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(skillTypeIconPath));
		haveCount = zbSelf.Instance.heroHandler.GetHeroFragCount(m_record.id);
		needCount = zbSelf.Instance.heroHandler.GetComposeOrUpgradeCount(m_record.id);
		if (needCount > 0)
		{
			((TextElement)m_lbProgress).text = haveCount + "/" + needCount;
			float num = Mathf.Clamp((float)haveCount / (float)needCount, 0f, 1f);
			m_progress.style.width = StyleLength.op_Implicit(new Length(num * maxWidth, (LengthUnit)0));
		}
		else
		{
			((TextElement)m_lbProgress).text = haveCount.ToString();
			m_progress.style.width = StyleLength.op_Implicit(new Length(1f * maxWidth, (LengthUnit)0));
		}
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/progress/process130x18_1.png");
		if (haveCount >= needCount && needCount != 0L)
		{
			val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/progress/process130x18_2.png");
		}
		m_progressBg.style.backgroundImage = StyleBackground.op_Implicit(val);
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)m_record.type);
		if (warRoomDetailBySkillType != null)
		{
			HomelandRoomDetail homelandRoomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(warRoomDetailBySkillType.RoomId);
			((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbLevel).text = "Lv." + homelandRoomDetail.Level;
		}
		else
		{
			((VisualElement)m_lbLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbUtil.SetElementGray(m_spriteRarity, m_data == null);
		zbUtil.SetElementGray(m_spriteSkill, m_data == null);
		m_spriteMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data != null));
		bool flag = zbSelf.Instance.heroHandler.GetWarRoomDetailByHeroTid(m_record.id) != null;
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		bool flag2 = zbSelf.Instance.heroHandler.HasHeroComposeNotify(m_record.id) || zbSelf.Instance.heroHandler.HasHeroStarNotify(m_record.id) || (flag && zbSelf.Instance.heroHandler.HasHeroUpgradeNotify(m_record.id)) || (flag && zbSelf.Instance.heroHandler.HasHeroAwakenNotify(m_record.id)) || zbSelf.Instance.geneHandler.HasHeroTabGeneNotify((ESkillType)m_record.type, m_record.id) || zbSelf.Instance.heroHandler.HasHeroRarityNotify(m_record.id);
		bool flag3 = !flag2 && zbSelf.Instance.newTagHandler.IsNewHero(m_record.id);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		m_new.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
		m_starTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((flag2 || flag3 || !zbSelf.Instance.heroHandler.HasHeroStarFragNotify(m_record.id)) ? 1 : 0));
	}

	public void OnTouch(ClickEvent evt)
	{
		zbSelf.Instance.newTagHandler.RemoveNewHero(m_record.id);
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Hero);
		zbUIMgr.Instance.ShowHeroDetail(m_record);
	}

	public void update()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (needCount == 0L || haveCount < needCount)
		{
			return;
		}
		updateTimer += Time.deltaTime;
		if (!(updateTimer < 1f / 15f))
		{
			updateTimer = 0f;
			offset += Time.deltaTime * 50f;
			if (offset > 14f)
			{
				offset = -2f;
			}
			float num = 0f - offset;
			StyleLength left = m_progressBg.style.left;
			Length value = ((StyleLength)(ref left)).value;
			if (num != ((Length)(ref value)).value)
			{
				m_progressBg.style.left = StyleLength.op_Implicit(num);
			}
		}
	}
}
