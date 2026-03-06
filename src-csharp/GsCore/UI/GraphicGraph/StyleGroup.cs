using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Org.GraphStream.UI.GraphicGraph
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
/// A group of graph elements that share the same style. <p> The purpose of a style group is to allow retrieving all elements with the same style easily. Most of the time, with graphic engines, pushing the graphic state (the style, colors, line width, textures, gradients) is a costly operation. Doing it once for several elements can speed up things a lot. This is the purpose of the style group. </p> <p> The action of drawing elements in group (first push style, then draw all elements) are called bulk drawing. All elements that can be drawn at once this way are called bulk elements. </p> <p> In a style group it is not always possible do draw elements in a such a "bulk" operation. If the style contains "dynamic values" for example, that is value that depend on the value of an attribute stored on the element, or if the element is modified by an event (clicked, selected), the element will not be drawn the same as others. </p> <p> The style group provides iterators on each of these categories of elements : <ul> <li>{@link #elements()} allows to browse all elements contained in the group without exception.</li> <li>{@link #dynamicElements()} allows to browse the subset of elements having a attribute that modify their style.</li> <li>{@link #elementsEvents()} allows to browse the subset of elements modified by an evt.</li> <li>{@link #bulkElements()} allows to browse all remaining elements that have no dynamic attribute or evt.</li> </ul> The calling the three last iterators would yield the same elements as calling the first one. When drawing you can optimise the drawing by first pushing the graphic state and then drawing at once all bulk elements. If the dynamic and event subsets are not empty you then must draw such elements modifying the graphic state for each one. </p>
/// </summary>
public class StyleGroup : Style, IEnumerable<IElement> {
	// Attribute

	/// <summary>
/// The group unique identifier.
/// </summary>
	protected string id;

	/// <summary>
/// The set of style rules.
/// </summary>
	protected List<Rule> rules = new List<Rule>();

	/// <summary>
/// Graph elements of this group.
/// </summary>
	protected Dictionary<string, IElement> elements = new Dictionary<string, IElement>();

	/// <summary>
/// The global events actually occurring.
/// </summary>
	protected StyleGroupSet.EventSet eventSet;

	/// <summary>
/// Set of elements whose style is actually modified individually by an evt. Such elements must be rendered one by one, not in groups like others.
/// </summary>
	protected Dictionary<IElement, ElementEvents> eventsFor;

	/// <summary>
/// Set of elements that have some dynamic style values. Such elements must be rendered one by one, not in groups, like others.
/// </summary>
	protected HashSet<IElement> dynamicOnes;

	/// <summary>
/// A set of events actually pushed only for this group.
/// </summary>
	protected string[] curEvents;

	/// <summary>
/// The set of bulk elements.
/// </summary>
	protected BulkElements bulkElements = new BulkElements();

	/// <summary>
/// Associated renderers.
/// </summary>
	public Dictionary<string, SwingElementRenderer> renderers;

	// Construction

	/// <summary>
/// New style group for a first graph element and the set of style rules that matches it. More graph elements can be added later.
/// </summary>
/// <param name="identifier"> The unique group identifier (see {@link org.graphstream.ui.graphicGraph.stylesheet.StyleSheet#getStyleGroupIdFor(Element, ArrayList)} ).</param>
/// <param name="rules"> The set of style rules for the style group (see {@link org.graphstream.ui.graphicGraph.stylesheet.StyleSheet#getRulesFor(Element)} ).</param>
/// <param name="firstElement"> The first element to construct the group.</param>
	public StyleGroup(string identifier, ICollection<Rule> rules, IElement firstElement,
			StyleGroupSet.EventSet eventSet) {
		this.id = identifier;
		this.rules.AddRange(rules);
		this.elements[firstElement.getId()] = firstElement;
		this.values = null; // To avoid consume memory since this style will not
							// store anything.
		this.eventSet = eventSet;

		foreach (Rule rule in rules)
			rule.addGroup(identifier);
	}

	// Access

	/// <summary>
/// The group unique identifier.
/// </summary>
/// <returns>A style group identifier.</returns>
	public string getId() {
		return id;
	}

