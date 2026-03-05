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


/// <summary>
/// A three component vector made of doubles.
/// </summary>
public class Vector3 : Vector2 {
	// Attributes:

	private static readonly long serialVersionUID = 8839258036865851454L;

	// Constructors

	/// <summary>
/// New zero vector.
/// </summary>
	public Vector3() {
		data = new double[3];
		data[0] = 0;
		data[1] = 0;
		data[2] = 0;
	}

	/// <summary>
/// New (<code>x</code>,<code>y</code>,<code>z</code>) vector.
/// </summary>
	public Vector3(double x, double y, double z) {
		data = new double[3];
		data[0] = x;
		data[1] = y;
		data[2] = z;
	}

	/// <summary>
/// New vector copy of <code>other</code>.
/// </summary>
	public Vector3(Vector3 other) {
		data = new double[3];
		copy(other);
	}

	/// <summary>
/// New vector copy of <code>point</code>.
/// </summary>
	public Vector3(Point3 point) {
		data = new double[3];
		copy(point);
	}

	// Predicates

	/// <summary>
/// Are all components to zero?.
/// </summary>
	
	public bool isZero() {
		return (data[0] == 0 && data[1] == 0 && data[2] == 0);
	}

	/// <summary>
/// Is this equal to other ?
/// </summary>
	
	public bool equals(object other) {
		Vector3 v;

		if (!(other is Vector3)) {
			return false;
		}

		v = (Vector3) other;

		return (data[0] == v.data[0] && data[1] == v.data[1] && data[2] == v.data[2]);
	}

	/// <summary>
/// Is i the index of a component ? In other words, is i &gt;= 0 &amp;&amp; &lt; than #count() ?
/// </summary>
	
	public bool validComponent(int i) {
		return (i >= 0 && i < 3);
	}

	// Access

	
	public object clone() {
		return new Vector3(this);
	}

	// Access

	public double dotProduct(double ox, double oy, double oz) {
		return ((data[0] * ox) + (data[1] * oy) + (data[2] * oz));
	}

	/// <summary>
/// Dot product of this and other.
/// </summary>
	public double dotProduct(Vector3 other) {
		return ((data[0] * other.data[0]) + (data[1] * other.data[1]) + (data[2] * other.data[2]));
	}

	/// <summary>
/// Cartesian length.
/// </summary>
	
	public double length() {
		return Math.Sqrt((data[0] * data[0]) + (data[1] * data[1]) + (data[2] * data[2]));
	}

	public double z() {
		return data[2];
	}

	// Commands

	/// <summary>
/// Assign value to all elements.
/// </summary>
	
	public void fill(double value) {
		data[0] = data[1] = data[2] = value;
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
	public void set(double x, double y, double z) {
		data[0] = x;
		data[1] = y;
		data[2] = z;
	}

	/// <summary>
/// Add each element of other to the corresponding element of this.
/// </summary>
	public void add(Vector3 other) {
		data[0] += other.data[0];
		data[1] += other.data[1];
		data[2] += other.data[2];
	}

	/// <summary>
/// Substract each element of other to the corresponding element of this.
/// </summary>
	public void sub(Vector3 other) {
		data[0] -= other.data[0];
		data[1] -= other.data[1];
		data[2] -= other.data[2];
	}

	/// <summary>
/// Multiply each element of this by the corresponding element of other.
/// </summary>
	public void mult(Vector3 other) {
		data[0] *= other.data[0];
		data[1] *= other.data[1];
		data[2] *= other.data[2];
	}

	/// <summary>
/// Add value to each element.
/// </summary>
	
	public void scalarAdd(double value) {
		data[0] += value;
		data[1] += value;
		data[2] += value;
	}

	/// <summary>
/// Substract value to each element.
/// </summary>
	
	public void scalarSub(double value) {
		data[0] -= value;
		data[1] -= value;
		data[2] -= value;
	}

	/// <summary>
/// Multiply each element by value.
/// </summary>
	
	public void scalarMult(double value) {
		data[0] *= value;
		data[1] *= value;
		data[2] *= value;
	}

	/// <summary>
/// Divide each element by value.
/// </summary>
	
	public void scalarDiv(double value) {
		data[0] /= value;
		data[1] /= value;
		data[2] /= value;
	}

	/// <summary>
/// Set this to the cross product of this and other.
/// </summary>
	public void crossProduct(Vector3 other) {
		double x;
		double y;

		x = (data[1] * other.data[2]) - (data[2] * other.data[1]);
		y = (data[2] * other.data[0]) - (data[0] * other.data[2]);
		data[2] = (data[0] * other.data[1]) - (data[1] * other.data[0]);
		data[0] = x;
		data[1] = y;
	}

	/// <summary>
/// Set this to the cross product of A and B.
/// </summary>
	public void crossProduct(Vector3 A, Vector3 B) {
		data[0] = (A.data[1] * B.data[2]) - (A.data[2] * B.data[1]);
		data[1] = (A.data[2] * B.data[0]) - (A.data[0] * B.data[2]);
		data[2] = (A.data[0] * B.data[1]) - (A.data[1] * B.data[0]);
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
			data[2] /= len;
		}

		return len;
	}

	// Utility

	/// <summary>
/// Make this a copy of other.
/// </summary>
	public void copy(Vector3 other) {
		data[0] = other.data[0];
		data[1] = other.data[1];
		data[2] = other.data[2];
	}

	/// <summary>
/// Make this a copy of <code>point</code>.
/// </summary>
	public void copy(Point3 point) {
		data[0] = point.x;
		data[1] = point.y;
		data[2] = point.z;
	}

	// Misc.

	
	public string toString() {
		StringBuffer sb = new StringBuffer("[");

		sb.Append(data[0]);
		sb.Append('|');
		sb.Append(data[1]);
		sb.Append('|');
		sb.Append(data[2]);
		sb.Append(']');

		return sb.ToString();
	}
}
}
