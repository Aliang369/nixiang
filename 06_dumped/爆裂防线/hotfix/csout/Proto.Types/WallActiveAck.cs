using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class WallActiveAck : IMessage<WallActiveAck>, IMessage, IEquatable<WallActiveAck>, IDeepCloneable<WallActiveAck>, IBufferMessage
{
	private static readonly MessageParser<WallActiveAck> _parser = new MessageParser<WallActiveAck>((Func<WallActiveAck>)(() => new WallActiveAck()));

	private UnknownFieldSet _unknownFields;

	public const int WallIdFieldNumber = 1;

	private int wallId_;

	public const int WallInfoFieldNumber = 2;

	private WallInfo wallInfo_;

	public const int DefenseInfoFieldNumber = 3;

	private DefenseInfo defenseInfo_;

	public const int DataChangeFieldNumber = 4;

	private DataChange dataChange_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<WallActiveAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeWallActiveAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int WallId
	{
		get
		{
			return wallId_;
		}
		set
		{
			wallId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WallInfo WallInfo
	{
		get
		{
			return wallInfo_;
		}
		set
		{
			wallInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DefenseInfo DefenseInfo
	{
		get
		{
			return defenseInfo_;
		}
		set
		{
			defenseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DataChange DataChange
	{
		get
		{
			return dataChange_;
		}
		set
		{
			dataChange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WallActiveAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WallActiveAck(WallActiveAck other)
		: this()
	{
		wallId_ = other.wallId_;
		wallInfo_ = ((other.wallInfo_ != null) ? other.wallInfo_.Clone() : null);
		defenseInfo_ = ((other.defenseInfo_ != null) ? other.defenseInfo_.Clone() : null);
		dataChange_ = ((other.dataChange_ != null) ? other.dataChange_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public WallActiveAck Clone()
	{
		return new WallActiveAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as WallActiveAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(WallActiveAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (WallId != other.WallId)
		{
			return false;
		}
		if (!object.Equals(WallInfo, other.WallInfo))
		{
			return false;
		}
		if (!object.Equals(DefenseInfo, other.DefenseInfo))
		{
			return false;
		}
		if (!object.Equals(DataChange, other.DataChange))
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
		if (WallId != 0)
		{
			num ^= WallId.GetHashCode();
		}
		if (wallInfo_ != null)
		{
			num ^= WallInfo.GetHashCode();
		}
		if (defenseInfo_ != null)
		{
			num ^= DefenseInfo.GetHashCode();
		}
		if (dataChange_ != null)
		{
			num ^= DataChange.GetHashCode();
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
		if (WallId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(WallId);
		}
		if (wallInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)WallInfo);
		}
		if (defenseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DefenseInfo);
		}
		if (dataChange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DataChange);
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
		if (WallId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(WallId);
		}
		if (wallInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)WallInfo);
		}
		if (defenseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DefenseInfo);
		}
		if (dataChange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DataChange);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(WallActiveAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.WallId != 0)
		{
			WallId = other.WallId;
		}
		if (other.wallInfo_ != null)
		{
			if (wallInfo_ == null)
			{
				WallInfo = new WallInfo();
			}
			WallInfo.MergeFrom(other.WallInfo);
		}
		if (other.defenseInfo_ != null)
		{
			if (defenseInfo_ == null)
			{
				DefenseInfo = new DefenseInfo();
			}
			DefenseInfo.MergeFrom(other.DefenseInfo);
		}
		if (other.dataChange_ != null)
		{
			if (dataChange_ == null)
			{
				DataChange = new DataChange();
			}
			DataChange.MergeFrom(other.DataChange);
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
			case 8u:
				WallId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 18u:
				if (wallInfo_ == null)
				{
					WallInfo = new WallInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)WallInfo);
				break;
			case 26u:
				if (defenseInfo_ == null)
				{
					DefenseInfo = new DefenseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DefenseInfo);
				break;
			case 34u:
				if (dataChange_ == null)
				{
					DataChange = new DataChange();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DataChange);
				break;
			}
		}
	}
}
