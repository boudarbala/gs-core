using System.Collections.Generic;
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
/// A N-Tree cell data that both compute the barycenter of each cell (aggregate position), the aggregate weight of each cell (sum of all of the cell node weights) and the aggregate degree of each cell (sum of all of the cell node degree).
/// </summary>
public class GraphCellData : BarycenterCellData {
	/// <summary>
/// Aggregate degree. The sum of the degrees of each node aggregated in this barycenter.
/// </summary>
	public double degree;

	/// <summary>
/// Aggregate degree. The sum of the degrees of each node aggregated in this barycenter.
/// </summary>
	public double getDegree() {
		return degree;
	}

	
	public CellData newCellData() {
		return new GraphCellData();
	}

	
	public void recompute() {
		double x = 0;
		double y = 0;
		double z = 0;
		double n = 0;

		weight = 0;
		degree = 0;

		if (cell.isLeaf()) {
			IEnumerator<Particle> particles = cell.getParticles();

			while (particles.MoveNext()) {
				NodeParticle particle = (NodeParticle) particles.next();

				x += particle.getPosition().x;
				y += particle.getPosition().y;
				z += particle.getPosition().z;

				weight += particle.getWeight();
				degree += particle.getEdges().Count;

				n++;
			}

			if (n > 0) {
				x /= n;
				y /= n;
				z /= n;
			}

			center.set(x, y, z);
		} else {
			double subcnt = cell.getSpace().getDivisions();
			double totpop = cell.getPopulation();
			int verif = 0;

			if (totpop > 0) {
				for (int i = 0; i < subcnt; ++i) {
					Cell subcell = cell.getSub(i);
					GraphCellData data = (GraphCellData) subcell.getData();
					double pop = subcell.getPopulation();

					verif += pop;

					x += data.center.x * pop;
					y += data.center.y * pop;
					z += data.center.z * pop;

					weight += data.weight;
					degree += data.degree;
				}

				System.Diagnostics.Debug.Assert(verif == totpop, "Discrepancy in population counts ?");

				x /= totpop;
				y /= totpop;
				z /= totpop;
			}

			center.set(x, y, z);
		}

		foreach (NTreeListener listener in cell.getTree().getListeners()) {
			listener.cellData(cell.getId(), "barycenter", this);
		}
	}
}
}
