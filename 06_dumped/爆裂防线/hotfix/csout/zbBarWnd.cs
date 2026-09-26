using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBarWnd : zbBaseWnd
{
	private Coroutine barCoroutine;

	private GameObject m_spineSceneBar;

	public SkeletonAnimation m_chouKaDiZuoIcon;

	private SkeletonAnimation m_spineAnimationChouka;

	private SkeletonAnimation m_spineAnimation;

	private Bone[] m_bones = (Bone[])(object)new Bone[10];

	private Slot[] m_slots = (Slot[])(object)new Slot[10];

	private Bone m_bone;

	private Slot m_slot;

	private VisualElement m_uiMain;

	private Label m_lbRareHit;

	private Button m_btnOne;

	private Label m_lbOne;

	private VisualElement m_spriteOne;

	private Label m_lbFree;

	private Button m_btnTen;

	private Label m_lbTen;

	private VisualElement m_spriteTen;

	private zbItem[] m_items = new zbItem[10];

	private zbItem m_item;

	private Button m_btnClose;

	private Button m_btnTouch;

	private bool m_skipAnimation;

	private EBarType m_barType;

	private BarDetail m_barDetail;

	private EMoneyType m_moneyType;

	private AwardDetail[] m_awardList;

	private int m_barCost;

	private List<int> m_heroTids = new List<int>();

	private VisualElement m_notifyFree;

	private VisualElement m_notifyTen;

	protected override void OnEnable()
	{
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineSceneBar = ((Component)m_GameObjectSceneRoot.transform.Find("spineSceneBar")).gameObject;
		m_chouKaDiZuoIcon = ((Component)m_spineSceneBar.transform.Find("chouKaDiZuoIcon")).GetComponent<SkeletonAnimation>();
		m_spineAnimationChouka = ((Component)m_GameObjectSceneRoot.transform.Find("SkeletonAnimationChouka")).GetComponent<SkeletonAnimation>();
		m_spineAnimation = ((Component)m_GameObjectSceneRoot.transform.Find("SkeletonAnimationSpine")).GetComponent<SkeletonAnimation>();
		m_uiMain = UQueryExtensions.Q<VisualElement>(m_RootElement, "main", (string)null);
		m_lbRareHit = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnOne = UQueryExtensions.Q<Button>(m_RootElement, "btn_one", (string)null);
		m_lbOne = UQueryExtensions.Q<Label>(m_RootElement, "lb_one", (string)null);
		m_spriteOne = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_one", (string)null);
		m_lbFree = UQueryExtensions.Q<Label>(m_RootElement, "lb_free", (string)null);
		m_btnTen = UQueryExtensions.Q<Button>(m_RootElement, "btn_ten", (string)null);
		m_lbTen = UQueryExtensions.Q<Label>(m_RootElement, "lb_ten", (string)null);
		m_spriteTen = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ten", (string)null);
		for (int i = 0; i < 10; i++)
		{
			m_bones[i] = ((SkeletonRenderer)m_spineAnimation).Skeleton.FindBone("icon" + i);
			m_slots[i] = ((SkeletonRenderer)m_spineAnimation).Skeleton.FindSlot("icon" + i);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_bone = ((SkeletonRenderer)m_spineAnimation).Skeleton.FindBone("icon0");
		m_slot = ((SkeletonRenderer)m_spineAnimation).Skeleton.FindSlot("icon0");
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOne, (Action<ClickEvent>)delegate
		{
			OnBar(1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTen, (Action<ClickEvent>)delegate
		{
			OnBar(10);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_notifyFree = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnOne, (Vector2?)new Vector2(2f, 2f), 1f, true);
		m_notifyTen = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTen, (Vector2?)new Vector2(2f, 2f), 1f, true);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.BarSpecial,
			EMoneyType.BarNormal
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnUpdateMoney);
	}

	public void RefreshCustom(EBarType barType, AwardDetail[] awardList)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		if (barType == EBarType.Normal)
		{
			if ((int)Application.platform == 8)
			{
				m_moneyPanel.Refresh(new EMoneyType[1] { EMoneyType.BarNormal });
			}
			else
			{
				m_moneyPanel.Refresh(new EMoneyType[2]
				{
					EMoneyType.Diamond,
					EMoneyType.BarNormal
				});
			}
		}
		else if ((int)Application.platform == 8)
		{
			m_moneyPanel.Refresh(new EMoneyType[1] { EMoneyType.BarSpecial });
		}
		else
		{
			m_moneyPanel.Refresh(new EMoneyType[2]
			{
				EMoneyType.Diamond,
				EMoneyType.BarSpecial
			});
		}
		m_heroTids.Clear();
		for (int i = 0; i < 10; i++)
		{
			((VisualElement)m_items[i]).visible = false;
			m_items[i].m_spriteFrame.visible = false;
			((VisualElement)m_items[i].m_lbCount).visible = false;
			m_items[i].m_spriteFrag.visible = false;
		}
		((VisualElement)m_item).visible = false;
		m_item.m_spriteFrame.visible = false;
		((VisualElement)m_item.m_lbCount).visible = false;
		m_item.m_spriteFrag.visible = false;
		m_spriteOne.visible = false;
		((VisualElement)m_lbOne).visible = false;
		((VisualElement)m_lbFree).visible = false;
		((VisualElement)m_btnTouch).visible = false;
		m_barType = barType;
		m_awardList = awardList;
		RefreshSpineScene();
		if (barCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(barCoroutine);
			barCoroutine = null;
		}
		string animationName = GetAnimationName();
		barCoroutine = ((MonoBehaviour)this).StartCoroutine(SpineAnimation(animationName));
	}

	public string GetAnimationName()
	{
		string result = "blue";
		if (m_barType == EBarType.Normal)
		{
			EQuality eQuality = EQuality.White;
			AwardDetail[] awardList = m_awardList;
			foreach (AwardDetail awardDetail in awardList)
			{
				EQuality resourceQuality = zbResourceHelper.GetResourceQuality(awardDetail.Type, (int)awardDetail.Tid);
				if (resourceQuality > eQuality)
				{
					eQuality = resourceQuality;
				}
			}
			if (eQuality == EQuality.Purple)
			{
				result = "purple";
			}
			else if (eQuality == EQuality.Golden)
			{
				result = "golden";
			}
			else if (eQuality >= EQuality.Red)
			{
				result = "red";
			}
		}
		else if (m_barType == EBarType.Special)
		{
			ERarityType eRarityType = ERarityType.B;
			AwardDetail[] awardList = m_awardList;
			foreach (AwardDetail awardDetail2 in awardList)
			{
				if (awardDetail2.Type == EResourceType.Hero)
				{
					zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey((int)awardDetail2.Tid);
					if (recordByKey != null && recordByKey.rarity > (int)eRarityType)
					{
						eRarityType = (ERarityType)recordByKey.rarity;
					}
				}
			}
			if (eRarityType == ERarityType.A)
			{
				result = "purple";
			}
			else if (eRarityType == ERarityType.S)
			{
				result = "golden";
			}
			else if (eRarityType >= ERarityType.Ss)
			{
				result = "red";
			}
		}
		return result;
	}

	private IEnumerator SpineAnimation(string animationName)
	{
		m_skipAnimation = false;
		m_uiMain.visible = false;
		((VisualElement)m_btnTouch).visible = true;
		m_spineSceneBar.gameObject.SetActive(false);
		((Component)m_spineAnimationChouka).gameObject.SetActive(true);
		m_spineAnimationChouka.AnimationState.SetAnimation(0, animationName, false);
		float waitTime1 = 2.6666667f;
		float elapsed = 0f;
		while (elapsed < waitTime1 && !m_skipAnimation)
		{
			elapsed += Time.deltaTime;
			yield return null;
		}
		if (m_skipAnimation)
		{
			EndSpineAnimation();
			yield break;
		}
		zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
		float waitTime2 = 3f - waitTime1;
		elapsed = 0f;
		while (elapsed < waitTime2 && !m_skipAnimation)
		{
			elapsed += Time.deltaTime;
			yield return null;
		}
		if (m_skipAnimation)
		{
			EndSpineAnimation();
			yield break;
		}
		elapsed = 0f;
		while (elapsed < 0.77f && !m_skipAnimation)
		{
			elapsed += Time.deltaTime;
			yield return null;
		}
		EndSpineAnimation();
	}

	private void EndSpineAnimation()
	{
		RefreshAwardList();
		if (m_heroTids.Count > 0)
		{
			zbUIMgr.Instance.ShowHeroGet(m_heroTids.ToArray());
		}
		RefreshDetail();
		m_uiMain.visible = true;
		((VisualElement)m_btnTouch).visible = false;
		m_spineSceneBar.gameObject.SetActive(true);
		((Component)m_spineAnimationChouka).gameObject.SetActive(false);
		barCoroutine = null;
	}

	private void RefreshSpineScene()
	{
		string spinePath = "animation/ui/chouka_dizuo/chouka_dizuo_icon1/chouka_dizuo_icon1_SkeletonData.asset";
		if (m_barType == EBarType.Special)
		{
			spinePath = "animation/ui/chouka_dizuo/chouka_dizuo_icon2/chouka_dizuo_icon2_SkeletonData.asset";
		}
		zbUtil.RefreshSpine(m_chouKaDiZuoIcon, spinePath, "animation", loop: true);
	}

	private void RefreshAwardList()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
		if (m_awardList.Length == 1)
		{
			AwardDetail awardDetail = m_awardList[0];
			if (awardDetail.Type == EResourceType.Hero)
			{
				m_heroTids.Add((int)awardDetail.Tid);
			}
			((VisualElement)m_item).visible = true;
			((VisualElement)m_item).style.opacity = StyleFloat.op_Implicit(0f);
			zbItemData data = new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count);
			m_item.Refresh(data, isNameVis: false, isCountVis: false);
			m_item.SetZhenGuiVisible(awardDetail.Type == EResourceType.Hero);
			m_spineAnimation.AnimationState.SetAnimation(0, "1", false);
			return;
		}
		int num = 0;
		AwardDetail[] awardList = m_awardList;
		foreach (AwardDetail awardDetail2 in awardList)
		{
			if (awardDetail2.Type == EResourceType.Hero)
			{
				m_heroTids.Add((int)awardDetail2.Tid);
			}
			if (num < m_items.Length)
			{
				((VisualElement)m_items[num]).visible = true;
				((VisualElement)m_items[num]).style.opacity = StyleFloat.op_Implicit(0f);
				zbItemData data2 = new zbItemData(awardDetail2.Type, awardDetail2.Tid, awardDetail2.Count);
				m_items[num].Refresh(data2);
				bool zhenGuiVisible = awardDetail2.Type == EResourceType.Hero || (awardDetail2.Type == EResourceType.Gem && zbResourceHelper.GetResourceQuality(EResourceType.Gem, awardDetail2.Tid) >= EQuality.Purple);
				m_items[num].SetZhenGuiVisible(zhenGuiVisible);
			}
			num++;
		}
		m_spineAnimation.AnimationState.SetAnimation(0, "10", false);
	}

	public void RefreshDetail()
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		switch (m_barType)
		{
		case EBarType.Normal:
			m_moneyType = EMoneyType.BarNormal;
			break;
		case EBarType.Special:
			m_moneyType = EMoneyType.BarSpecial;
			break;
		}
		m_barDetail = zbSelf.Instance.barHandler.GetBarDetail(m_barType);
		m_barCost = zbSelf.Instance.barHandler.GetBarCost(m_barType);
		((TextElement)m_lbOne).text = "x" + m_barCost;
		((TextElement)m_lbTen).text = "x" + m_barCost * 10;
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Money, (long)m_moneyType);
		m_spriteOne.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		m_spriteTen.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		m_spriteOne.visible = m_barDetail.DailyFree == EBoolState.False;
		((VisualElement)m_lbOne).visible = m_barDetail.DailyFree == EBoolState.False;
		((VisualElement)m_lbFree).visible = m_barDetail.DailyFree == EBoolState.True;
		m_notifyFree.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_barDetail.DailyFree != EBoolState.True));
		if (m_barType == EBarType.Normal)
		{
			((TextElement)m_lbRareHit).text = $"<color=#3399FF>{m_barDetail.RareHit}</color>次抽取必出<color=#3399FF>晶体Ⅲ</color>";
			m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(7, m_barCost * 10)));
		}
		else if (m_barType == EBarType.Special)
		{
			((TextElement)m_lbRareHit).text = string.Format(zbUtil.Language("lang.10297"), m_barDetail.RareHit);
			m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.moneyHandler.HasEnoughMoney(8, m_barCost * 10)));
		}
		else
		{
			m_notifyTen.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, (int)m_moneyType, m_barCost);
		((VisualElement)m_lbOne).style.color = StyleColor.op_Implicit(flag ? Color.white : Color.red);
		bool flag2 = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, (int)m_moneyType, m_barCost * 10);
		((VisualElement)m_lbTen).style.color = StyleColor.op_Implicit(flag2 ? Color.white : Color.red);
	}

	private void OnBar(int count)
	{
		if ((count == 1 && m_barDetail.DailyFree == EBoolState.True) || zbSelf.Instance.moneyHandler.HasEnoughMoney((int)m_moneyType, m_barCost * count))
		{
			zbWsMsgHandler.Instance.Bar_Draw_Req(m_barType, count);
		}
		else
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, (long)m_moneyType, m_barCost * count);
		}
	}

	public void OnClose(ClickEvent evt)
	{
		OperateTargetData barDrawTargetDetail = zbSelf.Instance.barHandler.GetBarDrawTargetDetail(m_barType);
		if (barDrawTargetDetail != null)
		{
			if (barDrawTargetDetail.UUID == "")
			{
				zbWsMsgHandler.Instance.Journey_Data_Req();
			}
			else
			{
				zbWsMsgHandler.Instance.Operate_Data_Req();
			}
		}
		Hide();
	}

	private void OnTouch(ClickEvent evt)
	{
		if (barCoroutine != null)
		{
			m_skipAnimation = true;
		}
	}

	protected override void UpdateAtShow()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		for (int i = 0; i < 10; i++)
		{
			if (m_items[i] != null && m_bones[i] != null && m_slots[i] != null)
			{
				((VisualElement)m_items[i]).style.opacity = StyleFloat.op_Implicit(m_slots[i].A);
				((VisualElement)m_items[i]).transform.scale = new Vector3(m_bones[i].ScaleX, m_bones[i].ScaleY, 1f);
			}
		}
		if (m_item != null && m_bone != null && m_slot != null)
		{
			((VisualElement)m_item).style.opacity = StyleFloat.op_Implicit(m_slot.A);
			((VisualElement)m_item).transform.scale = new Vector3(m_bone.ScaleX, m_bone.ScaleY, 1f);
		}
	}

	private void OnUpdateMoney(object obj)
	{
		if (m_uiMain.visible)
		{
			RefreshDetail();
		}
	}
}
