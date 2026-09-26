using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MineWarBaseInfo : IMessage<MineWarBaseInfo>, IMessage, IEquatable<MineWarBaseInfo>, IDeepCloneable<MineWarBaseInfo>, IBufferMessage
{
	private static readonly MessageParser<MineWarBaseInfo> _parser = new MessageParser<MineWarBaseInfo>((Func<MineWarBaseInfo>)(() => new MineWarBaseInfo()));

	private UnknownFieldSet _unknownFields;

	public const int MineWarOpenInfoFieldNumber = 1;

	private MineWarOpenInfo mineWarOpenInfo_;

	public const int MineWarSelfInfoFieldNumber = 2;

	private MineWarSelfInfo mineWarSelfInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MineWarBaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMineWarBaseInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarOpenInfo MineWarOpenInfo
	{
		get
		{
			return mineWarOpenInfo_;
		}
		set
		{
			mineWarOpenInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarSelfInfo MineWarSelfInfo
	{
		get
		{
			return mineWarSelfInfo_;
		}
		set
		{
			mineWarSelfInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarBaseInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarBaseInfo(MineWarBaseInfo other)
		: this()
	{
		mineWarOpenInfo_ = ((other.mineWarOpenInfo_ != null) ? other.mineWarOpenInfo_.Clone() : null);
		mineWarSelfInfo_ = ((other.mineWarSelfInfo_ != null) ? other.mineWarSelfInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarBaseInfo Clone()
	{
		return new MineWarBaseInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MineWarBaseInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MineWarBaseInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(MineWarOpenInfo, other.MineWarOpenInfo))
		{
			return false;
		}
		if (!object.Equals(MineWarSelfInfo, other.MineWarSelfInfo))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (mineWarOpenInfo_ != null)
		{
			num ^= MineWarOpenInfo.GetHashCode();
		}
		if (mineWarSelfInfo_ != null)
		{
			num ^= MineWarSelfInfo.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= ((object)_unknownFields).GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (mineWarOpenInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MineWarOpenInfo);
		}
		if (mineWarSelfInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MineWarSelfInfo);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (mineWarOpenInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MineWarOpenInfo);
		}
		if (mineWarSelfInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MineWarSelfInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MineWarBaseInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.mineWarOpenInfo_ != null)
		{
			if (mineWarOpenInfo_ == null)
			{
				MineWarOpenInfo = new MineWarOpenInfo();
			}
			MineWarOpenInfo.MergeFrom(other.MineWarOpenInfo);
		}
		if (other.mineWarSelfInfo_ != null)
		{
			if (mineWarSelfInfo_ == null)
			{
				MineWarSelfInfo = new MineWarSelfInfo();
			}
			MineWarSelfInfo.MergeFrom(other.MineWarSelfInfo);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = ((ParseContext)(ref input)).ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 10u:
				if (mineWarOpenInfo_ == null)
				{
					MineWarOpenInfo = new MineWarOpenInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MineWarOpenInfo);
				break;
			case 18u:
				if (mineWarSelfInfo_ == null)
				{
					MineWarSelfInfo = new MineWarSelfInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MineWarSelfInfo);
				break;
			}
		}
	}
}
