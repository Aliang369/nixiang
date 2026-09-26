using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateRestaurantBuyPieceAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateRestaurantBuyPieceAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk9wZXJhdGVSZXN0YXVyYW50QnV5UGllY2VBY2sucHJvdG8SBXR5" + "cGVzGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aG3R5cGUuUmVzdGF1cmFudERl" + "dGFpbC5wcm90byJ2ChxPcGVyYXRlUmVzdGF1cmFudEJ1eVBpZWNlQWNrEi8K" + "DlJlc3RhdXJhbnRMaXN0GAEgAygLMhcudHlwZXMuUmVzdGF1cmFudERldGFp" + "bBIlCgpEYXRhQ2hhbmdlGAIgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeRestaurantDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateRestaurantBuyPieceAck), (MessageParser)(object)OperateRestaurantBuyPieceAck.Parser, new string[2] { "RestaurantList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
