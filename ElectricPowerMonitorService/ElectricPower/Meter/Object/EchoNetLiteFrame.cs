using System;
using System.Collections.Generic;
using System.Text;

namespace ElectricPowerMonitorService.ElectricPower.Meter.Object {
	public class EchoNetLiteFrame {
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="ehd1">ECHONET Lite電文ヘッダー1</param>
		/// <param name="ehd2">ECHONET Lite電文ヘッダー2</param>
		/// <param name="tid">トランザクションID (2byte)</param>
		/// <param name="seoj">送信元ECHONET Liteオブジェクト指定 (3byte)</param>
		/// <param name="deoj">相手先ECHONET Liteオブジェクト指定 (3byte)</param>
		/// <param name="esv">ECHONET Liteサービス</param>
		/// <param name="opc">処理プロパティ数</param>
		/// <param name="epc">ECHONET Liteプロパティ</param>
		/// <param name="pdc">EDTのバイト数</param>
		/// <param name="edt">プロパティ値データ</param>
		public EchoNetLiteFrame(byte ehd1, byte ehd2, byte[]? tid, byte[]? seoj, byte[]? deoj, byte esv, byte opc, byte epc, byte pdc, byte[]? edt) {
			this.Ehd1 = ehd1;
			this.Ehd2 = ehd2;
			this.Tid = tid ?? Array.Empty<byte>();
			this.Seoj = seoj ?? Array.Empty<byte>();
			this.Deoj = deoj ?? Array.Empty<byte>();
			this.Esv = esv;
			this.Opc = opc;
			this.Epc = epc;
			this.Pdc = pdc;
			this.Edt = edt ?? Array.Empty<byte>();
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="frameBinaryData">フレームデータ</param>
		public EchoNetLiteFrame(byte[]? frameBinaryData) {
			if (frameBinaryData == null || frameBinaryData.Length == 0) {
				this.Tid = Array.Empty<byte>();
				this.Seoj = Array.Empty<byte>();
				this.Deoj = Array.Empty<byte>();
				this.Edt = Array.Empty<byte>();
				return;
			}

			var span = frameBinaryData.AsSpan();
			this.Ehd1 = span.Length > 0 ? span[0] : (byte)0;
			this.Ehd2 = span.Length > 1 ? span[1] : (byte)0;
			this.Tid = span.Length >= 4 ? span[2..4].ToArray() : (span.Length > 2 ? span[2..].ToArray() : Array.Empty<byte>());
			this.Seoj = span.Length >= 7 ? span[4..7].ToArray() : (span.Length > 4 ? span[4..].ToArray() : Array.Empty<byte>());
			this.Deoj = span.Length >= 10 ? span[7..10].ToArray() : (span.Length > 7 ? span[7..].ToArray() : Array.Empty<byte>());
			this.Esv = span.Length > 10 ? span[10] : (byte)0;
			this.Opc = span.Length > 11 ? span[11] : (byte)0;
			this.Epc = span.Length > 12 ? span[12] : (byte)0;
			this.Pdc = span.Length > 13 ? span[13] : (byte)0;

			if (span.Length >= 14) {
				var availableLength = span.Length - 14;
				var lengthToCopy = Math.Min((int)this.Pdc, availableLength);
				this.Edt = span.Slice(14, lengthToCopy).ToArray();
			} else {
				this.Edt = Array.Empty<byte>();
			}
		}

		/// <summary>
		/// ECHONET Lite電文ヘッダー1
		/// </summary>
		public byte Ehd1 {
			get;
		}

		/// <summary>
		/// ECHONET Lite電文ヘッダー2
		/// </summary>
		public byte Ehd2 {
			get;
		}

		/// <summary>
		/// トランザクションID (2byte)
		/// </summary>
		public byte[] Tid {
			get;
		}

		/// <summary>
		/// 送信元ECHONET Liteオブジェクト指定 (3byte)
		/// </summary>
		public byte[] Seoj {
			get;
		}

		/// <summary>
		/// 相手先ECHONET Liteオブジェクト指定 (3byte)
		/// </summary>
		public byte[] Deoj {
			get;
		}

		/// <summary>
		/// ECHONET Liteサービス
		/// </summary>
		public byte Esv {
			get;
		}

		/// <summary>
		/// 処理プロパティ数
		/// </summary>
		public byte Opc {
			get;
		}

		/// <summary>
		/// ECHONET Liteプロパティ
		/// </summary>
		public byte Epc {
			get;
		}

		/// <summary>
		/// EDTのバイト数
		/// </summary>
		public byte Pdc {
			get;
		}

		/// <summary>
		/// プロパティ値データ
		/// </summary>
		public byte[] Edt {
			get;
		}

		public byte[] GetFrame() {
			var result = new List<byte> {
				this.Ehd1,
				this.Ehd2
			};
			if (this.Tid != null) result.AddRange(this.Tid);
			if (this.Seoj != null) result.AddRange(this.Seoj);
			if (this.Deoj != null) result.AddRange(this.Deoj);
			result.Add(this.Esv);
			result.Add(this.Opc);
			result.Add(this.Epc);
			result.Add(this.Pdc);
			if (this.Edt != null) result.AddRange(this.Edt);
			return result.ToArray();
		}

		public string GetFrameString() {
			return Encoding.Default.GetString(this.GetFrame());
		}

		public int GetFrameSize() {
			return this.GetFrame().Length;
		}
	}
}
