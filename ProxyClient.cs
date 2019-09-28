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
using Rishi.ProxyClient;
using Rishi.PairStream;


namespace Rishi.ProxyClient {
	public class ProxySocket{
		
		///<summary>
		///Verbosity.
		///</summary>
		protected bool VERBOSE;
		protected StreamWriter A;
		protected StreamReader B;
		protected Process Proc = new Process();
		protected string HostName;
		protected int Port;
		protected int ProxyPort;
		protected string Method;
		protected string ProxyServerName;
		public string Unbuffer;
		public string Unbuffer_Args;

		public ProxySocket(string HostName, int Port, string ProxyServerName, int ProxyPort, string Method){
			this.Proc = new Process();
			this.ProxyPort=ProxyPort;
			this.HostName=HostName;
			this.Port=Port;
			this.ProxyServerName=ProxyServerName;
			this.Method=Method;
			this.VERBOSE=false;
			Unbuffer = "stdbuf";
			Unbuffer_Args="-i0 -o0";

		}
		public ProxySocket(string HostName, int Port, string ProxyServerName, int ProxyPort, string Method, string Unbuffer_Command, string Unbuffer_Args){
			this.Proc = new Process();
			this.ProxyPort=ProxyPort;
			this.HostName=HostName;
			this.Port=Port;
			this.ProxyServerName=ProxyServerName;
			this.Method=Method;
			this.VERBOSE=false;
			this.Unbuffer = Unbuffer_Command;
			this.Unbuffer_Args=Unbuffer_Args;

		}
		public void Start(){

			this.Proc.StartInfo.FileName=$"{Unbuffer}";
			this.Proc.StartInfo.UseShellExecute = false;
			this.Proc.StartInfo.RedirectStandardOutput = true;
			if (Method != "4" && Method != "5" && Method != "connect"){
				System.Console.WriteLine($"Warning: Supported protocols are 4 (SOCKS v.4), 5 (SOCKS v.5) and connect (HTTPS proxy). If the protocol is not specified, SOCKS version 5 is used. Got: {Method}.");
			}

			Proc.StartInfo.Arguments=$"{Unbuffer_Args} nc -X {Method} -x {ProxyServerName}:{ProxyPort} {HostName} {Port}";
			Proc.StartInfo.RedirectStandardInput=true;
			if (VERBOSE){
				SetColour(5,0);
				System.Console.Error.WriteLine(Proc.StartInfo.FileName + " " + Proc.StartInfo.Arguments);
				ResetColour();
			}
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
