using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBagSelectBoxOpenReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBagSelectBoxOpenReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkJhZ1NlbGVjdEJveE9wZW5SZXEucHJvdG8SBXR5cGVzGhd0eXBl" + "LlNlbGVjdERldGFpbC5wcm90byJMChNCYWdTZWxlY3RCb3hPcGVuUmVxEg4K" + "BkJveFRpZBgBIAEoBRIlCghPcGVuTGlzdBgCIAMoCzITLnR5cGVzLlNlbGVj" + "dERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSelectDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BagSelectBoxOpenReq), (MessageParser)(object)BagSelectBoxOpenReq.Parser, new string[2] { "BoxTid", "OpenList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
