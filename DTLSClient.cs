
using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using ProxyClient;

namespace DTLSClient{
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