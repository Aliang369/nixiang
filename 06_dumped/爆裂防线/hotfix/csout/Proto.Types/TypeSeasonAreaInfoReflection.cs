using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonAreaInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonAreaInfoReflection()
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlNlYXNvbkFyZWFJbmZvLnByb3RvEgV0eXBlcxobZW51bS5FU2Vh" + "c29uQXJlYVN0YXRlLnByb3RvGhplbnVtLkVTZWFzb25BcmVhVHlwZS5wcm90" + "bxoXZW51bS5FU2Vhc29uRm9yY2UucHJvdG8ikQIKDlNlYXNvbkFyZWFJbmZv" + "Eg4KBkFyZWFJZBgBIAEoBRINCgVHcmlkWBgCIAEoBRINCgVHcmlkWRgDIAEo" + "BRIrCgdGb3JjZUlkGAQgASgOMhouRVNlYXNvbkZvcmNlLkVTZWFzb25Gb3Jj" + "ZRIxCgVTdGF0ZRgFIAEoDjIiLkVTZWFzb25BcmVhU3RhdGUuRVNlYXNvbkFy" + "ZWFTdGF0ZRIyCghBcmVhVHlwZRgGIAEoDjIgLkVTZWFzb25BcmVhVHlwZS5F" + "U2Vhc29uQXJlYVR5cGUSDQoFTGV2ZWwYByABKAUSLgoKQmlydGhGb3JjZRgI" + "IAEoDjIaLkVTZWFzb25Gb3JjZS5FU2Vhc29uRm9yY2VCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumESeasonAreaStateReflection.Descriptor,
			EnumESeasonAreaTypeReflection.Descriptor,
			EnumESeasonForceReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonAreaInfo), (MessageParser)(object)SeasonAreaInfo.Parser, new string[8] { "AreaId", "GridX", "GridY", "ForceId", "State", "AreaType", "Level", "BirthForce" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
