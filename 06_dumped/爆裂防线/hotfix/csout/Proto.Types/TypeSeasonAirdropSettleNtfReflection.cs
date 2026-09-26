using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonAirdropSettleNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonAirdropSettleNtfReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlNlYXNvbkFpcmRyb3BTZXR0bGVOdGYucHJvdG8SBXR5cGVzGiBl" + "bnVtLkVTZWFzb25BaXJkcm9wUXVhbGl0eS5wcm90byKSAQoWU2Vhc29uQWly" + "ZHJvcFNldHRsZU50ZhIOCgZEcm9wSWQYASABKAUSPQoHUXVhbGl0eRgCIAEo" + "DjIsLkVTZWFzb25BaXJkcm9wUXVhbGl0eS5FU2Vhc29uQWlyZHJvcFF1YWxp" + "dHkSCwoDV2luGAMgASgFEgwKBFJhbmsYBCABKAUSDgoGU291cmNlGAUgASgF" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESeasonAirdropQualityReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonAirdropSettleNtf), (MessageParser)(object)SeasonAirdropSettleNtf.Parser, new string[5] { "DropId", "Quality", "Win", "Rank", "Source" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
