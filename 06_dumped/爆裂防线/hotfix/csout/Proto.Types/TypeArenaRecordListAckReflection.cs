using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeArenaRecordListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaRecordListAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkFyZW5hUmVjb3JkTGlzdEFjay5wcm90bxIFdHlwZXMaHHR5cGUu" + "QXJlbmFSZWNvcmREZXRhaWwucHJvdG8iQgoSQXJlbmFSZWNvcmRMaXN0QWNr" + "EiwKClJlY29yZExpc3QYASADKAsyGC50eXBlcy5BcmVuYVJlY29yZERldGFp" + "bEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeArenaRecordDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaRecordListAck), (MessageParser)(object)ArenaRecordListAck.Parser, new string[1] { "RecordList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
