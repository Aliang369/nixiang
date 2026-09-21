using System;
using ChillyRoom.Services.ContainerOfSoul;
using TMPro;

public class RankingRecordCell : CellUI<RankingManager.RankingData>
{
	public TextMeshProUGUI indexText;

	public TextMeshProUGUI playerNameText;

	public TextMeshProUGUI characterText;

	public TextMeshProUGUI scoreText;

	public GameButton inspectButton;

	[NonSerialized]
	private ContractRankingPanel contractRankingPanel;

	private RankingManager.RankingData data => base.setupData;

	protected override void Init()
	{
		contractRankingPanel = GetUIInParents<ContractRankingPanel>();
		inspectButton.onClick.AddListener(_OnInspectButtonClicked);
	}

	protected override void UpdateDisplay()
	{
		ContractRankingPanel componentInParent = GetComponentInParent<ContractRankingPanel>();
		indexText.text = ((this == componentInParent.currentBestRecordCell) ? $"{data.rankIndex + 1}" : $"{data.rankIndex + 1 + (componentInParent.CurrentPage - 1) * componentInParent.RankCountPerPage}");
		playerNameText.text = data.recordPreviewInfo.playerName;
		characterText.text = data.recordPreviewInfo.character.characterName;
		scoreText.text = data.recordPreviewInfo.rankingScore.ToString();
	}

	private void _OnInspectButtonClicked()
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			FetchGameRecordDetailRequest request = new FetchGameRecordDetailRequest(data.recordPreviewInfo);
			GeneralGameAsyncOperation<FetchGameRecordDetailRequest, FetchGameRecordDetailResult> op = new GeneralGameAsyncOperation<FetchGameRecordDetailRequest, FetchGameRecordDetailResult>(request, CellUI<RankingManager.RankingData>.applicationState.ChillyRoomSDK.GameRecord.FetchGameRecordDetailAsync);
			op.OnComplete(delegate
			{
				contractRankingPanel.recordInfoPanel.SetActiveAndDirty(active: true);
				contractRankingPanel.recordInfoPanel.Setup(op.OpResult.GameRecord.ToClient(new NetworkConvertContext(CellUI<RankingManager.RankingData>.applicationState)));
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_game_record_detail);
		}
	}
}
