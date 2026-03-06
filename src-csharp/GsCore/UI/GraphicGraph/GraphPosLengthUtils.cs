using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.GraphicGraph
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
/// Lots of small often used measuring algorithms on graphs. <p> Use this class with a static import. </p>
/// </summary>
public class GraphPosLengthUtils {

	/// <summary>
/// class level logger
/// </summary>
	private static readonly object /* Logger */ logger = null /* Logger */;

	/// <summary>
/// Retrieve a node position from its attributes ("x", "y", "z", or "xy", or "xyz").
/// </summary>
/// <param name="id"> The node identifier.</param>
/// <returns>A newly allocated array of three floats containing the (x,y,z) position of the node, or null if the node is not part of the graph.</returns>
	public static double[] nodePosition(IGraph graph, string id) {
		INode node = graph.getNode(id);

		if (node != null)
			return nodePosition(node);

		return null;
	}

	/// <summary>
/// Retrieve a node position from its attributes ("x", "y", "z", or "xy", or "xyz").
/// </summary>
/// <param name="id"> The node identifier.</param>
/// <returns>A newly allocated point containing the (x,y,z) position of the node, or null if the node is not part of the graph.</returns>
	public static Point3 nodePointPosition(IGraph graph, string id) {
		INode node = graph.getNode(id);

		if (node != null)
			return nodePointPosition(node);

		return null;
	}

	/// <summary>
/// Like {@link #nodePosition(Graph,String)} but use an existing node as argument.
/// </summary>
/// <param name="node"> The node to consider.</param>
/// <returns>A newly allocated array of three floats containing the (x,y,z) position of the node.</returns>
	public static double[] nodePosition(INode node) {
		double[] xyz = new double[3];

		nodePosition(node, xyz);

		return xyz;
	}

	/// <summary>
/// Like {@link #nodePointPosition(Graph,String)} but use an existing node as argument.
/// </summary>
/// <param name="node"> The node to consider.</param>
/// <returns>A newly allocated point containing the (x,y,z) position of the node.</returns>
	public static Point3 nodePointPosition(INode node) {
		return nodePosition(node, new Point3());
	}

	/// <summary>
/// Like {@link #nodePosition(Graph,String)}, but instead of returning a newly allocated array, fill up the array given as parameter. This array must have at least three cells. If the node with the given identifier does not exist.
/// </summary>
/// <param name="id"> The node identifier.</param>
/// <param name="xyz"> An array of at least three cells.</param>
	public static void nodePosition(IGraph graph, string id, double xyz[]) {
		INode node = graph.getNode(id);

		if (node != null)
			nodePosition(node, xyz);

		throw new Exception("node '" + id + "' does not exist");
	}

	/// <summary>
/// Like {@link #nodePointPosition(Graph,String)}, but instead of returning a newly allocated array, fill up the array given as parameter. This array must have at least three cells. If the node with the given identifier does not exist.
/// </summary>
/// <param name="id"> The node identifier.</param>
/// <param name="pos"> A point that will receive the node position.</param>
	public static Point3 nodePosition(IGraph graph, string id, Point3 pos) {
		INode node = graph.getNode(id);

		if (node != null)
			return nodePosition(node, pos);

		throw new Exception("node '" + id + "' does not exist");
	}

	/// <summary>
/// Like {@link #nodePosition(Graph,String,double[])} but use an existing node as argument.
/// </summary>
/// <param name="node"> The node to consider.</param>
/// <param name="xyz"> An array of at least three cells.</param>
	public static void nodePosition(INode node, double xyz[]) {
		if (xyz.Length < 3)
			return;

		if (node.hasAttribute("xyz") || node.hasAttribute("xy")) {
			object o = node.getAttribute("xyz");

			if (o == null)
				o = node.getAttribute("xy");

			if (o != null) {
				positionFromObject(o, xyz);
			}

		} else if (node.hasAttribute("x")) {
			xyz[0] = (double) node.getNumber("x");

			if (node.hasAttribute("y"))
				xyz[1] = (double) node.getNumber("y");

			if (node.hasAttribute("z"))
				xyz[2] = (double) node.getNumber("z");
		}
	}

