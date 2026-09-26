using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRolePvpTipsInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRolePvpTipsInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLlJvbGVQdnBUaXBzSW5mby5wcm90bxIFdHlwZXMaFXR5cGUuUHZw" + "TW9kSW5mby5wcm90bxoXdHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8iYwoPUm9s" + "ZVB2cFRpcHNJbmZvEikKDFJvbGVCYXNlSW5mbxgBIAEoCzITLnR5cGVzLlJv" + "bGVCYXNlSW5mbxIlCgpQdnBNb2RJbmZvGAIgASgLMhEudHlwZXMuUHZwTW9k" + "SW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypePvpModInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RolePvpTipsInfo), (MessageParser)(object)RolePvpTipsInfo.Parser, new string[2] { "RoleBaseInfo", "PvpModInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
