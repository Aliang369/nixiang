using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class BattleSideTeamInfo : IMessage<BattleSideTeamInfo>, IMessage, IEquatable<BattleSideTeamInfo>, IDeepCloneable<BattleSideTeamInfo>, IBufferMessage
{
	private static readonly MessageParser<BattleSideTeamInfo> _parser = new MessageParser<BattleSideTeamInfo>((Func<BattleSideTeamInfo>)(() => new BattleSideTeamInfo()));

	private UnknownFieldSet _unknownFields;

	public const int MasterTeamInfoFieldNumber = 1;

	private BattleTeamInfo masterTeamInfo_;

	public const int HelperTeamInfoFieldNumber = 2;

	private BattleTeamInfo helperTeamInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<BattleSideTeamInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeBattleSideTeamInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleTeamInfo MasterTeamInfo
	{
		get
		{
			return masterTeamInfo_;
		}
		set
		{
			masterTeamInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleTeamInfo HelperTeamInfo
	{
		get
		{
			return helperTeamInfo_;
		}
		set
		{
			helperTeamInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleSideTeamInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleSideTeamInfo(BattleSideTeamInfo other)
		: this()
	{
		masterTeamInfo_ = ((other.masterTeamInfo_ != null) ? other.masterTeamInfo_.Clone() : null);
		helperTeamInfo_ = ((other.helperTeamInfo_ != null) ? other.helperTeamInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BattleSideTeamInfo Clone()
	{
		return new BattleSideTeamInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as BattleSideTeamInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(BattleSideTeamInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(MasterTeamInfo, other.MasterTeamInfo))
		{
			return false;
		}
		if (!object.Equals(HelperTeamInfo, other.HelperTeamInfo))
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
		if (masterTeamInfo_ != null)
		{
			num ^= MasterTeamInfo.GetHashCode();
		}
		if (helperTeamInfo_ != null)
		{
			num ^= HelperTeamInfo.GetHashCode();
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
		if (masterTeamInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MasterTeamInfo);
		}
		if (helperTeamInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HelperTeamInfo);
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
		if (masterTeamInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MasterTeamInfo);
		}
		if (helperTeamInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HelperTeamInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(BattleSideTeamInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.masterTeamInfo_ != null)
		{
			if (masterTeamInfo_ == null)
			{
				MasterTeamInfo = new BattleTeamInfo();
			}
			MasterTeamInfo.MergeFrom(other.MasterTeamInfo);
		}
		if (other.helperTeamInfo_ != null)
		{
			if (helperTeamInfo_ == null)
			{
				HelperTeamInfo = new BattleTeamInfo();
			}
			HelperTeamInfo.MergeFrom(other.HelperTeamInfo);
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
				if (masterTeamInfo_ == null)
				{
					MasterTeamInfo = new BattleTeamInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MasterTeamInfo);
				break;
			case 18u:
				if (helperTeamInfo_ == null)
				{
					HelperTeamInfo = new BattleTeamInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HelperTeamInfo);
				break;
			}
		}
	}
}
