using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossMineDestroyEmptyPointAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossMineDestroyEmptyPointAckReflection()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cih0eXBlLkNyb3NzTWluZURlc3Ryb3lFbXB0eVBvaW50QWNrLnByb3RvEgV0" + "eXBlcxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdl" + "LnByb3RvGhp0eXBlLk1pbmVXYXJCYXNlSW5mby5wcm90bxoZdHlwZS5NaW5l" + "V2FyTWFwSW5mby5wcm90byLkAQodQ3Jvc3NNaW5lRGVzdHJveUVtcHR5UG9p" + "bnRBY2sSEAoIU2VydmVySWQYASABKAUSLwoPTWluZVdhckJhc2VJbmZvGAIg" + "ASgLMhYudHlwZXMuTWluZVdhckJhc2VJbmZvEjIKE0Nyb3NzTWluZVdhck1h" + "cEluZm8YAyABKAsyFS50eXBlcy5NaW5lV2FyTWFwSW5mbxIlCglBd2FyZExp" + "c3QYBCADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAUg" + "ASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor,
			TypeMineWarMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossMineDestroyEmptyPointAck), (MessageParser)(object)CrossMineDestroyEmptyPointAck.Parser, new string[5] { "ServerId", "MineWarBaseInfo", "CrossMineWarMapInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
