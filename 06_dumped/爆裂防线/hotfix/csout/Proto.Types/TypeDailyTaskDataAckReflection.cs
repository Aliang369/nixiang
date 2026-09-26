using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeDailyTaskDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDailyTaskDataAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkRhaWx5VGFza0RhdGFBY2sucHJvdG8SBXR5cGVzGhh0eXBlLkRh" + "aWx5VGFza0luZm8ucHJvdG8iPwoQRGFpbHlUYXNrRGF0YUFjaxIrCg1EYWls" + "eVRhc2tJbmZvGAEgASgLMhQudHlwZXMuRGFpbHlUYXNrSW5mb0IuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeDailyTaskInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DailyTaskDataAck), (MessageParser)(object)DailyTaskDataAck.Parser, new string[1] { "DailyTaskInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
