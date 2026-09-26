using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MineWarPointIdleInfo : IMessage<MineWarPointIdleInfo>, IMessage, IEquatable<MineWarPointIdleInfo>, IDeepCloneable<MineWarPointIdleInfo>, IBufferMessage
{
	private static readonly MessageParser<MineWarPointIdleInfo> _parser = new MessageParser<MineWarPointIdleInfo>((Func<MineWarPointIdleInfo>)(() => new MineWarPointIdleInfo()));

	private UnknownFieldSet _unknownFields;

	public const int SettleAwardFieldNumber = 1;

	private AwardDetail settleAward_;

	public const int StartTmFieldNumber = 2;

	private long startTm_;

	public const int FullTmFieldNumber = 3;

	private long fullTm_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MineWarPointIdleInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMineWarPointIdleInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public AwardDetail SettleAward
	{
		get
		{
			return settleAward_;
		}
		set
		{
			settleAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StartTm
	{
		get
		{
			return startTm_;
		}
		set
		{
			startTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long FullTm
	{
		get
		{
			return fullTm_;
		}
		set
		{
			fullTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarPointIdleInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarPointIdleInfo(MineWarPointIdleInfo other)
		: this()
	{
		settleAward_ = ((other.settleAward_ != null) ? other.settleAward_.Clone() : null);
		startTm_ = other.startTm_;
		fullTm_ = other.fullTm_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarPointIdleInfo Clone()
	{
		return new MineWarPointIdleInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MineWarPointIdleInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MineWarPointIdleInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(SettleAward, other.SettleAward))
		{
			return false;
		}
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (FullTm != other.FullTm)
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
		if (settleAward_ != null)
		{
			num ^= SettleAward.GetHashCode();
		}
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (FullTm != 0L)
		{
			num ^= FullTm.GetHashCode();
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
		if (settleAward_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)SettleAward);
		}
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (FullTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt64(FullTm);
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
		if (settleAward_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SettleAward);
		}
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (FullTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(FullTm);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MineWarPointIdleInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.settleAward_ != null)
		{
			if (settleAward_ == null)
			{
				SettleAward = new AwardDetail();
			}
			SettleAward.MergeFrom(other.SettleAward);
		}
		if (other.StartTm != 0L)
		{
			StartTm = other.StartTm;
		}
		if (other.FullTm != 0L)
		{
			FullTm = other.FullTm;
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
				if (settleAward_ == null)
				{
					SettleAward = new AwardDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)SettleAward);
				break;
			case 16u:
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				FullTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			}
		}
	}
}
