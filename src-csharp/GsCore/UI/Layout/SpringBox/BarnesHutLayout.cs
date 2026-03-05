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
/// Base implementation of a Barnes-Hut space decomposition and particle interaction algorithm to be used for force-based layout algorithms. <p> This base class creates the space decomposition method and manages the main loop of the simulation. The simulation is made of {@link NodeParticle} and {@link EdgeSpring} elements that are created and linked for you in response to graph events received via the {@link Sink} interface. However you have to provide an implementation of the abstract {@link NodeParticle} class (by overriding the method {@link #newNodeParticle(String)}). </p> <p> As almost all the repulsion/attraction forces computation is done in the {@link NodeParticle} class, this is the most important one. </p> <p> This algorithm can be configured using several attributes put on the graph : <ul> <li>layout.force : a floating point number (default 0.5f), that allows to define the importance of movement of each node at each computation step. The larger the value the quicker nodes move to their position of lowest energy. However too high values can generate non stable layouts and oscillations.</li> <li>layout.quality : an integer between 0 and 4. With value 0 the layout is faster but it also can be farther from equilibrium. With value 4 the algorithm tries to be as close as possible from equilibrium (the n-tree and Barnes-Hut algorithms are disabled), but the computation can take a lot of time (the algorithm becomes O(n^2)). TODO change this into layout.barneshut or something similar.</li> </ul> You can also put the following attributes on nodes : <ul> <li>layout.weight : The force of repulsion of a node. The larger the value, the more the node repulses its neighbors.</li> </ul> And on edges : <ul> <li>layout.weight : the multiplier for the desired edge length. By default the algorithm tries to make each edge of length one. This is the position of lowest energy for a spring. This coefficient allows to modify this target spring length. Value larger than one will make the edge longer. Values between 0 and 1 will make the edge smaller.</li> <li>layout.stabilization-limit : the stabilization of a layout is a number between 0 and 1. 1 means fully stable, but this value is rare. Therefore one can consider the layout stable at a lower value. The default is 0.9. You can fix it with this attribute.</li> </ul> </p>
/// </summary>
abstract class BarnesHutLayout : SourceBase, ILayout, ParticleBoxListener {

	/// <summary>
/// class level logger
/// </summary>
	private static readonly object /* Logger */ logger = null /* Logger */;

	/// <summary>
/// The nodes representation and the n-tree. The particle-box is an implementation of a recursive space decomposition method that is used here to break the O(n^2) complexity into a Barnes-Hut algorithm that is closer to O(n log n).
/// </summary>
	protected ParticleBox nodes;

	/// <summary>
/// The set of edges.
/// </summary>
	protected Dictionary<string, EdgeSpring> edges = new Dictionary<string, EdgeSpring>();

	/// <summary>
/// Used to avoid stabilizing if an event occurred.
/// </summary>
	protected int lastElementCount = 0;

	/// <summary>
/// Random number generator.
/// </summary>
	protected Random random;

	/// <summary>
/// The lowest node position.
/// </summary>
	protected Point3 lo = new Point3(0, 0, 0);

	/// <summary>
/// The highest node position.
/// </summary>
	protected Point3 hi = new Point3(1, 1, 1);

	/// <summary>
/// The point in the middle of the layout.
/// </summary>
	protected Point3 center = new Point3(0.5, 0.5, 0.5);

	/// <summary>
/// Output stream for statistics if in debug mode.
/// </summary>
	protected System.IO.TextWriter statsOut;

	/// <summary>
/// Energy, and the history of energies.
/// </summary>
	protected Energies energies = new Energies();

	/// <summary>
/// Global force strength. This is a factor in [0..1] that is used to scale all computed displacements.
/// </summary>
	protected double force = 1f;

	/// <summary>
/// The view distance at which the cells of the n-tree are explored exhaustively, after this the poles are used. This is a multiple of k.
/// </summary>
	protected double viewZone = 5f;

	/// <summary>
/// The Barnes/Hut theta threshold to know if we use a pole or not.
/// </summary>
	protected double theta = .7f;

	/// <summary>
/// The quality level.
/// </summary>
	protected double quality = 1;

	/// <summary>
/// Number of nodes per space-cell.
/// </summary>
	protected int nodesPerCell = 10;

	/// <summary>
/// The diagonal of the graph area at the current step.
/// </summary>
	protected double area = 1;

	/// <summary>
/// The stabilization limit of this algorithm.
/// </summary>
	protected double stabilizationLimit = 0.9;

