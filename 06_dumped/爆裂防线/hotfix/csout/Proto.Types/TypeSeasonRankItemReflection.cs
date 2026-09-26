using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonRankItemReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonRankItemReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlNlYXNvblJhbmtJdGVtLnByb3RvEgV0eXBlcxoXZW51bS5FU2Vh" + "c29uRm9yY2UucHJvdG8aF3R5cGUuUm9sZUJhc2VJbmZvLnByb3RvIoUBCg5T" + "ZWFzb25SYW5rSXRlbRIpCgxSb2xlQmFzZUluZm8YASABKAsyEy50eXBlcy5S" + "b2xlQmFzZUluZm8SKwoHRm9yY2VJZBgCIAEoDjIaLkVTZWFzb25Gb3JjZS5F" + "U2Vhc29uRm9yY2USDQoFU2NvcmUYAyABKAMSDAoEUmFuaxgEIAEoBUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESeasonForceReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonRankItem), (MessageParser)(object)SeasonRankItem.Parser, new string[4] { "RoleBaseInfo", "ForceId", "Score", "Rank" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
