using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeElementTrialBuyCombatAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeElementTrialBuyCombatAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkVsZW1lbnRUcmlhbEJ1eUNvbWJhdEFjay5wcm90bxIFdHlwZXMa" + "FXR5cGUuRGF0YUNoYW5nZS5wcm90bxobdHlwZS5FbGVtZW50VHJpYWxJbmZv" + "LnByb3RvInQKGEVsZW1lbnRUcmlhbEJ1eUNvbWJhdEFjaxIxChBFbGVtZW50" + "VHJpYWxJbmZvGAEgASgLMhcudHlwZXMuRWxlbWVudFRyaWFsSW5mbxIlCgpE" + "YXRhQ2hhbmdlGAIgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeElementTrialInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ElementTrialBuyCombatAck), (MessageParser)(object)ElementTrialBuyCombatAck.Parser, new string[2] { "ElementTrialInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