	/// <summary>
/// Like {@link #nodePosition(Graph,String,Point3)} but use an existing node as argument.
/// </summary>
/// <param name="node"> The node to consider.</param>
/// <param name="pos"> A point that will serve as the default position if node doesn't have position</param>
	public static Point3 nodePosition(INode node, Point3 pos) {
		if (node.hasAttribute("xyz") || node.hasAttribute("xy")) {
			object o = node.getAttribute("xyz");

			if (o == null)
				o = node.getAttribute("xy");

			if (o != null) {
				return positionFromObject(o, pos);
			}
		} else if (node.hasAttribute("x")) {
			double x = node.getNumber("x");
			double y;
			double z;

			if (node.hasAttribute("y"))
				y = node.getNumber("y");
			else
				y = pos.y;

			if (node.hasAttribute("z"))
				z = node.getNumber("z");
			else
				z = pos.z;

			return new Point3(x, y, z);
		}
		return pos;
	}

	/// <summary>
/// Try to convert an object to a position. The object can be an array of numbers, an array of base numeric types or their object counterparts.
/// </summary>
/// <param name="o"> The object to try to convert.</param>
/// <param name="xyz"> The result.</param>
	public static void positionFromObject(object o, double xyz[]) {
		if (o is object[]) {
			object[] oo = (object[]) o;

			if (oo.Length > 0 && oo[0] is IConvertible) {
				xyz[0] = ((IConvertible) oo[0]);
				if (oo.Length > 1)
					xyz[1] = ((IConvertible) oo[1]);
				if (oo.Length > 2)
					xyz[2] = ((IConvertible) oo[2]);
			}
		} else if (o is double[]) {
			double[] oo = (double[]) o;
			if (oo.Length > 0)
				xyz[0] = oo[0];
			if (oo.Length > 1)
				xyz[1] = oo[1];
			if (oo.Length > 2)
				xyz[2] = oo[2];
		} else if (o is float[]) {
			float[] oo = (float[]) o;
			if (oo.Length > 0)
				xyz[0] = oo[0];
			if (oo.Length > 1)
				xyz[1] = oo[1];
			if (oo.Length > 2)
				xyz[2] = oo[2];
		} else if (o is int[]) {
			int[] oo = (int[]) o;
			if (oo.Length > 0)
				xyz[0] = oo[0];
			if (oo.Length > 1)
				xyz[1] = oo[1];
			if (oo.Length > 2)
				xyz[2] = oo[2];
		} else if (o is double[]) {
			double[] oo = (double[]) o;
			if (oo.Length > 0)
				xyz[0] = oo[0];
			if (oo.Length > 1)
				xyz[1] = oo[1];
			if (oo.Length > 2)
				xyz[2] = oo[2];
		} else if (o is float[]) {
			float[] oo = (float[]) o;
			if (oo.Length > 0)
				xyz[0] = oo[0];
			if (oo.Length > 1)
				xyz[1] = oo[1];
			if (oo.Length > 2)
				xyz[2] = oo[2];
		} else if (o is int[]) {
			int[] oo = (int[]) o;
			if (oo.Length > 0)
				xyz[0] = oo[0];
			if (oo.Length > 1)
				xyz[1] = oo[1];
			if (oo.Length > 2)
				xyz[2] = oo[2];
		} else if (o is IConvertible[]) {
			IConvertible[] oo = (IConvertible[]) o;
			if (oo.Length > 0)
				xyz[0] = oo[0];
			if (oo.Length > 1)
				xyz[1] = oo[1];
			if (oo.Length > 2)
				xyz[2] = oo[2];
		} else if (o is Point3) {
			Point3 oo = (Point3) o;
			xyz[0] = oo.x;
			xyz[1] = oo.y;
			xyz[2] = oo.z;
		} else if (o is Point2) {
			Point2 oo = (Point2) o;
			xyz[0] = oo.x;
			xyz[1] = oo.y;
			xyz[2] = 0;
		} else {
			Console.Error.WriteLine(string.Format("Do not know how to handle xyz attribute {0}.", o.GetType().Name));
		}
	}

