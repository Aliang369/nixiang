using System;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine.UIElements;

public class zbPlayerGunUpgradeWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private Button m_btnConfirm;

	private Label m_lbMaxDesc;

	private SpineVisualElement m_spineBg;

	private Bone boneShoot;

	private VisualElement[] m_spriteStars1 = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_spriteStars2 = (VisualElement[])(object)new VisualElement[5];

	private Label m_lbName;

	private Label m_lbAttr1;

	private Label m_lbAttr2;

	private Label[] m_costLabels = (Label[])(object)new Label[3];

	private zbItem[] m_items = new zbItem[3];

	private zbItem m_convertItem;

	private Label m_lbConvertItem;

	private Label m_lbOr;

	private int gunId;

	private VisualElement m_cost;

	protected override void OnEnable()
	{
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_lbMaxDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_maxDesc", (string)null);
		m_spineBg = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_gun", (string)null);
		m_lbConvertItem = UQueryExtensions.Q<Label>(m_RootElement, "lb_convertItem", (string)null);
		m_convertItem = UQueryExtensions.Q<zbItem>(m_RootElement, "convertItem", (string)null);
		m_convertItem.OnEnable(this);
		m_lbOr = UQueryExtensions.Q<Label>(m_RootElement, "lb_or", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_Star0", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_Star1", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_spriteStars1[i] = UQueryExtensions.Q<VisualElement>(val, "iconstar" + i, (string)null);
			m_spriteStars2[i] = UQueryExtensions.Q<VisualElement>(val2, "iconstar" + i, (string)null);
		}
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbAttr1 = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr0", (string)null);
		m_lbAttr2 = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr1", (string)null);
		m_cost = UQueryExtensions.Q<VisualElement>(m_RootElement, "costList", (string)null);
		for (int j = 0; j < 3; j++)
		{
			m_costLabels[j] = UQueryExtensions.Q<Label>(m_cost, $"lb_cost{j}", (string)null);
			m_items[j] = UQueryExtensions.Q<zbItem>(m_cost, $"item{j}", (string)null);
			m_items[j].OnEnable(this);
			((VisualElement)m_costLabels[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_gameEevent.RegisterEvent(EGameEvent.Event_Gun_Refresh, OnEventUpdateSuitList);
		AddDark();
	}

	private void OnEventUpdateSuitList(object obj)
	{
		RefreshCustom(gunId);
	}

	public void RefreshCustom(int _gunId)
	{
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		gunId = _gunId;
		PlayerGunDetail playerGunDetail = zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(gunId);
		if (playerGunDetail == null)
		{
			Hide();
			return;
		}
		int num = 0;
		int num2 = 0;
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(gunId);
		((TextElement)m_lbMaxDesc).text = zbSelf.Instance.itemHandler.GetStarMaxDesc(recordByKey.propId);
		string resName = recordByKey.frontBone + "_SkeletonData.asset";
		SkeletonDataAsset skeletonDataAsset = zbAssetLoader.Instance.LoadResouce<SkeletonDataAsset>(resName);
		m_spineBg.skeletonDataAsset = skeletonDataAsset;
		m_spineBg.Initialize(true);
		m_spineBg.AnimationState.SetAnimation(0, "animation", true);
		boneShoot = m_spineBg.skeleton.FindBone("position");
		((TextElement)m_lbName).text = recordByKey.name;
		num = playerGunDetail.Star;
		num2 = Math.Min(playerGunDetail.Star + 1, zbSelf.Instance.suitGunHandler.maxGunStar);
		zbPlayerGunUpgradeCsvRecord gunStarUpgrade = zbSelf.Instance.suitGunHandler.GetGunStarUpgrade(recordByKey.id, num);
		((TextElement)m_lbAttr1).text = string.Format(zbUtil.Language("lang.10581"), gunStarUpgrade.dmg * 100f);
		zbPlayerGunUpgradeCsvRecord gunStarUpgrade2 = zbSelf.Instance.suitGunHandler.GetGunStarUpgrade(recordByKey.id, num2);
		string arg = zbUtil.AddHighlightStyle((gunStarUpgrade2.dmg * 100f).ToString(), "007008", 18);
		((TextElement)m_lbAttr2).text = string.Format(zbUtil.Language("lang.10581"), arg);
		zbUIEffect.SetStars(m_spriteStars1, num);
		zbUIEffect.SetStars(m_spriteStars2, num2);
		((TextElement)m_btnConfirm).text = zbUtil.Language("lang.10583");
		((VisualElement)m_btnConfirm).SetEnabled(false);
		m_cost.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (playerGunDetail.Star < zbSelf.Instance.suitGunHandler.maxGunStar)
		{
			m_cost.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			int[][] costLists = new int[1][] { new int[3] { 2, recordByKey.propId, gunStarUpgrade2.costProp } };
			zbSelf.Instance.homelandHandler.UpdateCostUI(costLists, m_costLabels, m_items);
			((VisualElement)m_btnConfirm).SetEnabled(true);
			((TextElement)m_btnConfirm).text = zbUtil.Language("lang.10584");
		}
		((VisualElement)m_lbMaxDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((playerGunDetail.Star < zbSelf.Instance.suitGunHandler.maxGunStar) ? 1 : 0));
		bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, recordByKey.convertPropId, 1);
		((VisualElement)m_lbConvertItem).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_lbOr).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			zbItemData data = new zbItemData(EResourceType.Prop, recordByKey.convertPropId, 1L);
			m_convertItem.Refresh(data, isNameVis: false, isCountVis: false);
			int[][] costLists2 = new int[1][] { new int[3] { 2, recordByKey.convertPropId, gunStarUpgrade2.costProp } };
			zbSelf.Instance.homelandHandler.UpdateCostUI(costLists2, (Label[])(object)new Label[1] { m_lbConvertItem }, new zbItem[1] { m_convertItem });
		}
	}

	public void OnConfirm(ClickEvent evt)
	{
		PlayerGunDetail playerGunDetail = zbSelf.Instance.suitGunHandler.GetPlayerGunDetail(gunId);
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(gunId);
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.propId);
		zbPlayerGunUpgradeCsvRecord gunStarUpgrade = zbSelf.Instance.suitGunHandler.GetGunStarUpgrade(recordByKey.id, playerGunDetail.Star + 1);
		if (gunStarUpgrade == null)
		{
			return;
		}
		if (resourceCount < gunStarUpgrade.costProp)
		{
			long resourceCount2 = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.convertPropId);
			if (resourceCount2 > 0 && resourceCount2 + resourceCount >= gunStarUpgrade.costProp)
			{
				zbWsMsgHandler.Instance.Gun_UpgradeStar_Req(gunId);
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Prop, recordByKey.propId, gunStarUpgrade.costProp);
			}
		}
		else
		{
			zbWsMsgHandler.Instance.Gun_UpgradeStar_Req(gunId);
		}
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}
}
