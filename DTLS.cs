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
	class DTLSClient{

		protected StreamWriter A;
		protected StreamReader B;
		protected Process Proc = new Process();
		protected string hostname;
		protected string port;
		protected byte[] PSK;
		public string Unbuffer;
		public string Unbuffer_Args;

		public DTLSClient(string hostname, string port, byte[] PSK){
			this.Proc = new Process();
			this.port=port;
			this.hostname=hostname;
			this.PSK=PSK;
			//Unbuffer = "stdbuf";
			//Unbuffer_Args="-i0 -o0";

		}
		public void Start(){

			this.Proc.StartInfo.FileName=$"{Unbuffer}";
			this.Proc.StartInfo.UseShellExecute = false;
			this.Proc.StartInfo.RedirectStandardOutput = true;
			string psk_hex=BitConverter.ToString(PSK).Replace("-", String.Empty);
			//Proc.StartInfo.Arguments=$"{Unbuffer_Args} openssl s_server -nocert -dtls -accept {port} -psk {psk_hex}";

			Proc.StartInfo.Arguments=$"{Unbuffer_Args} openssl s_client -dtls -connect {hostname}:{port} -psk {psk_hex} -quiet";
			SetColour(5,0);
			System.Console.Error.WriteLine(Proc.StartInfo.FileName + " " + Proc.StartInfo.Arguments);
			ResetColour();
			//Proc.OutputDataReceived += ret2;
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

	internal class DupStream : statpair {
		public DupStream (StreamReader A, StreamWriter B): base(A, B){}
		public async Task CopyToAsyncInternal(Stream[] destinations, Int32 bufferSize, CancellationToken cancellationToken)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
			try
			{
				while (true)
				{
					int bytesRead = await ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false);
					if (bytesRead == 0) break;
					foreach (Stream destination in destinations){
						await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
					}
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}
	}
}
