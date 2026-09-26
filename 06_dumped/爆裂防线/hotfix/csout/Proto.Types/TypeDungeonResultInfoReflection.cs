using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDungeonResultInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDungeonResultInfoReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkR1bmdlb25SZXN1bHRJbmZvLnByb3RvEgV0eXBlcxoVZW51bS5F" + "UGFzc1N0YXRlLnByb3RvGiN0eXBlLkR1bmdlb25SZXN1bHRBd2FyZERldGFp" + "bC5wcm90byLCAQoRRHVuZ2VvblJlc3VsdEluZm8SEQoJRHVuZ2VvbklkGAEg" + "ASgFEikKCVBhc3NTdGF0ZRgCIAEoDjIWLkVQYXNzU3RhdGUuRVBhc3NTdGF0" + "ZRISCgpFdmVudENvdW50GAMgASgFEhQKDFRyaWdnZXJDb3VudBgEIAEoBRIR" + "CglLaWxsQ291bnQYBSABKAUSMgoJQXdhcmRMaXN0GAYgAygLMh8udHlwZXMu" + "RHVuZ2VvblJlc3VsdEF3YXJkRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEPassStateReflection.Descriptor,
			TypeDungeonResultAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DungeonResultInfo), (MessageParser)(object)DungeonResultInfo.Parser, new string[6] { "DungeonId", "PassState", "EventCount", "TriggerCount", "KillCount", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
