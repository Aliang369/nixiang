using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntermissionPlayReclaimArea : CellUI<IntermissionPlayReclaimAreaState>
{
	private const int GiftPerTicket = 10;

	[SerializeField]
	private MatterDef ticketMatter;

	[SerializeField]
	private MatterDef giftMatter;

	[SerializeField]
	private Image reclaimRewardIcon;

	[SerializeField]
	private TextMeshProUGUI reclaimRewardCountText;

	[SerializeField]
	private TextMeshProUGUI reclaimRewardAmountText;

	[SerializeField]
	private CellUIPrefabPoolConfig<GlobalItemDisplay, IGlobalItemDisplaySetup> ticketDisplayPool;

	[SerializeField]
	private GameButton closeButton;

	[SerializeField]
	private GameButton reclaimButton;

	public string TicketName => ticketMatter.matterName;

	public string GiftName => giftMatter.matterName;

	protected override void Init()
	{
		Init(ticketDisplayPool);
		closeButton.onClick.AddListener(Close);
		reclaimButton.onClick.AddListener(OnReclaimClicked);
	}

	protected override void OnSetup(IntermissionPlayReclaimAreaState state)
	{
		ticketDisplayPool.Sync(new List<IGlobalItemDisplaySetup>
		{
			new MatterInfo(ticketMatter, state.AvailableCount)
		});
		reclaimRewardIcon.sprite = giftMatter.icon;
		reclaimRewardCountText.text = CellUI<IntermissionPlayReclaimAreaState>.textData.term_intermission_play_reclaim_reward_arg0;
		reclaimRewardAmountText.text = $"×{state.AvailableCount * 10}";
	}

	protected override void UpdateDisplay()
	{
		reclaimButton.interactable = base.setupData.CanReclaim && base.setupData.AvailableCount > 0;
	}

	private void OnReclaimClicked()
	{
		if (CellUI<IntermissionPlayReclaimAreaState>.applicationState.intermissionPlayManager.TryReclaimTickets(base.setupData.AvailableCount))
		{
			Close();
		}
	}

	public void Close()
	{
		Unset();
		base.gameObject.SetActive(value: false);
	}
}
