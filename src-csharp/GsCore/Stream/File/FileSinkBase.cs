using System.Collections.Generic;
using System.IO;
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


/// <summary>
/// Base implementation for graph output to files. <p> <p> This class provides base services to write graphs into files using a specific file format. It allows to create an output stream. By default a print stream for easy text output, but binary files are possible. </p> <p> <p> It handles completely the {@link #writeAll(Graph, OutputStream)} {@link #writeAll(Graph, String)} {@link #begin(OutputStream)} {@link #begin(String)} {@link #flush()} and {@link #end()} methods. You should not have to modify or override these. </p> <p> <p> In order to implement an output you have to: <ul> <li>Eventually override {@link #createWriter(OutputStream)} or {@link #createWriter(String)} to replace the default instance of PrintStream created for you.</li> <li>Implement the {@link #outputHeader()} method. This method is called at start, before any graph event is sent to output. Use it to output the header of your file.</li> <li>Implement the {@link #outputEndOfFile()} method. This method is called at the end of the output, just before closing the output stream. Use it to output any terminating syntax for the file format you implement.</li> <li>Implement all the methods of {@link org.graphstream.stream.Sink}. All these methods will be called for each graph event and must export these events to the file you are writing. You should use the {@link #output} field to write to the file. This field has type {@link java.io.OutputStream} but by default is of type {@link java.io.PrintStream}, as most of the file format will be textual.</li> </ul> </p>
/// </summary>
abstract class FileSinkBase : IFileSink {
	// Attribute

	/// <summary>
/// The output.
/// </summary>
	protected System.IO.TextWriter output;

	// Command

	public void writeAll(IGraph graph, string fileName){
		begin(fileName);
		exportGraph(graph);
		end();
	}

	public void writeAll(IGraph graph, System.IO.Stream stream){
		begin(stream);
		exportGraph(graph);
		end();
	}

	public void writeAll(IGraph graph, System.IO.TextWriter writer){
		begin(writer);
		exportGraph(graph);
		end();
	}

	/// <summary>
/// Echo each element and attribute of the graph to the actual output. <p> The elements are echoed as add events (add node, add edge, add attribute). This method guarantees there are no change or delete events.
/// </summary>
/// <param name="graph"> The graph to export.</param>
	protected void exportGraph(IGraph graph) {
		string graphId = graph.getId();
		AtomicLong timeId = new AtomicLong(0);

		graph.attributeKeys()
				.ToList().ForEach(key => graphAttributeAdded(graphId, timeId.getAndIncrement(), key, graph.getAttribute(key)));

		graph.nodes().ToList().ForEach(node => {
			string nodeId = node.getId();
			nodeAdded(graphId, timeId.getAndIncrement(), nodeId);

			node.attributeKeys().ToList().ForEach(
					key => nodeAttributeAdded(graphId, timeId.getAndIncrement(), nodeId, key, node.getAttribute(key)));
		});

		graph.edges().ToList().ForEach(edge => {
			string edgeId = edge.getId();
			edgeAdded(graphId, timeId.getAndIncrement(), edgeId, edge.getNode0().getId(), edge.getNode1().getId(),
					edge.isDirected());

			edge.attributeKeys().ToList().ForEach(
					key => edgeAttributeAdded(graphId, timeId.getAndIncrement(), edgeId, key, edge.getAttribute(key)));
		});
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#begin(java.lang.String)
	 */
	public void begin(string fileName){
		if (output != null)
			throw new System.IO.IOException("cannot call begin() twice without calling end() before.");

		output = createWriter(fileName);

		outputHeader();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#begin(java.io.OutputStream)
	 */
	public void begin(System.IO.Stream stream){
		if (output != null)
			throw new System.IO.IOException("cannot call begin() twice without calling end() before.");

		output = createWriter(stream);

		outputHeader();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#begin(java.io.Writer)
	 */
	public void begin(System.IO.TextWriter writer){
		if (output != null)
			throw new System.IO.IOException("cannot call begin() twice without calling end() before.");

		output = createWriter(writer);

		outputHeader();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#flush()
	 */
	public void flush(){
		if (output != null)
			/* output.Flush(); */
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#end()
	 */
	public void end(){
		outputEndOfFile();
		/* output.Flush(); */
		output.Close();
		output = null;
	}

	/// <summary>
/// Method called at start just after the {@link #output} field is created. Use it to output the header of the file. If any I/O error occurs.
/// </summary>
	protected abstract void outputHeader();

	/// <summary>
/// Method called at the end just before the {@link #output} field is flushed and closed. Use it to output any information that closes the file. If any I/O error occurs.
/// </summary>
	protected abstract void outputEndOfFile();

	/// <summary>
/// Create a a writer from a file name. Override this method if the default PrintWriter does not suits your needs. This method is called by {@link #begin(String)} and {@link #writeAll(Graph, String)}. If any I/O error occurs.
/// </summary>
/// <param name="fileName"> Name of the file to output to.</param>
/// <returns>A new writer.</returns>
	protected System.IO.TextWriter createWriter(string fileName){
		return new System.IO.StreamWriter(fileName);
	}

	/// <summary>
/// Create a writer from an existing output stream. Override this method if the default PrintWriter does not suits your needs. This method is called by {@link #begin(OutputStream)} and {@link #writeAll(Graph, OutputStream)}. This method does not create an output stream if the given stream is already instance of PrintStream. If any I/O error occurs.
/// </summary>
/// <param name="stream"> An already existing output stream.</param>
/// <returns>A new writer.</returns>
	protected System.IO.TextWriter createWriter(System.IO.Stream stream){
		return new System.IO.StreamWriter(stream);
	}

	/// <summary>
/// Create a writer from an existing writer. Override this method if the default PrintWriter does not suits your needs. This method is called by {@link #begin(Writer)} and {@link #writeAll(Graph, Writer)}. This method does not create a new writer if the given writer is already instance of PrintWriter. If any I/O error occurs.
/// </summary>
/// <param name="writer"> An already existing writer.</param>
/// <returns>A new writer.</returns>
	protected System.IO.TextWriter createWriter(System.IO.TextWriter writer){
		if (writer is System.IO.StreamWriter)
			return writer;

		return new System.IO.StreamWriter(writer);
	}
}
}