	// Attributes -- Statistics

	/// <summary>
/// Current step.
/// </summary>
	protected int time;

	/// <summary>
/// The duration of the last step in milliseconds.
/// </summary>
	protected long lastStepTime;

	/// <summary>
/// The maximum length of a node displacement at the current step.
/// </summary>
	protected double maxMoveLength;

	/// <summary>
/// Average move length.
/// </summary>
	protected double avgLength;

	/// <summary>
/// Number of nodes that moved during last step.
/// </summary>
	protected int nodeMoveCount;

	// Attributes -- Settings

	/// <summary>
/// Compute the third coordinate ?.
/// </summary>
	protected bool is3D = false;

	/// <summary>
/// The gravity factor. If set to 0 the gravity computation is disabled.
/// </summary>
	protected double gravity = 0;

	/// <summary>
/// Send node informations?.
/// </summary>
	protected bool sendNodeInfos = false;

	/// <summary>
/// If true a file is created to output the statistics of the spring box algorithm.
/// </summary>
	protected bool outputStats = false;

	/// <summary>
/// If true a file is created for each node (!!!) and its movement statistics are logged.
/// </summary>
	protected bool outputNodeStats = false;

	/// <summary>
/// If greater than one, move events are sent only every N steps.
/// </summary>
	protected int sendMoveEventsEvery = 1;

	/// <summary>
/// Sink time.
/// </summary>
	protected SinkTime sinkTime;

	/// <summary>
/// New 2D Barnes-Hut simulation.
/// </summary>
	public BarnesHutLayout() : this(false) {
	}

	/// <summary>
/// New Barnes-Hut simulation.
/// </summary>
/// <param name="is3D"> If true the simulation dimensions count is 3 else 2.</param>
	public BarnesHutLayout(bool is3D) : this(is3D, new Random(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())) {
	}

	/// <summary>
/// New Barnes-Hut simulation.
/// </summary>
/// <param name="is3D"> If true the simulation dimensions count is 3 else 2.</param>
/// <param name="randomNumberGenerator"> The random number generator to use.</param>
	public BarnesHutLayout(bool is3D, Random randomNumberGenerator) {
		CellSpace space;

		this.is3D = is3D;
		this.random = randomNumberGenerator;

		if (is3D) {
			space = new OctreeCellSpace(new Anchor(-1, -1, -1), new Anchor(1, 1, 1));
		} else {
			space = new QuadtreeCellSpace(new Anchor(-1, -1, -0.01f), new Anchor(1, 1, 0.01f));
		}

		this.nodes = new ParticleBox(nodesPerCell, space, new GraphCellData());

		nodes.addParticleBoxListener(this);
		setQuality(quality);

		sinkTime = new SinkTime();
		sourceTime.setSinkTime(sinkTime);
	}

	public Point3 getLowPoint() {
		org.miv.pherd.geom.Point3 p = nodes.getNTree().getLowestPoint();
		lo = new Point3(p.x, p.y, p.z);
		return lo;
	}

	public Point3 getHiPoint() {
		org.miv.pherd.geom.Point3 p = nodes.getNTree().getHighestPoint();
		hi = new Point3(p.x, p.y, p.z);
		return hi;
	}

	public double randomXInsideBounds() {
		org.miv.pherd.geom.Point3 c = ((GraphCellData) nodes.getNTree().getRootCell().getData()).center;
		return c.x + (random.nextDouble() * 2 - 1);
		// org.miv.pherd.geom.Point3 lo = nodes.getNTree().getLowestPoint();
		// org.miv.pherd.geom.Point3 hi = nodes.getNTree().getHighestPoint();
		// return lo.x + ((hi.x - lo.x)*random.nextDouble());
	}

	public double randomYInsideBounds() {
		org.miv.pherd.geom.Point3 c = ((GraphCellData) nodes.getNTree().getRootCell().getData()).center;
		return c.y + (random.nextDouble() * 2 - 1);
		// org.miv.pherd.geom.Point3 lo = nodes.getNTree().getLowestPoint();
		// org.miv.pherd.geom.Point3 hi = nodes.getNTree().getHighestPoint();
		// return lo.y + ((hi.y - lo.y)*random.nextDouble());
	}

