using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Org.GraphStream.Stream
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


public class Timeline : ISource, IReplayable, IEnumerable<IGraph> {

	public static readonly string TIME_PREFIX = "time";

	private class StepDiff {
		double step;
		GraphDiff diff;

		StepDiff(double step, GraphDiff diff) {
			this.step = step;
			this.diff = diff;
		}
	}

	List<StepDiff> diffs;

	protected bool changed;
	protected IGraph initialGraph, currentGraph;
	protected GraphDiff currentDiff;
	protected Connector connector;
	protected PipeBase pipe;
	protected int seeker;

	public Timeline() {
		this.diffs = new List<StepDiff>();
		this.changed = false;
		this.connector = new Connector();
		this.currentDiff = null;
		this.pipe = new PipeBase();
	}

	public void reset() {

	}

	public void play(double from, double to) {
		play(from, to, pipe);
	}

	public void play(double from, double to, ISink sink) {
		if (diffs.Count == 0)
			return;

		if (from > to) {
			int i = diffs.Count - 1, j;

			while (i > 0 && diffs[i].step > from)
				i--;

			j = i;

			while (j > 0 && diffs[j].step >= to)
				j--;

			for (int k = i; k >= j; k--)
				diffs[k].diff.reverse(sink);
		} else {
			int i = 0, j;

			while (i < diffs.Count - 1 && diffs[i].step < from)
				i++;

			j = i;

			while (j < diffs.Count - 1 && diffs[j].step <= to)
				j++;

			for (int k = i; k <= j; k++)
				diffs[k].diff.apply(sink);
		}
	}

	public void play() {
		play(initialGraph.getStep(), currentGraph.getStep());
	}

	public void play(ISink sink) {
		play(initialGraph.getStep(), currentGraph.getStep(), sink);
	}

	public void playback() {
		play(currentGraph.getStep(), initialGraph.getStep());
	}

	public void playback(ISink sink) {
		play(currentGraph.getStep(), initialGraph.getStep(), sink);
	}

	public void seek(int i) {
		seeker = i;
	}

	public void seekStart() {
		seeker = 0;
	}

	public void seekEnd() {
		seeker = diffs.Count;
	}

	public bool hasNext() {
		return seeker < diffs.Count;
	}

	public void next() {
		if (seeker >= diffs.Count)
			return;

		diffs[seeker++].diff.apply(pipe);
	}

	public bool hasPrevious() {
		return seeker > 0;
	}

	public void previous() {
		if (seeker <= 0)
			return;

		diffs[--seeker].diff.reverse(pipe);
	}

	/// <param name="source"></param>
	public void begin(ISource source) {
		initialGraph = new AdjacencyListGraph("initial");
		currentGraph = new AdjacencyListGraph("initial");
		begin();
	}

	/// <param name="source"></param>
	public void begin(IGraph source) {
		initialGraph = Graphs.clone(source);
		currentGraph = source;
		begin();
	}

	protected void begin() {
		currentGraph.addSink(connector);
		pushDiff();
	}

	
	public void end() {
		if (currentDiff != null) {
			currentDiff.end();
			diffs.Add(new StepDiff(currentGraph.getStep(), currentDiff));
		}

		currentGraph.removeSink(connector);
		currentGraph = Graphs.clone(currentGraph);
	}

	protected void pushDiff() {
		if (currentDiff != null) {
			currentDiff.end();
			diffs.Add(new StepDiff(currentGraph.getStep(), currentDiff));
		}

		currentDiff = new GraphDiff();
		currentDiff.start(currentGraph);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see java.lang.Iterable#iterator()
	 */
	public IEnumerator<IGraph> iterator() {
		return new TimelineIterator();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Replayable#getReplayController()
	 */
	public Controller getReplayController() {
		return new TimelineReplayController();
	}

	protected class Connector : SinkAdapter {
		
		public void stepBegins(string sourceId, long timeId, double step) {
			this.pushDiff();
		}
	}

	protected class TimelineReplayController : PipeBase, Controller {
		public void replay() {
			play(this);
		}

		public void replay(string sourceId) {
			string tmp = this.sourceId;
			this.sourceId = sourceId;
			play(this);
			this.sourceId = tmp;
		}
	}

	protected class TimelineIterator : IEnumerator<IGraph> {
		IGraph current;
		int idx;

		public TimelineIterator() {
			current = Graphs.clone(initialGraph);
			idx = 0;
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see java.util.Iterator#hasNext()
		 */
		public bool hasNext() {
			return idx < diffs.Count;
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see java.util.Iterator#next()
		 */
		public IGraph next() {
			if (idx >= diffs.Count)
				return null;

			diffs[idx++].diff.apply(current);
			return Graphs.clone(current);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see java.util.Iterator#remove()
		 */
		public void remove() {
		}

	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#addSink(org.graphstream.stream.Sink)
	 */
	public void addSink(ISink sink) {
		pipe.addSink(sink);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#removeSink(org.graphstream.stream.Sink)
	 */
	public void removeSink(ISink sink) {
		pipe.removeSink(sink);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#addAttributeSink(org.graphstream.stream
	 * .AttributeSink)
	 */
	public void addAttributeSink(IAttributeSink sink) {
		pipe.addAttributeSink(sink);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#removeAttributeSink(org.graphstream.stream
	 * .AttributeSink)
	 */
	public void removeAttributeSink(IAttributeSink sink) {
		pipe.removeAttributeSink(sink);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#addElementSink(org.graphstream.stream.
	 * ElementSink)
	 */
	public void addElementSink(IElementSink sink) {
		pipe.addElementSink(sink);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#removeElementSink(org.graphstream.stream
	 * .ElementSink)
	 */
	public void removeElementSink(IElementSink sink) {
		pipe.removeElementSink(sink);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#clearElementSinks()
	 */
	public void clearElementSinks() {
		pipe.clearElementSinks();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#clearAttributeSinks()
	 */
	public void clearAttributeSinks() {
		pipe.clearAttributeSinks();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#clearSinks()
	 */
	public void clearSinks() {
		pipe.clearSinks();
	}

	public static void main(params string[] strings){
		IGraph g = new AdjacencyListGraph("g");
		Timeline timeline = new Timeline();
		timeline.addSink(new VerboseSink());

		timeline.begin(g);

		g.stepBegins(0.0);
		g.addNode("A");
		g.addNode("B");
		g.stepBegins(1.0);
		g.addNode("C");

		timeline.end();

		Console.Write(string.Format("############\n");
		Console.Write(string.Format("# Play :\n");
		timeline.play();
		Console.Write(string.Format("############\n");
		Console.Write(string.Format("# Playback :\n");
		timeline.playback();
		Console.Write(string.Format("############\n");
		Console.Write(string.Format("# Sequence :\n");
		int i = 0;
		foreach (IGraph it in timeline) {
			Console.Write(string.Format(" IGraph#{0} {1}\n", i, toString(it));
		}
		Console.Write(string.Format("############\n");
	}

	private static string toString(IGraph g) {
		System.Text.System.Text.StringBuilder buffer = new System.Text.System.Text.StringBuilder();
		buffer.Append("id=\"").Append(g.getId()).Append("\" node={");

		g.nodes().ToList().ForEach(n => buffer.Append("\"").Append(n.getId()).Append("\", "));

		buffer.Append("}, edges={");

		g.edges().ToList().ForEach(e => {
			buffer.Append("\"").Append(e.getId()).Append("\":\"").Append(e.getSourceNode().getId()).Append("\"--\"")
					.Append(e.getTargetNode().getId()).Append("\", ");
		});

		buffer.Append("}");

		return buffer.ToString();
	}
}

}
