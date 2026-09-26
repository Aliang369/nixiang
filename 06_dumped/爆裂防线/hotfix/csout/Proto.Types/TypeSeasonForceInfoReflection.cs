using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonForceInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonForceInfoReflection()
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLlNlYXNvbkZvcmNlSW5mby5wcm90bxIFdHlwZXMaF2VudW0uRVNl" + "YXNvbkZvcmNlLnByb3RvGhd0eXBlLlJvbGVCYXNlSW5mby5wcm90byLAAQoP" + "U2Vhc29uRm9yY2VJbmZvEisKB0ZvcmNlSWQYASABKA4yGi5FU2Vhc29uRm9y" + "Y2UuRVNlYXNvbkZvcmNlEgwKBE5hbWUYAiABKAkSDAoERmxhZxgDIAEoCRIO" + "CgZOb3RpY2UYBCABKAkSDQoFTGV2ZWwYBSABKAUSCwoDRXhwGAYgASgFEiMK" + "BkxlYWRlchgHIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mbxITCgtNZW1iZXJD" + "b3VudBgIIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESeasonForceReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonForceInfo), (MessageParser)(object)SeasonForceInfo.Parser, new string[8] { "ForceId", "Name", "Flag", "Notice", "Level", "Exp", "Leader", "MemberCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
