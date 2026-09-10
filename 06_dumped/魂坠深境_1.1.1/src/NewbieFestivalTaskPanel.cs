using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewbieFestivalTaskPanel : UIBase
{
	[SerializeField]
	private NewbieFestivalTaskCell runTaskCell;

	[SerializeField]
	private NewbieFestivalTaskCell winTaskCell;

	[SerializeField]
	private NewbieFestivalTaskProgressCell realMoneyCell;

	private NewbieFestivalManager newbieFestivalManager => UIBase.applicationState.newbieFestivalManager;

	protected override void Init()
	{
		InitUI(runTaskCell);
		InitUI(winTaskCell);
		InitUI(realMoneyCell);
		newbieFestivalManager.StateChanged += RefreshTaskCellsWhenActive;
	}

	protected override void Dispose()
	{
		newbieFestivalManager.StateChanged -= RefreshTaskCellsWhenActive;
	}

	public void Setup()
	{
		RefreshTaskCells();
		newbieFestivalManager.SettlePaidTicket();
	}

	private void RefreshTaskCellsWhenActive()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshTaskCells();
		}
	}

	private void RefreshTaskCells()
	{
		List<RoutineQuest> list = newbieFestivalManager.currentDailyTasks.Where((RoutineQuest quest) => quest?.def != null && quest.def.rewards.OfType<MatterReward>().Any((MatterReward reward) => reward.matterDef == newbieFestivalManager.currentActivity.freeTicketMatter)).ToList();
		RoutineQuest routineQuest = newbieFestivalManager.currentDailyTasks.FirstOrDefault((RoutineQuest quest) => quest?.def != null && quest.def.rewards.OfType<MatterReward>().Any((MatterReward reward) => reward.matterDef == newbieFestivalManager.currentActivity.paidTicketMatter));
		runTaskCell.gameObject.SetActive(list.Count > 0);
		if (list.Count > 0)
		{
			runTaskCell.Setup(list[0]);
		}
		winTaskCell.gameObject.SetActive(list.Count > 1);
		if (list.Count > 1)
		{
			winTaskCell.Setup(list[1]);
		}
		realMoneyCell.gameObject.SetActive(routineQuest != null);
		if (routineQuest != null)
		{
			realMoneyCell.Setup(routineQuest);
		}
	}
}
