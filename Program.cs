using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ffmpegprogress
{
	static class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length > 0)
			{
				string main_arg = args [0];
				switch (main_arg) {
				case "ffmpeg":
					{
						string ffmpeg_args = String.Join (" ", args.Skip (1));
						FFMpegTask.RunProcessProgress (OSHelper.GetFFmpeg (), ffmpeg_args);
						return;
					}
				case "--simple":
					{
						string ffmpeg_args = String.Join (" ", args.Skip (1));
						FFMpegTask.RunProcessSimple (OSHelper.GetFFmpeg (), ffmpeg_args);
						return;
					}
				case "--help":
					{
						Help ();
						return;
					}
				case "--version":
					{
						FFMpegTask.RunProcessSimple (OSHelper.GetFFmpeg (), "-version");
						return;
					}
				case "--test":
					{
						Test ();
						return;
					}
				default:
					{
						Help ();
						return;
					}
				}
			}
			else
			{
				Console.Error.WriteLine ("No arguments!");
				Help ();
				return;
			}
		}
		public static void Help()
		{
			//TODO: usage help
			Console.WriteLine (@"//TODO: usage help");
		}
		public static void Test()
		{
			Console.WriteLine ("ffmpeg-progress");
			Console.WriteLine ("OS: {0}", OSHelper.GetOS ());
			Console.WriteLine ("Windows: {0}", OSHelper.IsWin ());
			Console.WriteLine ("*nix: {0}", OSHelper.IsUnix ());
			if (OSHelper.IsUnix ())
				Console.Error.WriteLine("Warning: Unix is not supported!");
			Test0();
			Test1();
		}
		public static void Test0()
		{
			string CommandLineV="-version";
			FFMpegTask.DebugOn ();
			FFMpegTask.RunProcessSimple (OSHelper.GetFFmpeg (), CommandLineV);
		}
		public static void Test1()
		{
			string CommandLineConvert="-i test.mkv -f psp -r 29.97 -b 768k -ar 24000 -ab 64k -s 320x240 psp.mp4 -y";
			FFMpegTask.DebugOn ();
			FFMpegTask.RunProcessProgress (OSHelper.GetFFmpeg (),CommandLineConvert);
		}
	}
	#region OSHelper
	static class OSHelper
	{
		public static string GetOS()
		{
			return Environment.OSVersion.Platform.ToString();
		}
		public static bool IsWin()
		{
			string s = GetOS ();
			if (s.Contains ("Win"))
				return true;
			else
				return false;
		}
		public static bool IsUnix()
		{
			string s = GetOS ();
			if (s.Contains ("Unix"))
				return true;
			else
				return false;
		}
		public static string GetFFmpeg()
		{
			//TODO: improve
			if (IsWin ())
				return "C:\\ffmpeg-win64\\bin\\ffmpeg";
			if (IsUnix ())
				return "ffmpeg";
			return null;
		}
	}
	#endregion
	#region ffmpeg
	static class FFMpegTask
	{
		private static bool debug=false;
		public static void DebugOn()
		{
			debug = true;
		}
		public static void DebugOff()
		{
			debug = false;
		}
		public static bool IsDebug()
		{
			return debug;
		}
		private static void ProgressOutputHandle(object o,DataReceivedEventArgs a)
		{
			ProgressHandleS (o, a, "stdout");
		}
		private static void ProgressErrorHandle(object o,DataReceivedEventArgs a)
		{
			ProgressHandleS (o, a, "stderr");
		}
		private static void SimpleOutputHandle(object o,DataReceivedEventArgs a)
		{
			SimplecHandleS (o, a, "stdout");
		}
		private static void SimpleErrorHandle(object o,DataReceivedEventArgs a)
		{
			SimplecHandleS (o, a, "stderr");
		}
		private static void ExitHandle(object o,EventArgs a)
		{
			GenericHandle (o, a, "exit");
		}
		private static string[] GetElements(string s,char[] Delims)
		{
			string[] els = s.Split (Delims, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < els.Length; ++i) {
				els [i] = els [i].TrimStart ().TrimEnd ();
			}
			return els;
		}
		public static double GetProgress(string Time,string Duration)
		{
			string s1 = Time;
			string s2 = Duration;
			long i1 = TimeToLong (s1);
			long i2 = TimeToLong (s2);
			double d = (double)i1 / (double)i2;
			double p = d * 100;
			double r = Math.Round (p, 2);
			return r;
		}
		private static string Duration = "";
		private static string Time="";
		private static long TimeToLong(string sTime)
		{
			int len = sTime.Length;
			if (len != 11)
				return -1;
			string[] digits = sTime.Split (new char[]{ ':', '.', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			long sum = 0;
			int v0 = int.Parse (digits [3]);
			int v1 = int.Parse (digits [2]);
			int v2 = int.Parse (digits [1]);
			int v3 = int.Parse (digits [0]);
			sum += (long)v0;
			sum += (long)v1 * 100L;
			sum += (long)v2 * 100L * 60L;
			sum += (long)v3 * 100L * 60L * 60L;
			return sum;
		}
		private static void ProgressHandleS(object o, DataReceivedEventArgs a,string tag)
		{
			try
			{
				if(a==null) return;
				if(o==null) return;

				string s = a.Data;
				if(IsDebug()) Console.Error.WriteLine("[DEBUG] [{0}]: {1}",tag,s);
				if(a.Data==null) return;
				string s_down = s.ToLower ();
				if(IsDebug())
				{
					if(s_down.Contains("duration"))
						Console.Error.WriteLine("[DEBUG] [duration data] [{0}]: {1}",tag,s_down);
					if(s_down.Contains("time"))
						Console.Error.WriteLine("[DEBUG] [time data] [{0}]: {1}",tag,s_down);

				}
				if (s_down.Contains ("duration")&&s_down.Contains("start")&&s_down.Contains("bitrate")) {
					string[] els=GetElements (s_down,new char[]{ ',', '\r', '\n',' '});
					Duration = els [1];
					return;
				}
				if (s_down.Contains ("time")&&s_down.Contains("frame")&&s_down.Contains("fps")&&s_down.Contains("q")&&s_down.Contains("bitrate")) {
					string[] els=GetElements (s_down,new char[]{' ','\t','\r','\n','='});
					Time=els[9];
					double progress=GetProgress(Time,Duration);
					Console.WriteLine ("{0}%", progress);
					return;
				}
			}
			catch(Exception E) {
				Console.Error.WriteLine ("Exception!");
				Console.Error.WriteLine (E.Message);
				Console.Error.WriteLine ("Stack trace:");
				Console.Error.WriteLine (E.StackTrace);
			}
		}
		private static void SimplecHandleS(object o, DataReceivedEventArgs a,string tag)
		{
			if (a == null)
				return;
			if (a.Data == null)
				return;
			string s = a.Data;
			Console.WriteLine ("[{1}] Data: {0}", s,tag);
		}
		private static void GenericHandle(object o, EventArgs a,string tag)
		{
			Console.WriteLine ("[{1}] Data: {0}", a,tag);
		}
		public static void RunProcessGeneric(string ffmpeg,string ffmpeg_args,DataReceivedEventHandler OutputH,DataReceivedEventHandler ErrorH)
		{
			try
			{
				if(IsDebug()) Console.WriteLine ("ffmpeg: {0}", ffmpeg);
				if(IsDebug()) Console.WriteLine ("ffmpeg args: {0}", ffmpeg_args);
				Process process = new Process();
				process.StartInfo = new ProcessStartInfo (ffmpeg, ffmpeg_args);
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.CreateNoWindow = true;
				process.OutputDataReceived += OutputH;
				process.ErrorDataReceived += ErrorH;
				process.EnableRaisingEvents = true;
				process.Exited += ExitHandle;
				bool flag=process.Start ();
				process.BeginErrorReadLine ();
				process.BeginOutputReadLine ();
				process.WaitForExit ();
			}
			catch(Exception E)
			{
				Console.Error.WriteLine (E.Message);
				Console.Error.WriteLine (E.StackTrace);
			}
		}
		public static void RunProcessSimple(string ffmpeg,string ffmpeg_args)
		{
			RunProcessGeneric (ffmpeg, ffmpeg_args, SimpleOutputHandle, SimpleErrorHandle);
		}
		public static void RunProcessProgress(string ffmpeg,string ffmpeg_args)
		{
			RunProcessGeneric (ffmpeg, ffmpeg_args, ProgressOutputHandle, ProgressErrorHandle);
		}
	}
	#endregion
}
