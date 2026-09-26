using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilPropBuyReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilPropBuyReqReflection()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLk9wZXJhdGVTbm93VmVpbFByb3BCdXlSZXEucHJvdG8SBXR5cGVz" + "IikKGU9wZXJhdGVTbm93VmVpbFByb3BCdXlSZXESDAoEVVVJRBgBIAEoCUIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilPropBuyReq), (MessageParser)(object)OperateSnowVeilPropBuyReq.Parser, new string[1] { "UUID" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
