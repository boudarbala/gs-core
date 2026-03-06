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


public class RMISink : object /* RMIAdapterOut */, ISink {
	private static readonly long serialVersionUID = 23444722897331612L;

	Dictionary<string, object /* RMIAdapterIn */> inputs;

	public RMISink(): base() {
		inputs = new Dictionary<string, object /* RMIAdapterIn */>();
	}

	public RMISink(string name): this() {
		bind(name);
	}

	public void bind(string name) {
		try {
			Naming.Rebind(name, this);
		} catch (Exception e) {
			Console.Error.WriteLine(e);
		}
	}

	public void register(string url){
		try {
			object /* RMIAdapterIn */ input = (object /* RMIAdapterIn */) null /* RMI Naming.lookup not available */;

			if (input != null)
				inputs[url] = input;
		} catch (Exception e) {
			Console.Error.WriteLine(e);
		}
	}

	public void unregister(string url){
		if (inputs.ContainsKey(url))
			inputs.Remove(url);
	}

	public void edgeAttributeAdded(string graphId, long timeId, string edgeId, string attribute, object value) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.edgeAttributeAdded(graphId, timeId, edgeId, attribute, value);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void edgeAttributeChanged(string graphId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.edgeAttributeChanged(graphId, timeId, edgeId, attribute, oldValue, newValue);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void edgeAttributeRemoved(string graphId, long timeId, string edgeId, string attribute) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.edgeAttributeRemoved(graphId, timeId, edgeId, attribute);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void graphAttributeAdded(string graphId, long timeId, string attribute, object value) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.graphAttributeAdded(graphId, timeId, attribute, value);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void graphAttributeChanged(string graphId, long timeId, string attribute, object oldValue, object newValue) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.graphAttributeChanged(graphId, timeId, attribute, oldValue, newValue);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void graphAttributeRemoved(string graphId, long timeId, string attribute) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.graphAttributeRemoved(graphId, timeId, attribute);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void nodeAttributeAdded(string graphId, long timeId, string nodeId, string attribute, object value) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.nodeAttributeAdded(graphId, timeId, nodeId, attribute, value);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void nodeAttributeChanged(string graphId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.nodeAttributeChanged(graphId, timeId, nodeId, attribute, oldValue, newValue);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void nodeAttributeRemoved(string graphId, long timeId, string nodeId, string attribute) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.nodeAttributeRemoved(graphId, timeId, nodeId, attribute);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void edgeAdded(string graphId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.edgeAdded(graphId, timeId, edgeId, fromNodeId, toNodeId, directed);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void edgeRemoved(string graphId, long timeId, string edgeId) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.edgeRemoved(graphId, timeId, edgeId);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void graphCleared(string graphId, long timeId) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.graphCleared(graphId, timeId);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void nodeAdded(string graphId, long timeId, string nodeId) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.nodeAdded(graphId, timeId, nodeId);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void nodeRemoved(string graphId, long timeId, string nodeId) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.nodeRemoved(graphId, timeId, nodeId);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}

	public void stepBegins(string graphId, long timeId, double step) {
		foreach (object /* RMIAdapterIn */ input in ((inputs[])Enum.GetValues(typeof(inputs)))) {
			try {
				input.stepBegins(graphId, timeId, step);
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}
	}
}
}
