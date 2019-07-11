using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;

namespace ProxyClient{
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
