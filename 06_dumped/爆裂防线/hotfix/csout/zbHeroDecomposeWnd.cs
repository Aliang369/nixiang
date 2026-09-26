using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine.UIElements;

public class zbHeroDecomposeWnd : zbBaseWnd
{
	private SpineVisualElement m_spineDecompose;

	private VisualElement[] m_spriteFrames = (VisualElement[])(object)new VisualElement[6];

	private Button[] m_btnAdds = (Button[])(object)new Button[6];

	private zbItem[] m_items = new zbItem[6];

	private Label m_lbHeroSoul;

	private Button m_btnAuto;

	private Button m_btnDecompose;

	private Button m_btnClean;

	private Button m_btnClose;

	public List<ResourceDetail> decomposeList = new List<ResourceDetail>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineDecompose = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_spriteFrames[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "item" + i, (string)null);
			m_btnAdds[i] = UQueryExtensions.Q<Button>(m_spriteFrames[i], "btn_add", (string)null);
			int curIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdds[i], (Action<ClickEvent>)delegate
			{
				OnAdds(curIndex);
			}, "", (TrickleDown)0);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_spriteFrames[i], "item", (string)null);
			m_items[i].OnEnable(this);
			m_items[i].SetTouchCallback(delegate
			{
				OnAdds(curIndex);
			});
		}
		m_lbHeroSoul = UQueryExtensions.Q<Label>(m_RootElement, "lb_heroSoul", (string)null);
		m_btnAuto = UQueryExtensions.Q<Button>(m_RootElement, "btn_auto", (string)null);
		m_btnDecompose = UQueryExtensions.Q<Button>(m_RootElement, "btn_decompose", (string)null);
		m_btnClean = UQueryExtensions.Q<Button>(m_RootElement, "btn_clean", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAuto, (Action)OnAuto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDecompose, (Action)OnDecompose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClean, (Action)OnClean, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.HeroSoul });
		m_gameEevent.RegisterEvent(EGameEvent.Event_HeroFragDecompose_Ack, OnHeroFragDecomposeAck);
	}

	public override void Refresh()
	{
		ReSetDecomposeList();
	}

	public void ReSetDecomposeList()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 6; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		decomposeList.Clear();
		RefreshHeroSoul();
	}

	public void RefreshDecomposeList()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 6; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (ResourceDetail decompose in decomposeList)
		{
			if (m_items[num] != null)
			{
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(new zbItemData(EResourceType.HeroFrag, decompose.Tid, decompose.Count));
			}
			num++;
		}
		RefreshHeroSoul();
	}

	private void RefreshHeroSoul()
	{
		long num = 0L;
		foreach (ResourceDetail decompose in decomposeList)
		{
			if (decompose == null)
			{
				continue;
			}
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(decompose.Tid);
			if (recordByKey != null)
			{
				zbHeroRarityCsvRecord recordByKey2 = zbDataConfig.Instance.heroRarityCsvTable.GetRecordByKey(recordByKey.rarity);
				if (recordByKey2 != null)
				{
					num += decompose.Count * recordByKey2.soul;
				}
			}
		}
		((TextElement)m_lbHeroSoul).text = num.ToString();
	}

	private void OnAdds(int index)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (((VisualElement)m_items[index]).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1))
		{
			zbUIMgr.Instance.ShowHeroDecomposeSel(this);
			return;
		}
		((VisualElement)m_items[index]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		decomposeList[index] = null;
		RefreshHeroSoul();
	}

	private void OnAuto()
	{
		ReSetDecomposeList();
		zbUIMgr.Instance.ShowHeroDecomposeAuto();
	}

	private void OnDecompose()
	{
		bool flag = false;
		foreach (ResourceDetail decompose in decomposeList)
		{
			if (decompose != null)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10260"));
			return;
		}
		List<BagSelectDetail> list = new List<BagSelectDetail>();
		foreach (ResourceDetail decompose2 in decomposeList)
		{
			if (decompose2 != null)
			{
				list.Add(new BagSelectDetail
				{
					Tid = decompose2.Tid,
					Count = decompose2.Count
				});
			}
		}
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10261"), delegate(bool isOK)
		{
			if (isOK)
			{
				zbWsMsgHandler.Instance.Hero_FragDecompose_Req(list.ToArray());
			}
		});
	}

	private void OnClean()
	{
		ReSetDecomposeList();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnHeroFragDecomposeAck(object data)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
		m_spineDecompose.AnimationState.SetAnimation(0, "animation", false).Complete += new TrackEntryDelegate(OnAnimationComplete);
	}

	private void OnAnimationComplete(TrackEntry trackEntry)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		trackEntry.Complete -= new TrackEntryDelegate(OnAnimationComplete);
		zbSelf.Instance.dataHandler.DataChange(zbSelf.Instance.heroHandler.heroFragDecomposeAck.DataChange, ((IEnumerable<AwardDetail>)zbSelf.Instance.heroHandler.heroFragDecomposeAck.AwardList).ToArray());
		zbSelf.Instance.heroHandler.heroFragDecomposeAck = null;
		ReSetDecomposeList();
	}
}
