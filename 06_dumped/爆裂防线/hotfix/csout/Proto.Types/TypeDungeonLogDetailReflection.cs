using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDungeonLogDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDungeonLogDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkR1bmdlb25Mb2dEZXRhaWwucHJvdG8SBXR5cGVzGhxlbnVtLkVE" + "dW5nZW9uRXZlbnRUeXBlLnByb3RvImsKEER1bmdlb25Mb2dEZXRhaWwSDQoF" + "TG9nVG0YASABKAMSNwoJRXZlbnRUeXBlGAIgASgOMiQuRUR1bmdlb25FdmVu" + "dFR5cGUuRUR1bmdlb25FdmVudFR5cGUSDwoHRXZlbnRJZBgDIAEoBUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEDungeonEventTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DungeonLogDetail), (MessageParser)(object)DungeonLogDetail.Parser, new string[3] { "LogTm", "EventType", "EventId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
