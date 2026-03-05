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
/// 2D point. A Point2 is a 2D location in an affine space described by three values along the X, and Y axes. This differs from the Vector3 and Vector4 classes in that it is only 2D and has no vector arithmetic bound to it (to points cannot be added, this would have no mathematical meaning).
/// </summary>
public class Point2 : java.io.System.Runtime.Serialization.ISerializable {
	// Attributes

	private static readonly long serialVersionUID = 965985679540486895L;

	/// <summary>
/// X axis value.
/// </summary>
	public double x;

	/// <summary>
/// Y axis value.
/// </summary>
	public double y;

	// Attributes -- Shared

	/// <summary>
/// Specific point at (0,0).
/// </summary>
	public static readonly Point2 NULL_POINT2 = new Point2(0, 0);

	// Constructors

	/// <summary>
/// New 2D point at (0,0).
/// </summary>
	public Point2() {
	}

	/// <summary>
/// New 2D point at (x,y).
/// </summary>
	public Point2(double x, double y) {
		set(x, y);
	}

	/// <summary>
/// New copy of other.
/// </summary>
	public Point2(Point2 other) {
		copy(other);
	}

	/// <summary>
/// New 2D point at (x,y).
/// </summary>
	public void make(double x, double y) {
		set(x, y);
	}

	// Accessors

	/// <summary>
/// Are all components to zero?.
/// </summary>
	public bool isZero() {
		return (x == 0 && y == 0);
	}

	// /// <summary>
/// // * Is other equal to this ? //
/// </summary>
	// public boolean
	// equals( const Point2 < double > & other ) const
	// {
	// return( x == other.x
	// and y == other.y
	// and z == other.z );
	// }

	/// <summary>
/// Create a new point linear interpolation of this and <code>other</code>. The new point is located between this and <code>other</code> if <code>factor</code> is between 0 and 1 (0 yields this point, 1 yields the <code>other</code> point).
/// </summary>
	public Point2 interpolate(Point2 other, double factor) {
		Point2 p = new Point2(x + ((other.x - x) * factor), y + ((other.y - y) * factor));

		return p;
	}

	/// <summary>
/// Distance between this and <code>other</code>.
/// </summary>
	public double distance(Point2 other) {
		double xx = other.x - x;
		double yy = other.y - y;
		return Math.Abs(Math.Sqrt((xx * xx) + (yy * yy)));
	}

	// Commands

	/// <summary>
/// Make this a copy of other.
/// </summary>
	public void copy(Point2 other) {
		x = other.x;
		y = other.y;
	}

	/// <summary>
/// Like #moveTo().
/// </summary>
	public void set(double x, double y) {
		this.x = x;
		this.y = y;
	}

	// Commands -- moving

	/// <summary>
/// Move to absolute position (x,y).
/// </summary>
	public void moveTo(double x, double y) {
		this.x = x;
		this.y = y;
	}

	/// <summary>
/// Move of given vector (dx,dy).
/// </summary>
	public void move(double dx, double dy) {
		this.x += dx;
		this.y += dy;
	}

	/// <summary>
/// Move of given point <code>p</code>.
/// </summary>
	public void move(Point2 p) {
		this.x += p.x;
		this.y += p.y;
	}

	/// <summary>
/// Move horizontally of dx.
/// </summary>
	public void moveX(double dx) {
		x += dx;
	}

	/// <summary>
/// Move vertically of dy.
/// </summary>
	public void moveY(double dy) {
		y += dy;
	}

	/// <summary>
/// Scale of factor (sx,sy).
/// </summary>
	public void scale(double sx, double sy) {
		x *= sx;
		y *= sy;
	}

	/// <summary>
/// Scale by factor s.
/// </summary>
	public void scale(Point2 s) {
		x *= s.x;
		y *= s.y;
	}

	/// <summary>
/// Change only abscissa at absolute coordinate x.
/// </summary>
	public void setX(double x) {
		this.x = x;
	}

	/// <summary>
/// Change only ordinate at absolute coordinate y.
/// </summary>
	public void setY(double y) {
		this.y = y;
	}

	/// <summary>
/// Exchange the values of this and other.
/// </summary>
	public void swap(Point2 other) {
		double t;

		if (other != this) {
			t = this.x;
			this.x = other.x;
			other.x = t;

			t = this.y;
			this.y = other.y;
			other.y = t;
		}
	}

	// Commands -- misc.

	
	public string toString() {
		StringBuffer buf;

		buf = new StringBuffer("Point2[");

		buf.Append(x);
		buf.Append('|');
		buf.Append(y);
		buf.Append("]");

		return buf.ToString();
	}

	
	public bool equals(object o) {
		if (this == o) {
			return true;
		}
		if (o == null || getClass() != o.GetType()) {
			return false;
		}

		Point2 point2 = (Point2) o;

		if (double.compare(point2.x, x) != 0) {
			return false;
		}
		if (double.compare(point2.y, y) != 0) {
			return false;
		}

		return true;
	}

	
	public int hashCode() {
		int result;
		long temp;
		temp = double.doubleToLongBits(x);
		result = (int) (temp ^ (temp >>> 32));
		temp = double.doubleToLongBits(y);
		result = 31 * result + (int) (temp ^ (temp >>> 32));
		return result;
	}
}
}
