using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOperateBarrageCmdReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateBarrageCmdReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLk9wZXJhdGVCYXJyYWdlQ21kUmVxLnByb3RvEgV0eXBlcxoYZW51" + "bS5FTGFuZ3VhZ2VUeXBlLnByb3RvIlAKFE9wZXJhdGVCYXJyYWdlQ21kUmVx" + "EgwKBFVVSUQYASABKAkSKgoETGFuZxgCIAEoDjIcLkVMYW5ndWFnZVR5cGUu" + "RUxhbmd1YWdlVHlwZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumELanguageTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateBarrageCmdReq), (MessageParser)(object)OperateBarrageCmdReq.Parser, new string[2] { "UUID", "Lang" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
