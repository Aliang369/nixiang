using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateRestaurantOfferPieceAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateRestaurantOfferPieceAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVSZXN0YXVyYW50T2ZmZXJQaWVjZUFjay5wcm90bxIF" + "dHlwZXMaG3R5cGUuUmVzdGF1cmFudERldGFpbC5wcm90byJRCh5PcGVyYXRl" + "UmVzdGF1cmFudE9mZmVyUGllY2VBY2sSLwoOUmVzdGF1cmFudExpc3QYASAD" + "KAsyFy50eXBlcy5SZXN0YXVyYW50RGV0YWlsQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeRestaurantDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateRestaurantOfferPieceAck), (MessageParser)(object)OperateRestaurantOfferPieceAck.Parser, new string[1] { "RestaurantList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
