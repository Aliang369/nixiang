public class CutsceneInfoData : AssetPreData<Cutscene>
{
	public string name;

	public float screenFadeTime;

	public bool hideCurrentRoom;

	public override string CTAssetIDPrepend => "CUTINFO";

	public override bool IsValidAsset()
	{
		return true;
	}
}
