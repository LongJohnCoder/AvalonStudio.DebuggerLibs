using System;
using Microsoft.Samples.Debugging.CorDebug;
using Mono.Debugging.Client;

namespace Mono.Debugging.Win32
{
	public class CoreClrDebuggerSession : CorDebuggerSession
	{
		private readonly DbgShimInterop dbgShimInterop;
		static readonly TimeSpan RuntimeLoadTimeout = TimeSpan.FromSeconds (1);

		public CoreClrDebuggerSession (char[] badPathChars, string dbgShimPath) : base (badPathChars)
		{
			dbgShimInterop = new DbgShimInterop(dbgShimPath);

		}

		protected override void OnRun (DebuggerStartInfo startInfo)
		{
			MtaThread.Run (() => {
				var workingDir = PrepareWorkingDirectory (startInfo);
				var env = PrepareEnvironment (startInfo);
				var cmd = PrepareCommandLine (startInfo);
				int procId;
				var iCorDebug = CoreClrShimUtil.CreateCorDebugForCommand (
					dbgShimInterop, cmd, workingDir, env, RuntimeLoadTimeout, out procId);				
				dbg = new CorDebugger (iCorDebug);
				var lprocess = dbg.DebugActiveProcess (procId, false);
				//processId = process.Id;
				//SetupProcess (process);
				//process.Continue (false);

				lprocess.Continue(new SharpDX.Mathematics.Interop.RawBool(false));
			});
			OnStarted();
		}

		protected override void OnAttachToProcess (long procId)
		{
			AttachToProcessImpl ((int) procId);
		}

		protected override void OnAttachToProcess (ProcessInfo processInfo)
		{
			AttachToProcessImpl ((int) processInfo.Id);
		}

		void AttachToProcessImpl (int procId)
		{
			attaching = true;
			MtaThread.Run(delegate
			{
				var iCorDebug = CoreClrShimUtil.CreateICorDebugForProcess (dbgShimInterop, procId, RuntimeLoadTimeout);
				dbg = new CorDebugger(iCorDebug);
				var lprocess = dbg.DebugActiveProcess(procId, false);
				//SetupProcess(process);
				//process.Continue(false);
				lprocess.Continue(new SharpDX.Mathematics.Interop.RawBool(false));
			});
			OnStarted();
		}
	}
}