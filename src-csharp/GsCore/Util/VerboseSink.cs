using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;

namespace Org.GraphStream.Util
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
/// A sink that can be used to display event in a PrintStream like System.out. Format of messages can be customized, inserting keywords quoted with '%' in the format. '%sourceId%' and '%timeId%' keywords are defined for each evt. Following defines keywords available for each event types: <dl> <dt>ADD_NODE</dt> <dd> <ul> <li>%nodeId%</li> </ul> </dd> <dt>ADD_NODE_ATTRIBUTE</dt> <dd> <ul> <li>%nodeId%</li> <li>%attributeId%</li> <li>%value%</li> </ul> </dd> <dt>SET_NODE_ATTRIBUTE</dt> <dd> <ul> <li>%nodeId%</li> <li>%attributeId%</li> <li>%value%</li> </ul> </dd> <dt>DEL_NODE_ATTRIBUTE</dt> <dd> <ul> <li>%nodeId%</li> <li>%attributeId%</li> </ul> </dd> <dt>DEL_NODE</dt> <dd> <ul> <li>%nodeId%</li> </ul> </dd> <dt>ADD_EDGE</dt> <dd> <ul> <li>%edgeId%</li> <li>%source%</li> <li>%target%</li> <li>%directed%</li> </ul> </dd> <dt>ADD_EDGE_ATTRIBUTE</dt> <dd> <ul> <li>%edgeId%</li> <li>%attributeId%</li> <li>%value%</li> </ul> </dd> <dt>SET_EDGE_ATTRIBUTE</dt> <dd> <ul> <li>%edgeId%</li> <li>%attributeId%</li> <li>%value%</li> </ul> </dd> <dt>DEL_EDGE_ATTRIBUTE</dt> <dd> <ul> <li>%edgeId%</li> <li>%attributeId%</li> </ul> </dd> <dt>DEL_EDGE</dt> <dd> <ul> <li>%edgeId%</li> </ul> </dd> <dt>ADD_GRAPH_ATTRIBUTE</dt> <dd> <ul> <li>%attributeId%</li> <li>%value%</li> </ul> </dd> <dt>SET_GRAPH_ATTRIBUTE</dt> <dd> <ul> <li>%attributeId%</li> <li>%value%</li> </ul> </dd> <dt>DEL_GRAPH_ATTRIBUTE</dt> <dd> <ul> <li>%attributeId%</li> </ul> </dd> <dt>CLEAR</dt> <dd></dd> <dt>STEP_BEGINS</dt> <dd> <ul> <li>%step%</li> </ul> </dd> </dl>
/// </summary>
public class VerboseSink : ISink {
	public static readonly string DEFAULT_AN_FORMAT = "%prefix%[{0}ourceId%:%timeId%] add node \"%nodeId%\"{0}uffix%";
	public static readonly string DEFAULT_CNA_FORMAT = "%prefix%[{0}ourceId%:%timeId%] set node \"%nodeId%\" +\"%attributeId%\"=%value%suffix%";
	public static readonly string DEFAULT_CNC_FORMAT = "%prefix%[{0}ourceId%:%timeId%] set node \"%nodeId%\" \"%attributeId%\"=%value%suffix%";
	public static readonly string DEFAULT_CNR_FORMAT = "%prefix%[{0}ourceId%:%timeId%] set node \"%nodeId%\" -\"%attributeId%\"{0}uffix%";
	public static readonly string DEFAULT_DN_FORMAT = "%prefix%[{0}ourceId%:%timeId%] remove node \"%nodeId%\"{0}uffix%";

	public static readonly string DEFAULT_AE_FORMAT = "%prefix%[{0}ourceId%:%timeId%] add edge \"%edgeId%\" : \"{0}ource%\" -- \"%target%\"{0}uffix%";
	public static readonly string DEFAULT_CEA_FORMAT = "%prefix%[{0}ourceId%:%timeId%] set edge \"%edgeId%\" +\"%attributeId%\"=%value%suffix%";
	public static readonly string DEFAULT_CEC_FORMAT = "%prefix%[{0}ourceId%:%timeId%] set edge \"%edgeId%\" \"%attributeId%\"=%value%suffix%";
	public static readonly string DEFAULT_CER_FORMAT = "%prefix%[{0}ourceId%:%timeId%] set edge \"%edgeId%\" -\"%attributeId%\"{0}uffix%";
	public static readonly string DEFAULT_DE_FORMAT = "%prefix%[{0}ourceId%:%timeId%] remove edge \"%edgeId%\"{0}uffix%";

	public static readonly string DEFAULT_CGA_FORMAT = "%prefix%[{0}ourceId%:%timeId%] set +\"%attributeId%\"=%value%suffix%";
	public static readonly string DEFAULT_CGC_FORMAT = "%prefix%[{0}ourceId%:%timeId%] set \"%attributeId%\"=%value%suffix%";
	public static readonly string DEFAULT_CGR_FORMAT = "%prefix%[{0}ourceId%:%timeId%] set -\"%attributeId%\"{0}uffix%";

