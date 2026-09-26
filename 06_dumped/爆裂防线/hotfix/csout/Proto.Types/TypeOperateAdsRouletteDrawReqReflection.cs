using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsRouletteDrawReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsRouletteDrawReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLk9wZXJhdGVBZHNSb3VsZXR0ZURyYXdSZXEucHJvdG8SBXR5cGVz" + "IjgKGU9wZXJhdGVBZHNSb3VsZXR0ZURyYXdSZXESDAoEVVVJRBgBIAEoCRIN" + "CgVBZHZJZBgCIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsRouletteDrawReq), (MessageParser)(object)OperateAdsRouletteDrawReq.Parser, new string[2] { "UUID", "AdvId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
