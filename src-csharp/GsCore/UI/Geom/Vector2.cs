using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.Geom
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


public class Vector2 : java.io.System.Runtime.Serialization.ISerializable {
	// Attributes

	private static readonly long serialVersionUID = 8839258036865851454L;

	/// <summary>
/// Sequence of 3 coefficients.
/// </summary>
	public double data[];

	// Constructors

	/// <summary>
/// New zero vector.
/// </summary>
	public Vector2() {
		data = new double[2];
		data[0] = 0;
		data[1] = 0;
	}

	/// <summary>
/// New (<code>x</code>,<code>y</code>) vector.
/// </summary>
	public Vector2(double x, double y) {
		data = new double[2];
		data[0] = x;
		data[1] = y;
	}

	/// <summary>
/// New vector copy of <code>other</code>.
/// </summary>
	public Vector2(Vector2 other) {
		data = new double[2];
		copy(other);
	}

	/// <summary>
/// New vector copy of <code>point</code>.
/// </summary>
	public Vector2(Point2 point) {
		data = new double[2];
		copy(point);
	}

	public Vector2(Point2 from, Point2 to) {
		data = new double[2];
		data[0] = to.x - from.x;
		data[1] = to.y - from.y;
	}

	// Predicates

	/// <summary>
/// Are all components to zero?.
/// </summary>
	public bool isZero() {
		return (data[0] == 0 && data[1] == 0);
	}

	/// <summary>
/// Is this equal to other ?
/// </summary>
	
	public bool equals(object other) {
		Vector2 v;

		if (!(other is Vector2)) {
			return false;
		}

		v = (Vector2) other;

		return (data[0] == v.data[0] && data[1] == v.data[1]);
	}

	/// <summary>
/// Is i the index of a component ? In other words, is i &gt;= 0 &amp;&amp; &lt; than #count() ?
/// </summary>
	public bool validComponent(int i) {
		return (i >= 0 && i < 2);
	}

	// Accessors:

	/// <summary>
/// i-th element.
/// </summary>
	public double at(int i) {
		return data[i];
	}

	public double x() {
		return data[0];
	}

	public double y() {
		return data[1];
	}

	
	public object clone() {
		return new Vector2(this);
	}

	// Accessors

	public double dotProduct(double ox, double oy) {
		return ((data[0] * ox) + (data[1] * oy));
	}

	/// <summary>
/// Dot product of this and other.
/// </summary>
	public double dotProduct(Vector2 other) {
		return ((data[0] * other.data[0]) + (data[1] * other.data[1]));
	}

	/// <summary>
/// Cartesian length.
/// </summary>
	public double length() {
		return Math.Sqrt((data[0] * data[0]) + (data[1] * data[1]));
	}

	// Commands

	/// <summary>
/// Assign value to all elements.
/// </summary>
	public void fill(double value) {
		data[0] = data[1] = value;
	}

	/// <summary>
/// Explicitly set the i-th component to value.
/// </summary>
	public void set(int i, double value) {
		data[i] = value;
	}

	/// <summary>
/// Explicitly set the three components.
/// </summary>
	public void set(double x, double y) {
		data[0] = x;
		data[1] = y;
	}

	/// <summary>
/// Add each element of other to the corresponding element of this.
/// </summary>
	public void add(Vector2 other) {
		data[0] += other.data[0];
		data[1] += other.data[1];
	}

	/// <summary>
/// Subtract each element of other to the corresponding element of this.
/// </summary>
	public void sub(Vector2 other) {
		data[0] -= other.data[0];
		data[1] -= other.data[1];
	}

	/// <summary>
/// Multiply each element of this by the corresponding element of other.
/// </summary>
	public void mult(Vector2 other) {
		data[0] *= other.data[0];
		data[1] *= other.data[1];
	}

	/// <summary>
/// Add value to each element.
/// </summary>
	public void scalarAdd(double value) {
		data[0] += value;
		data[1] += value;
	}

	/// <summary>
/// Substract value to each element.
/// </summary>
	public void scalarSub(double value) {
		data[0] -= value;
		data[1] -= value;
	}

	/// <summary>
/// Multiply each element by value.
/// </summary>
	public void scalarMult(double value) {
		data[0] *= value;
		data[1] *= value;
	}

	/// <summary>
/// Divide each element by value.
/// </summary>
	public void scalarDiv(double value) {
		data[0] /= value;
		data[1] /= value;
	}

	/// <summary>
/// Transform this into an unit vector.
/// </summary>
/// <returns>the vector length.</returns>
	public double normalize() {
		double len = length();

		if (len != 0) {
			data[0] /= len;
			data[1] /= len;
		}

		return len;
	}

	// Utility

	/// <summary>
/// Make this a copy of other.
/// </summary>
	public void copy(Vector2 other) {
		data[0] = other.data[0];
		data[1] = other.data[1];
	}

	/// <summary>
/// Make this a copy of <code>point</code>.
/// </summary>
	public void copy(Point2 point) {
		data[0] = point.x;
		data[1] = point.y;
	}

	// Misc.

	
	public string toString() {
		StringBuffer sb = new StringBuffer("[");

		sb.Append(data[0]);
		sb.Append('|');
		sb.Append(data[1]);
		sb.Append(']');

		return sb.ToString();
	}

	
	public int hashCode() {
		return data != null ? Arrays.hashCode(data) : 0;
	}
}
}