	public static readonly string DEFAULT_CL_FORMAT = "%prefix%[{0}ourceId%:%timeId%] clear{1}uffix%";
	public static readonly string DEFAULT_ST_FORMAT = "%prefix%[{0}ourceId%:%timeId%] step {1}tep% begins{2}uffix%";

	/*
	 * Shortcut to use HashMap<String, object>.
	 */
	class Args : Dictionary<string, object> {
		private static readonly long serialVersionUID = 3064164898156692557L;
	}

	/// <summary>
/// Enumeration defining type of events.
/// </summary>
	enum EventType {
		ADD_NODE, ADD_NODE_ATTRIBUTE, SET_NODE_ATTRIBUTE, DEL_NODE_ATTRIBUTE, DEL_NODE, ADD_EDGE, ADD_EDGE_ATTRIBUTE, SET_EDGE_ATTRIBUTE, DEL_EDGE_ATTRIBUTE, DEL_EDGE, ADD_GRAPH_ATTRIBUTE, SET_GRAPH_ATTRIBUTE, DEL_GRAPH_ATTRIBUTE, CLEAR, STEP_BEGINS
	}

	/// <summary>
/// Flag used to indicate if the sink has to flush the output when writting a message.
/// </summary>
	protected bool autoflush;
	/// <summary>
/// Stream used to write message.
/// </summary>
	protected System.IO.TextWriter out;
	/// <summary>
/// Format of messages associated with each evt.
/// </summary>
	protected EnumMap<EventType, string> formats;
	/// <summary>
/// Flag used to indicate if an event has to be written or note.
/// </summary>
	protected EnumMap<EventType, bool> enable;
	/*
	 * Used to avoid to create a lot of hashmap when passing event arguments.
	 */
	private Stack<Args> argsStack;

	protected string prefix;

	protected string suffix;

	/// <summary>
/// Create a new verbose sink using System.out.
/// </summary>
	public VerboseSink() : this(System.out) {
	}

	/// <summary>
/// Create a new verbose sink.
/// </summary>
/// <param name="out"> stream used to output message</param>
	public VerboseSink(System.IO.TextWriter out) {
		this.out = out;
		argsStack = new Stack<Args>();
		enable = new EnumMap<EventType, bool>(typeof(EventType));
		formats = new EnumMap<EventType, string>(typeof(EventType));

		formats[EventType.ADD_NODE] = DEFAULT_AN_FORMAT;
		formats[EventType.ADD_NODE_ATTRIBUTE] = DEFAULT_CNA_FORMAT;
		formats[EventType.SET_NODE_ATTRIBUTE] = DEFAULT_CNC_FORMAT;
		formats[EventType.DEL_NODE_ATTRIBUTE] = DEFAULT_CNR_FORMAT;
		formats[EventType.DEL_NODE] = DEFAULT_DN_FORMAT;

		formats[EventType.ADD_EDGE] = DEFAULT_AE_FORMAT;
		formats[EventType.ADD_EDGE_ATTRIBUTE] = DEFAULT_CEA_FORMAT;
		formats[EventType.SET_EDGE_ATTRIBUTE] = DEFAULT_CEC_FORMAT;
		formats[EventType.DEL_EDGE_ATTRIBUTE] = DEFAULT_CER_FORMAT;
		formats[EventType.DEL_EDGE] = DEFAULT_DE_FORMAT;

		formats[EventType.ADD_GRAPH_ATTRIBUTE] = DEFAULT_CGA_FORMAT;
		formats[EventType.SET_GRAPH_ATTRIBUTE] = DEFAULT_CGC_FORMAT;
		formats[EventType.DEL_GRAPH_ATTRIBUTE] = DEFAULT_CGR_FORMAT;

		formats[EventType.CLEAR] = DEFAULT_CL_FORMAT;
		formats[EventType.STEP_BEGINS] = DEFAULT_ST_FORMAT;

		foreach (EventType t in ((EventType[])Enum.GetValues(typeof(EventType))))
			enable[t] = bool.TRUE;

		suffix = "";
		prefix = "";
	}

	/// <summary>
/// Enable or disable autoflush.
/// </summary>
/// <param name="on"> true to enable autoflush</param>
	public void setAutoFlush(bool on) {
		this.autoflush = on;
	}

	/// <summary>
/// Redefines message format of an evt.
/// </summary>
/// <param name="type"> type of the event</param>
/// <param name="format"> new format of the message attached with the event</param>
	public void setEventFormat(EventType type, string format) {
		formats[type] = format;
	}

	/// <summary>
/// Enable or disable an evt.
/// </summary>
/// <param name="type"> type of the event</param>
/// <param name="on"> true to enable message for this event</param>
	public void setEventEnabled(EventType type, bool on) {
		enable[type] = on;
	}

	/// <summary>
/// Enable or disable all messages associated with attribute events.
/// </summary>
/// <param name="on"> true to enable events</param>
	public void setElementEventEnabled(bool on) {
		enable[EventType.ADD_EDGE_ATTRIBUTE] = on;
		enable[EventType.SET_EDGE_ATTRIBUTE] = on;
		enable[EventType.DEL_EDGE_ATTRIBUTE] = on;
		enable[EventType.ADD_NODE_ATTRIBUTE] = on;
		enable[EventType.SET_NODE_ATTRIBUTE] = on;
		enable[EventType.DEL_NODE_ATTRIBUTE] = on;
		enable[EventType.ADD_GRAPH_ATTRIBUTE] = on;
		enable[EventType.SET_GRAPH_ATTRIBUTE] = on;
		enable[EventType.DEL_GRAPH_ATTRIBUTE] = on;
	}

