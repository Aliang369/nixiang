using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeShopDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeShopDataAckReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLlNob3BEYXRhQWNrLnByb3RvEgV0eXBlcxoVdHlwZS5TaG9wRGV0" + "YWlsLnByb3RvIjIKC1Nob3BEYXRhQWNrEiMKCFNob3BMaXN0GAEgAygLMhEu" + "dHlwZXMuU2hvcERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oC" + "ElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeShopDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ShopDataAck), (MessageParser)(object)ShopDataAck.Parser, new string[1] { "ShopList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
