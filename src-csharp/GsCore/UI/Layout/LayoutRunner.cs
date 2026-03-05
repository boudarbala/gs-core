using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;

namespace Org.GraphStream.UI.Layout
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
/// Allows to run a layout in a distinct thread. <p> A layout runner will run in its own thread and periodically activate a layout algorithm on a graph event stream (you do not need a graph). This implementation is mainly used by the graph viewer but could be used by any program that needs a layout algorithm that run continuously on a dynamic graph (adapting the layout as the graph changes). </p> <p> The layout algorithms in GraphStream are iterative versions that can be called repeatedly to take graph dynamics into account and may produce a result only after several invocations. This is why the layout runner invokes the layout on a regular basis. The runner is temporized, it will not run in a loop as fast as possible, instead it will wait a little between each layout invocation. When the last layout invocation indicated the layout was good, it will wait longer that when the last invocation indicated the layout was not good (stabilized). These two times can be configured using {@link #setNaps(long, long)}. </p> <p> Once you finished using the runner, you must call {@link #release()} to break the link with the event source and stop the thread. The runner cannot be used after. </p>
/// </summary>
public class LayoutRunner {

	/// <summary>
/// class level logger
/// </summary>
	private static readonly object /* Logger */ logger = null /* Logger */;

	/// <summary>
/// The layout algorithm.
/// </summary>
	protected ILayout layout = null;

	/// <summary>
/// The proxy on the source of graph events.
/// </summary>
	protected ThreadProxyPipe pumpPipe = null;

	/// <summary>
/// The meaning of life.
/// </summary>
	protected bool loop = true;

	/// <summary>
/// The time to wait between each layout invocation, when the layout stabilized.
/// </summary>
	protected long longNap = 80;

	/// <summary>
/// The time to wait between each layout invocation, when the layout is not yet stabilized.
/// </summary>
	protected long shortNap = 10;

	/// <summary>
/// New layout runner that listens at the given source and compute a layout on its graph structure in a distinct thread.
/// </summary>
/// <param name="source"> The source of graph events.</param>
/// <param name="layout"> The layout algorithm to use.</param>
	public LayoutRunner(ISource source, ILayout layout) : this(source, layout, true) {
	}

	/// <summary>
/// New layout runner that listen at the given source and compute a layout on its graph structure in a distinct thread.
/// </summary>
/// <param name="source"> The source of graph events.</param>
/// <param name="layout"> The layout algorithm to use.</param>
/// <param name="start"> Start the layout thread immediately ? Else the start() method must be called later.</param>
	public LayoutRunner(ISource source, ILayout layout, bool start) {
		this.layout = layout;
		this.pumpPipe = new ThreadProxyPipe();
		this.pumpPipe.addSink(layout);

		if (start)
			start();

		this.pumpPipe.init(source);
	}

	/// <summary>
/// New layout runner that listen at the given graph and compute a layout on its graph structure in a distinct thread. A pipe is still created to listen at the graph. This means that the graph is never directly used.
/// </summary>
/// <param name="graph"> The source of graph events.</param>
/// <param name="layout"> The layout algorithm to use.</param>
/// <param name="start"> Start the layout thread immediately ? Else the start() method must be called later.</param>
/// <param name="replay"> If the graph already contains some data, replay events to create the data, this is mostly always needed.</param>
	public LayoutRunner(IGraph graph, ILayout layout, bool start, bool replay) {
		this.layout = layout;
		this.pumpPipe = new ThreadProxyPipe();
		this.pumpPipe.addSink(layout);

		if (start)
			start();

		this.pumpPipe.init(graph, replay);
	}

	/// <summary>
/// Pipe out whose input is connected to the layout algorithm. You can safely connect as a sink to it to receive events of the layout from a distinct thread.
/// </summary>
	public IProxyPipe newLayoutPipe() {
		ThreadProxyPipe tpp = new ThreadProxyPipe();
		tpp.init(layout);

		return tpp;
	}

	
	public void run() {
		string layoutName = layout.getLayoutAlgorithmName();

		while (loop) {
			double limit = layout.getStabilizationLimit();

			pumpPipe.pump();
			if (limit > 0) {
				if (layout.getStabilization() > limit) {
					nap(longNap);
				} else {
					layout.compute();
					nap(shortNap);
				}
			} else {
				layout.compute();
				nap(shortNap);
			}
		}
		Console.WriteLine(string.Format("ILayout '{0}' process stopped.", layoutName));
	}

	/// <summary>
/// Release any link to the source of events and stop the layout proces. The thread will end after this method has been called.
/// </summary>
	public void release() {
		pumpPipe.unregisterFromSource();
		pumpPipe.removeSink(layout);
		pumpPipe = null;
		loop = false;

		if (System.Threading.Thread.CurrentThread != this) {
			try {
				this.Join();
			} catch (Exception e) {
				Console.Error.WriteLine("Unable to stop/release layout." + " " + e);
			}
		}

		layout = null;
	}

	/// <summary>
/// Sleep for the given period of time in milliseconds.
/// </summary>
/// <param name="ms"> The number of milliseconds to wait.</param>
	protected void nap(long ms) {
		try {
			System.Threading.Thread.Sleep((int)ms);
		} catch (Exception e) {
		}
	}

	/// <summary>
/// Configure the time to wait between each layout invocation. The long nap configures the time to wait when the last layout invocation indicated the layout was stabilized, the short nap is used in the other case.
/// </summary>
/// <param name="longNap"> The time to wait between stabilized layout invocations, by default 80.</param>
/// <param name="shortNap"> The time to wait between non stabilized layout invocations, by default 10.</param>
	public void setNaps(long longNap, long shortNap) {
		this.longNap = longNap;
		this.shortNap = shortNap;
	}
}
}
