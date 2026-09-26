using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarPointInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarPointInfoAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkxlZ2lvbldhclBvaW50SW5mb0Fjay5wcm90bxIFdHlwZXMaJHR5" + "cGUuTGVnaW9uV2FyUG9pbnRTdGF0ZURldGFpbC5wcm90byJNChVMZWdpb25X" + "YXJQb2ludEluZm9BY2sSNAoKUG9pbnRTdGF0ZRgBIAEoCzIgLnR5cGVzLkxl" + "Z2lvbldhclBvaW50U3RhdGVEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeLegionWarPointStateDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarPointInfoAck), (MessageParser)(object)LegionWarPointInfoAck.Parser, new string[1] { "PointState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
