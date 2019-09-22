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
using Rishi.PairStream;

namespace Rishi
{
	public class DTLSServer{
		protected StreamWriter A;
		protected StreamReader B;
		protected Process Proc = new Process();
		protected string hostname;
		protected string port;
		protected byte[] PSK;
		public string Unbuffer;
		public string Unbuffer_Args;

		public DTLSServer(string hostname, string port, byte[] PSK){
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
			Proc.StartInfo.Arguments=$"{Unbuffer_Args} openssl s_server -dtls -accept {port} -nocert -psk {psk_hex}";
			System.Console.WriteLine(Proc.StartInfo.Arguments);
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
	}
}