	/// <summary>
/// Type of graph element concerned by this style (node, edge, sprite, graph).
/// </summary>
/// <returns>The type of the style group elements.</returns>
	public Selector.Type getType() {
		return rules[0].selector.type;
	}

	/// <summary>
/// True if at least one of the style properties is dynamic (set according to an attribute of the element to draw). Such elements cannot therefore be drawn in a group operation, but one by one.
/// </summary>
/// <returns>True if one property is dynamic.</returns>
	public bool hasDynamicElements() {
		return (dynamicOnes != null && dynamicOnes.Count > 0);
	}

	/// <summary>
/// If true this group contains some elements that are actually changed by an evt. Such elements cannot therefore be drawn in a group operation, but one by one.
/// </summary>
/// <returns>True if the group contains some elements changed by an evt.</returns>
	public bool hasEventElements() {
		return (eventsFor != null && eventsFor.Count > 0);
	}

	/// <summary>
/// True if the given element actually has active events.
/// </summary>
/// <param name="element"> The element to test.</param>
/// <returns>True if the element has actually active events.</returns>
	public bool elementHasEvents(IElement element) {
		return (eventsFor != null && eventsFor.ContainsKey(element));
	}

	/// <summary>
/// True if the given element has dynamic style values provided by specific attributes.
/// </summary>
/// <param name="element"> The element to test.</param>
/// <returns>True if the element has actually specific style attributes.</returns>
	public bool elementIsDynamic(IElement element) {
		return (dynamicOnes != null && dynamicOnes.Contains(element));
	}

	/// <summary>
/// Get the value of a given property. This is a redefinition of the method in {@link Style} to consider the fact a style group aggregates several style rules.
/// </summary>
/// <param name="property"> The style property the value is searched for.</param>
	
	public object getValue(string property, params string[] events) {
		int n = rules.Count;

		if (events == null || events.Length == 0) {
			if (curEvents != null && curEvents.Length > 0) {
				events = curEvents;
			} else if (eventSet.events != null && eventSet.events.Length > 0) {
				events = eventSet.events;
			}
		}

		for (int i = 1; i < n; i++) {
			Style style = rules[i].getStyle();

			if (style.hasValue(property, events))
				return style.getValue(property, events);
		}

		return rules[0].getStyle().getValue(property, events);
	}

	/// <summary>
/// True if there are no elements in the group.
/// </summary>
/// <returns>True if the group is empty of elements.</returns>
	public bool isEmpty() {
		return elements.Length == 0;
	}

	/// <summary>
/// True if the group contains the element whose identifier is given.
/// </summary>
/// <param name="elementId"> The element to search.</param>
/// <returns>true if the element is in the group.</returns>
	public bool contains(string elementId) {
		return elements.ContainsKey(elementId);
	}

	/// <summary>
/// True if the group contains the element given.
/// </summary>
/// <param name="element"> The element to search.</param>
/// <returns>true if the element is in the group.</returns>
	public bool contains(IElement element) {
		return elements.ContainsKey(element.getId());
	}

	/// <summary>
/// Return an element of the group, knowing its identifier.
/// </summary>
/// <param name="id"> The searched element identifier.</param>
/// <returns>The element corresponding to the identifier or null if not found.</returns>
	public IElement getElement(string id) {
		return elements[id];
	}

	/// <summary>
/// The number of elements of the group.
/// </summary>
/// <returns>The element count.</returns>
	public int getElementCount() {
		return elements.Count;
	}

	/// <summary>
/// Iterator on the set of graph elements of this group.
/// </summary>
/// <returns>The elements iterator.</returns>
	public IEnumerator<IElement> getElementIterator() {
		return ((elements[])Enum.GetValues(typeof(elements))).GetEnumerator();
	}

