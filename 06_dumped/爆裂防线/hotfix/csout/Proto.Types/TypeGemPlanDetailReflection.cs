using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeGemPlanDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGemPlanDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkdlbVBsYW5EZXRhaWwucHJvdG8SBXR5cGVzGhx0eXBlLkdlbVBs" + "YW5TbG90RGV0YWlsLnByb3RvIlIKDUdlbVBsYW5EZXRhaWwSDgoGUGxhbklk" + "GAEgASgFEjEKD0dlbVBsYW5TbG90TGlzdBgCIAMoCzIYLnR5cGVzLkdlbVBs" + "YW5TbG90RGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJn" + "YW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeGemPlanSlotDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GemPlanDetail), (MessageParser)(object)GemPlanDetail.Parser, new string[2] { "PlanId", "GemPlanSlotList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
