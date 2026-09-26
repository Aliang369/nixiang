using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSnowVeilTurntableDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSnowVeilTurntableDetailReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLlNub3dWZWlsVHVybnRhYmxlRGV0YWlsLnByb3RvEgV0eXBlcxom" + "dHlwZS5Tbm93VmVpbFR1cm50YWJsZVNsb3REZXRhaWwucHJvdG8iYQoXU25v" + "d1ZlaWxUdXJudGFibGVEZXRhaWwSEAoIUnVuQ291bnQYASABKAUSNAoIU2xv" + "dExpc3QYAiADKAsyIi50eXBlcy5Tbm93VmVpbFR1cm50YWJsZVNsb3REZXRh" + "aWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSnowVeilTurntableSlotDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SnowVeilTurntableDetail), (MessageParser)(object)SnowVeilTurntableDetail.Parser, new string[2] { "RunCount", "SlotList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
