using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace Org.GraphStream.UI.Layout.SpringBox
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
/// Base implementation of a node particle to be used in the {@link BarnesHutLayout} to represent nodes and choose their positions. <p> Several abstract methods have to be overrided to provide a computation of the layout (all the attraction/repulsion computation is done in this class): <ul> <li>{@link #attraction(Vector3)}</li> <li>{@link #repulsionN2(Vector3)}</li> <li>{@link #repulsionNLogN(Vector3)}</li> </ul> </p>
/// </summary>
abstract class NodeParticle : Particle {
	/// <summary>
/// Set of edge connected to this node.
/// </summary>
	public List<EdgeSpring> neighbours = new List<EdgeSpring>();

	/// <summary>
/// Should the node move?.
/// </summary>
	public bool frozen = false;

	/// <summary>
/// Displacement vector.
/// </summary>
	public Vector3 disp;

	/// <summary>
/// Last computed displacement vector length.
/// </summary>
	public double len;

	/// <summary>
/// Attraction energy for this node only.
/// </summary>
	public double attE;

	/// <summary>
/// Repulsion energy for this node only.
/// </summary>
	public double repE;

	/// <summary>
/// If non null, all this node statistics will be output to this stream.
/// </summary>
	public System.IO.TextWriter out;

	/// <summary>
/// The box.
/// </summary>
	protected BarnesHutLayout box;

	// Constructors

	/// <summary>
/// New node. The node is placed at random in the space of the simulation.
/// </summary>
/// <param name="box"> The spring box.</param>
/// <param name="id"> The node identifier.</param>
	public NodeParticle(BarnesHutLayout box, string id) {
		// this(box, id, box.getCenterPoint().x, box.getCenterPoint().y, box.is3D() ?
		// box.getCenterPoint().z : 0);
		this(box, id, box.randomXInsideBounds(), box.randomYInsideBounds(), box.is3D ? box.randomZInsideBounds() : 0);
		// this(box, id, (box.random.nextDouble() * 2) - 1, (box.random
		// .nextDouble() * 2) - 1,
		// box.is3D ? (box.random.nextDouble() * 2) - 1 : 0);

		this.box = box;
	}

	/// <summary>
/// New node at a given position.
/// </summary>
/// <param name="box"> The spring box.</param>
/// <param name="id"> The node identifier.</param>
/// <param name="x"> The abscissa.</param>
/// <param name="y"> The ordinate.</param>
/// <param name="z"> The depth.</param>
	public NodeParticle(BarnesHutLayout box, string id, double x, double y, double z) : base(id, x, y, box.is3D ? z : 0) {
		this.box = box;
		disp = new Vector3();
		createDebug();
	}

	/// <summary>
/// Create a file for statistics about this node.
/// </summary>
	protected void createDebug() {
		if (box.outputNodeStats) {
			try {
				out = new System.IO.TextWriter(new FileOutputStream("out" + getId() + ".data"));
			} catch (Exception e) {
				Console.Error.WriteLine(e);
				System.Environment.Exit(1);
			}
		}
	}

	/// <summary>
/// All the edges connected to this node.
/// </summary>
/// <returns>A set of edges.</returns>
	public ICollection<EdgeSpring> getEdges() {
		return neighbours;
	}

	
	public void move(int time) {
		if (!frozen) {
			disp.fill(0);

			Vector3 delta = new Vector3();

			repE = 0;
			attE = 0;

			if (box.viewZone < 0)
				repulsionN2(delta);
			else
				repulsionNLogN(delta);

			attraction(delta);

			if (box.gravity != 0)
				gravity(delta);

			disp.scalarMult(box.force);

			len = disp.Length;

			if (len > (box.area / 2)) {
				disp.scalarMult((box.area / 2) / len);
				len = box.area / 2;
			}

			box.avgLength += len;

			if (len > box.maxMoveLength)
				box.maxMoveLength = len;
		}
	}

	
	public void nextStep(int time) {
		if (!frozen) {
			nextPos.x = pos.x + disp.data[0];
			nextPos.y = pos.y + disp.data[1];

			if (box.is3D)
				nextPos.z = pos.z + disp.data[2];

			box.nodeMoveCount++;
			moved = true;
		} else {
			nextPos.x = pos.x;
			nextPos.y = pos.y;
			if (box.is3D)
				nextPos.z = pos.z;
		}

		if (out != null) {
			output.printf(System.Globalization.CultureInfo.InvariantCulture, "{0} {1} {2} {3}\n", getId(), len, attE, repE);
			/* output.Flush(); */
		}

		base.nextStep(time);
	}

	/// <summary>
/// Force a node to move from a given vector.
/// </summary>
/// <param name="dx"> The x component.</param>
/// <param name="dy"> The y component.</param>
/// <param name="dz"> The z component.</param>
	public void moveOf(double dx, double dy, double dz) {
		pos.set(pos.x + dx, pos.y + dy, pos.z + dz);
	}

	/// <summary>
/// Force a node to move at a given position.
/// </summary>
/// <param name="x"> The new x.</param>
/// <param name="y"> The new y.</param>
/// <param name="z"> The new z.</param>
	public void moveTo(double x, double y, double z) {
		pos.set(x, y, z);
		moved = true;
	}

	/// <summary>
/// Compute the repulsion for each other node. This is the most precise way, but the algorithm is a time hog : complexity is O(n^2).
/// </summary>
/// <param name="delta"> The computed displacement vector.</param>
	protected abstract void repulsionN2(Vector3 delta);

	/// <summary>
/// Compute the repulsion for each node in the viewing distance, and use the n-tree to find them. For a certain distance the node repulsion is computed one by one. At a larger distance the repulsion is computed using nodes barycenters.
/// </summary>
/// <param name="delta"> The computed displacement vector.</param>
	protected abstract void repulsionNLogN(Vector3 delta);

	/// <summary>
/// Compute the global attraction toward each connected node.
/// </summary>
/// <param name="delta"> The computed displacement vector.</param>
	protected abstract void attraction(Vector3 delta);

	/// <summary>
/// Compute the global attraction toward the layout center (if enabled).
/// </summary>
/// <param name="delta"> The computed displacement vector.</param>
	protected abstract void gravity(Vector3 delta);

	/// <summary>
/// The given edge is connected to this node.
/// </summary>
/// <param name="e"> The edge to connect.</param>
	public void registerEdge(EdgeSpring e) {
		neighbours.Add(e);
	}

	/// <summary>
/// The given edge is no more connected to this node.
/// </summary>
/// <param name="e"> THe edge to disconnect.</param>
	public void unregisterEdge(EdgeSpring e) {
		int i = neighbours.IndexOf(e);

		if (i >= 0) {
			neighbours.Remove(i);
		}
	}

	/// <summary>
/// Remove all edges connected to this node.
/// </summary>
	public void removeNeighborEdges() {
		List<EdgeSpring> edges = new List<EdgeSpring>(neighbours);

		foreach (EdgeSpring edge in edges)
			box.removeEdge(box.getLayoutAlgorithmName(), edge.id);

		neighbours.Clear();
	}

	
	public void inserted() {
	}

	
	public void removed() {
	}
}
}
