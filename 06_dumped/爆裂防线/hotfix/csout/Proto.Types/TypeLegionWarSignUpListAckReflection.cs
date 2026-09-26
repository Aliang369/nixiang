using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarSignUpListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarSignUpListAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkxlZ2lvbldhclNpZ25VcExpc3RBY2sucHJvdG8SBXR5cGVzGhl0" + "eXBlLkxlZ2lvbkJhc2VJbmZvLnByb3RvIm4KFkxlZ2lvbldhclNpZ25VcExp" + "c3RBY2sSDAoEUGFnZRgBIAEoBRIMCgRTaXplGAIgASgFEg0KBUNvdW50GAMg" + "ASgFEikKCkxlZ2lvbkxpc3QYBCADKAsyFS50eXBlcy5MZWdpb25CYXNlSW5m" + "b0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeLegionBaseInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarSignUpListAck), (MessageParser)(object)LegionWarSignUpListAck.Parser, new string[4] { "Page", "Size", "Count", "LegionList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
