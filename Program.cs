using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using ProxyClient;

namespace DTLSClient
{
	class Program
	{
		static void SetColour(int fg, int bg){
			System.Console.Error.WriteLine($"\u001b[1;3{fg}m");
			System.Console.Error.WriteLine($"\u001b[4{bg}m");
		}
		static void ResetColour(){
			System.Console.Error.WriteLine("\u001b[39m");
			System.Console.Error.WriteLine("\u001b[49m");
		}
		static void Main(string[] args)
		{
			Console.Error.WriteLine("\u001b[31mHey!\u001b[0m");
			SetColour(2,0);
			Console.Error.WriteLine("Hello World!");
			ResetColour();
			DTLSClient dtls = new DTLSClient("127.0.0.1", "10000", new byte[] {0xBA,0xA0});
			dtls.Unbuffer="stdbuf";
			dtls.Unbuffer_Args="-i0 -o0";
			dtls.Start();
			statpair IOStream = new statpair(new StreamReader(Console.OpenStandardInput()), new StreamWriter(Console.OpenStandardOutput()));
			new Thread(()=>IOStream.CopyTo(dtls.GetStream(), 16)).Start();
			new Thread(() => dtls.GetStream().CopyTo(IOStream, 16)).Start();
			//new Thread(() => dtls.GetStream().Write(Encoding.Default.GetBytes("It Works!"+Environment.NewLine))).Start();
			pair.BindStreams(dtls.GetStream(), IOStream);
			pair.BindStreams(dtls.GetStream(), IOStream);
			Timer T = new Timer((S)=>{float BR = (float)IOStream.BytesRead/(1024*1024*5); float BW = (float)IOStream.BytesWritten/(1024*1024*5); SetColour(2,0);Console.Error.WriteLine($"R: {BR:000.00} MB/s.\tW: {BW:000.00} MB/s.");IOStream.ResetStats();ResetColour();},new AutoResetEvent(false),5000,5000);
			Console.WriteLine("End of File");
			dtls.WaitForExit();
		}
	}
	class ProxySocket{

		protected StreamWriter A;
		protected StreamReader B;
		protected Process Proc = new Process();
		protected string HostName;
		protected int Port;
		protected int ProxyPort;
		protected string Method;
		protected string ProxyServerPort;
		public string Unbuffer;
		public string Unbuffer_Args;

		public ProxyClient(string HostName, int port, string ProxyServerName, int Port, string Method){
			this.Proc = new Process();
			this.ProxyPort=ProxyPort;
			this.HostName=HostName;
			this.Port=Port;
			this.ProxyServerName=ProxyServerName;
			//Unbuffer = "stdbuf";
			//Unbuffer_Args="-i0 -o0";

		}
		public ProxyClient(string HostName, int port, string ProxyServerName, int Port, string Method, string Unbuffer_Command, string Unbuffer_Args){
			this.Proc = new Process();
			this.ProxyPort=ProxyPort;
			this.HostName=HostName;
			this.Port=Port;
			this.ProxyServerName=ProxyServerName;
			this.Unbuffer = Unbuffer_Command;
			this.Unbuffer_Args=Unbuffer_Args;

		}
		public void Start(){

			this.Proc.StartInfo.FileName=$"{Unbuffer}";
			this.Proc.StartInfo.UseShellExecute = false;
			this.Proc.StartInfo.RedirectStandardOutput = true;

			Proc.StartInfo.Arguments=$"{Unbuffer_Args} nc -X {Method} -x {ProxyServerName}:{ProxyPort} {HostName} {Port}";
			SetColour(5,0);
			System.Console.Error.WriteLine(Proc.StartInfo.FileName + " " + Proc.StartInfo.Arguments);
			ResetColour();
			Proc.StartInfo.RedirectStandardInput=true;
			Proc.Start();

			A = Proc.StandardInput;
			B = Proc.StandardOutput;
		}

		public Stream GetStream(){
			return new pair(B,A);
		}
		public void Kill(){
			Proc.Kill();
		}
		public void Close(){
			Proc.Close();
		}
		public void WaitForExit(){
			Proc.WaitForExit();
		}

		private static void SetColour(int fg, int bg){
			System.Console.Error.WriteLine($"\u001b[1;3{fg}m");
			System.Console.Error.WriteLine($"\u001b[4{bg}m");
		}
		private static void ResetColour(){
			System.Console.Error.WriteLine("\u001b[39m");
			System.Console.Error.WriteLine("\u001b[49m");
		}
	}

}
