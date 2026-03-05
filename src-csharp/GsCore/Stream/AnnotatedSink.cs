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
/// A sink easily allowing a bind between attribute modifications and method calls. <pre> public class MyObject : AnnotatedSink { String a1; double a2; &#064;Bind(&quot;myobject.set.a1&quot;) public void setA1(String eventId, object value) { a1 = (String) value; } &#064;Bind(&quot;myobject.set.a2&quot;) public void setA2(String eventId, object value) { a2 = (double) value; } public static void main(String ... args) { Graph g = ...; MyObject obj = new MyObject(); g.addSink(obj); g.setAttribute("myobject.set.a1", "MyObject A1"); g.setAttribute("myobject.set.a2", 100.0); } } </pre>
/// </summary>
abstract class AnnotatedSink : ISink {
	/// <summary>
/// Annotation used to bind an event to a method. This bind is composed of a name (the attribute key) and an element type. For example, the annotation <pre> &#64;Bind(value = &quot;test&quot;, type = ElementType.NODE) </pre> will be triggered the annotated method when receiving 'nodeAttributeXXX()' methods.
/// </summary>
	@Documented
	@Retention(RetentionPolicy.RUNTIME)
	@Target(java.lang.annotation.ElementType.METHOD)
	public static @interface Bind {
		/// <summary>
/// Name of the attribute key that triggered the annotated method.
/// </summary>
/// <returns>an attribute key</returns>
		string value();

		/// <summary>
/// Type of element that triggered the annotated method. Default is GRAPH.
/// </summary>
/// <returns>type of element in GRAPH, NODE or EDGE</returns>
		ElementType type() ElementType.GRAPH;
	}

	private EnumMap<ElementType, MethodMap> methods;

	protected AnnotatedSink() {
		methods = new EnumMap<ElementType, MethodMap>(typeof(ElementType));
		methods[ElementType.GRAPH] = new MethodMap();
		methods[ElementType.EDGE] = new MethodMap();
		methods[ElementType.NODE] = new MethodMap();

		Method[] ms = getClass().getMethods();

		if (ms != null) {
			for (int i = 0; i < ms.Length; i++) {
				Method m = ms[i];
				Bind b = m.getAnnotation(typeof(Bind));

				if (b != null)
					methods[b.type()][b.value()] = m;
			}
		}
	}

	private void invoke(Method m, params object[] args) {
		try {
			m.invoke(this, args);
		} catch (ArgumentException e) {
			Console.Error.WriteLine(e);
		} catch (IllegalAccessException e) {
			Console.Error.WriteLine(e);
		} catch (InvocationTargetException e) {
			Console.Error.WriteLine(e);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		Method m = methods[ElementType.EDGE][attribute];

		if (m != null)
			invoke(m, edgeId, attribute, value);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeChanged(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		Method m = methods[ElementType.EDGE][attribute];

		if (m != null)
			invoke(m, edgeId, attribute, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeRemoved(java.lang.String,
	 * long, java.lang.String, java.lang.String)
	 */
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		Method m = methods[ElementType.EDGE][attribute];

		if (m != null)
			invoke(m, edgeId, attribute, null);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#graphAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.object)
	 */
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		Method m = methods[ElementType.GRAPH][attribute];

		if (m != null)
			invoke(m, attribute, value);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#graphAttributeChanged(java.lang.String,
	 * long, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		Method m = methods[ElementType.GRAPH][attribute];

		if (m != null)
			invoke(m, attribute, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#graphAttributeRemoved(java.lang.String,
	 * long, java.lang.String)
	 */
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		Method m = methods[ElementType.GRAPH][attribute];

		if (m != null)
			invoke(m, attribute, null);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		Method m = methods[ElementType.NODE][attribute];

		if (m != null)
			invoke(m, nodeId, attribute, value);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeChanged(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		Method m = methods[ElementType.NODE][attribute];

		if (m != null)
			invoke(m, nodeId, attribute, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeRemoved(java.lang.String,
	 * long, java.lang.String, java.lang.String)
	 */
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		Method m = methods[ElementType.NODE][attribute];

		if (m != null)
			invoke(m, nodeId, attribute, null);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeAdded(java.lang.String, long,
	 * java.lang.String, java.lang.String, java.lang.String, boolean)
	 */
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#graphCleared(java.lang.String, long)
	 */
	public void graphCleared(string sourceId, long timeId) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeAdded(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#stepBegins(java.lang.String, long,
	 * double)
	 */
	public void stepBegins(string sourceId, long timeId, double step) {
	}

	class MethodMap : Dictionary<string, Method> {
		private static readonly long serialVersionUID = 1664854698109523697L;
	}
}

}
