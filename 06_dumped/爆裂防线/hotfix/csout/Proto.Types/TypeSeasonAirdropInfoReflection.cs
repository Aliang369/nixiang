using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonAirdropInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonAirdropInfoReflection()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNlYXNvbkFpcmRyb3BJbmZvLnByb3RvEgV0eXBlcxogZW51bS5F" + "U2Vhc29uQWlyZHJvcFF1YWxpdHkucHJvdG8aI3R5cGUuU2Vhc29uQWlyZHJv" + "cFBhcnRpY2lwYW50LnByb3RvIpECChFTZWFzb25BaXJkcm9wSW5mbxIOCgZE" + "cm9wSWQYASABKAUSPQoHUXVhbGl0eRgCIAEoDjIsLkVTZWFzb25BaXJkcm9w" + "UXVhbGl0eS5FU2Vhc29uQWlyZHJvcFF1YWxpdHkSDAoEUG9zWBgDIAEoAhIM" + "CgRQb3NZGAQgASgCEhEKCVRvdGFsUHJvZxgFIAEoBRIPCgdDdXJQcm9nGAYg" + "ASgFEhAKCENhcGFjaXR5GAcgASgFEjUKDFBhcnRpY2lwYW50cxgIIAMoCzIf" + "LnR5cGVzLlNlYXNvbkFpcmRyb3BQYXJ0aWNpcGFudBIQCghEYWlseVdpbhgJ" + "IAEoBRISCgpEYWlseUxpbWl0GAogASgFQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESeasonAirdropQualityReflection.Descriptor,
			TypeSeasonAirdropParticipantReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonAirdropInfo), (MessageParser)(object)SeasonAirdropInfo.Parser, new string[10] { "DropId", "Quality", "PosX", "PosY", "TotalProg", "CurProg", "Capacity", "Participants", "DailyWin", "DailyLimit" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
