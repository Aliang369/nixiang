using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEFuncTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEFuncTypeReflection()
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChRlbnVtLkVGdW5jVHlwZS5wcm90bxIJRUZ1bmNUeXBlKpkHCglFRnVuY1R5" + "cGUSDgoKRW1wdHlWYWx1ZRAAEg0KCU1haW5RdWVzdBABEggKBFRhc2sQAhIM" + "CghBY3Rpdml0eRADEg4KCkVsaXRlUXVlc3QQBBILCgdFeHBsb3JlEAUSCwoH" + "RnJpZW5kcxAGEg0KCVdvcmxkQm9zcxAHEg0KCVdvcmxkTGFpchAIEgoKBkxl" + "Z2lvbhAJEg0KCUhlcm9UcmlhbBAKEg4KCkxlZ2lvbkJvc3MQCxIOCgpVbmRl" + "ckFyZW5hEA4SEgoOWm9tYmllQ3JlYXRpb24QEBIOCgpMZWdpb25SYWlkEBES" + "DQoJQmxvY2tXYXJzEBISDAoIVW5kZXJXYXIQExINCglSb29tQnVpbGQQFBIH" + "CgNHZW0QFRISCg5NYXRlcmlhbFJlZmluZRAWEg8KC1Jvb21VcGdyYWRlEBcS" + "DwoLV2FsbFVwZ3JhZGUQGBIQCgxFcXVpcFVwZ3JhZGUQGRIOCgpDYXJVcGdy" + "YWRlEBoSEwoPR2VuZXRpY1Jlc2VhcmNoEBwSEQoNU3Vid2F5VXBncmFkZRAd" + "Eg8KC0dlbUNyZWF0aW9uEB4SEQoNRXF1aXBDcmVhdGlvbhAfEgcKA0JhchAg" + "EgwKCFBhc3NQb3J0ECESCAoEUmFuaxAiEggKBElkbGUQIxIQCgxFbGVtZW50" + "VHJpYWwQJBIMCghIb21lbGFuZBAlEg4KCkhlcm9Bd2FrZW4QJhIQCgxFbmRs" + "ZXNzSG9yZGUQJxIJCgVNZWNoYRAoEg4KCk1lY2hhU2tpbGwQKRIPCgtIYW5k" + "Ym9va01vZBAqEg8KC1dhbGxEZWZlbnNlECsSDQoJTGVnZW5kYXJ5ECwSCQoF" + "QXJlbmEQLRINCglHdWFyZElkbGUQLhIJCgVTaG9wMRAvEgkKBVNob3AyEDAS" + "CQoFU2hvcDMQMRIJCgVTaG9wNBAyEg0KCUd1YXJkUGxhbhAzEgsKB0dlbVBs" + "YW4QNBIPCgtDcm9zc0xhZGRlchA1EgwKCEdlbmVMb2NrEDYSDAoIVHJlYXN1" + "cmUQNxILCgdNaW5lV2FyEDgSCwoHU2VydmFudBA5Eg4KCkhlcm9Gb2xsb3cQ" + "OhIOCgpIZXJvUmFyaXR5EDsSCQoFVHJ1Y2sQPBINCglMZWdpb25XYXIQPRIM" + "CghGYWNpbGl0eRA+EhEKDUFkdmVydGlzZW1lbnQQPxIJCgVTaG9wNRBAEg0K" + "CURhaWx5U2lnbhBBQi5aCS4vO3Byb3RvY6oCC1Byb3RvLkVudW1zygISWmJn" + "YW1lXFByb3RvXEVudW1zYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EFuncType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
