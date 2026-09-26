using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateWallSubjectFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateWallSubjectFightResultAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cit0eXBlLk9wZXJhdGVXYWxsU3ViamVjdEZpZ2h0UmVzdWx0QWNrLnByb3Rv" + "EgV0eXBlcxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxobdHlwZS5CYXR0bGVS" + "ZXN1bHRJbmZvLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aHHR5cGUu" + "V2FsbFN1YmplY3REZXRhaWwucHJvdG8i0gEKIE9wZXJhdGVXYWxsU3ViamVj" + "dEZpZ2h0UmVzdWx0QWNrEjEKD1dhbGxTdWJqZWN0TGlzdBgBIAMoCzIYLnR5" + "cGVzLldhbGxTdWJqZWN0RGV0YWlsEi0KDEJhdHRsZVNldHRsZRgCIAEoCzIX" + "LnR5cGVzLkJhdHRsZVJlc3VsdEluZm8SJQoJQXdhcmRMaXN0GAMgAygLMhIu" + "dHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgEIAEoCzIRLnR5cGVz" + "LkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeWallSubjectDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateWallSubjectFightResultAck), (MessageParser)(object)OperateWallSubjectFightResultAck.Parser, new string[4] { "WallSubjectList", "BattleSettle", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
