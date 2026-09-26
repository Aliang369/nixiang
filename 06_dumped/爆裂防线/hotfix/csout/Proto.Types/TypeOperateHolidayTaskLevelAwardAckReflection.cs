using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateHolidayTaskLevelAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateHolidayTaskLevelAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLk9wZXJhdGVIb2xpZGF5VGFza0xldmVsQXdhcmRBY2sucHJvdG8S" + "BXR5cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFu" + "Z2UucHJvdG8aHHR5cGUuSG9saWRheVRhc2tEZXRhaWwucHJvdG8iogEKH09w" + "ZXJhdGVIb2xpZGF5VGFza0xldmVsQXdhcmRBY2sSMQoPSG9saWRheVRhc2tM" + "aXN0GAEgAygLMhgudHlwZXMuSG9saWRheVRhc2tEZXRhaWwSJQoJQXdhcmRM" + "aXN0GAIgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgD" + "IAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeHolidayTaskDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateHolidayTaskLevelAwardAck), (MessageParser)(object)OperateHolidayTaskLevelAwardAck.Parser, new string[3] { "HolidayTaskList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
