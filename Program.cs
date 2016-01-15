using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ffmpegprogress
{
	static class MainClass
	{
		public static string ffmpeg_win="C:\\ffmpeg-win64\\bin\\ffmpeg";
		public static string ffmpeg_unix="ffmpeg";
		public static string CommandLine="-i test.mkv -f psp -r 29.97 -b 768k -ar 24000 -ab 64k -s 320x240 psp.mp4 -y";
		public static string CommandLineV="-version";
		public static void Main (string[] args)
		{
			Test ();
		}
		public static void Test()
		{
			Console.WriteLine ("ffmpeg-progress");
			Console.WriteLine ("OS: {0}", OSHelper.GetOS ());
			Console.WriteLine ("Windows: {0}", OSHelper.IsWin ());
			Console.WriteLine ("*nix: {0}", OSHelper.IsUnix ());
			if (OSHelper.IsUnix ())
				Test_Unix ();
			if (OSHelper.IsWin ())
				Test_Win();
		}
		public static void Test_Unix()
		{
			FFMpegTask.DebugOn ();
			Console.Write ("Unix test");
			Console.Write ("Warning: Unix is not supported!");
			FFMpegTask.RunProcessSimple (ffmpeg_unix, CommandLineV);
			FFMpegTask.RunProcessProgress (ffmpeg_unix,CommandLine);
		}
		public static void Test_Win()
		{
			FFMpegTask.DebugOff ();
			Console.Write ("Windows test");
			FFMpegTask.RunProcessSimple (ffmpeg_win, CommandLineV);
			FFMpegTask.RunProcessProgress (ffmpeg_win,CommandLine);
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
		public static void TimeToLong_Test()
		{
			string s1 = "00:03:11.99";
			string s2 = "00:07:50.04";
			Console.WriteLine ("{0}/{1}", s1, s2);
			long i1 = TimeToLong (s1);
			long i2 = TimeToLong (s2);
			Console.WriteLine ("{0}/{1}", i1, i2);
			double d = (double)i1 / (double)i2;
			double p = d * 100;
			double r = Math.Round (p, 2);
			Console.WriteLine (d);
			Console.WriteLine (p);
			Console.WriteLine (r);
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
				Console.WriteLine ("ffmpeg: {0}", ffmpeg);
				Console.WriteLine ("ffmpeg args: {0}", ffmpeg_args);
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
