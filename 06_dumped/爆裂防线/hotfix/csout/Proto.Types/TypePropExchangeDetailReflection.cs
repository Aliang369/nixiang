using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePropExchangeDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePropExchangeDetailReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlByb3BFeGNoYW5nZURldGFpbC5wcm90bxIFdHlwZXMaGnR5cGUu" + "T3BlcmF0ZUJhc2VJbmZvLnByb3RvGiR0eXBlLk9wZXJhdGVQcm9wRXhjaGFu" + "Z2VEZXRhaWwucHJvdG8aHnR5cGUuT3BlcmF0ZVRhcmdldERldGFpbC5wcm90" + "byLFAQoSUHJvcEV4Y2hhbmdlRGV0YWlsEigKCEJhc2VJbmZvGAEgASgLMhYu" + "dHlwZXMuT3BlcmF0ZUJhc2VJbmZvEg4KBlByb3BJZBgCIAEoBRINCgVDb3Vu" + "dBgDIAEoBRIuCgpUYXJnZXRMaXN0GAQgAygLMhoudHlwZXMuT3BlcmF0ZVRh" + "cmdldERldGFpbBI2CgxFeGNoYW5nZUxpc3QYBSADKAsyIC50eXBlcy5PcGVy" + "YXRlUHJvcEV4Y2hhbmdlRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperatePropExchangeDetailReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PropExchangeDetail), (MessageParser)(object)PropExchangeDetail.Parser, new string[5] { "BaseInfo", "PropId", "Count", "TargetList", "ExchangeList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
