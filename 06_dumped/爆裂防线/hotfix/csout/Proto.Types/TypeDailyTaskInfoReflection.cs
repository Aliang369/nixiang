using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeDailyTaskInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDailyTaskInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkRhaWx5VGFza0luZm8ucHJvdG8SBXR5cGVzGhV0eXBlLlRhc2tE" + "ZXRhaWwucHJvdG8aFXR5cGUuVGFza1RhcmdldC5wcm90byJbCg1EYWlseVRh" + "c2tJbmZvEiMKCFRhc2tMaXN0GAEgAygLMhEudHlwZXMuVGFza0RldGFpbBIl" + "CgpUYXNrVGFyZ2V0GAIgASgLMhEudHlwZXMuVGFza1RhcmdldEIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeTaskDetailReflection.Descriptor,
			TypeTaskTargetReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DailyTaskInfo), (MessageParser)(object)DailyTaskInfo.Parser, new string[2] { "TaskList", "TaskTarget" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