	/// <summary>
/// Iterable set of elements. This the complete set of elements contained in this group without regard to the fact they are modified by an event or are dynamic. If you plan to respect events or dynamic elements, you must check the elements are not modified by events using {@link #elementHasEvents(Element)} and are not dynamic by using {@link #elementIsDynamic(Element)} and then draw modified elements using {@link #elementsEvents()} and {@link #dynamicElements()}. But the easiest way of drawing is to use first {@link #bulkElements()} for all non dynamic non event elements, then the {@link #dynamicElements()} and {@link #elementsEvents()} to draw all dynamic and event elements.
/// </summary>
/// <returns>All the elements in no particular order.</returns>
	public IEnumerable<IElement> elements() {
		return ((elements[])Enum.GetValues(typeof(elements)));
	}

	/// <summary>
/// Iterable set of elements that can be drawn in a bulk operation, that is the subset of all elements that are not dynamic or modified by an evt.
/// </summary>
/// <returns>The iterable set of bulk elements.</returns>
	public IEnumerable<IElement> bulkElements() {
		return bulkElements;
	}

	/// <summary>
/// Subset of elements that are actually modified by one or more events. The {@link ElementEvents} class contains the element and an array of events that can be pushed on the style group set.
/// </summary>
/// <returns>The subset of elements modified by one or more events.</returns>
	public IEnumerable<ElementEvents> elementsEvents() {
		return ((eventsFor[])Enum.GetValues(typeof(eventsFor)));
	}

	/// <summary>
/// Subset of elements that have dynamic style values and therefore must be rendered one by one, not in groups like others. Even though elements style can specify some dynamics, the elements must individually have attributes that specify the dynamic value. If the elements do not have these attributes they can be rendered in bulk operations.
/// </summary>
/// <returns>The subset of dynamic elements of the group.</returns>
	public IEnumerable<IElement> dynamicElements() {
		return dynamicOnes;
	}

	public IEnumerator<IElement> iterator() {
		return ((elements[])Enum.GetValues(typeof(elements))).GetEnumerator();
	}

	/// <summary>
/// The associated renderers.
/// </summary>
/// <returns>A renderer or null if not found.</returns>
	public SwingElementRenderer getRenderer(string id) {
		if (renderers != null)
			return renderers[id];

		return null;
	}

	/// <summary>
/// Set of events for a given element or null if the element has not currently occurring events.
/// </summary>
/// <returns>A set of events or null if none occurring at that time.</returns>
	public ElementEvents getEventsFor(IElement element) {
		if (eventsFor != null)
			return eventsFor[element];

		return null;
	}

	/// <summary>
/// Test if an element is pushed as dynamic.
/// </summary>
	public bool isElementDynamic(IElement element) {
		if (dynamicOnes != null)
			return dynamicOnes.Contains(element);

		return false;
	}

	// Command

	/// <summary>
/// Add a new graph element to the group.
/// </summary>
/// <param name="element"> The new graph element to add.</param>
	public void addElement(IElement element) {
		elements[element.getId()] = element;
	}

	/// <summary>
/// Remove a graph element from the group.
/// </summary>
/// <param name="element"> The element to remove.</param>
/// <returns>The removed element, or null if the element was not found.</returns>
	public IElement removeElement(IElement element) {
		if (eventsFor != null && eventsFor.ContainsKey(element))
			eventsFor.Remove(element); // Remove an eventual remaining evt.

		if (dynamicOnes != null && dynamicOnes.Contains(element))
			dynamicOnes.Remove(element); // Remove an eventual remaining dynamic
											// information.

		return elements.Remove(element.getId());
	}

	/// <summary>
/// Push an event specifically for the given element. Events are stacked in order. Called by the GraphicElement.
/// </summary>
/// <param name="element"> The element to modify with an evt.</param>
/// <param name="event"> The event to push.</param>
	protected void pushEventFor(IElement element, string evt) {
		if (elements.ContainsKey(element.getId())) {
			if (eventsFor == null)
				eventsFor = new Dictionary<IElement, ElementEvents>();

			ElementEvents evs = eventsFor[element];

			if (evs == null) {
				evs = new ElementEvents(element, this, evt);
				eventsFor[element] = evs;
			} else {
				evs.pushEvent(evt);
			}
		}
	}

