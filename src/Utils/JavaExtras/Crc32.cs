// http://sanity-free.org/12/crc32_implementation_in_csharp.html

namespace NullFX.Security {
	public static class Crc32 {
		static uint[] table;

		public static uint ComputeChecksum(byte[] bytes, int offset, int length) {
			uint crc = 0xffffffff;
			for (int i = offset; i < offset + length; ++i) {
				byte index = (byte) (((crc) & 0xff) ^ bytes[i]);
				crc = (uint) ((crc >> 8) ^ table[index]);
			}
			return ~crc;
		}

		static Crc32() {
			uint poly = 0xedb88320;
			table = new uint[256];
			uint temp = 0;
			for (uint i = 0; i < table.Length; ++i) {
				temp = i;
				for (int j = 8; j > 0; --j) {
					if ((temp & 1) == 1) {
						temp = (uint) ((temp >> 1) ^ poly);
					} else {
						temp >>= 1;
					}
				}
				table[i] = temp;
			}
		}
	}
}
