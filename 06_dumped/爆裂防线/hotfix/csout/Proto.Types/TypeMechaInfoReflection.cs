using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMechaInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMechaInfoReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChR0eXBlLk1lY2hhSW5mby5wcm90bxIFdHlwZXMaFnR5cGUuTWVjaGFEZXRh" + "aWwucHJvdG8iVwoJTWVjaGFJbmZvEiUKCU1lY2hhTGlzdBgBIAMoCzISLnR5" + "cGVzLk1lY2hhRGV0YWlsEhMKC1RvdGFsQXR0YWNrGAIgASgFEg4KBkVuZXJn" + "eRgDIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeMechaDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MechaInfo), (MessageParser)(object)MechaInfo.Parser, new string[3] { "MechaList", "TotalAttack", "Energy" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
