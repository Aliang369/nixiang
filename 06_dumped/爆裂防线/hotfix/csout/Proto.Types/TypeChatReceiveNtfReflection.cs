using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeChatReceiveNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeChatReceiveNtfReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkNoYXRSZWNlaXZlTnRmLnByb3RvEgV0eXBlcxoVdHlwZS5DaGF0" + "RGV0YWlsLnByb3RvIjgKDkNoYXRSZWNlaXZlTnRmEiYKC1JlY2VpdmVMaXN0" + "GAEgAygLMhEudHlwZXMuQ2hhdERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeChatDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ChatReceiveNtf), (MessageParser)(object)ChatReceiveNtf.Parser, new string[1] { "ReceiveList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
