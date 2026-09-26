using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonForceExpNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonForceExpNtfReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNlYXNvbkZvcmNlRXhwTnRmLnByb3RvEgV0eXBlcxoXZW51bS5F" + "U2Vhc29uRm9yY2UucHJvdG8iXAoRU2Vhc29uRm9yY2VFeHBOdGYSKwoHRm9y" + "Y2VJZBgBIAEoDjIaLkVTZWFzb25Gb3JjZS5FU2Vhc29uRm9yY2USDQoFTGV2" + "ZWwYAiABKAUSCwoDRXhwGAMgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESeasonForceReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonForceExpNtf), (MessageParser)(object)SeasonForceExpNtf.Parser, new string[3] { "ForceId", "Level", "Exp" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
