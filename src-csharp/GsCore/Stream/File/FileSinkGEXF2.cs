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


public class FileSinkGEXF2 : PipeBase, IFileSink {
	class Context {
		GEXF gexf;
		System.IO.TextWriter output;
		SmartXMLWriter stream;
		bool closeStreamAtEnd;
	}

	Context currentContext;

	Context createContext(string fileName){
		System.IO.StreamWriter w = new System.IO.StreamWriter(fileName);
		Context ctx = createContext(w);
		ctx.closeStreamAtEnd = true;

		return ctx;
	}

	Context createContext(System.IO.Stream output){
		System.IO.StreamWriter w = new System.IO.StreamWriter(output);
		return createContext(w);
	}

	Context createContext(System.IO.TextWriter w){
		Context ctx = new Context();

		ctx.output = w;
		ctx.closeStreamAtEnd = false;
		ctx.gexf = new GEXF();

		try {
			ctx.stream = new SmartXMLWriter(w, true);
		} catch (Exception e) {
			throw new System.IO.IOException(e);
		}

		return ctx;
	}

	protected void export(Context ctx, IGraph g){
		ctx.gexf.disable(GEXF.Extension.DYNAMICS);

		GraphReplay replay = new GraphReplay("replay");
		replay.addSink(ctx.gexf);
		replay.replay(g);

		try {
			ctx.gexf.export(ctx.stream);
			ctx.stream.Close();

			if (ctx.closeStreamAtEnd)
				ctx.output.Close();
		} catch (Exception e) {
			throw new System.IO.IOException(e);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.file.FileSink#writeAll(org.graphstream.graph.Graph ,
	 * java.lang.String)
	 */
	public void writeAll(IGraph graph, string fileName){
		Context ctx = createContext(fileName);
		export(ctx, graph);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.file.FileSink#writeAll(org.graphstream.graph.Graph ,
	 * java.io.OutputStream)
	 */
	public void writeAll(IGraph graph, System.IO.Stream stream){
		Context ctx = createContext(stream);
		export(ctx, graph);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.file.FileSink#writeAll(org.graphstream.graph.Graph ,
	 * java.io.Writer)
	 */
	public void writeAll(IGraph graph, System.IO.TextWriter writer){
		Context ctx = createContext(writer);
		export(ctx, graph);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#begin(java.lang.String)
	 */
	public void begin(string fileName){
		if (currentContext != null)
			throw new System.IO.IOException("cannot call begin() twice without calling end() before.");

		currentContext = createContext(fileName);
		addSink(currentContext.gexf);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#begin(java.io.OutputStream)
	 */
	public void begin(System.IO.Stream stream){
		if (currentContext != null)
			throw new System.IO.IOException("cannot call begin() twice without calling end() before.");

		currentContext = createContext(stream);
		addSink(currentContext.gexf);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#begin(java.io.Writer)
	 */
	public void begin(System.IO.TextWriter writer){
		if (currentContext != null)
			throw new System.IO.IOException("cannot call begin() twice without calling end() before.");

		currentContext = createContext(writer);
		addSink(currentContext.gexf);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#flush()
	 */
	public void flush(){
		if (currentContext != null)
			currentContext./* stream.Flush(); */
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#end()
	 */
	public void end(){
		removeSink(currentContext.gexf);

		try {
			currentContext.gexf.export(currentContext.stream);
			currentContext.stream.Close();

			if (currentContext.closeStreamAtEnd)
				currentContext.output.Close();
		} catch (Exception e) {
			throw new System.IO.IOException(e);
		}

		currentContext = null;
	}
}

}