	/// <summary>
/// Enable or disable all messages associated with element events.
/// </summary>
/// <param name="on"> true to enable events</param>
	public void setAttributeEventEnabled(bool on) {
		enable[EventType.ADD_EDGE] = on;
		enable[EventType.DEL_EDGE] = on;
		enable[EventType.ADD_NODE] = on;
		enable[EventType.DEL_NODE] = on;
		enable[EventType.CLEAR] = on;
	}

	/// <summary>
/// Set prefix used in messages.
/// </summary>
/// <param name="prefix"> new prefix</param>
	public void setPrefix(string prefix) {
		this.prefix = prefix;
	}

	/// <summary>
/// Set suffix used in messages.
/// </summary>
/// <param name="suffix"> new suffix</param>
	public void setSuffix(string suffix) {
		this.suffix = suffix;
	}

	private void print(EventType type, Args args) {
		if (!enable[type])
			return;

		string out = formats[type];

		foreach (string k in args.Keys) {
			object o = args[k];
			out = output.Replace(string.Format("%{0}%", k), o == null ? "null" : o.ToString());
		}

		this.output.print(output);
		this.output.printf("\n");

		if (autoflush)
			this./* output.Flush(); */

		argsPnP(args);
	}

	private Args argsPnP(Args args) {
		if (args == null) {
			if (argsStack.Count > 0)
				args = argsStack.Pop();
			else
				args = new Args();

			args["prefix"] = prefix;
			args["suffix"] = suffix;

			return args;
		} else {
			args.Clear();
			argsStack.Push(args);

			return null;
		}
	}

	private string toStringValue(object o) {
		if (o == null)
			return "<null>";

		if (o is string)
			return "\"" + ((string) o).Replace("\"", "\\\"") + "\"";
		else if (o.GetType().IsArray) {
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append("{");

			for (int i = 0; i < Array.getLength(o); i++) {
				if (i > 0)
					buffer.Append(", ");
				buffer.Append(toStringValue(Array[o, i]));
			}

			buffer.Append("}");
			return buffer.ToString();
		}

		return o.ToString();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["edgeId"] = edgeId;
		args["attributeId"] = attribute;
		args["value"] = toStringValue(value);

		print(EventType.ADD_EDGE_ATTRIBUTE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeChanged(java.lang.String ,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["edgeId"] = edgeId;
		args["attributeId"] = attribute;
		args["value"] = toStringValue(newValue);

		print(EventType.SET_EDGE_ATTRIBUTE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["edgeId"] = edgeId;
		args["attributeId"] = attribute;

		print(EventType.DEL_EDGE_ATTRIBUTE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#graphAttributeAdded(java.lang.String ,
	 * long, java.lang.String, java.lang.object)
	 */
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["attributeId"] = attribute;
		args["value"] = toStringValue(value);

		print(EventType.ADD_GRAPH_ATTRIBUTE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeChanged(java.lang.
	 * String, long, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["attributeId"] = attribute;
		args["value"] = toStringValue(newValue);

		print(EventType.SET_GRAPH_ATTRIBUTE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeRemoved(java.lang.
	 * String, long, java.lang.String)
	 */
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["attributeId"] = attribute;

		print(EventType.DEL_GRAPH_ATTRIBUTE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["nodeId"] = nodeId;
		args["attributeId"] = attribute;
		args["value"] = toStringValue(value);

		print(EventType.ADD_NODE_ATTRIBUTE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeChanged(java.lang.String ,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["nodeId"] = nodeId;
		args["attributeId"] = attribute;
		args["value"] = toStringValue(newValue);

		print(EventType.SET_NODE_ATTRIBUTE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["nodeId"] = nodeId;
		args["attributeId"] = attribute;

		print(EventType.DEL_NODE_ATTRIBUTE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeAdded(java.lang.String, long,
	 * java.lang.String, java.lang.String, java.lang.String, boolean)
	 */
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["edgeId"] = edgeId;
		args["source"] = fromNodeId;
		args["target"] = toNodeId;

		print(EventType.ADD_EDGE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["edgeId"] = edgeId;

		print(EventType.DEL_EDGE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#graphCleared(java.lang.String, long)
	 */
	public void graphCleared(string sourceId, long timeId) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;

		print(EventType.CLEAR, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeAdded(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["nodeId"] = nodeId;

		print(EventType.ADD_NODE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["nodeId"] = nodeId;

		print(EventType.DEL_NODE, args);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#stepBegins(java.lang.String, long,
	 * double)
	 */
	public void stepBegins(string sourceId, long timeId, double step) {
		Args args = argsPnP(null);

		args["sourceId"] = sourceId;
		args["timeId"] = timeId;
		args["step"] = step;

		print(EventType.STEP_BEGINS, args);
	}
}
}
