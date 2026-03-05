using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.Layout.SpringBox.Implementations
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


public class LinLog : BarnesHutLayout {
	protected double k = 1;

	/// <summary>
/// Default general attraction factor.
/// </summary>
	protected double aFactor = 1f;

	/// <summary>
/// Default general repulsion factor.
/// </summary>
	protected double rFactor = 1f;

	protected bool edgeBased = true;

	protected double maxR = 0.5;

	protected double a = 0;

	protected double r = -1.2;

	// protected

	/// <summary>
/// New "LinLog" 2D Barnes-Hut simulation.
/// </summary>
	public LinLog() : this(false) {
	}

	/// <summary>
/// New "LinLog" Barnes-Hut simulation.
/// </summary>
/// <param name="is3D"> If true the simulation dimensions count is 3 else 2.</param>
	public LinLog(bool is3D) : this(is3D, new Random(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())) {
	}

	/// <summary>
/// New "LinLog" Barnes-Hut simulation.
/// </summary>
/// <param name="is3D"> If true the simulation dimensions count is 3 else 2.</param>
/// <param name="randomNumberGenerator"> The random number generator to use.</param>
	public LinLog(bool is3D, Random randomNumberGenerator) : base(is3D, randomNumberGenerator) {
		setQuality(1);
		force = 3;
	}

	public void configure(double a, double r, bool edgeBased, double force) {
		this.a = a;
		this.r = r;
		this.edgeBased = edgeBased;
		this.force = force;
	}

	
	public string getLayoutAlgorithmName() {
		return "LinLog";
	}

	
	public void setQuality(double qualityLevel) {
		base.setQuality(qualityLevel);

		if (quality >= 1) {
			viewZone = -1;
		} else {
			viewZone = k;
		}
	}

	
	public void compute() {
		if (viewZone > 0)
			viewZone = area / 1.5;
		base.compute();
	}

	
	protected void chooseNodePosition(NodeParticle n0, NodeParticle n1) {
		// double delta = k * 0.1;
		// if (n0.getEdges().Count == 1 && n1.getEdges().Count > 1) {
		// org.miv.pherd.geom.Point3 pos = n1.getPosition();
		// n0.moveTo(pos.x + delta, pos.y + delta, pos.z + delta);
		// } else if (n1.getEdges().Count == 1 && n0.getEdges().Count > 1) {
		// org.miv.pherd.geom.Point3 pos = n0.getPosition();
		// n1.moveTo(pos.x + delta, pos.y + delta, pos.z + delta);
		// }
	}

	
	public NodeParticle newNodeParticle(string id) {
		return new LinLogNodeParticle(this, id);
	}
}
}
