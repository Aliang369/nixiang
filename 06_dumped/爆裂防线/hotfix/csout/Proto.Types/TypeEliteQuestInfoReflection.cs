using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEliteQuestInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEliteQuestInfoReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkVsaXRlUXVlc3RJbmZvLnByb3RvEgV0eXBlcxoadHlwZS5NYWlu" + "UXVlc3REZXRhaWwucHJvdG8iswEKDkVsaXRlUXVlc3RJbmZvEhIKCkVsaXRl" + "TGV2ZWwYASABKAUSEQoJTmV4dExldmVsGAIgASgFEikKCUVsaXRlTGlzdBgD" + "IAMoCzIWLnR5cGVzLk1haW5RdWVzdERldGFpbBITCgtEb3VibGVBd2FyZBgE" + "IAEoBRITCgtDb21iYXRDb3VudBgFIAEoBRIRCglCdXlDb21iYXQYBiABKAUS" + "EgoKRGFpbHlDb3VudBgHIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeMainQuestDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EliteQuestInfo), (MessageParser)(object)EliteQuestInfo.Parser, new string[7] { "EliteLevel", "NextLevel", "EliteList", "DoubleAward", "CombatCount", "BuyCombat", "DailyCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
