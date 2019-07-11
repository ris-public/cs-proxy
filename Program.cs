using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;

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
	internal class pair : System.IO.Stream{
		private StreamWriter _B;
		private StreamReader _A;
		public pair(StreamReader A, StreamWriter B){
			this._A = A;
			this._B = B;
		}
		public override int Read(byte[] A, int B, int C){
			return _A.BaseStream.Read(A,B,C);
		}
		public override void Write(byte[] A, int B, int C){
			//Console.WriteLine(Encoding.Default.GetString(A));
			_B.BaseStream.Write(A,B,C);
			_B.Flush();
		}
		public override bool CanSeek{
			get {
				return false;
			}
		}
		public override long Position{
			set {
				throw new NotSupportedException();
			}
			get {
				throw new NotSupportedException();
			}
		}
		public override bool CanRead{
			get{
				return true;
			}
		}
		public override bool CanWrite{
			get{
				return true;
			}
		}
		public override long Seek(long offset, System.IO.SeekOrigin origin){
			throw new NotSupportedException();
		}
		public override long Length{
			get {
				throw new NotSupportedException();
			}
		}
		public override void SetLength(long val){
			throw new NotSupportedException();
		}
		public override void Flush(){
		}
		public bool EndOfStream{
			get{
				return _A.EndOfStream;
			}
		}
		public static void BindStreams(Stream A, Stream B){
			new Thread(()=>A.CopyTo(B)).Start();
			new Thread(()=>B.CopyTo(A)).Start();
		}
	}
	internal class statpair: pair {
		private ulong _BR;
		private ulong _BW;
		public statpair(StreamReader A, StreamWriter B) : base(A,B){
			_BR=0;
			_BW=0;
		}
		public ulong BytesRead{
			get {
				return _BR;
			}
		}
		public ulong BytesWritten{
			get {
				return _BW;
			}
		}
		public override int Read(byte[] A, int B, int C){
			_BR += Convert.ToUInt64(C);
			return base.Read(A, B, C);
		}
		public override void Write(byte[] A, int B, int C){
			_BW += Convert.ToUInt64(C);
			base.Write(A, B, C);
			return;
		}
		public void ResetStats(){
			_BR=0;
			_BW=0;
		}
	}
}
