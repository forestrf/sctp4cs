using SCTP4CS.Utils;
using System;
using System.Text;

public static class ExtensionMethods {
	public static byte[] getBytes(this string s) {
		return Encoding.ASCII.GetBytes(s);
	}
	public static string getString(this byte[] bb) {
		return Encoding.ASCII.GetString(bb);
	}
	public static string getString(this byte[] bb, int index, int count) {
		return Encoding.ASCII.GetString(bb, index, count);
	}
	public static byte[] clone(this byte[] b) {
		var bb = new byte[b.Length];
		b.CopyTo(bb, 0);
		return bb;
	}

	
	public static string GetHex(this ByteBuffer _in, string separation = "") {
		return GetHex(_in.Data, (uint) _in.offset, _in.Length, separation);
	}
	public static string GetHex(this byte[] _in, string separation = "") {
		return GetHex(_in, 0, _in.Length, separation);
	}
	public static string GetHex(this byte[] _in, uint off, int len, string separation = "") {
		StringBuilder ret = new StringBuilder();
		int top = Math.Min(_in.Length, len);
		for (int i = (int) off; i < top; i++) {
			ret.AppendFormat("{0:x2}", _in[i]);
			if (i < top - 1) ret.Append(separation);
		}
		return ret.ToString().ToUpper();
	}
}
