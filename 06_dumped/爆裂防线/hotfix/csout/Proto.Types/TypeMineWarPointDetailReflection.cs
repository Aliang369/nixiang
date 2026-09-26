using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMineWarPointDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarPointDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLk1pbmVXYXJQb2ludERldGFpbC5wcm90bxIFdHlwZXMipAEKEk1p" + "bmVXYXJQb2ludERldGFpbBIPCgdQb2ludElkGAEgASgFEhAKCFBsYXllcklk" + "GAIgASgFEhIKClBsYXllck5hbWUYAyABKAkSEQoJUHJvdGVjdFRtGAQgASgD" + "EhAKCExvY2tlZFRtGAUgASgDEhEKCURlc3Ryb3lUbRgGIAEoAxIPCgdTdGFy" + "dFRtGAcgASgDEg4KBkZ1bGxUbRgIIAEoA0IuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarPointDetail), (MessageParser)(object)MineWarPointDetail.Parser, new string[8] { "PointId", "PlayerId", "PlayerName", "ProtectTm", "LockedTm", "DestroyTm", "StartTm", "FullTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