	public double randomZInsideBounds() {
		org.miv.pherd.geom.Point3 c = ((GraphCellData) nodes.getNTree().getRootCell().getData()).center;
		return c.z + (random.nextDouble() * 2 - 1);
		// org.miv.pherd.geom.Point3 lo = nodes.getNTree().getLowestPoint();
		// org.miv.pherd.geom.Point3 hi = nodes.getNTree().getHighestPoint();
		// return lo.z + ((hi.z - lo.z)*random.nextDouble());
	}

	public Point3 getCenterPoint() {
		return center;
	}

	/// <summary>
/// A gravity factor that attracts all nodes to the center of the layout to avoid flying components. If set to zero, the gravity computation is disabled.
/// </summary>
/// <returns>The gravity factor, usually between 0 and 1.</returns>
	public double getGravityFactor() {
		return gravity;
	}

	/// <summary>
/// Set the gravity factor that attracts all nodes to the center of the layout to avoid flying components. If set to zero, the gravity computation is disabled.
/// </summary>
/// <param name="value"> The new gravity factor, usually between 0 and 1.</param>
	public void setGravityFactor(double value) {
		gravity = value;
	}

	/// <summary>
/// The spatial index as a n-tree.
/// </summary>
/// <returns>The n-tree.</returns>
	public ParticleBox getSpatialIndex() {
		return nodes;
	}

	public long getLastStepTime() {
		return lastStepTime;
	}

	public abstract string getLayoutAlgorithmName();

	public int getNodeMovedCount() {
		return nodeMoveCount;
	}

	public double getStabilization() {
		if (lastElementCount == nodes.getParticleCount() + edges.Count) {
			if (time > energies.getBufferSize())
				return energies.getStabilization();
		}

		lastElementCount = nodes.getParticleCount() + edges.Count;

		return 0;
	}

	public double getStabilizationLimit() {
		return stabilizationLimit;
	}

	public int getSteps() {
		return time;
	}

	public double getQuality() {
		return quality;
	}

	public bool is3D() {
		return is3D;
	}

	public double getForce() {
		return force;
	}

	public Random getRandom() {
		return random;
	}

	public Energies getEnergies() {
		return energies;
	}

	/// <summary>
/// The Barnes-Hut theta value used to know if we use a pole or not.
/// </summary>
/// <returns>The theta value (between 0 and 1).</returns>
	public double getBarnesHutTheta() {
		return theta;
	}

	public double getViewZone() {
		return viewZone;
	}

	public void setSendNodeInfos(bool on) {
		sendNodeInfos = on;
	}

	/// <summary>
/// Change the barnes-hut theta parameter allowing to know if we use a pole or not.
/// </summary>
/// <param name="theta"> The new value for theta (between 0 and 1).</param>
	public void setBarnesHutTheta(double theta) {
		if (theta > 0 && theta < 1) {
			this.theta = theta;
		}
	}

	public void setForce(double value) {
		this.force = value;
	}

	public void setStabilizationLimit(double value) {
		this.stabilizationLimit = value;
	}

	public void setQuality(double qualityLevel) {
		if (qualityLevel > 1)
			qualityLevel = 1;
		else if (qualityLevel < 0)
			qualityLevel = 0;
		quality = qualityLevel;
	}

	public void clear() {
		energies.clearEnergies();
		nodes.removeAllParticles();
		edges.Clear();
		nodeMoveCount = 0;
		lastStepTime = 0;
	}

	public void compute() {
		long t1;

		computeArea();

		maxMoveLength = double.Epsilon;
		t1 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		nodeMoveCount = 0;
		avgLength = 0;

		// All the movement computation is done in this call.
		nodes.step();

		if (nodeMoveCount > 0)
			avgLength /= nodeMoveCount;

		// Ready for the next step.

		getLowPoint();
		getHiPoint();
		center = new Point3(lo.x + (hi.x - lo.x) / 2, lo.y + (hi.y - lo.y) / 2, lo.z + (hi.z - lo.z) / 2);
		// center.set(0, 0, 0);
		energies.storeEnergy();
		printStats();
		time++;
		lastStepTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - t1;
	}

	/// <summary>
/// Output some statistics on the layout process. This method is active only if {@link #outputStats} is true.
/// </summary>
	protected void printStats() {
		if (outputStats) {
			if (statsOut == null) {
				try {
					statsOut = new System.IO.TextWriter("springBox.dat");
					statsOut.printf("# stabilization nodeMoveCount energy energyDiff maxMoveLength avgLength area%n");
					statsOut.Flush();
				} catch (FileNotFoundException e) {
					Console.Error.WriteLine(e);
				}
			}

			if (statsOut != null) {
				double energyDiff = energies.getEnergy() - energies.getPreviousEnergyValue(30);

				statsOut.printf(System.Globalization.CultureInfo.InvariantCulture, "%f %d %f %f %f %f%n", getStabilization(), nodeMoveCount,
						energies.getEnergy(), energyDiff, maxMoveLength, avgLength, area);
				statsOut.Flush();
			}
		}
	}

