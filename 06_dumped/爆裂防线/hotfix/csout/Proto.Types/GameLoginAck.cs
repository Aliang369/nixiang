using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class GameLoginAck : IMessage<GameLoginAck>, IMessage, IEquatable<GameLoginAck>, IDeepCloneable<GameLoginAck>, IBufferMessage
{
	private static readonly MessageParser<GameLoginAck> _parser = new MessageParser<GameLoginAck>((Func<GameLoginAck>)(() => new GameLoginAck()));

	private UnknownFieldSet _unknownFields;

	public const int GameEnvFieldNumber = 1;

	private GameEnv gameEnv_;

	public const int RoleStateFieldNumber = 2;

	private RoleState roleState_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<GameLoginAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeGameLoginAckReflection.Descriptor.MessageTypes[0];

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
	public RoleState RoleState
	{
		get
		{
			return roleState_;
		}
		set
		{
			roleState_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GameLoginAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GameLoginAck(GameLoginAck other)
		: this()
	{
		gameEnv_ = ((other.gameEnv_ != null) ? other.gameEnv_.Clone() : null);
		roleState_ = ((other.roleState_ != null) ? other.roleState_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public GameLoginAck Clone()
	{
		return new GameLoginAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as GameLoginAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(GameLoginAck other)
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
		if (!object.Equals(RoleState, other.RoleState))
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
		if (roleState_ != null)
		{
			num ^= RoleState.GetHashCode();
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
		if (roleState_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleState);
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
		if (roleState_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleState);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(GameLoginAck other)
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
		if (other.roleState_ != null)
		{
			if (roleState_ == null)
			{
				RoleState = new RoleState();
			}
			RoleState.MergeFrom(other.RoleState);
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
				if (roleState_ == null)
				{
					RoleState = new RoleState();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleState);
				break;
			}
		}
	}
}
