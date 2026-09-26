using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateThemeTaskDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateThemeTaskDataAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLk9wZXJhdGVUaGVtZVRhc2tEYXRhQWNrLnByb3RvEgV0eXBlcxoa" + "dHlwZS5UaGVtZVRhc2tEZXRhaWwucHJvdG8iSAoXT3BlcmF0ZVRoZW1lVGFz" + "a0RhdGFBY2sSLQoNVGhlbWVUYXNrTGlzdBgBIAMoCzIWLnR5cGVzLlRoZW1l" + "VGFza0RldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeThemeTaskDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateThemeTaskDataAck), (MessageParser)(object)OperateThemeTaskDataAck.Parser, new string[1] { "ThemeTaskList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
