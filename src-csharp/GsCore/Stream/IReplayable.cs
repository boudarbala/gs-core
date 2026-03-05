using System.Collections.Generic;
using System.Linq;
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


/// <summary>
/// Defines sources that can be replayed. This is usefull when you are connecting a sink to a source but you need to get informations about the current state of the dynamic graph. <p> <pre> Replayable source = ... ; Replayable.Controller replay = source.getReplayController(); ... // source is building a graph ... Graph g = ... ; // // Replay the source to get the current state of the graph // replay.addSink(g); replay.replay(); </pre>
/// </summary>
public interface IReplayable {
	/// <summary>
/// Get a controller to replay the graph.
/// </summary>
/// <returns>a new replay controller</returns>
	Controller getReplayController();

	/// <summary>
/// A controller used to replay a source. Controller should be used as a source by adding sinks on it. When sinks are set, a call to {@link #replay()} send events describing the current state of the original source to sinks.
/// </summary>
	static interface Controller : ISource {
		/// <summary>
/// Replay events describing the current state of the object being built by the source.
/// </summary>
		void replay();

		/// <summary>
/// Same as {@link #replay(Sink)} but you can set the id of the source sent in events.
/// </summary>
/// <param name="sourceId"> id of the event source</param>
		void replay(string sourceId);
	}

	/// <summary>
/// Util method to replay a replayable source into a sink.
/// </summary>
/// <param name="source"> a source implementing the Replayable interface</param>
/// <param name="sink"> sink which will receive the events produced by the replay</param>
	public static void replay(IReplayable source, ISink sink) {
		Controller controller = source.getReplayController();

		controller.addSink(sink);
		controller.replay();
		controller.removeSink(sink);
	}

	/// <summary>
/// Same as {@link #replay(Replayable, Sink)} but the first parameter is just a {@link org.graphstream.stream.Source} and it will be replayed only if the Replayable interface is implemented.
/// </summary>
/// <param name="source"> a source</param>
/// <param name="sink"> sink which will receive the events produced by the replay</param>
	public static void tryReplay(ISource source, ISink sink) {
		if (source is IReplayable)
			replay((IReplayable) source, sink);
	}
}

}
