using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateHolidayTaskTaskAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateHolidayTaskTaskAwardAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVIb2xpZGF5VGFza1Rhc2tBd2FyZEFjay5wcm90bxIF" + "dHlwZXMaHHR5cGUuSG9saWRheVRhc2tEZXRhaWwucHJvdG8iUwoeT3BlcmF0" + "ZUhvbGlkYXlUYXNrVGFza0F3YXJkQWNrEjEKD0hvbGlkYXlUYXNrTGlzdBgB" + "IAMoCzIYLnR5cGVzLkhvbGlkYXlUYXNrRGV0YWlsQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeHolidayTaskDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateHolidayTaskTaskAwardAck), (MessageParser)(object)OperateHolidayTaskTaskAwardAck.Parser, new string[1] { "HolidayTaskList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
