using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarSelfInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarSelfInfoReflection()
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkxlZ2lvbldhclNlbGZJbmZvLnByb3RvEgV0eXBlcxoVZW51bS5F" + "Qm9vbFN0YXRlLnByb3RvIpcCChFMZWdpb25XYXJTZWxmSW5mbxIRCglBdHRh" + "Y2tOdW0YASABKAUSEgoKS2lsbFN0cmVhaxgCIAEoBRIQCghBd2FyZE51bRgD" + "IAEoBRITCgtBd2FyZExhc3RUbRgEIAEoAxIRCglTZWxmU2NvcmUYBSABKAUS" + "FAoMQWN0aW9uUG9pbnRzGAYgASgFEhMKC0Zyb21Qb2ludElkGAcgASgFEhEK" + "CVRvUG9pbnRJZBgIIAEoBRIRCglBcnJpdmFsVG0YCSABKAMSDgoGTW92ZUNE" + "GAogASgFEhMKC1Jlc3RTdGFydFRtGAsgASgDEisKC0JlYmFja1N0YXRlGAwg" + "ASgOMhYuRUJvb2xTdGF0ZS5FQm9vbFN0YXRlQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarSelfInfo), (MessageParser)(object)LegionWarSelfInfo.Parser, new string[12]
			{
				"AttackNum", "KillStreak", "AwardNum", "AwardLastTm", "SelfScore", "ActionPoints", "FromPointId", "ToPointId", "ArrivalTm", "MoveCD",
				"RestStartTm", "BebackState"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
