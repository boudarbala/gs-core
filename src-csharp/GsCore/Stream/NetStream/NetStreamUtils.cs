using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.NetStream
{
/*
 * This file is part of GraphStream <http://graphstream-project.org>.
 * 
 * GraphStream is a library whose purpose is to handle static or dynamic
 * graph, create them from scratch, file or any source and display them.
 * 
 * This program is free software distributed under the terms of two licenses, the
 * CeCILL-C license that fits European law, and the GNU Lesser General Public
 * License. You can  use, modify and/ or redistribute the software under the terms
 * of the CeCILL-C license as circulated by CEA, CNRS and INRIA at the following
 * URL <http://www.cecill.info> or under the terms of the GNU LGPL as published by
 * the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * The fact that you are presently reading this means that you have had
 * knowledge of the CeCILL-C and LGPL licenses and that you accept their terms.
 */



public class NetStreamUtils {
	private static byte[] NULL_BUFFER = new byte[0];
	private static readonly object /* Logger */ LOGGER = null /* Logger */;

	public static ByteFactory getDefaultNetStreamFactory() {
		return null /* TODO: implement ByteFactory */;
	}

	public static int getType(object value) {
		int valueType = NetStreamConstants.TYPE_UNKNOWN;

		if (value == null)
			return NetStreamConstants.TYPE_NULL;

		Type valueClass = value.GetType();
		bool isArray = valueClass.IsArray;
		if (isArray) {
			if (Array.getLength(value) > 0) {
				valueClass = Array[value, 0].GetType();
			} else {
				return NetStreamConstants.TYPE_ARRAY;
			}
		}
		if (valueClass.Equals(typeof(bool))) {
			if (isArray) {
				valueType = NetStreamConstants.TYPE_BOOLEAN_ARRAY;
			} else {
				valueType = NetStreamConstants.TYPE_BOOLEAN;
			}
		} else if (valueClass.Equals(typeof(byte))) {
			if (isArray) {
				valueType = NetStreamConstants.TYPE_BYTE_ARRAY;
			} else {
				valueType = NetStreamConstants.TYPE_BYTE;
			}
		} else if (valueClass.Equals(typeof(short))) {
			if (isArray) {
				valueType = NetStreamConstants.TYPE_SHORT_ARRAY;
			} else {
				valueType = NetStreamConstants.TYPE_SHORT;
			}
		} else if (valueClass.Equals(typeof(int))) {
			if (isArray) {
				valueType = NetStreamConstants.TYPE_INT_ARRAY;
			} else {
				valueType = NetStreamConstants.TYPE_INT;
			}
		} else if (valueClass.Equals(typeof(long))) {
			if (isArray) {
				valueType = NetStreamConstants.TYPE_LONG_ARRAY;
			} else {
				valueType = NetStreamConstants.TYPE_LONG;
			}
		} else if (valueClass.Equals(typeof(float))) {
			if (isArray) {
				valueType = NetStreamConstants.TYPE_FLOAT_ARRAY;
			} else {
				valueType = NetStreamConstants.TYPE_FLOAT;
			}
		} else if (valueClass.Equals(typeof(double))) {
			if (isArray) {
				valueType = NetStreamConstants.TYPE_DOUBLE_ARRAY;
			} else {
				valueType = NetStreamConstants.TYPE_DOUBLE;
			}
		} else if (valueClass.Equals(typeof(string))) {
			if (isArray) {
				valueType = NetStreamConstants.TYPE_STRING_ARRAY;
			} else {
				valueType = NetStreamConstants.TYPE_STRING;
			}
		} else
			Console.Error.WriteLine(string.Format("can not find type of {0}.", valueClass));

		return valueType;
	}

	public static int getVarintSize(long data) {
		// 7 bits -> 127
		if (data < (1L << 7)) {
			return 1;
		}

		// 14 bits -> 16383
		if (data < (1L << 14)) {
			return 2;
		}

		// 21 bits -> 2097151
		if (data < (1L << 21)) {
			return 3;
		}

		// 28 bits -> 268435455
		if (data < (1L << 28)) {
			return 4;
		}

		// 35 bits -> 34359738367
		if (data < (1L << 35)) {
			return 5;
		}

		// 42 bits -> 4398046511103
		if (data < (1L << 42)) {
			return 6;
		}

		// 49 bits -> 562949953421311
		if (data < (1L << 49)) {
			return 7;
		}

		// 56 bits -> 72057594037927935
		if (data < (1L << 56)) {
			return 8;
		}

		return 9;
	}

	public static void putVarint(byte[] buffer, long number, int byteSize) {
		for (int i = 0; i < byteSize; i++) {
			int head = 128;
			if (i == byteSize - 1)
				head = 0;
			long b = ((number >> (7 * i)) & 127) ^ head;
			buffer.Add((byte) (b & 255));
		}
	}

	//
	// ENCODING METHODS
	//

	public static byte[] encodeValue(object input, int valueType) {
		if (NetStreamConstants.TYPE_BOOLEAN == valueType) {
			return encodeBoolean(in);
		} else if (NetStreamConstants.TYPE_BOOLEAN_ARRAY == valueType) {
			return encodeBooleanArray(in);
		} else if (NetStreamConstants.TYPE_BYTE == valueType) {
			return encodeByte(in);
		} else if (NetStreamConstants.TYPE_BYTE_ARRAY == valueType) {
			return encodeByteArray(in);
		} else if (NetStreamConstants.TYPE_SHORT == valueType) {
			return encodeShort(in);
		} else if (NetStreamConstants.TYPE_SHORT_ARRAY == valueType) {
			return encodeShortArray(in);
		} else if (NetStreamConstants.TYPE_INT == valueType) {
			return encodeInt(in);
		} else if (NetStreamConstants.TYPE_INT_ARRAY == valueType) {
			return encodeIntArray(in);
		} else if (NetStreamConstants.TYPE_LONG == valueType) {
			return encodeLong(in);
		} else if (NetStreamConstants.TYPE_LONG_ARRAY == valueType) {
			return encodeLongArray(in);
		} else if (NetStreamConstants.TYPE_FLOAT == valueType) {
			return encodeFloat(in);
		} else if (NetStreamConstants.TYPE_FLOAT_ARRAY == valueType) {
			return encodeFloatArray(in);
		} else if (NetStreamConstants.TYPE_DOUBLE == valueType) {
			return encodeDouble(in);
		} else if (NetStreamConstants.TYPE_DOUBLE_ARRAY == valueType) {
			return encodeDoubleArray(in);
		} else if (NetStreamConstants.TYPE_STRING == valueType) {
			return encodeString(in);
		} else if (NetStreamConstants.TYPE_STRING_ARRAY == valueType) {
			return encodeStringArray(in);
		} else if (NetStreamConstants.TYPE_ARRAY == valueType) {
			return encodeArray(in);
		} else if (NetStreamConstants.TYPE_NULL == valueType) {
			return NULL_BUFFER;
		}

		return null;
	}

	public static byte[] encodeUnsignedVarint(object input) {
		long data = ((IConvertible) in);
		int size = getVarintSize(data);

		byte[] buff = new byte[size];
		for (int i = 0; i < size; i++) {
			int head = 128;
			if (i == size - 1)
				head = 0;
			long b = ((data >> (7 * i)) & 127) ^ head;
			buff.Add((byte) (b & 255));
		}
		/* buff.rewind() */;

		return buff;
	}

	public static byte[] encodeVarint(object input) {
		long data = ((IConvertible) in);

		// signed integers encoding
		// (n << 1) ^ (n >> 31)
		// OK but java's negative values are two's params complements[] return encodeUnsignedVarint(data >= 0 ? (data << 1) : ((Math.Abs(data) << 1) ^ 1));
	}

	public static byte[] encodeString(object input) {
		string s = (string) in;
		byte[] data = s.getBytes(Charset.forName("UTF-8"));

		byte[] lenBuff = encodeUnsignedVarint(data.Length);
		// outBuffer(lenBuff);
		byte[] bb = byte[].allocate(lenBuff.Length + data.Length);
		bb.Add(lenBuff).Add(data);
		/* bb.rewind() */;
		// outBuffer(bb);

		return bb;
	}

	public static byte[] encodeArray(object input) {
		// params TODO[] return null;
	}

	public static byte[] encodeDoubleArray(object input) {
		object[] data = (object[]) in;

		int ssize = getVarintSize(data.Length);

		byte[] b = byte[].allocate(ssize + data.Length * 8);

		putVarint(b, data.Length, ssize);

		for (int i = 0; i < data.Length; i++) {
			b.putDouble((double) data[i]);
		}
		/* b.rewind() */;
		return b;
	}

	public static byte[] encodeStringArray(object input) {
		object[] data = (object[]) in;

		int ssize = getVarintSize(data.Length);

		byte[][] dataArray = new byte[data.Length][];
		byte[][] lenBuffArray = new byte[][data.Length];
		int bufferSize = 0;
		for(int i = 0; i < data.Length; i++){
			byte[] bs = ((string)data[i]).getBytes(Charset.forName("UTF-8"));
			dataArray[i] = bs;

			byte[] lenBuff = encodeUnsignedVarint(bs.Length);
			lenBuffArray[i] = lenBuff;

			bufferSize += lenBuff.Length +bs.Length;
		}


		byte[] bb = byte[].allocate(ssize + bufferSize);

		putVarint(bb, data.Length, ssize);

		for(int i = 0; i < data.Length; i++) {
			bb.Add(lenBuffArray[i]).Add(dataArray[i]);
		}
		/* bb.rewind() */;

		return bb;
	}

	/// <param name="in"> The double to encode</param>
/// <returns>ByteBuffer with encoded double in it</returns>
	public static byte[] encodeDouble(object input) {
		byte[] bb = new byte[8].putDouble((double) in);
		/* bb.rewind() */;
		return bb;
	}

	/// <param name="in"> The float array to encode</param>
/// <returns>ByteBuffer with encoded float array in it</returns>
	public static byte[] encodeFloatArray(object input) {
		object[] data = (object[]) in;

		int ssize = getVarintSize(data.Length);

		byte[] b = byte[].allocate(ssize + data.Length * 4);

		putVarint(b, data.Length, ssize);

		for (int i = 0; i < data.Length; i++) {
			b.putFloat((float) data[i]);
		}
		/* b.rewind() */;
		return b;
	}

	/// <param name="in"> The float to encode</param>
/// <returns>ByteBuffer with encoded float in it</returns>
	public static byte[] encodeFloat(object input) {
		byte[] b = new byte[4];
		b.putFloat(((float) in));
		/* b.rewind() */;
		return b;
	}

	/// <param name="in"> The long array to encode</param>
/// <returns>ByteBuffer with encoded long array in it</returns>
	public static byte[] encodeLongArray(object input) {
		return encodeVarintArray(in);
	}

	/// <param name="in"> The long to encode</param>
/// <returns>ByteBuffer with encoded long in it</returns>
	public static byte[] encodeLong(object input) {
		return encodeVarint(in);
	}

	/// <param name="in"> The integer array to encode</param>
/// <returns>ByteBuffer with encoded integer array in it</returns>
	public static byte[] encodeIntArray(object input) {
		return encodeVarintArray(in);
	}

	/// <param name="in"> The integer to encode</param>
/// <returns>ByteBuffer with encoded integer in it</returns>
	public static byte[] encodeInt(object input) {
		return encodeVarint(in);
	}

	/// <param name="in"></param>
/// <returns></returns>
	public static byte[] encodeShortArray(object input) {
		return encodeVarintArray(in);
	}

	/// <param name="in"></param>
/// <returns></returns>
	public static byte[] encodeShort(object input) {
		return encodeVarint(in);
	}

	/// <param name="in"></param>
/// <returns></returns>
	public static byte[] encodeByteArray(object input) {
		object[] data = (object[]) in;

		int ssize = getVarintSize(data.Length);

		byte[] b = byte[].allocate(ssize + data.Length);

		putVarint(b, data.Length, ssize);

		for (int i = 0; i < data.Length; i++) {
			b.Add((byte) data[i]);
		}
		/* b.rewind() */;
		return b;
	}

	/// <param name="in"></param>
/// <returns></returns>
	public static byte[] encodeByte(object input) {
		byte[] b = new byte[1];
		b.Add(((byte) in));
		/* b.rewind() */;
		return b;
	}

	/// <param name="in"></param>
/// <returns></returns>
	public static byte[] encodeBooleanArray(object input) {
		object[] data = (object[]) in;

		int ssize = getVarintSize(data.Length);

		byte[] b = byte[].allocate(ssize + data.Length);

		putVarint(b, data.Length, ssize);

		for (int i = 0; i < data.Length; i++) {
			b.Add((byte) ((bool) data[i] == false ? 0 : 1));
		}
		/* b.rewind() */;
		return b;
	}

	/// <param name="in"></param>
/// <returns></returns>
	public static byte[] encodeBoolean(object input) {
		byte[] b = new byte[1];
		b.Add((byte) (((bool) in) == false ? 0 : 1));
		/* b.rewind() */;
		return b;
	}

	public static byte[] encodeVarintArray(object input) {
		object[] data = (object[]) in;
		int[] sizes = new int[data.Length];
		long[] zigzags = new long[data.Length];
		int sumsizes = 0;
		for (int i = 0; i < data.Length; i++) {
			long datum = ((IConvertible) data[i]);
			// signed integers encoding
			// (n << 1) ^ (n >> 31)
			// OK but java's negative values are two's params complements[] zigzags[i] = datum > 0 ? (datum << 1) : ((Math.Abs(datum) << 1) ^ 1);

			sizes[i] = getVarintSize(zigzags[i]);
			sumsizes += sizes[i];
			// System.output.printf("i=%d, zigzag=%d, size=%d\n",i, zigzags[i], sizes[i]);
		}

		// the size of the size!
		int ssize = getVarintSize(data.Length);

		byte[] b = byte[].allocate(ssize + sumsizes);

		putVarint(b, data.Length, ssize);

		for (int i = 0; i < data.Length; i++) {
			putVarint(b, zigzags[i], sizes[i]);
		}
		/* b.rewind() */;
		// outBuffer(b);
		return b;
	}

	//
	// DECODING METHODS
	//

	/// <param name="bb"></param>
/// <returns></returns>
	public static int decodeType(byte[] bb) {
		try {
			return bb[];
		} catch (BufferUnderflowException e) {
			Console.WriteLine("decodeType: could not decode type");
			Console.Error.WriteLine(e);
		}

		return 0;
	}

	public static object decodeValue(byte[] bb, int valueType) {
		if (NetStreamConstants.TYPE_BOOLEAN == valueType) {
			return decodeBoolean(bb);
		} else if (NetStreamConstants.TYPE_BOOLEAN_ARRAY == valueType) {
			return decodeBooleanArray(bb);
		} else if (NetStreamConstants.TYPE_BYTE == valueType) {
			return decodeByte(bb);
		} else if (NetStreamConstants.TYPE_BYTE_ARRAY == valueType) {
			return decodeByteArray(bb);
		} else if (NetStreamConstants.TYPE_SHORT == valueType) {
			return decodeShort(bb);
		} else if (NetStreamConstants.TYPE_SHORT_ARRAY == valueType) {
			return decodeShortArray(bb);
		} else if (NetStreamConstants.TYPE_INT == valueType) {
			return decodeInt(bb);
		} else if (NetStreamConstants.TYPE_INT_ARRAY == valueType) {
			return decodeIntArray(bb);
		} else if (NetStreamConstants.TYPE_LONG == valueType) {
			return decodeLong(bb);
		} else if (NetStreamConstants.TYPE_LONG_ARRAY == valueType) {
			return decodeLongArray(bb);
		} else if (NetStreamConstants.TYPE_FLOAT == valueType) {
			return decodeFloat(bb);
		} else if (NetStreamConstants.TYPE_FLOAT_ARRAY == valueType) {
			return decodeFloatArray(bb);
		} else if (NetStreamConstants.TYPE_DOUBLE == valueType) {
			return decodeDouble(bb);
		} else if (NetStreamConstants.TYPE_DOUBLE_ARRAY == valueType) {
			return decodeDoubleArray(bb);
		} else if (NetStreamConstants.TYPE_STRING == valueType) {
			return decodeString(bb);
		} else if (NetStreamConstants.TYPE_STRING_ARRAY == valueType) {
			return decodeStringArray(bb);
		} else if (NetStreamConstants.TYPE_ARRAY == valueType) {
			return decodeArray(bb);
		}
		return null;
	}

	/// <param name="bb"></param>
/// <returns></returns>
	public static object[] decodeArray(byte[] bb) {

		int len = (int) decodeUnsignedVarint(bb);

		object[] array = new object[len];
		for (int i = 0; i < len; i++) {
			array[i] = decodeValue(bb, decodeType(bb));
		}
		return array;

	}

	public static string decodeString(byte[] bb) {
		try {
			int len = (int) decodeUnsignedVarint(bb);
			byte[] data = new byte[len];

			bb[data];

			return new string(data, Charset.forName("UTF-8"));
		} catch (BufferUnderflowException e) {
			Console.WriteLine("decodeString: could not decode string");
			Console.Error.WriteLine(e);
		}

		return null;
	}

	public static string[] decodeStringArray(byte[] bb) {
		int len = (int) decodeUnsignedVarint(bb);
		string[] array = new string[len];
		for (int i = 0; i < len; i++) {
			array[i] = decodeString(bb);
		}
		return array;
	}

	public static bool decodeBoolean(byte[] bb) {
		int data = 0;

		try {
			data = bb[];
		} catch (BufferUnderflowException e) {
			Console.WriteLine("decodeByte: could not decode");
			Console.Error.WriteLine(e);
		}

		return data != 0;
	}

	public static byte decodeByte(byte[] bb) {
		byte data = 0;

		try {
			data = bb[];
		} catch (BufferUnderflowException e) {
			Console.WriteLine("decodeByte: could not decode");
			Console.Error.WriteLine(e);
		}

		return data;
	}

	public static long decodeUnsignedVarint(byte[] bb) {
		try {
			int size = 0;
			long[] data = new long[9];

			do {
				data[size] = bb[];

				size++;

				// int bt =data[size-1];
				// if (bt < 0) bt = (bt & 127) + (bt & 128);
				// System.output.println("test "+bt+" -> "+(data[size - 1]& 128) );
			} while ((data[size - 1] & 128) == 128);
			long number = 0;

			for (int i = 0; i < size; i++) {
				number ^= (data[i] & 127L) << (i * 7L);
			}

			return number;

		} catch (BufferUnderflowException e) {
			Console.WriteLine("decodeUnsignedVarintFromInteger: could not decode");
			Console.Error.WriteLine(e);
		}

		return 0L;
	}

	public static long decodeVarint(byte[] bb) {
		long number = decodeUnsignedVarint(bb);
		return ((number & 1) == 0) ? number >> 1 : -(number >> 1);
	}

	public static short decodeShort(byte[] bb) {
		return (short) decodeVarint(bb);
	}

	public static int decodeInt(byte[] bb) {
		return (int) decodeVarint(bb);
	}

	public static long decodeLong(byte[] bb) {
		return decodeVarint(bb);
	}

	public static float decodeFloat(byte[] bb) {
		return bb.getFloat();
	}

	public static double decodeDouble(byte[] bb) {
		return bb.getDouble();
	}

	public static int[] decodeIntArray(byte[] bb) {
		int len = (int) decodeUnsignedVarint(bb);

		int[] res = new int[len];
		for (int i = 0; i < len; i++) {
			res[i] = (int) decodeVarint(bb);
			// System.output.printf("array[%d]=%d%n",i,res[i]);
		}

		return res;
	}

	public static bool[] decodeBooleanArray(byte[] bb) {
		try {
			int len = (int) decodeUnsignedVarint(bb);
			bool[] res = new bool[len];

			for (int i = 0; i < len; i++) {
				byte b = bb[];
				res[i] = b != 0;
			}

			return res;
		} catch (BufferUnderflowException e) {
			Console.WriteLine("decodeBooleanArray: could not decode array");
			Console.Error.WriteLine(e);
		}

		return null;
	}

	public static byte[] decodeByteArray(byte[] bb) {
		try {
			int len = (int) decodeUnsignedVarint(bb);
			byte[] res = new byte[len];

			for (int i = 0; i < len; i++) {
				res[i] = bb[];
			}

			return res;
		} catch (BufferUnderflowException e) {
			Console.WriteLine("decodeBooleanArray: could not decode array");
			Console.Error.WriteLine(e);
		}

		return null;
	}

	public static double[] decodeDoubleArray(byte[] bb) {
		try {
			int len = (int) decodeUnsignedVarint(bb);
			double[] res = new double[len];

			for (int i = 0; i < len; i++) {
				res[i] = bb.getDouble();
			}

			return res;
		} catch (BufferUnderflowException e) {
			Console.WriteLine("decodeDoubleArray: could not decode array");
			Console.Error.WriteLine(e);
		}

		return null;
	}

	public static float[] decodeFloatArray(byte[] bb) {
		try {
			int len = (int) decodeUnsignedVarint(bb);
			float[] res = new float[len];

			for (int i = 0; i < len; i++) {
				res[i] = bb.getFloat();
			}

			return res;
		} catch (BufferUnderflowException e) {
			Console.WriteLine("decodeFloatArray: could not decode array");
			Console.Error.WriteLine(e);
		}

		return null;
	}

	public static long[] decodeLongArray(byte[] bb) {
		int len = (int) decodeUnsignedVarint(bb);
		long[] res = new long[len];

		for (int i = 0; i < len; i++) {
			res[i] = decodeVarint(bb);
		}

		return res;
	}

	public static short[] decodeShortArray(byte[] bb) {
		int len = (int) decodeUnsignedVarint(bb);
		short[] res = new short[len];

		for (int i = 0; i < len; i++) {
			res[i] = (short) decodeVarint(bb);
		}

		return res;
	}
}

}
