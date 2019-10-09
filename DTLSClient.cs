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
#if !NETSTANDARD2_0
using System.Buffers;
#endif
using Rishi.PairStream;
using Rishi.ShellBind;

namespace Rishi{
		public class DTLSClient{

				///<summary>
				///Verbosity.
				///</summary>
				protected bool VERBOSE;
				///<summary>
				///The Stream Writer.
				///</summary>
				protected StreamWriter A;
				///<summary>
				///The Stream Reader.
				///</summary>
				protected StreamReader B;
				///<summary>
				///The ShellSocket.
				///</summary>
				protected ShellSocket SS;
				///<summary>
				///The Hostname of the final destination.
				///</summary>
				protected string HostName;
				///<summary>
				///The Port of the final destination.
				///</summary>
				protected string Port;
				///<summary>
				///The shell unbuffer/stdbuf command, default: none.
				///</summary>
				public string Unbuffer;
				///<summary>
				///Arguments to the shell unbuffer/stdbuf command, default: none.
				///</summary>
				public string Unbuffer_Args;
				///<summary>
				///The <see cref="System.IO.Stream" />.
				///</summary>
				protected Stream S;
				protected byte[] PSK;
				///<summary>
				///Auto configure the environment on failure on presumed interactive terminals.
				///</summary>
				public bool AutoConfigure=true;

				///<summary>
				///Constructor.
				///</summary>
				///<seealso cref="DTLSClient(string, int, byte[], string, string)"/>
				/// <param name="hostname">Target Hostname</param>
				/// <param name="port">Target Port.</param>
				/// <param name="PSK">PSK.</param>
				/// <param name="Unbuffer_Command">Unbuffer command. Use "" or null to run directly at your own risk.</param>
				/// <param name="Unbuffer_Args">Unbuffer arguments.</param>
				public DTLSClient(string hostname, string port, byte[] PSK, string Unbuffer_Command, string Unbuffer_Args){
						this.Port=port;
						this.HostName=hostname;
						this.PSK=PSK;
						this.Unbuffer=Unbuffer_Command;
						this.Unbuffer_Args=Unbuffer_Args;
				}
				///<summary>
				///Constructor.
				///</summary>
				///<seealso cref="DTLSClient(string, int, byte[])"/>
				/// <param name="hostname">Target Hostname</param>
				/// <param name="port">Target Port.</param>
				/// <param name="PSK">PSK.</param>
				public DTLSClient(string hostname, string port, byte[] PSK){
						this.Port=port;
						this.HostName=hostname;
						this.PSK=PSK;
				}
				public void Start(){
						string psk_hex=BitConverter.ToString(PSK).Replace("-", String.Empty);
						string PrCommand=$"openssl";
						string ClArguments=$"s_client -dtls -connect {HostName}:{Port} -psk {psk_hex} -quiet";
						SS = new ShellSocket(PrCommand, ClArguments, Unbuffer, Unbuffer_Args);
						SetColour(5,0);
						System.Console.Error.WriteLine(PrCommand + " " + ClArguments);
						ResetColour();
						if (AutoConfigure)
						{
							SS.AutoConfigure = true;
							SS.PackageName = "NC";
						}
						SS.Start();
				}

				///<summary>
				///Get the Stream formed by the process.
				///Should be Start()ed first.
				///</summary>
				public Stream GetStream(){
						return S;
				}
				///<summary>
				///Kill the proxy process.
				///</summary>
				public void Kill(){
						SS.Kill();
				}
				///<summary>
				///Close the proxy process.
				///</summary>
				public void Close(){
						SS.Close();
				}
				///<summary>
				///Wait for the proxy process to exit.
				///</summary>
				public void WaitForExit(){
						SS.WaitForExit();
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