	protected void computeArea() {
		area = getHiPoint().distance(getLowPoint());
	}

	public void shake() {
		energies.clearEnergies();
	}

	protected NodeParticle addNode(string sourceId, string id) {
		NodeParticle np = newNodeParticle(id);
		nodes.addParticle(np);
		return np;
	}

	public void moveNode(string id, double x, double y, double z) {
		NodeParticle node = (NodeParticle) nodes.getParticle(id);

		if (node != null) {
			node.moveTo(x, y, z);
			energies.clearEnergies();
		}
	}

	public void freezeNode(string id, bool on) {
		NodeParticle node = (NodeParticle) nodes.getParticle(id);

		if (node != null) {
			node.frozen = on;
		}
	}

	protected void setNodeWeight(string id, double weight) {
		NodeParticle node = (NodeParticle) nodes.getParticle(id);

		if (node != null)
			node.setWeight(weight);
	}

	protected void removeNode(string sourceId, string id) {
		NodeParticle node = (NodeParticle) nodes.removeParticle(id);

		if (node != null) {
			node.removeNeighborEdges();
		} else {
			Console.Error.WriteLine(
					string.Format("layout {0}: cannot remove non existing node {1}\n", getLayoutAlgorithmName(), id));
		}
	}

	protected void addEdge(string sourceId, string id, string from, string to, bool directed) {
		NodeParticle n0 = (NodeParticle) nodes.getParticle(from);
		NodeParticle n1 = (NodeParticle) nodes.getParticle(to);

		if (n0 != null && n1 != null) {
			EdgeSpring e = new EdgeSpring(id, n0, n1);
			EdgeSpring o = edges[id] = e;

			if (o != null) {
				Console.Error.WriteLine(string.Format("layout {0}: edge '{1}' already exists.", getLayoutAlgorithmName(), id));
			} else {
				n0.registerEdge(e);
				n1.registerEdge(e);
			}

			chooseNodePosition(n0, n1);
		} else {
			if (n0 == null)
				Console.Error.WriteLine(string.Format("layout {0}: node '{1}' does not exist, cannot create edge {2}.",
						getLayoutAlgorithmName(), from, id));
			if (n1 == null)
				Console.Error.WriteLine(string.Format("layout {0}: node '{1}' does not exist, cannot create edge {2}.",
						getLayoutAlgorithmName(), to, id));
		}
	}

	/// <summary>
/// Choose the best position for a node that was just connected by only one edge to a cluster of nodes.
/// </summary>
/// <param name="n0"> source node of the edge.</param>
/// <param name="n1"> target node of the edge.</param>
	protected abstract void chooseNodePosition(NodeParticle n0, NodeParticle n1);

	protected void addEdgeBreakPoint(string edgeId, int points) {
		Console.Error.WriteLine(string.Format("layout {0}: edge break points are not handled yet.", getLayoutAlgorithmName()));
	}

	protected void ignoreEdge(string edgeId, bool on) {
		EdgeSpring edge = edges[edgeId];

		if (edge != null) {
			edge.ignored = on;
		}
	}

	protected void setEdgeWeight(string id, double weight) {
		EdgeSpring edge = edges[id];

		if (edge != null)
			edge.weight = weight;
	}

	protected void removeEdge(string sourceId, string id) {
		EdgeSpring e = edges.Remove(id);

		if (e != null) {
			e.node0.unregisterEdge(e);
			e.node1.unregisterEdge(e);
		} else {
			Console.Error.WriteLine(
					string.Format("layout {0}: cannot remove non existing edge {1}\n", getLayoutAlgorithmName(), id));
		}
	}

	// Particle box listener

	public void particleAdded(object id, double x, double y, double z, object mark) {
	}

	public void particleAdded(object id, double x, double y, double z) {
	}

	public void particleMarked(object id, object mark) {
	}

	public void particleMoved(object id, double x, double y, double z) {
		if ((time % sendMoveEventsEvery) == 0) {
			object xyz[] = new object[3];
			xyz[0] = x;
			xyz[1] = y;
			xyz[2] = z;

			sendNodeAttributeChanged(sourceId, (string) id, "xyz", xyz, xyz);
		}
	}