	/// <summary>
/// Pop an event for the given element. Called by the GraphicElement.
/// </summary>
/// <param name="element"> The element.</param>
/// <param name="event"> The evt.</param>
	protected void popEventFor(IElement element, string evt) {
		if (elements.ContainsKey(element.getId())) {
			if ( eventsFor != null ) {
				ElementEvents evs = eventsFor[element];
				
				if (evs != null) {
					evs.popEvent(evt);

					if (evs.eventCount() == 0)
						eventsFor.Remove(element);
				}

				if (eventsFor.Length == 0)
					eventsFor = null;
	
			}
		}
	}

	/// <summary>
/// Before drawing an element that has events, use this method to activate the events, the style values will be modified accordingly. Events for this element must have been registered via {@link #pushEventFor(Element, String)}. After rendering the {@link #deactivateEvents()} MUST be called.
/// </summary>
/// <param name="element"> The element to push events for.</param>
	public void activateEventsFor(IElement element) {
		ElementEvents evs = eventsFor[element];

		if (evs != null && curEvents == null)
			curEvents = evs.events();
	}

	/// <summary>
/// De-activate any events activated for an element. This method MUST be called if {@link #activateEventsFor(Element)} has been called.
/// </summary>
	public void deactivateEvents() {
		curEvents = null;
	}

	/// <summary>
/// Indicate the element has dynamic values and thus cannot be drawn in bulk operations. Called by the GraphicElement.
/// </summary>
/// <param name="element"> The element.</param>
	protected void pushElementAsDynamic(IElement element) {
		if (dynamicOnes == null)
			dynamicOnes = new HashSet<IElement>();

		dynamicOnes.Add(element);
	}

	/// <summary>
/// Indicate the element has no more dynamic values and can be drawn in bulk operations. Called by the GraphicElement.
/// </summary>
/// <param name="element"> The element.</param>
	protected void popElementAsDynamic(IElement element) {
		dynamicOnes.Remove(element);

		if (dynamicOnes.Length == 0)
			dynamicOnes = null;
	}

	/// <summary>
/// Remove all graph elements of this group, and remove this group from the group list of each style rule.
/// </summary>
	public void release() {
		foreach (Rule rule in rules)
			rule.removeGroup(id);

		elements.Clear();
	}

	/// <summary>
/// Redefinition of the {@link Style} to forbid changing the values.
/// </summary>
	
	public void setValue(string property, object value) {
		throw new Exception("you cannot change the values of a style group.");
	}

	/// <summary>
/// Add a renderer to this group.
/// </summary>
/// <param name="id"> The renderer identifier.</param>
/// <param name="renderer"> The renderer.</param>
	public void addRenderer(string id, SwingElementRenderer renderer) {
		if (renderers == null)
			renderers = new Dictionary<string, SwingElementRenderer>();

		renderers[id] = renderer;
	}

	/// <summary>
/// Remove a renderer.
/// </summary>
/// <param name="id"> The renderer identifier.</param>
/// <returns>The removed renderer or null if not found.</returns>
	public SwingElementRenderer removeRenderer(string id) {
		return renderers.Remove(id);
	}

	
	public string toString() {
		return toString(-1);
	}

	
	public string toString(int level) {
		System.Text.StringBuilder builder = new System.Text.StringBuilder();
		string prefix = "";
		string sprefix = "    ";

		for (int i = 0; i < level; i++)
			prefix += sprefix;

		builder.Append(string.Format("{0}{1}\n", prefix, id));
		builder.Append(string.Format("{0}{1}Contains : ", prefix, sprefix));

		foreach (IElement element in ((elements[])Enum.GetValues(typeof(elements)))) {
			builder.Append(string.Format("{0} ", element.getId()));
		}

		builder.Append(string.Format("\n{0}{1}Style : ", prefix, sprefix));

		foreach (Rule rule in rules) {
			builder.Append(string.Format("{0} ", rule.selector.ToString()));
		}

		builder.Append(string.Format("\n"));

		return builder.ToString();
	}

	// Nested classes

