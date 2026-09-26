using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonCityOwnerNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonCityOwnerNtfReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlNlYXNvbkNpdHlPd25lck50Zi5wcm90bxIFdHlwZXMaF2VudW0u" + "RVNlYXNvbkZvcmNlLnByb3RvIoABChJTZWFzb25DaXR5T3duZXJOdGYSDgoG" + "Q2l0eUlkGAEgASgFEiwKCE9sZEZvcmNlGAIgASgOMhouRVNlYXNvbkZvcmNl" + "LkVTZWFzb25Gb3JjZRIsCghOZXdGb3JjZRgDIAEoDjIaLkVTZWFzb25Gb3Jj" + "ZS5FU2Vhc29uRm9yY2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESeasonForceReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonCityOwnerNtf), (MessageParser)(object)SeasonCityOwnerNtf.Parser, new string[3] { "CityId", "OldForce", "NewForce" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