	public void particleRemoved(object id) {
	}

	public void stepFinished(int time) {
	}

	public void particleAttributeChanged(object id, string attribute, object newValue, bool removed) {
	}

	// SourceBase interface

	public void edgeAdded(string graphId, long time, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		if (sinkTime.isNewEvent(graphId, time)) {
			addEdge(graphId, edgeId, fromNodeId, toNodeId, directed);
			sendEdgeAdded(graphId, time, edgeId, fromNodeId, toNodeId, directed);
		}
	}

	public void nodeAdded(string graphId, long time, string nodeId) {
		if (sinkTime.isNewEvent(graphId, time)) {
			NodeParticle np = addNode(graphId, nodeId);
			sendNodeAdded(graphId, time, nodeId);
		}
	}

	public void edgeRemoved(string graphId, long time, string edgeId) {
		if (sinkTime.isNewEvent(graphId, time)) {
			removeEdge(graphId, edgeId);
			sendEdgeRemoved(graphId, time, edgeId);
		}
	}

	public void nodeRemoved(string graphId, long time, string nodeId) {
		if (sinkTime.isNewEvent(graphId, time)) {
			removeNode(graphId, nodeId);
			sendNodeRemoved(graphId, time, nodeId);
		}
	}

	public void graphCleared(string graphId, long time) {
		if (sinkTime.isNewEvent(graphId, time)) {
			clear();
			sendGraphCleared(graphId, time);
		}
	}

	public void stepBegins(string graphId, long time, double step) {
		if (sinkTime.isNewEvent(graphId, time)) {
			sendStepBegins(graphId, time, step);
		}
	}

	public void graphAttributeAdded(string graphId, long time, string attribute, object value) {
		if (sinkTime.isNewEvent(graphId, time)) {
			graphAttributeChanged_(graphId, attribute, null, value);
			sendGraphAttributeAdded(graphId, time, attribute, value);
		}
	}

	public void graphAttributeChanged(string graphId, long time, string attribute, object oldValue, object newValue) {
		if (sinkTime.isNewEvent(graphId, time)) {
			graphAttributeChanged_(graphId, attribute, oldValue, newValue);
			sendGraphAttributeChanged(graphId, time, attribute, oldValue, newValue);
		}
	}

	protected void graphAttributeChanged_(string graphId, string attribute, object oldValue, object newValue) {
		if (attribute.Equals("layout.force")) {
			if (newValue is IConvertible)
				setForce(((IConvertible) newValue));
			energies.clearEnergies();
		} else if (attribute.Equals("layout.quality")) {
			if (newValue is IConvertible) {
				int q = ((IConvertible) newValue);

				q = q > 4 ? 4 : q;
				q = q < 0 ? 0 : q;

				setQuality(q);
				Console.WriteLine(string.Format("layout.{0}.quality {1}.", getLayoutAlgorithmName(), q));
			}

			energies.clearEnergies();
		} else if (attribute.Equals("layout.gravity")) {
			if (newValue is IConvertible) {
				double value = ((IConvertible) newValue);
				setGravityFactor(value);
				Console.WriteLine(string.Format("layout.{0}.gravity {1}.", getLayoutAlgorithmName(), value));
			}
		} else if (attribute.Equals("layout.exact-zone")) {
			if (newValue is IConvertible) {
				double factor = ((IConvertible) newValue);

				factor = factor > 1 ? 1 : factor;
				factor = factor < 0 ? 0 : factor;

				viewZone = factor;
				Console.WriteLine(string.Format("layout.{0}.exact-zone {1} of [0..1]\n", getLayoutAlgorithmName(), viewZone));

				energies.clearEnergies();
			}
		} else if (attribute.Equals("layout.output-stats")) {
			if (newValue == null)
				outputStats = false;
			else
				outputStats = true;

			Console.WriteLine(string.Format("layout.{0}.output-stats {1}\n", getLayoutAlgorithmName(), outputStats));
		} else if (attribute.Equals("layout.stabilization-limit")) {
			if (newValue is IConvertible) {
				stabilizationLimit = ((IConvertible) newValue);
				if (stabilizationLimit > 1)
					stabilizationLimit = 1;
				else if (stabilizationLimit < 0)
					stabilizationLimit = 0;

				energies.clearEnergies();
			}
		}
	}

