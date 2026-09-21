using System;
using System.Text.RegularExpressions;

public interface ICTAsset : IChildren<ICTAsset>
{
	const string CTAssetFullIdRegex = "#[A-Z]*#[0-9]*#";

	const string CTAssetImportSpriteIdRegex = "#[A-Z]*#[0-9]*#([A-Za-z]*#)?";

	const string CTAssetImportSpriteIdMatchRegex = "#(?<append>[A-Z]*)#(?<index>[0-9]*)#((?<suffix>[A-Za-z]*)#)?";

	int AssetIndex { get; set; }

	string AssetGuid { get; set; }

	string CTAssetIDPrepend { get; }

	Type CTAssetType { get; }

	bool IsValidAsset();

	string GetPaddingID()
	{
		return AssetIndex.ToString().PadLeft(6, '0');
	}

	string GetFullID()
	{
		return "#" + CTAssetIDPrepend + "#" + GetPaddingID() + "#";
	}

	static string GetImportIdAppend(string id)
	{
		return Regex.Match(id, "#(?<append>[A-Z]*)#(?<index>[0-9]*)#((?<suffix>[A-Za-z]*)#)?", RegexOptions.Compiled).Groups["append"].Value;
	}

	static string GetImportIdSuffix(string id)
	{
		return Regex.Match(id, "#(?<append>[A-Z]*)#(?<index>[0-9]*)#((?<suffix>[A-Za-z]*)#)?", RegexOptions.Compiled).Groups["suffix"].Value;
	}

	static int GetImportIdIndex(string id)
	{
		return int.Parse(Regex.Match(id, "#(?<append>[A-Z]*)#(?<index>[0-9]*)#((?<suffix>[A-Za-z]*)#)?", RegexOptions.Compiled).Groups["index"].Value);
	}
}
