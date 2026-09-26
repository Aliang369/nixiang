using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeThemeTaskDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeThemeTaskDetailReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLlRoZW1lVGFza0RldGFpbC5wcm90bxIFdHlwZXMaGnR5cGUuT3Bl" + "cmF0ZUJhc2VJbmZvLnByb3RvGh50eXBlLk9wZXJhdGVUYXJnZXREZXRhaWwu" + "cHJvdG8aFXR5cGUuVGFza0RldGFpbC5wcm90byKiAQoPVGhlbWVUYXNrRGV0" + "YWlsEigKCEJhc2VJbmZvGAEgASgLMhYudHlwZXMuT3BlcmF0ZUJhc2VJbmZv" + "EiMKCFRhc2tMaXN0GAIgAygLMhEudHlwZXMuVGFza0RldGFpbBIQCghUb3Rh" + "bEV4cBgDIAEoBRIuCgpUYXJnZXRMaXN0GAQgAygLMhoudHlwZXMuT3BlcmF0" + "ZVRhcmdldERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor,
			TypeTaskDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ThemeTaskDetail), (MessageParser)(object)ThemeTaskDetail.Parser, new string[4] { "BaseInfo", "TaskList", "TotalExp", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
