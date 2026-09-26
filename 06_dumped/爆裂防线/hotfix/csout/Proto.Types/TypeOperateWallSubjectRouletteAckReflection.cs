using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateWallSubjectRouletteAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateWallSubjectRouletteAckReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cih0eXBlLk9wZXJhdGVXYWxsU3ViamVjdFJvdWxldHRlQWNrLnByb3RvEgV0" + "eXBlcxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdl" + "LnByb3RvGhx0eXBlLldhbGxTdWJqZWN0RGV0YWlsLnByb3RvIrEBCh1PcGVy" + "YXRlV2FsbFN1YmplY3RSb3VsZXR0ZUFjaxIxCg9XYWxsU3ViamVjdExpc3QY" + "ASADKAsyGC50eXBlcy5XYWxsU3ViamVjdERldGFpbBIPCgdIaXRMaXN0GAIg" + "AygFEiUKCUF3YXJkTGlzdBgDIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUK" + "CkRhdGFDaGFuZ2UYBCABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeWallSubjectDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateWallSubjectRouletteAck), (MessageParser)(object)OperateWallSubjectRouletteAck.Parser, new string[4] { "WallSubjectList", "HitList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