	/// <summary>
/// Description of an element that is actually modified by one or more events occurring on it.
/// </summary>
	class ElementEvents {
		// Attribute

		/// <summary>
/// Set of events on the element.
/// </summary>
		protected string[] events;

		/// <summary>
/// The element.
/// </summary>
		protected IElement element;

		/// <summary>
/// The group the element pertains to.
/// </summary>
		protected StyleGroup group;

		// Construction

		protected ElementEvents(IElement element, StyleGroup group, string evt) {
			this.element = element;
			this.group = group;
			this.events = new string[1];

			events[0] = evt;
		}

		// Access

		/// <summary>
/// The element on which the events are occurring.
/// </summary>
/// <returns>an element.</returns>
		public IElement getElement() {
			return element;
		}

		/// <summary>
/// Number of events actually affecting the element.
/// </summary>
/// <returns>The number of events affecting the element.</returns>
		public int eventCount() {
			if (events == null)
				return 0;

			return events.Length;
		}

		/// <summary>
/// The set of events actually occurring on the element.
/// </summary>
/// <returns>A set of strings.</returns>
		public string[] events() {
			return events;
		}

		// Command

		public void activate() {
			group.activateEventsFor(element);
		}

		public void deactivate() {
			group.deactivateEvents();
		}

		protected void pushEvent(string evt) {
			int n = events.Length + 1;
			string[] e = new string[n];
			bool found = false;

			for (int i = 0; i < events.Length; i++) {
				if (!events[i].Equals(evt))
					e[i] = events[i];
				else
					found = true;
			}

			e[events.Length] = evt;

			if (!found)
				events = e;
		}

		protected void popEvent(string evt) {
			if (events.Length > 1) {
				string[] e = new string[events.Length - 1];
				bool found = false;

				for (int i = 0, j = 0; i < events.Length; i++) {
					if (!events[i].Equals(evt)) {
						if (j < e.Length) {
							e[j++] = events[i];
						}
					} else {
						found = true;
					}
				}

				if (found)
					events = e;
			} else {
				if (events[0].Equals(evt)) {
					events = null;
				}
			}
		}

		
		public string toString() {
			System.Text.StringBuilder builder = new System.Text.StringBuilder();

			builder.Append(string.Format("{0} events {", element.getId()));
			foreach (string evt in events)
				builder.Append(string.Format(" {0}", evt));
			builder.Append(" }");

			return builder.ToString();
		}
	}

	/// <summary>
/// Virtual set on the elements that have not dynamic style value or evt.
/// </summary>
	protected class BulkElements : IEnumerable<IElement> {
		public IEnumerator<IElement> iterator() {
			return new BulkIterator(((elements[])Enum.GetValues(typeof(elements))).GetEnumerator());
		}
	}

	/// <summary>
/// Iterator on the set of elements that have no event or dynamic style values.
/// </summary>
	protected class BulkIterator : IEnumerator<IElement> {
		/// <summary>
/// Iterator on the set of all elements.
/// </summary>
		protected IEnumerator<IElement> iterator;

		/// <summary>
/// The next element without event or dynamic style.value.
/// </summary>
		IElement next;

		/// <summary>
/// New bulk iterator positioned on the first element with no event or dynamic style attribute.
/// </summary>
/// <param name="iterator"> Iterator on the set of all elements.</param>
		public BulkIterator(IEnumerator<IElement> iterator) {
			this.iterator = iterator;
			bool loop = true;

			while (loop && iterator.MoveNext()) {
				next = iterator.next();

				if (!elementHasEvents(next) && !elementIsDynamic(next))
					loop = false;
				else
					next = null;
			}
		}

		public bool hasNext() {
			return (next != null);
		}

		public IElement next() {
			IElement e = next;
			bool loop = true;

			next = null;

			while (loop && iterator.MoveNext()) {
				next = iterator.next();

				if (!elementIsDynamic(next) && !elementHasEvents(next))
					loop = false;
				else
					next = null;
			}

			return e;
		}

		public void remove() {
			throw new NotSupportedException("this iterator does not allows removing elements");
		}
	}
}
}
