using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeShopBuyAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeShopBuyAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLlNob3BCdXlBY2sucHJvdG8SBXR5cGVzGhZ0eXBlLkF3YXJkRGV0" + "YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aFXR5cGUuU2hvcERl" + "dGFpbC5wcm90byJ/CgpTaG9wQnV5QWNrEiMKCFNob3BMaXN0GAEgAygLMhEu" + "dHlwZXMuU2hvcERldGFpbBIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5B" + "d2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNo" + "YW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeShopDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ShopBuyAck), (MessageParser)(object)ShopBuyAck.Parser, new string[3] { "ShopList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
