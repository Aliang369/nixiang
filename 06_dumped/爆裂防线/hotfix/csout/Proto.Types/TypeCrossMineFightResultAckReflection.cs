using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossMineFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossMineFightResultAckReflection()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkNyb3NzTWluZUZpZ2h0UmVzdWx0QWNrLnByb3RvEgV0eXBlcxoW" + "dHlwZS5Bd2FyZERldGFpbC5wcm90bxobdHlwZS5CYXR0bGVSZXN1bHRJbmZv" + "LnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aGnR5cGUuTWluZVdhckJh" + "c2VJbmZvLnByb3RvGhl0eXBlLk1pbmVXYXJNYXBJbmZvLnByb3RvIo0CChdD" + "cm9zc01pbmVGaWdodFJlc3VsdEFjaxIQCghTZXJ2ZXJJZBgBIAEoBRIvCg9N" + "aW5lV2FyQmFzZUluZm8YAiABKAsyFi50eXBlcy5NaW5lV2FyQmFzZUluZm8S" + "MgoTQ3Jvc3NNaW5lV2FyTWFwSW5mbxgDIAEoCzIVLnR5cGVzLk1pbmVXYXJN" + "YXBJbmZvEi0KDEJhdHRsZVNldHRsZRgEIAEoCzIXLnR5cGVzLkJhdHRsZVJl" + "c3VsdEluZm8SJQoJQXdhcmRMaXN0GAUgAygLMhIudHlwZXMuQXdhcmREZXRh" + "aWwSJQoKRGF0YUNoYW5nZRgGIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor,
			TypeMineWarMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossMineFightResultAck), (MessageParser)(object)CrossMineFightResultAck.Parser, new string[6] { "ServerId", "MineWarBaseInfo", "CrossMineWarMapInfo", "BattleSettle", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
