using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeActiveNovicePackAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeActiveNovicePackAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLlJlY2hhcmdlQWN0aXZlTm92aWNlUGFja0Fjay5wcm90bxIFdHlw" + "ZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5w" + "cm90bxoddHlwZS5SZWNoYXJnZUFjdGl2ZUluZm8ucHJvdG8iogEKG1JlY2hh" + "cmdlQWN0aXZlTm92aWNlUGFja0FjaxI1ChJSZWNoYXJnZUFjdGl2ZUluZm8Y" + "ASABKAsyGS50eXBlcy5SZWNoYXJnZUFjdGl2ZUluZm8SJQoJQXdhcmRMaXN0" + "GAIgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEo" + "CzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlw" + "ZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeRechargeActiveInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeActiveNovicePackAck), (MessageParser)(object)RechargeActiveNovicePackAck.Parser, new string[3] { "RechargeActiveInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
