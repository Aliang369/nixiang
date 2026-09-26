using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using zbType;

public class zbMainScene : zbBaseScene
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRedPointerUpdateAsync_003Ed__4 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CancellationToken token;

		private TimeSpan _003CwaitTick_003E5__2;

		private TimeSpan _003CwaitFight_003E5__3;

		private bool _003CisWorldHandlerHasRedPoint_003E5__4;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_023e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0354: Unknown result type (might be due to invalid IL or missing references)
			//IL_0359: Unknown result type (might be due to invalid IL or missing references)
			//IL_0360: Unknown result type (might be due to invalid IL or missing references)
			//IL_03df: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_046a: Unknown result type (might be due to invalid IL or missing references)
			//IL_046f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0476: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0501: Unknown result type (might be due to invalid IL or missing references)
			//IL_0580: Unknown result type (might be due to invalid IL or missing references)
			//IL_0585: Unknown result type (might be due to invalid IL or missing references)
			//IL_058c: Unknown result type (might be due to invalid IL or missing references)
			//IL_060d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0612: Unknown result type (might be due to invalid IL or missing references)
			//IL_0619: Unknown result type (might be due to invalid IL or missing references)
			//IL_069a: Unknown result type (might be due to invalid IL or missing references)
			//IL_069f: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0727: Unknown result type (might be due to invalid IL or missing references)
			//IL_072c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0733: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0866: Unknown result type (might be due to invalid IL or missing references)
			//IL_086b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0872: Unknown result type (might be due to invalid IL or missing references)
			//IL_08f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_08f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_08ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0980: Unknown result type (might be due to invalid IL or missing references)
			//IL_0985: Unknown result type (might be due to invalid IL or missing references)
			//IL_098c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a21: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a26: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a2d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aba: Unknown result type (might be due to invalid IL or missing references)
			//IL_0abf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ac6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b75: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b7a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b81: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c38: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c3d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c44: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d1d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d22: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d29: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dbe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dc3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0dca: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e4b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e50: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e57: Unknown result type (might be due to invalid IL or missing references)
			//IL_0efd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f02: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f09: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fe3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fe8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fef: Unknown result type (might be due to invalid IL or missing references)
			//IL_106d: Unknown result type (might be due to invalid IL or missing references)
			//IL_1072: Unknown result type (might be due to invalid IL or missing references)
			//IL_1079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0293: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Unknown result type (might be due to invalid IL or missing references)
			//IL_029b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0319: Unknown result type (might be due to invalid IL or missing references)
			//IL_031e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			//IL_0326: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_042f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0434: Unknown result type (might be due to invalid IL or missing references)
			//IL_0437: Unknown result type (might be due to invalid IL or missing references)
			//IL_043c: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0545: Unknown result type (might be due to invalid IL or missing references)
			//IL_054a: Unknown result type (might be due to invalid IL or missing references)
			//IL_054d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0552: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_05de: Unknown result type (might be due to invalid IL or missing references)
			//IL_065e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0663: Unknown result type (might be due to invalid IL or missing references)
			//IL_0666: Unknown result type (might be due to invalid IL or missing references)
			//IL_066b: Unknown result type (might be due to invalid IL or missing references)
			//IL_06eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0778: Unknown result type (might be due to invalid IL or missing references)
			//IL_077d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0780: Unknown result type (might be due to invalid IL or missing references)
			//IL_0785: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_08bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_08bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_08c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0944: Unknown result type (might be due to invalid IL or missing references)
			//IL_0949: Unknown result type (might be due to invalid IL or missing references)
			//IL_094c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0951: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a7e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a83: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a86: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a8b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e0f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e14: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e17: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e1c: Unknown result type (might be due to invalid IL or missing references)
			//IL_1034: Unknown result type (might be due to invalid IL or missing references)
			//IL_1039: Unknown result type (might be due to invalid IL or missing references)
			//IL_103c: Unknown result type (might be due to invalid IL or missing references)
			//IL_1041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_02af: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_033a: Unknown result type (might be due to invalid IL or missing references)
			//IL_033b: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0450: Unknown result type (might be due to invalid IL or missing references)
			//IL_0451: Unknown result type (might be due to invalid IL or missing references)
			//IL_04db: Unknown result type (might be due to invalid IL or missing references)
			//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0566: Unknown result type (might be due to invalid IL or missing references)
			//IL_0567: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0680: Unknown result type (might be due to invalid IL or missing references)
			//IL_0681: Unknown result type (might be due to invalid IL or missing references)
			//IL_070d: Unknown result type (might be due to invalid IL or missing references)
			//IL_070e: Unknown result type (might be due to invalid IL or missing references)
			//IL_079a: Unknown result type (might be due to invalid IL or missing references)
			//IL_079b: Unknown result type (might be due to invalid IL or missing references)
			//IL_08d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_08da: Unknown result type (might be due to invalid IL or missing references)
			//IL_0966: Unknown result type (might be due to invalid IL or missing references)
			//IL_0967: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aa0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aa1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e31: Unknown result type (might be due to invalid IL or missing references)
			//IL_0e32: Unknown result type (might be due to invalid IL or missing references)
			//IL_1056: Unknown result type (might be due to invalid IL or missing references)
			//IL_1057: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_082a: Unknown result type (might be due to invalid IL or missing references)
			//IL_082f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0832: Unknown result type (might be due to invalid IL or missing references)
			//IL_0837: Unknown result type (might be due to invalid IL or missing references)
			//IL_09e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_09ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_09ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b39: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b3e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b41: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b46: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bfc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c01: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c04: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c09: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ce1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ce6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ce9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d82: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d87: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d8a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d8f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ec1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ec6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ec9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ece: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fa7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fac: Unknown result type (might be due to invalid IL or missing references)
			//IL_0faf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fb4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_084c: Unknown result type (might be due to invalid IL or missing references)
			//IL_084d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a07: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a08: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b5b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b5c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c1e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c1f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d03: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d04: Unknown result type (might be due to invalid IL or missing references)
			//IL_0da4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0da5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ee3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ee4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fc9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0fca: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				UniTask val2;
				switch (num)
				{
				default:
					_003CwaitTick_003E5__2 = TimeSpan.FromSeconds(0.03333333507180214);
					_003CwaitFight_003E5__3 = TimeSpan.FromSeconds(1.0);
					goto IL_009d;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_012a;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_01ce;
				case 2:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0259;
				case 3:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_02e4;
				case 4:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_036f;
				case 5:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_03fa;
				case 6:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0485;
				case 7:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0510;
				case 8:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_059b;
				case 9:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0628;
				case 10:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_06b5;
				case 11:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0742;
				case 12:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_07cf;
				case 13:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0881;
				case 14:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_090e;
				case 15:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_099b;
				case 16:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0a3c;
				case 17:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0ad5;
				case 18:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0b90;
				case 19:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0c53;
				case 20:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0d38;
				case 21:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0dd9;
				case 22:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0e66;
				case 23:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0f18;
				case 24:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0ffe;
				case 25:
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_1088;
					}
					IL_009d:
					token.ThrowIfCancellationRequested();
					if (zbGameStateMachine.Instance.IsFighting() || zbUIMgr.Instance.IsShow(EWndId.Dungeon))
					{
						val2 = UniTask.Delay(_003CwaitFight_003E5__3, false, (PlayerLoopTiming)8, token, false);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
							break;
						}
						goto IL_012a;
					}
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Formation, zbSelf.Instance.heroHandler.HasRedPoint() || zbSelf.Instance.mechaHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_01ce;
					IL_0e66:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Journey, zbSelf.Instance.journeyOneHandler.HasRedPoint() || zbSelf.Instance.journeyTwoHandler.HasRedPoint() || zbSelf.Instance.journeyThreeHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 23);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0f18;
					IL_07cf:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Leader, zbSelf.Instance.equipHandler.HasRedPoint() || zbSelf.Instance.suitGunHandler.CanAllSuitActiveOrUpgrade() || zbSelf.Instance.suitGunHandler.CanAllGunActiveOrUpgrade());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 13);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0881;
					IL_0742:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Head, zbSelf.Instance.headHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 12);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_07cf;
					IL_0c53:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Daily, zbSelf.Instance.worldLairHandler.HasRedPoint() || zbSelf.Instance.elementTrialHandler.HasRedPoint() || zbSelf.Instance.worldBossHandler.HasRedPoint() || zbSelf.Instance.legionHandler.HasCallengeCountRedPoint() || zbSelf.Instance.legionHandler.HasCallengeAwardRedPoint() || zbSelf.Instance.endlessHordeHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 20);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0d38;
					IL_1088:
					((Awaiter)(ref val)).GetResult();
					goto IL_009d;
					IL_0ad5:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Challenge, _003CisWorldHandlerHasRedPoint_003E5__4 || zbSelf.Instance.eliteQuestHandler.HasRedPoint() || zbSelf.Instance.legendaryHandler.HasRedPoint() || zbSelf.Instance.facilityHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 18);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0b90;
					IL_01ce:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Bag, zbSelf.Instance.itemHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0259;
					IL_0881:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Operate, zbSelf.Instance.operateHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 14);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_090e;
					IL_0259:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Friend, zbSelf.Instance.friendHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 3);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_02e4;
					IL_0f18:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Arena, zbSelf.Instance.arenaHandler.HasRedPoint() || zbSelf.Instance.guardHandler.HasGurarIdleNotify() || zbSelf.Instance.guardHandler.HasFootholdsNotify() || zbSelf.Instance.crossLadderHandler.HasRedPoint() || zbSelf.Instance.mineWarHandler.HasRedPoint() || zbSelf.Instance.truckHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 24);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0ffe;
					IL_02e4:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Legion, zbSelf.Instance.legionHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 4);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_036f;
					IL_090e:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Holiday, zbSelf.Instance.holidayHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 15);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_099b;
					IL_036f:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Mail, zbSelf.Instance.mailHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 5);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_03fa;
					IL_0dd9:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.OpenServerRank, zbSelf.Instance.rankHandler.HasOpenServerRankRedDot());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 22);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0e66;
					IL_03fa:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Task, zbSelf.Instance.taskHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 6);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0485;
					IL_099b:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Monster, zbSelf.Instance.handbookHandler.HasRedPoint() || zbSelf.Instance.handbookHandler.HasModRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 16);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0a3c;
					IL_0485:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Chat, zbSelf.Instance.chatHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 7);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0510;
					IL_0d38:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Passport, zbSelf.Instance.passportHandler.HasPassportRedPoint() || zbSelf.Instance.passportHandler.HasMainQuestRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 21);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0dd9;
					IL_0510:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Rank, zbSelf.Instance.rankHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 8);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_059b;
					IL_0ffe:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.GrowthGift, zbSelf.Instance.growthGiftHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 25);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_1088;
					IL_059b:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.MainQuest, zbSelf.Instance.mainQuestHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 9);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0628;
					IL_0b90:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Home, zbSelf.Instance.homelandHandler.HasRedPoint() || zbSelf.Instance.wallHandler.HasRedPoint() || zbSelf.Instance.treasureHandler.HasRedPoint() || zbSelf.Instance.homeEditHandler.CanRoomCreate());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 19);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0c53;
					IL_0628:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Bar, zbSelf.Instance.barHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 10);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_06b5;
					IL_012a:
					((Awaiter)(ref val)).GetResult();
					goto IL_009d;
					IL_06b5:
					((Awaiter)(ref val)).GetResult();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.Vip, zbSelf.Instance.vipHandler.HasRedPoint());
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 11);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0742;
					IL_0a3c:
					((Awaiter)(ref val)).GetResult();
					_003CisWorldHandlerHasRedPoint_003E5__4 = zbSelf.Instance.worldHandler.HasRedPoint();
					zbSelf.Instance.redPointHandler.SetStateValue(ERedPointType.World, _003CisWorldHandlerHasRedPoint_003E5__4);
					val2 = UniTask.Delay(_003CwaitTick_003E5__2, false, (PlayerLoopTiming)8, token, false);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 17);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRedPointerUpdateAsync_003Ed__4>(ref val, ref this);
						break;
					}
					goto IL_0ad5;
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
			}
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private CancellationTokenSource m_Cts;

	protected override void OnEnable()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_Cts = new CancellationTokenSource();
		UniTaskVoid val = RedPointerUpdateAsync(m_Cts.Token);
		((UniTaskVoid)(ref val)).Forget();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_Cts != null)
		{
			m_Cts.Cancel();
			m_Cts.Dispose();
			m_Cts = null;
		}
	}

	protected override void Start()
	{
		base.Start();
		zbUIMgr.Instance.ShowLoading();
	}

	[AsyncStateMachine(typeof(_003CRedPointerUpdateAsync_003Ed__4))]
	private UniTaskVoid RedPointerUpdateAsync(CancellationToken token)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CRedPointerUpdateAsync_003Ed__4 obj = default(_003CRedPointerUpdateAsync_003Ed__4);
		obj._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		obj.token = token;
		obj._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref obj._003C_003Et__builder)).Start<_003CRedPointerUpdateAsync_003Ed__4>(ref obj);
		return ((AsyncUniTaskVoidMethodBuilder)(ref obj._003C_003Et__builder)).Task;
	}
}
