using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeSumDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeSumDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlJlY2hhcmdlU3VtRGV0YWlsLnByb3RvEgV0eXBlcxoadHlwZS5P" + "cGVyYXRlQmFzZUluZm8ucHJvdG8aH3R5cGUuUmVjaGFyZ2VUYXJnZXREZXRh" + "aWwucHJvdG8ifQoRUmVjaGFyZ2VTdW1EZXRhaWwSKAoIQmFzZUluZm8YASAB" + "KAsyFi50eXBlcy5PcGVyYXRlQmFzZUluZm8SLwoKVGFyZ2V0TGlzdBgCIAMo" + "CzIbLnR5cGVzLlJlY2hhcmdlVGFyZ2V0RGV0YWlsEg0KBUNvdW50GAMgASgF" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeOperateBaseInfoReflection.Descriptor,
			TypeRechargeTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeSumDetail), (MessageParser)(object)RechargeSumDetail.Parser, new string[3] { "BaseInfo", "TargetList", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
