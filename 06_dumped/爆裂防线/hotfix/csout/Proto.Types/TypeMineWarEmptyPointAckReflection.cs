using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMineWarEmptyPointAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarEmptyPointAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLk1pbmVXYXJFbXB0eVBvaW50QWNrLnByb3RvEgV0eXBlcxoWdHlw" + "ZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhp0" + "eXBlLk1pbmVXYXJCYXNlSW5mby5wcm90bxoZdHlwZS5NaW5lV2FyTWFwSW5m" + "by5wcm90byLEAQoUTWluZVdhckVtcHR5UG9pbnRBY2sSLwoPTWluZVdhckJh" + "c2VJbmZvGAEgASgLMhYudHlwZXMuTWluZVdhckJhc2VJbmZvEi0KDk1pbmVX" + "YXJNYXBJbmZvGAIgASgLMhUudHlwZXMuTWluZVdhck1hcEluZm8SJQoJQXdh" + "cmRMaXN0GAMgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5n" + "ZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor,
			TypeMineWarMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarEmptyPointAck), (MessageParser)(object)MineWarEmptyPointAck.Parser, new string[4] { "MineWarBaseInfo", "MineWarMapInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
