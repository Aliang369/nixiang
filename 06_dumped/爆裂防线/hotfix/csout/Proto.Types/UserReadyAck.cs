using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class UserReadyAck : IMessage<UserReadyAck>, IMessage, IEquatable<UserReadyAck>, IDeepCloneable<UserReadyAck>, IBufferMessage
{
	private static readonly MessageParser<UserReadyAck> _parser = new MessageParser<UserReadyAck>((Func<UserReadyAck>)(() => new UserReadyAck()));

	private UnknownFieldSet _unknownFields;

	public const int GameEnvFieldNumber = 1;

	private GameEnv gameEnv_;

	public const int GateFieldNumber = 2;

	private Gate gate_;

	public const int ServerIdFieldNumber = 3;

	private int serverId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<UserReadyAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeUserReadyAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GameEnv GameEnv
	{
		get
		{
			return gameEnv_;
		}
		set
		{
			gameEnv_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Gate Gate
	{
		get
		{
			return gate_;
		}
		set
		{
			gate_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ServerId
	{
		get
		{
			return serverId_;
		}
		set
		{
			serverId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserReadyAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserReadyAck(UserReadyAck other)
		: this()
	{
		gameEnv_ = ((other.gameEnv_ != null) ? other.gameEnv_.Clone() : null);
		gate_ = ((other.gate_ != null) ? other.gate_.Clone() : null);
		serverId_ = other.serverId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public UserReadyAck Clone()
	{
		return new UserReadyAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as UserReadyAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(UserReadyAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(GameEnv, other.GameEnv))
		{
			return false;
		}
		if (!object.Equals(Gate, other.Gate))
		{
			return false;
		}
		if (ServerId != other.ServerId)
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
		if (gameEnv_ != null)
		{
			num ^= GameEnv.GetHashCode();
		}
		if (gate_ != null)
		{
			num ^= Gate.GetHashCode();
		}
		if (ServerId != 0)
		{
			num ^= ServerId.GetHashCode();
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
		if (gameEnv_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)GameEnv);
		}
		if (gate_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Gate);
		}
		if (ServerId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(ServerId);
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
		if (gameEnv_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)GameEnv);
		}
		if (gate_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Gate);
		}
		if (ServerId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ServerId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(UserReadyAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.gameEnv_ != null)
		{
			if (gameEnv_ == null)
			{
				GameEnv = new GameEnv();
			}
			GameEnv.MergeFrom(other.GameEnv);
		}
		if (other.gate_ != null)
		{
			if (gate_ == null)
			{
				Gate = new Gate();
			}
			Gate.MergeFrom(other.Gate);
		}
		if (other.ServerId != 0)
		{
			ServerId = other.ServerId;
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
				if (gameEnv_ == null)
				{
					GameEnv = new GameEnv();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)GameEnv);
				break;
			case 18u:
				if (gate_ == null)
				{
					Gate = new Gate();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Gate);
				break;
			case 24u:
				ServerId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