	public void graphAttributeRemoved(string graphId, long time, string attribute) {
		if (sinkTime.isNewEvent(graphId, time)) {
			sendGraphAttributeRemoved(graphId, time, attribute);
		}
	}

	public void nodeAttributeAdded(string graphId, long time, string nodeId, string attribute, object value) {
		if (sinkTime.isNewEvent(graphId, time)) {
			nodeAttributeChanged_(graphId, nodeId, attribute, null, value);
			sendNodeAttributeAdded(graphId, time, nodeId, attribute, value);
		}
	}

	public void nodeAttributeChanged(string graphId, long time, string nodeId, string attribute, object oldValue,
			object newValue) {
		if (sinkTime.isNewEvent(graphId, time)) {
			nodeAttributeChanged_(graphId, nodeId, attribute, oldValue, newValue);
			sendNodeAttributeChanged(graphId, time, nodeId, attribute, oldValue, newValue);
		}
	}

	protected void nodeAttributeChanged_(string graphId, string nodeId, string attribute, object oldValue,
			object newValue) {
		if (attribute.Equals("layout.weight")) {
			if (newValue is IConvertible)
				setNodeWeight(nodeId, ((IConvertible) newValue));
			else if (newValue == null)
				setNodeWeight(nodeId, 1);

			energies.clearEnergies();
		} else if (attribute.Equals("layout.frozen")) {
			freezeNode(nodeId, (newValue != null));
		} else if (attribute.Equals("xyz") || attribute.Equals("xy")) {
			double xyz[] = new double[3];
			GraphPosLengthUtils.positionFromObject(newValue, xyz);
			moveNode(nodeId, xyz[0], xyz[1], xyz[2]);
		} else if (attribute.Equals("x") && newValue is IConvertible) {
			NodeParticle node = (NodeParticle) nodes.getParticle(nodeId);
			if (node != null) {
				moveNode(nodeId, ((IConvertible) newValue), node.getPosition().y, node.getPosition().z);
			}
		} else if (attribute.Equals("y") && newValue is IConvertible) {
			NodeParticle node = (NodeParticle) nodes.getParticle(nodeId);
			if (node != null) {
				moveNode(nodeId, node.getPosition().x, ((IConvertible) newValue), node.getPosition().z);
			}
		}
	}

	public void nodeAttributeRemoved(string graphId, long time, string nodeId, string attribute) {
		if (sinkTime.isNewEvent(graphId, time)) {
			nodeAttributeChanged_(graphId, nodeId, attribute, null, null);
			sendNodeAttributeRemoved(graphId, time, nodeId, attribute);
		}
	}

	public void edgeAttributeAdded(string graphId, long time, string edgeId, string attribute, object value) {
		if (sinkTime.isNewEvent(graphId, time)) {
			edgeAttributeChanged_(graphId, edgeId, attribute, null, value);
			sendEdgeAttributeAdded(graphId, time, edgeId, attribute, value);
		}
	}

	public void edgeAttributeChanged(string graphId, long time, string edgeId, string attribute, object oldValue,
			object newValue) {
		if (sinkTime.isNewEvent(graphId, time)) {
			edgeAttributeChanged_(graphId, edgeId, attribute, oldValue, newValue);
			sendEdgeAttributeChanged(graphId, time, edgeId, attribute, oldValue, newValue);
		}
	}

	protected void edgeAttributeChanged_(string graphId, string edgeId, string attribute, object oldValue,
			object newValue) {
		if (attribute.Equals("layout.weight")) {
			if (newValue is IConvertible)
				setEdgeWeight(edgeId, ((IConvertible) newValue));
			else if (newValue == null)
				setEdgeWeight(edgeId, 1);

			energies.clearEnergies();
		} else if (attribute.Equals("layout.ignored")) {
			if (newValue is bool)
				ignoreEdge(edgeId, (bool) newValue);
			energies.clearEnergies();
		}
	}

	public void edgeAttributeRemoved(string graphId, long time, string edgeId, string attribute) {
		if (sinkTime.isNewEvent(graphId, time)) {
			edgeAttributeChanged_(graphId, edgeId, attribute, null, null);
			sendEdgeAttributeRemoved(attribute, time, edgeId, attribute);
		}
	}

	/// <summary>
/// Factory method to create node particles.
/// </summary>
/// <param name="id"> The identifier of the new node/particle.</param>
/// <returns>The new node/particle.</returns>
	public NodeParticle newNodeParticle(string id);
}
}