	/// <summary>
/// Try to convert an object to a position. The object can be an array of numbers, an array of base numeric types or their object counterparts.
/// </summary>
/// <param name="o"> The object to try to convert.</param>
/// <param name="pos"> The default position if object doesn't have position data.</param>
	public static Point3 positionFromObject(object o, Point3 pos) {
		double x = pos.x, y = pos.y, z = pos.z;
		if (o is object[]) {
			object[] oo = (object[]) o;

			if (oo.Length > 0 && oo[0] is IConvertible) {
				x = ((IConvertible) oo[0]);
				if (oo.Length > 1)
					y = ((IConvertible) oo[1]);
				if (oo.Length > 2)
					z = ((IConvertible) oo[2]);
			}
		} else if (o is double[]) {
			double[] oo = (double[]) o;
			if (oo.Length > 0)
				x = oo[0];
			if (oo.Length > 1)
				y = oo[1];
			if (oo.Length > 2)
				z = oo[2];
		} else if (o is float[]) {
			float[] oo = (float[]) o;
			if (oo.Length > 0)
				x = oo[0];
			if (oo.Length > 1)
				y = oo[1];
			if (oo.Length > 2)
				z = oo[2];
		} else if (o is int[]) {
			int[] oo = (int[]) o;
			if (oo.Length > 0)
				x = oo[0];
			if (oo.Length > 1)
				y = oo[1];
			if (oo.Length > 2)
				z = oo[2];
		} else if (o is double[]) {
			double[] oo = (double[]) o;
			if (oo.Length > 0)
				x = oo[0];
			if (oo.Length > 1)
				y = oo[1];
			if (oo.Length > 2)
				z = oo[2];
		} else if (o is float[]) {
			float[] oo = (float[]) o;
			if (oo.Length > 0)
				x = oo[0];
			if (oo.Length > 1)
				y = oo[1];
			if (oo.Length > 2)
				z = oo[2];
		} else if (o is int[]) {
			int[] oo = (int[]) o;
			if (oo.Length > 0)
				x = oo[0];
			if (oo.Length > 1)
				y = oo[1];
			if (oo.Length > 2)
				z = oo[2];
		} else if (o is IConvertible[]) {
			IConvertible[] oo = (IConvertible[]) o;
			if (oo.Length > 0)
				x = oo[0];
			if (oo.Length > 1)
				y = oo[1];
			if (oo.Length > 2)
				z = oo[2];
		} else if (o is Point3) {
			Point3 oo = (Point3) o;
			x = oo.x;
			y = oo.y;
			z = oo.z;
		} else if (o is Point2) {
			Point2 oo = (Point2) o;
			x = oo.x;
			y = oo.y;
			z = 0;
		} else {
			Console.Error.WriteLine(string.Format("Do not know how to handle xyz attribute {0}\n", o.GetType().Name));
		}
		return new Point3(x, y, z);
	}

	/// <summary>
/// Compute the edge length of the given edge according to its two nodes positions. If the edge cannot be found.
/// </summary>
/// <param name="id"> The identifier of the edge.</param>
/// <returns>The edge length or -1 if the nodes of the edge have no positions.</returns>
	public static double edgeLength(IGraph graph, string id) {
		IEdge edge = graph.getEdge(id);

		if (edge != null)
			return edgeLength(edge);

		throw new Exception("edge '" + id + "' cannot be found");
	}

	/// <summary>
/// Like {@link #edgeLength(Graph,String)} but use an existing edge as argument.
/// </summary>
/// <param name="edge"></param>
/// <returns>The edge length or -1 if the nodes of the edge have no positions.</returns>
	public static double edgeLength(IEdge edge) {
		double[] xyz0 = nodePosition(edge.getNode0());
		double[] xyz1 = nodePosition(edge.getNode1());

		if (xyz0 == null || xyz1 == null)
			return -1;

		xyz0[0] = xyz1[0] - xyz0[0];
		xyz0[1] = xyz1[1] - xyz0[1];
		xyz0[2] = xyz1[2] - xyz0[2];

		return Math.Sqrt(xyz0[0] * xyz0[0] + xyz0[1] * xyz0[1] + xyz0[2] * xyz0[2]);
	}
}
}
