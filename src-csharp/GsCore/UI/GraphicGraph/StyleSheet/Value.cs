using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Org.GraphStream.UI.GraphicGraph.StyleSheet
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


/// <summary>
/// A value and the units of the value. <p> As a style sheet may express values in several different units. This class purpose is to pack the value and the units it is expressed in into a single object. </p>
/// </summary>
public class Value : IConvertible {
	// Attributes

	private static readonly long serialVersionUID = 1L;

	/// <summary>
/// The value.
/// </summary>
	public double value;

	/// <summary>
/// The value units.
/// </summary>
	public Style.Units units;

	// Constructor

	/// <summary>
/// New value.
/// </summary>
/// <param name="units"> The value units.</param>
/// <param name="value"> The value.</param>
	public Value(Style.Units units, double value) {
		this.value = value;
		this.units = units;
	}

	/// <summary>
/// New copy of another value.
/// </summary>
/// <param name="other"> The other value to copy.</param>
	public Value(Value other) {
		this.value = other.value;
		this.units = other.units;
	}

	
	public float floatValue() {
		return (float) value;
	}

	
	public double doubleValue() {
		return value;
	}

	
	public int intValue() {
		return (int) (int)Math.Round(value);
	}

	
	public long longValue() {
		return (int)Math.Round(value);
	}

	
	public string toString() {
		System.Text.StringBuilder builder = new System.Text.StringBuilder();

		builder.Append(value);

		switch (units) {
		case GU:
			builder.Append("gu");
			break;
		case PX:
			builder.Append("px");
			break;
		case PERCENTS:
			builder.Append("%");
			break;
		default:
			builder.Append("wtf (what's the fuck?)");
			break;
		}

		return builder.ToString();
	}

	public bool equals(Value o) {
		if (o != this) {
			if (!(o is Value))
				return false;

			Value other = (Value) o;

			if (other.units != units)
				return false;

			if (other.value != value)
				return false;
		}

		return true;
	}
}
}
