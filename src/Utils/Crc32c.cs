/* This is .NET safe implementation of Crc32C algorithm.
 * This implementation was found fastest from some variants, based on Robert Vazan native implementations
 * Also, it is good for x64 and for x86, so, it seems, there is no sense to do 2 different realizations.
 * Reference speed: Hardware: 20GB/s, Software Native: 2GB/s, this: 1GB/s
 * 
 * Max Vysokikh, 2016
 */
namespace SCTP4CS.Utils {
	public static class Crc32c {
		private const uint Poly = 0x82f63b78;
		private static readonly uint[] _table = new uint[256];

		static Crc32c() {
			for (uint i = 0; i < 256; i++) {
				uint res = i;
				for (int k = 0; k < 8; k++) res = (res & 1) == 1 ? Poly ^ (res >> 1) : (res >> 1);
				_table[i] = res;
			}
		}

		public static uint Calculate(byte[] buffer, int offset, int length) {
			uint crc = ~0u;
			while (--length >= 0)
				crc = _table[(crc ^ buffer[offset++]) & 0xff] ^ crc >> 8;
			return crc ^ 0xffffffff;
		}
	}
}
