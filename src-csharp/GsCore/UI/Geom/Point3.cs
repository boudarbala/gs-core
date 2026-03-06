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
/// 3D point. A Point3 is a 3D location in an affine space described by three values along the X, the Y and the Z axis. Note the difference with Vector3 wich is defined as an array and ensures that the three coordinates X, Y and Z are consecutive in memory. Here there are three separate attributes. Further, a point has no vector arithmetic bound to it (to points cannot be added, this would have no mathematical meaning).
/// </summary>
public class Point3 : Point2, java.io.System.Runtime.Serialization.ISerializable {
	// Attributes

	private static readonly long serialVersionUID = 5971336344439693816L;

	/// <summary>
/// Z axis value.
/// </summary>
	public double z;

	// Attributes -- Shared

	/// <summary>
/// Specific point at (0,0,0).
/// </summary>
	public static readonly Point3 NULL_POINT3 = new Point3(0, 0, 0);

	// Constructors

	/// <summary>
/// New 3D point at(0,0,0).
/// </summary>
	public Point3() {
	}

	/// <summary>
/// New 3D point at (x,y,0).
/// </summary>
	public Point3(double x, double y) {
		set(x, y, 0);
	}

	/// <summary>
/// New 3D point at(x,y,z).
/// </summary>
	public Point3(double x, double y, double z) {
		set(x, y, z);
	}

	/// <summary>
/// New copy of other.
/// </summary>
	public Point3(Point3 other) {
		copy(other);
	}

	public Point3(Vector3 vec) {
		copy(vec);
	}

	public Point3(float data[]) : this(0, data) {
	}

	public Point3(double data[]) : this(0, data) {
	}

	public Point3(int start, float data[]) {
		if (data != null) {
			if (data.Length > start + 0)
				x = data[start + 0];
			if (data.Length > start + 1)
				y = data[start + 1];
			if (data.Length > start + 2)
				z = data[start + 2];
		}
	}

	public Point3(int start, double data[]) {
		if (data != null) {
			if (data.Length > start + 0)
				x = data[start + 0];
			if (data.Length > start + 1)
				y = data[start + 1];
			if (data.Length > start + 2)
				z = data[start + 2];
		}
	}

	// Predicates

	/// <summary>
/// Are all components to zero?.
/// </summary>
	
	public bool isZero() {
		return (x == 0 && y == 0 && z == 0);
	}

	// /// <summary>
/// // * Is other equal to this ? //
/// </summary>
	// public boolean
	// equals( const Point3 < double > & other ) const
	// {
	// return( x == other.x
	// and y == other.y
	// and z == other.z );
	// }

	/// <summary>
/// Create a new point linear interpolation of this and <code>other</code>. The new point is located between this and <code>other</code> if <code>factor</code> is between 0 and 1 (0 yields this point, 1 yields the <code>other</code> point).
/// </summary>
	public Point3 interpolate(Point3 other, double factor) {
		Point3 p = new Point3(x + ((other.x - x) * factor), y + ((other.y - y) * factor), z + ((other.z - z) * factor));

		return p;
	}

	/// <summary>
/// Distance between this and <code>other</code>.
/// </summary>
	public double distance(Point3 other) {
		double xx = other.x - x;
		double yy = other.y - y;
		double zz = other.z - z;
		return Math.Abs(Math.Sqrt((xx * xx) + (yy * yy) + (zz * zz)));
	}

	/// <summary>
/// Distance between this and point (x,y,z).
/// </summary>
	public double distance(double x, double y, double z) {
		double xx = x - this.x;
		double yy = y - this.y;
		double zz = z - this.z;
		return Math.Abs(Math.Sqrt((xx * xx) + (yy * yy) + (zz * zz)));
	}

	// Commands

	/// <summary>
/// Make this a copy of other.
/// </summary>
	public void copy(Point3 other) {
		x = other.x;
		y = other.y;
		z = other.z;
	}

	public void copy(Vector3 vec) {
		x = vec.data[0];
		y = vec.data[1];
		z = vec.data[2];
	}

	/// <summary>
/// Like #moveTo().
/// </summary>
	public void set(double x, double y, double z) {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	// Commands -- moving

	/// <summary>
/// Move to absolute position (x,y,z).
/// </summary>
	public void moveTo(double x, double y, double z) {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	/// <summary>
/// Move of given vector(dx,dy,dz).
/// </summary>
	public void move(double dx, double dy, double dz) {
		this.x += dx;
		this.y += dy;
		this.z += dz;
	}

	/// <summary>
/// Move of given point <code>p</code>.
/// </summary>
	public void move(Point3 p) {
		this.x += p.x;
		this.y += p.y;
		this.z += p.z;
	}

	/// <summary>
/// Move of given vector d.
/// </summary>
	public void move(Vector3 d) {
		this.x += d.data[0];
		this.y += d.data[1];
		this.z += d.data[2];
	}

	/// <summary>
/// Move in depth of dz.
/// </summary>
	public void moveZ(double dz) {
		z += dz;
	}

	/// <summary>
/// Scale of factor (sx,sy,sz).
/// </summary>
	public void scale(double sx, double sy, double sz) {
		x *= sx;
		y *= sy;
		z *= sz;
	}

	/// <summary>
/// Scale by factor s.
/// </summary>
	public void scale(Point3 s) {
		x *= s.x;
		y *= s.y;
		z *= s.z;
	}

	/// <summary>
/// Scale by factor s.
/// </summary>
	public void scale(Vector3 s) {
		x *= s.data[0];
		y *= s.data[1];
		z *= s.data[2];
	}

	/// <summary>
/// Scale by a given scalar.
/// </summary>
/// <param name="scalar"> The multiplier.</param>
	public void scale(double scalar) {
		x *= scalar;
		y *= scalar;
		z *= scalar;
	}

	/// <summary>
/// Change only depth at absolute coordinate z.
/// </summary>
	public void setZ(double z) {
		this.z = z;
	}

	/// <summary>
/// Exchange the values of this and other.
/// </summary>
	public void swap(Point3 other) {
		double t;

		if (other != this) {
			t = this.x;
			this.x = other.x;
			other.x = t;

			t = this.y;
			this.y = other.y;
			other.y = t;

			t = this.z;
			this.z = other.z;
			other.z = t;
		}
	}

	// Commands -- misc.

	
	public string toString() {
		StringBuffer buf;

		buf = new StringBuffer("Point3[");

		buf.Append(x);
		buf.Append('|');
		buf.Append(y);
		buf.Append('|');
		buf.Append(z);
		buf.Append("]");

		return buf.ToString();
	}

	
	public bool equals(object o) {
		if (this == o) {
			return true;
		}
		if (o == null || GetType() != o.GetType()) {
			return false;
		}
		if (!base.Equals(o)) {
			return false;
		}

		Point3 point3 = (Point3) o;

		if (double.compare(point3.z, z) != 0) {
			return false;
		}

		return true;
	}

	
	public int hashCode() {
		int result = base.GetHashCode();
		long temp;
		temp = double.doubleToLongBits(z);
		result = 31 * result + (int) (temp ^ (temp >>> 32));
		return result;
	}
}
}
