using System;
using UnityEngine;
using UnityEngine.Playables;

public class GachaRankEffectTest : MonoBehaviour
{
	[SerializeField]
	private Cutscene gachaCutscenePrefab;

	private Cutscene playingCutscene;

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(24f, 24f, 180f, 260f), GUI.skin.box);
		GUILayout.Label("抽卡演出测试", Array.Empty<GUILayoutOption>());
		for (int i = 1; i <= 5; i++)
		{
			if (GUILayout.Button($"播放 Rank {i}", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(36f) }))
			{
				PlayRank(i);
			}
		}
		GUILayout.EndArea();
	}

	private void PlayRank(int index)
	{
		if (playingCutscene != null)
		{
			UnityEngine.Object.Destroy(playingCutscene.gameObject);
		}
		playingCutscene = UnityEngine.Object.Instantiate(gachaCutscenePrefab);
		playingCutscene.GetComponent<CutsceneRankController>().SetRank((QualityLevel)(index - 2));
		PlayableDirector director = playingCutscene.Director;
		director.time = 0.0;
		director.Evaluate();
		director.Play();
	}
}
