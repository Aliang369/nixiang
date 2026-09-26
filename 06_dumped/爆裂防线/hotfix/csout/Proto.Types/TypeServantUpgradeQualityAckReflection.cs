using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeServantUpgradeQualityAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeServantUpgradeQualityAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLlNlcnZhbnRVcGdyYWRlUXVhbGl0eUFjay5wcm90bxIFdHlwZXMa" + "FnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90" + "bxoWdHlwZS5TZXJ2YW50SW5mby5wcm90byKRAQoYU2VydmFudFVwZ3JhZGVR" + "dWFsaXR5QWNrEicKC1NlcnZhbnRJbmZvGAEgASgLMhIudHlwZXMuU2VydmFu" + "dEluZm8SJQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMuQXdhcmREZXRhaWwS" + "JQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeServantInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ServantUpgradeQualityAck), (MessageParser)(object)ServantUpgradeQualityAck.Parser, new string[3] { "ServantInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
