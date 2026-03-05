using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.Rmi
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


public interface object /* RMIAdapterIn */ : object /* Remote */ {
	void edgeAttributeAdded(string graphId, long timeId, string edgeId, string attribute, object value);

	void edgeAttributeChanged(string graphId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue);

	void edgeAttributeRemoved(string graphId, long timeId, string edgeId, string attribute);

	void graphAttributeAdded(string graphId, long timeId, string attribute, object value);

	void graphAttributeChanged(string graphId, long timeId, string attribute, object oldValue, object newValue);

	void graphAttributeRemoved(string graphId, long timeId, string attribute);

	void nodeAttributeAdded(string graphId, long timeId, string nodeId, string attribute, object value);

	void nodeAttributeChanged(string graphId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue);

	void nodeAttributeRemoved(string graphId, long timeId, string nodeId, string attribute);

	void edgeAdded(string graphId, long timeId, string edgeId, string fromNodeId, string toNodeId, bool directed);

	void edgeRemoved(string graphId, long timeId, string edgeId);

	void graphCleared(string graphId, long timeId);

	void nodeAdded(string graphId, long timeId, string nodeId);

	void nodeRemoved(string graphId, long timeId, string nodeId);

	void stepBegins(string graphId, long timeId, double step);
}

}
