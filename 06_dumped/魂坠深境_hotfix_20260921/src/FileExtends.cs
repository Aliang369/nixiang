using System.IO;
using Sirenix.Serialization;

public static class FileExtends
{
	public static T LoadSerializedObject<T>(string path)
	{
		return default(T);
	}

	public static void SaveSerializedObject<T>(string path, T t)
	{
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.Position = 0L;
		using StreamWriter streamWriter = new StreamWriter(path, append: false);
		SerializationUtility.SerializeValue(t, memoryStream, (DataFormat)1);
		new BinaryWriter(streamWriter.BaseStream).Write(memoryStream.ToArray());
	}

	public static long GetFileSize(string path)
	{
		return new FileInfo(path).Length;
	}
}
