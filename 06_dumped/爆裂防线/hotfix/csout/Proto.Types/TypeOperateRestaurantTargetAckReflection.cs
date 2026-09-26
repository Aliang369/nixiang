using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateRestaurantTargetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateRestaurantTargetAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLk9wZXJhdGVSZXN0YXVyYW50VGFyZ2V0QWNrLnByb3RvEgV0eXBl" + "cxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnBy" + "b3RvGht0eXBlLlJlc3RhdXJhbnREZXRhaWwucHJvdG8imwEKGk9wZXJhdGVS" + "ZXN0YXVyYW50VGFyZ2V0QWNrEi8KDlJlc3RhdXJhbnRMaXN0GAEgAygLMhcu" + "dHlwZXMuUmVzdGF1cmFudERldGFpbBIlCglBd2FyZExpc3QYAiADKAsyEi50" + "eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMu" + "RGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeRestaurantDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateRestaurantTargetAck), (MessageParser)(object)OperateRestaurantTargetAck.Parser, new string[3] { "RestaurantList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
