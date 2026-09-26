using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonCellPlayersReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonCellPlayersReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNlYXNvbkNlbGxQbGF5ZXJzLnByb3RvEgV0eXBlcxobdHlwZS5T" + "ZWFzb25Nb3ZlRGV0YWlsLnByb3RvGhp0eXBlLlNlYXNvblBsYXllclBvcy5w" + "cm90byKmAQoRU2Vhc29uQ2VsbFBsYXllcnMSDgoGQXJlYUlkGAEgASgFEioK" + "ClN0YXRpb25hcnkYAiADKAsyFi50eXBlcy5TZWFzb25QbGF5ZXJQb3MSKQoI" + "SW5jb21pbmcYAyADKAsyFy50eXBlcy5TZWFzb25Nb3ZlRGV0YWlsEioKCURl" + "cGFydGluZxgEIAMoCzIXLnR5cGVzLlNlYXNvbk1vdmVEZXRhaWxCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeSeasonMoveDetailReflection.Descriptor,
			TypeSeasonPlayerPosReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonCellPlayers), (MessageParser)(object)SeasonCellPlayers.Parser, new string[4] { "AreaId", "Stationary", "Incoming", "Departing" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
