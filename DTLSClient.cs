/* Copyright [2019] RISHIKESHAN LAVAKUMAR <github-public [at] ris.fi>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/


using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using Rishi.PairStream;

namespace Rishi.DTLSClient{
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
