using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaGuideCell : CellUI<GachaPoolState>
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI poolNameText;

	[SerializeField]
	private GameObject selectedMask;

	[SerializeField]
	private GameObject newHint;

	[SerializeField]
	private GameButton button;

	private GachaPoolState state => base.setupData;

	private GachaPanel gachaPanel => parent as GachaPanel;

	protected override void Init()
	{
		button.onClick.AddListener(delegate
		{
			gachaPanel.SetState(CellUI<GachaPoolState>.applicationState.gachaManager.GetGachaPoolState(state.def));
			gachaPanel.SetDirty();
		});
	}

	protected override void OnSetup(GachaPoolState _)
	{
		icon.sprite = state.def.gachaPoolIcon;
		if (poolNameText != null)
		{
			poolNameText.text = (string.IsNullOrEmpty(state.def.poolDisplayName) ? state.def.poolName : state.def.poolDisplayName);
		}
	}

	protected override void UpdateDisplay()
	{
		selectedMask.gameObject.SetActive(gachaPanel.targetState == state);
		newHint.gameObject.SetActive(!state.viewed);
	}
}
