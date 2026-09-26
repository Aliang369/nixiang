using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLoginSumDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLoginSumDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkxvZ2luU3VtRGV0YWlsLnByb3RvEgV0eXBlcxoadHlwZS5PcGVy" + "YXRlQmFzZUluZm8ucHJvdG8aHnR5cGUuT3BlcmF0ZVRhcmdldERldGFpbC5w" + "cm90byJ5Cg5Mb2dpblN1bURldGFpbBIoCghCYXNlSW5mbxgBIAEoCzIWLnR5" + "cGVzLk9wZXJhdGVCYXNlSW5mbxIuCgpUYXJnZXRMaXN0GAIgAygLMhoudHlw" + "ZXMuT3BlcmF0ZVRhcmdldERldGFpbBINCgVDb3VudBgDIAEoBUIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LoginSumDetail), (MessageParser)(object)LoginSumDetail.Parser, new string[3] { "BaseInfo", "TargetList", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
