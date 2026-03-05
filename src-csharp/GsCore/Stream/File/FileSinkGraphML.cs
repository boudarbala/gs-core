using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.File
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


public class FileSinkGraphML : FileSinkBase {

	protected void outputEndOfFile(){
		print("</graphml>\n");
	}

	protected void outputHeader(){
		print("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
		print("<graphml xmlns=\"http://graphml.graphdrawing.org/xmlns\"\n");
		print("\t xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n");
		print("\t xsi:schemaLocation=\"http://graphml.graphdrawing.org/xmlns\n");
		print("\t   http://graphml.graphdrawing.org/xmlns/1.0/graphml.xsd\">\n");
	}

	private void print(string format, params object[] args){
		output.Write(string.Format(format, args));
	}

	
	protected void exportGraph(IGraph g) {
		Consumer<Exception> onException = Exception::printStackTrace;

		AtomicInteger attribute = new AtomicInteger(0);
		Dictionary<string, string> nodeAttributes = new Dictionary<object, object>();
		Dictionary<string, string> edgeAttributes = new Dictionary<object, object>();

		g.nodes().ToList().ForEach(n => {
			n.attributeKeys().ToList().ForEach(k => {
				if (!nodeAttributes.ContainsKey(k)) {
					object value = n.getAttribute(k);
					string type;

					if (value == null)
						return;

					string id = string.Format("attr{0}", attribute.getAndIncrement());

					if (value is bool)
						type = "bool";
					else if (value is long)
						type = "long";
					else if (value is int)
						type = "int";
					else if (value is double)
						type = "double";
					else if (value is float)
						type = "float";
					else
						type = "string";

					nodeAttributes[k] = id;

					try {
						print("\t<key id=\"%s\" for=\"node\" attr.name=\"%s\" attr.type=\"%s\"/>\n", id,
								escapeXmlString(k), type);
					} catch (Exception ex) {
						onException.accept(ex);
					}
				}
			});
		});

		g.edges().ToList().ForEach(n => {
			n.attributeKeys().ToList().ForEach(k => {
				if (!edgeAttributes.ContainsKey(k)) {
					object value = n.getAttribute(k);
					string type;

					if (value == null)
						return;

					string id = string.Format("attr{0}", attribute.getAndIncrement());

					if (value is bool)
						type = "bool";
					else if (value is long)
						type = "long";
					else if (value is int)
						type = "int";
					else if (value is double)
						type = "double";
					else if (value is float)
						type = "float";
					else
						type = "string";

					edgeAttributes[k] = id;

					try {
						print("\t<key id=\"%s\" for=\"edge\" attr.name=\"%s\" attr.type=\"%s\"/>\n", id,
								escapeXmlString(k), type);
					} catch (Exception ex) {
						onException.accept(ex);
					}
				}
			});
		});

		try {
			print("\t<graph id=\"%s\" edgedefault=\"undirected\">\n", escapeXmlString(g.getId()));
		} catch (Exception e) {
			onException.accept(e);
		}

		g.nodes().ToList().ForEach(n => {
			try {
				print("\t\t<node id=\"%s\">\n", n.getId());

				n.attributeKeys().ToList().ForEach(k => {
					try {
						print("\t\t\t<data key=\"%s\">%s</data>\n", nodeAttributes[k],
								escapeXmlString(n.getAttribute(k).ToString()));
					} catch (System.IO.IOException e) {
						onException.accept(e);
					}
				});

				print("\t\t</node>\n");
			} catch (Exception ex) {
				onException.accept(ex);
			}
		});

		g.edges().ToList().ForEach(e => {
			try {
				print("\t\t<edge id=\"%s\" source=\"%s\" target=\"%s\" directed=\"%s\">\n", e.getId(),
						e.getSourceNode().getId(), e.getTargetNode().getId(), e.isDirected());

				e.attributeKeys().ToList().ForEach(k => {
					try {
						print("\t\t\t<data key=\"%s\">%s</data>\n", edgeAttributes[k],
								escapeXmlString(e.getAttribute(k).ToString()));
					} catch (System.IO.IOException e1) {
						onException.accept(e1);
					}
				});

				print("\t\t</edge>\n");
			} catch (Exception ex) {
				onException.accept(ex);
			}
		});

		try {
			print("\t</graph>\n");
		} catch (Exception e) {
			onException.accept(e);
		}
	}

	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		throw new NotSupportedException();
	}

	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		throw new NotSupportedException();
	}

	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		throw new NotSupportedException();
	}

	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		throw new NotSupportedException();
	}

	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		throw new NotSupportedException();
	}

	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		throw new NotSupportedException();
	}

	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		throw new NotSupportedException();
	}

	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		throw new NotSupportedException();
	}

	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		throw new NotSupportedException();
	}

	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		throw new NotSupportedException();
	}

	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		throw new NotSupportedException();
	}

	public void graphCleared(string sourceId, long timeId) {
		throw new NotSupportedException();
	}

	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		throw new NotSupportedException();
	}

	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		throw new NotSupportedException();
	}

	public void stepBegins(string sourceId, long timeId, double step) {
		throw new NotSupportedException();
	}

	private static string escapeXmlString(string string) {
		/*
		 * Thankfully, the unescaping part is done by the xml parser used in
		 * FileSourceGraphML
		 */
		return string.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;")
				.Replace("'", "&apos;");
	}
}

}
