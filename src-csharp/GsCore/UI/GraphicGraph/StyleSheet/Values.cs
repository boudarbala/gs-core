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
/// Several values and the units of these values. <p> As a style sheet may express values in several different units. This class purpose is to pack the value and the units it is expressed in into a single object. </p>
/// </summary>
public class Values : IEnumerable<double> {
	// Attributes

	/// <summary>
/// The value.
/// </summary>
	public List<double> values = new List<double>();

	/// <summary>
/// The values units.
/// </summary>
	public Style.Units units;

	// Constructor

	/// <summary>
/// New value set with one initial value.
/// </summary>
/// <param name="units"> The values units.</param>
/// <param name="values"> A variable count of values.</param>
	public Values(Style.Units units, params double[] values) {
		this.units = units;

		foreach (double value in values)
			this.values.Add(value);
	}

	/// <summary>
/// New copy of another value set.
/// </summary>
/// <param name="other"> The other values to copy.</param>
	public Values(Values other) {
		this.values = new List<double>(other.values);
		this.units = other.units;
	}

	/// <summary>
/// New set of one value.
/// </summary>
/// <param name="value"> The value to copy with its units.</param>
	public Values(Value value) {
		this.values = new List<double>();
		this.units = value.units;

		values.Add(value.value);
	}

	/// <summary>
/// Number of values in this set.
/// </summary>
/// <returns>The number of values.</returns>
	public int size() {
		return values.Count;
	}

	/// <summary>
/// Number of values in this set.
/// </summary>
/// <returns>The number of values.</returns>
	public int getValueCount() {
		return values.Count;
	}

	/// <summary>
/// The i-th value of this set. If the index is less than zero, the first value is given, if the index if greater or equal to the number of values, the last value is given.
/// </summary>
/// <param name="i"> The value index.</param>
/// <returns>The corresponding value.</returns>
	public double get(int i) {
		if (i < 0)
			return values[0];
		else if (i >= values.Count)
			return values[values.Count - 1];
		else
			return values[i];
	}

	/// <summary>
/// Values units.
/// </summary>
/// <returns>The units used for each value.</returns>
	public Style.Units getUnits() {
		return units;
	}

	
	public bool equals(object o) {
		if (o != this) {
			if (!(o is Values))
				return false;

			Values other = (Values) o;

			if (other.units != units)
				return false;

			int n = values.Count;

			if (other.values.Count != n)
				return false;

			for (int i = 0; i < n; i++) {
				if (!other.values[i].Equals(values[i]))
					return false;
			}
		}

		return true;
	}

	public IEnumerator<double> iterator() {
		return values.GetEnumerator();
	}

	
	public string toString() {
		System.Text.StringBuilder builder = new System.Text.StringBuilder();

		builder.Append('(');
		foreach (double value in values) {
			builder.Append(' ');
			builder.Append(value);
		}
		builder.Append(" )");

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

	/// <summary>
/// Copy the given values to this set. The units are also copied.
/// </summary>
/// <param name="values"> The values to copy.</param>
	public void copy(Values values) {
		units = values.units;
		this.values.Clear();
		this.values.AddRange(values.values);
	}

	/// <summary>
/// Append the given set of values at the end of this set.
/// </summary>
/// <param name="values"> The value set to append.</param>
	public void addValues(params double[] values) {
		foreach (double value in values)
			this.values.Add(value);
	}

	/// <summary>
/// Insert the given value at the given index.
/// </summary>
/// <param name="i"> Where to insert the value.</param>
/// <param name="value"> The value to insert.</param>
	public void insertValue(int i, double value) {
		values.Add(i, value);
	}

	/// <summary>
/// Change the i-th value.
/// </summary>
/// <param name="i"> The value index.</param>
/// <param name="value"> The value to put.</param>
	public void setValue(int i, double value) {
		values.set(i, value);
	}

	/// <summary>
/// Remove the i-th value.
/// </summary>
/// <param name="i"> The index at which the value is to be removed.</param>
	public void removeValue(int i) {
		values.Remove(i);
	}

	/// <summary>
/// Change the values units.
/// </summary>
/// <param name="units"> The units.</param>
	public void setUnits(Style.Units units) {
		this.units = units;
	}
}
}
