using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Org.GraphStream.Stream.File
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


public class FileSinkDGSUtility {
	protected static string formatStringForQuoting(string str) {
		return str.Replace("(^|[^\\\\])\"", "$1\\\\\"");
	}

	protected static string attributeString(string key, object value, bool remove) {
		if (key == null || key.Length == 0)
			return null;

		if (remove) {
			return string.Format(" -\"{0}\"", key);
		} else {
			if (value != null && value.GetType().IsArray)
				return string.Format(" \"{0}\":%s", key, arrayString(value));
			else
				return string.Format(" \"{0}\":%s", key, valueString(value));
		}
	}

	protected static string arrayString(object value) {
		if (value != null && value.GetType().IsArray) {
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append("{");

			if (Array.getLength(value) == 0)
				sb.Append("\"\"");
			else
				sb.Append(arrayString(Array[value, 0]));

			for (int i = 1; i < Array.getLength(value); ++i)
				sb.Append(string.Format(",{0}", arrayString(Array[value, i])));

			sb.Append("}");
			return sb.ToString();
		} else {
			return valueString(value);
		}
	}

	protected static string valueString(object value) {
		if (value == null)
			return "\"\"";

		if (value is string) {
			if (value is string)
				return string.Format("\"{0}\"", formatStringForQuoting((string) value));
			else
				return string.Format("\"{0}\"", (string) value);
		} else if (value is IConvertible) {
			IConvertible nval = (IConvertible) value;

			if (value is int || value is short || value is byte || value is long)
				return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", nval);
			else
				return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", nval);
		} else if (value is bool) {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", ((bool) value));
		} else if (value is char) {
			return string.Format("\"%c\"", ((char) value).charValue());
		} else if (value is object[]) {
			object array[] = (object[]) value;
			int n = array.Length;
			StringBuffer sb = new StringBuffer();

			if (array.Length > 0)
				sb.Append(valueString(array[0]));

			for (int i = 1; i < n; i++) {
				sb.Append(",");
				sb.Append(valueString(array[i]));
			}

			return sb.ToString();
		} else if (value is Dictionary<object, object>) {
			Dictionary<object, object> hash = (Dictionary<object, object>) value;

			return hashToString(hash);
		} else if (value is Color) {
			Color c = (Color) value;
			return string.Format("#{0}{1}{2}{3}", c.getRed(), c.getGreen(), c.getBlue(), c.getAlpha());
		} else {
			return string.Format("\"{0}\"", value.ToString());
		}
	}

	protected static string hashToString(Dictionary<object, object> hash) {
		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		sb.Append("[ ");

		foreach (object key in hash.Keys) {
			sb.Append(attributeString(key.ToString(), hash[key], false));
			sb.Append(",");
		}

		sb.Append(']');

		return sb.ToString();
	}
}
}
