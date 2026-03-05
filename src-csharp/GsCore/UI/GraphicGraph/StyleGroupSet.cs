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
/// A set of style groups. <p> This class is in charge or storing all the style groups and to update them. Each time an element is added or removed the groups are updated. Each time the style sheet changes the groups are updated. </p>
/// </summary>
public class StyleGroupSet : StyleSheetListener {
	// Attribute

	/// <summary>
/// The style sheet.
/// </summary>
	protected StyleSheet stylesheet;

	/// <summary>
/// All the groups indexed by their unique identifier.
/// </summary>
	protected Dictionary<string, StyleGroup> groups = new SortedDictionary<string, StyleGroup>();

	/// <summary>
/// Allows to retrieve the group containing a node knowing the node id.
/// </summary>
	protected Dictionary<string, string> byNodeIdGroups = new SortedDictionary<string, string>();

	/// <summary>
/// Allows to retrieve the group containing an edge knowing the node id.
/// </summary>
	protected Dictionary<string, string> byEdgeIdGroups = new SortedDictionary<string, string>();

	/// <summary>
/// Allows to retrieve the group containing a sprite knowing the node id.
/// </summary>
	protected Dictionary<string, string> bySpriteIdGroups = new SortedDictionary<string, string>();

	/// <summary>
/// Allows to retrieve the group containing a graph knowing the node id.
/// </summary>
	protected Dictionary<string, string> byGraphIdGroups = new SortedDictionary<string, string>();

	/// <summary>
/// Virtual set of nodes. This set provides fake methods to make it appear as a set of nodes whereas it only maps on the node style groups.
/// </summary>
	protected NodeSet nodeSet = new NodeSet();

	/// <summary>
/// Virtual set of edges. This set provides fake methods to make it appear as a set of edges whereas it only maps on the edge style groups.
/// </summary>
	protected EdgeSet edgeSet = new EdgeSet();

	/// <summary>
/// Virtual set of sprites. This set provides fake methods to make it appear as a set of sprites whereas it only maps on the sprite style groups.
/// </summary>
	protected SpriteSet spriteSet = new SpriteSet();

	/// <summary>
/// Virtual set of graphs. This set provides fake methods to make it appear as a set of graphs whereas it only maps on the graph style groups.
/// </summary>
	protected GraphSet graphSet = new GraphSet();

	/// <summary>
/// The set of events actually occurring.
/// </summary>
	protected EventSet eventSet = new EventSet();

	/// <summary>
/// The groups sorted by their Z index.
/// </summary>
	protected ZIndex zIndex = new ZIndex();

	/// <summary>
/// Set of groups that cast shadow.
/// </summary>
	protected ShadowSet shadow = new ShadowSet();

	/// <summary>
/// Remove groups if they become empty?.
/// </summary>
	protected bool removeEmptyGroups = true;

	/// <summary>
/// Set of listeners.
/// </summary>
	protected List<StyleGroupListener> listeners = new List<object>();

	// Construction

	/// <summary>
/// New empty style group set, using the given style sheet to create style groups. The group set installs itself as a listener of the style sheet. So in order to completely stop using such a group, you must call {@link #release()}.
/// </summary>
/// <param name="stylesheet"> The style sheet to use to create groups.</param>
	public StyleGroupSet(StyleSheet stylesheet) {
		this.stylesheet = stylesheet;

		stylesheet.addListener(this);
	}

	// Access

	/// <summary>
/// Number of groups.
/// </summary>
/// <returns>The number of groups.</returns>
	public int getGroupCount() {
		return groups.Count;
	}

	/// <summary>
/// Return a group by its unique identifier. The way group identifier are constructed reflects their contents.
/// </summary>
/// <param name="groupId"> The group identifier.</param>
/// <returns>The corresponding group or null if not found.</returns>
	public StyleGroup getGroup(string groupId) {
		return groups[groupId];
	}

	/// <summary>
/// Iterator on the set of groups in no particular order.
/// </summary>
/// <returns>An iterator on the group set.</returns>
	public IEnumerator<StyleGroup> getGroupIterator() {
		return ((groups[])Enum.GetValues(typeof(groups))).GetEnumerator();
	}

	/// <summary>
/// Iterable set of groups elements, in no particular order.
/// </summary>
/// <returns>An iterable on the set of groups.</returns>
	public IEnumerable<StyleGroup> groups() {
		return ((groups[])Enum.GetValues(typeof(groups)));
	}

	/// <summary>
/// Iterator on the Z index.
/// </summary>
/// <returns>The z index iterator.</returns>
	public IEnumerator<HashSet<StyleGroup>> getZIterator() {
		return zIndex.getIterator();
	}

	/// <summary>
/// Iterable set of "subsets of groups" sorted by Z level. Each subset of groups is at the same Z level.
/// </summary>
/// <returns>The z levels.</returns>
	public IEnumerable<HashSet<StyleGroup>> zIndex() {
		return zIndex;
	}

	/// <summary>
/// Iterator on the style groups that cast a shadow.
/// </summary>
/// <returns>The shadow groups iterator.</returns>
	public IEnumerator<StyleGroup> getShadowIterator() {
		return shadow.getIterator();
	}

	/// <summary>
/// Iterable set of groups that cast shadow.
/// </summary>
/// <returns>All the groups that cast a shadow.</returns>
	public IEnumerable<StyleGroup> shadows() {
		return shadow;
	}

	/// <summary>
/// True if the set contains and styles the node whose identifier is given.
/// </summary>
/// <param name="id"> The node identifier.</param>
/// <returns>True if the node is in this set.</returns>
	public bool containsNode(string id) {
		return byNodeIdGroups.ContainsKey(id);
	}

	/// <summary>
/// True if the set contains and styles the edge whose identifier is given.
/// </summary>
/// <param name="id"> The edge identifier.</param>
/// <returns>True if the edge is in this set.</returns>
	public bool containsEdge(string id) {
		return byEdgeIdGroups.ContainsKey(id);
	}

	/// <summary>
/// True if the set contains and styles the sprite whose identifier is given.
/// </summary>
/// <param name="id"> The sprite identifier.</param>
/// <returns>True if the sprite is in this set.</returns>
	public bool containsSprite(string id) {
		return bySpriteIdGroups.ContainsKey(id);
	}

	/// <summary>
/// True if the set contains and styles the graph whose identifier is given.
/// </summary>
/// <param name="id"> The graph identifier.</param>
/// <returns>True if the graph is in this set.</returns>
	public bool containsGraph(string id) {
		return byGraphIdGroups.ContainsKey(id);
	}

	/// <summary>
/// Get an element.
/// </summary>
/// <param name="id"> The element id.</param>
/// <param name="elt2grp"> The kind of element.</param>
/// <returns>The element or null if not found.</returns>
	protected IElement getElement(string id, Dictionary<string, string> elt2grp) {
		string gid = elt2grp[id];

		if (gid != null) {
			StyleGroup group = groups[gid];
			return group.getElement(id);
		}

		return null;
	}

	/// <summary>
/// Get a node element knowing its identifier.
/// </summary>
/// <param name="id"> The node identifier.</param>
/// <returns>The node if it is in this set, else null.</returns>
	public INode getNode(string id) {
		return (INode) getElement(id, byNodeIdGroups);
	}

	/// <summary>
/// Get an edge element knowing its identifier.
/// </summary>
/// <param name="id"> The edge identifier.</param>
/// <returns>The edge if it is in this set, else null.</returns>
	public IEdge getEdge(string id) {
		return (IEdge) getElement(id, byEdgeIdGroups);
	}

	/// <summary>
/// Get a sprite element knowing its identifier.
/// </summary>
/// <param name="id"> The sprite identifier.</param>
/// <returns>The sprite if it is in this set, else null.</returns>
	public GraphicSprite getSprite(string id) {
		return (GraphicSprite) getElement(id, bySpriteIdGroups);
	}

	/// <summary>
/// Get a graph element knowing its identifier.
/// </summary>
/// <param name="id"> The graph identifier.</param>
/// <returns>The graph if it is in this set, else null.</returns>
	public IGraph getGraph(string id) {
		return (IGraph) getElement(id, byGraphIdGroups);
	}

	/// <summary>
/// The number of nodes referenced.
/// </summary>
/// <returns>The node count.</returns>
	public int getNodeCount() {
		return byNodeIdGroups.Count;
	}

	/// <summary>
/// The number of edges referenced.
/// </summary>
/// <returns>The edge count.</returns>
	public int getEdgeCount() {
		return byEdgeIdGroups.Count;
	}

	/// <summary>
/// The number of sprites referenced.
/// </summary>
/// <returns>The sprite count.</returns>
	public int getSpriteCount() {
		return bySpriteIdGroups.Count;
	}

	/// <summary>
/// Iterator on the set of nodes.
/// </summary>
/// <returns>An iterator on all node elements contained in style groups.</returns>
	public IEnumerator<INode> getNodeIterator() {
		return new ElementIterator<INode>(byNodeIdGroups);
	}

	/// <summary>
/// Iterator on the set of graphs.
/// </summary>
/// <returns>An iterator on all graph elements contained in style groups.</returns>
	public IEnumerator<IGraph> getGraphIterator() {
		return new ElementIterator<IGraph>(byGraphIdGroups);
	}

	public IEnumerable<INode> nodes() {
		return byNodeIdGroups.map(entry => {
			return (INode) groups[entry.Value].getElement(entry.Key);
		});
	}

	public IEnumerable<IEdge> edges() {
		return byEdgeIdGroups.map(entry => {
			return (IEdge) groups[entry.Value].getElement(entry.Key);
		});
	}

	public IEnumerable<GraphicSprite> sprites() {
		return bySpriteIdGroups.map(entry => {
			return (GraphicSprite) groups[entry.Value].getElement(entry.Key);
		});
	}

	/// <summary>
/// Iterable set of graphs.
/// </summary>
/// <returns>The set of all graphs.</returns>
	public IEnumerable<IGraph> graphs() {
		return graphSet;
	}

	/// <summary>
/// Iterator on the set of edges.
/// </summary>
/// <returns>An iterator on all edge elements contained in style groups.</returns>
	public IEnumerator<IEdge> getEdgeIterator() {
		return new ElementIterator<IEdge>(byEdgeIdGroups);
	}

	/// <summary>
/// Iterator on the set of sprite.
/// </summary>
/// <returns>An iterator on all sprite elements contained in style groups.</returns>
	public IEnumerator<GraphicSprite> getSpriteIterator() {
		return new ElementIterator<GraphicSprite>(bySpriteIdGroups);
	}

	/// <summary>
/// Retrieve the group identifier of an element knowing the element identifier.
/// </summary>
/// <param name="element"> The element to search for.</param>
/// <returns>Identifier of the group containing the element.</returns>
	public string getElementGroup(IElement element) {
		if (element is INode) {
			return byNodeIdGroups[element.getId()];
		} else if (element is IEdge) {
			return byEdgeIdGroups[element.getId()];
		} else if (element is GraphicSprite) {
			return bySpriteIdGroups[element.getId()];
		} else if (element is IGraph) {
			return byGraphIdGroups[element.getId()];
		} else {
			throw new Exception("What ?");
		}
	}

	/// <summary>
/// Get the style of an element.
/// </summary>
/// <param name="element"> The element to search for.</param>
/// <returns>The style group of the element (which is also a style).</returns>
	public StyleGroup getStyleForElement(IElement element) {
		string gid = getElementGroup(element);

		return groups[gid];
	}

	/// <summary>
/// Get the style of a given node.
/// </summary>
/// <param name="node"> The node to search for.</param>
/// <returns>The node style.</returns>
	public StyleGroup getStyleFor(INode node) {
		string gid = byNodeIdGroups[node.getId()];
		return groups[gid];
	}

	/// <summary>
/// Get the style of a given edge.
/// </summary>
/// <param name="edge"> The edge to search for.</param>
/// <returns>The edge style.</returns>
	public StyleGroup getStyleFor(IEdge edge) {
		string gid = byEdgeIdGroups[edge.getId()];
		return groups[gid];
	}

	/// <summary>
/// Get the style of a given sprite.
/// </summary>
/// <param name="sprite"> The node to search for.</param>
/// <returns>The sprite style.</returns>
	public StyleGroup getStyleFor(GraphicSprite sprite) {
		string gid = bySpriteIdGroups[sprite.getId()];
		return groups[gid];
	}

	/// <summary>
/// Get the style of a given graph.
/// </summary>
/// <param name="graph"> The node to search for.</param>
/// <returns>The graph style.</returns>
	public StyleGroup getStyleFor(IGraph graph) {
		string gid = byGraphIdGroups[graph.getId()];
		return groups[gid];
	}

	/// <summary>
/// True if groups are removed when becoming empty. This setting allows to keep empty group when the set of elements is quite dynamic. This allows to avoid recreting groups when an element appears and disappears regularly.
/// </summary>
/// <returns>True if the groups are removed when empty.</returns>
	public bool areEmptyGroupRemoved() {
		return removeEmptyGroups;
	}

	/// <summary>
/// The Z index object.
/// </summary>
/// <returns>The Z index.</returns>
	public ZIndex getZIndex() {
		return zIndex;
	}

	/// <summary>
/// The set of style groups that cast a shadow.
/// </summary>
/// <returns>The set of shadowed style groups.</returns>
	public ShadowSet getShadowSet() {
		return shadow;
	}

	// Command

	/// <summary>
/// Release any dependency to the style sheet.
/// </summary>
	public void release() {
		stylesheet.removeListener(this);
	}

	/// <summary>
/// Empties this style group set. The style sheet is listener is not removed, use {@link #release()} to do that.
/// </summary>
	public void clear() {
		byEdgeIdGroups.Clear();
		byNodeIdGroups.Clear();
		bySpriteIdGroups.Clear();
		byGraphIdGroups.Clear();
		groups.Clear();
		zIndex.Clear();
		shadow.Clear();
	}

	/// <summary>
/// Remove or keep groups that becomes empty, if true the groups are removed. If this setting was set to false, and is now true, the group set is purged of the empty groups.
/// </summary>
/// <param name="on"> If true the groups will be removed.</param>
	public void setRemoveEmptyGroups(bool on) {
		if (removeEmptyGroups == false && on == true) {
			IEnumerator<StyleGroup> i = ((groups[])Enum.GetValues(typeof(groups))).GetEnumerator();

			while (i.MoveNext()) {
				StyleGroup g = i.next();

				if (g.Length == 0)
					i.Remove();
			}
		}

		removeEmptyGroups = on;
	}

	protected StyleGroup addGroup(string id, List<Rule> rules, IElement firstElement) {
		StyleGroup group = new StyleGroup(id, rules, firstElement, eventSet);

		groups[id] = group;
		zIndex.groupAdded(group);
		shadow.groupAdded(group);

		return group;
	}

	protected void removeGroup(StyleGroup group) {
		zIndex.groupRemoved(group);
		shadow.groupRemoved(group);
		groups.Remove(group.getId());
		group.release();
	}

	/// <summary>
/// Add an element and bind it to its style group. The group is created if needed.
/// </summary>
/// <param name="element"> The element to add.</param>
/// <returns>The style group where the element was added.</returns>
	public StyleGroup addElement(IElement element) {
		StyleGroup group = addElement_(element);

		foreach (StyleGroupListener listener in listeners)
			listener.elementStyleChanged(element, null, group);

		return group;
	}

	protected StyleGroup addElement_(IElement element) {
		List<Rule> rules = stylesheet.getRulesFor(element);
		string gid = stylesheet.getStyleGroupIdFor(element, rules);
		StyleGroup group = groups[gid];

		if (group == null)
			group = addGroup(gid, rules, element);
		else
			group.addElement(element);

		addElementToReverseSearch(element, gid);

		return group;
	}

	/// <summary>
/// Remove an element from the group set. If the group becomes empty after the element removal, depending on the setting of {@link #areEmptyGroupRemoved()}, the group is deleted or kept. Keeping groups allows to handle faster elements that constantly appear and disappear.
/// </summary>
/// <param name="element"> The element to remove.</param>
	public void removeElement(IElement element) {
		string gid = getElementGroup(element);
		if (null == gid) {
			return;
		}
		StyleGroup group = groups[gid];

		if (group != null) {
			group.removeElement(element);
			removeElementFromReverseSearch(element);

			if (removeEmptyGroups && group.Length == 0)
				removeGroup(group);
		}
	}

	/// <summary>
/// Check if an element need to change from a style group to another. <p> When an element can have potentially changed style due to some of its attributes (ui.class for example), instead of removing it then reading it, use this method to move the element from its current style group to a potentially different style group. </p> <p> Explanation of this method : checking the style of an element may be done by removing it ({@link #removeElement(Element)}) and then re-adding it ( {@link #addElement(Element)}). This must be done by the element since it knows when to check this. However you cannot only remove and add, since the style group inside which the element is can have events occurring on it, and these events must be passed from its old style to its new style. This method does all this information passing. </p>
/// </summary>
/// <param name="element"> The element to move.</param>
	public void checkElementStyleGroup(IElement element) {
		StyleGroup oldGroup = getGroup(getElementGroup(element));

		// Get the old element "dynamic" status.

		bool isDyn = false;

		// Get the old event set for the given element.

		StyleGroup.ElementEvents events = null;

		if (oldGroup != null) {
			isDyn = oldGroup.isElementDynamic(element);
			events = oldGroup.getEventsFor(element);
		}

		// Remove the element from its old style and add it to insert it in the
		// correct style.

		removeElement(element);
		addElement_(element);

		// Eventually push the events on the new style group.

		StyleGroup newGroup = getGroup(getElementGroup(element));

		if (newGroup != null && events != null) {
			foreach (string evt in events.events)
				pushEventFor(element, evt);
		}

		foreach (StyleGroupListener listener in listeners)
			listener.elementStyleChanged(element, oldGroup, newGroup);

		// Eventually set the element as dynamic, if it was.

		if (newGroup != null && isDyn)
			newGroup.pushElementAsDynamic(element);
	}

	protected void addElementToReverseSearch(IElement element, string groupId) {
		if (element is INode) {
			byNodeIdGroups[element.getId()] = groupId;
		} else if (element is IEdge) {
			byEdgeIdGroups[element.getId()] = groupId;
		} else if (element is GraphicSprite) {
			bySpriteIdGroups[element.getId()] = groupId;
		} else if (element is IGraph) {
			byGraphIdGroups[element.getId()] = groupId;
		} else {
			throw new Exception("What ?");
		}
	}

	protected void removeElementFromReverseSearch(IElement element) {
		if (element is INode) {
			byNodeIdGroups.Remove(element.getId());
		} else if (element is IEdge) {
			byEdgeIdGroups.Remove(element.getId());
		} else if (element is GraphicSprite) {
			bySpriteIdGroups.Remove(element.getId());
		} else if (element is IGraph) {
			byGraphIdGroups.Remove(element.getId());
		} else {
			throw new Exception("What ?");
		}
	}

	/// <summary>
/// Push a global event on the event stack. Events trigger the replacement of a style by an alternative style (or meta-class) when possible. If an event is on the event stack, each time a style has an alternative corresponding to the event, the alternative is used instead of the style.
/// </summary>
/// <param name="event"> The event to push.</param>
	public void pushEvent(string evt) {
		eventSet.pushEvent(evt);
	}

	/// <summary>
/// Push an event specifically for a given element. This is normally done automatically by the graphic element.
/// </summary>
/// <param name="element"> The element considered.</param>
/// <param name="event"> The event to push.</param>
	public void pushEventFor(IElement element, string evt) {
		StyleGroup group = getGroup(getElementGroup(element));

		if (group != null)
			group.pushEventFor(element, evt);
	}

	/// <summary>
/// Pop a global event from the event set.
/// </summary>
/// <param name="event"> The event to remove.</param>
	public void popEvent(string evt) {
		eventSet.popEvent(evt);
	}

	/// <summary>
/// Pop an event specifically for a given element. This is normally done automatically by the graphic element.
/// </summary>
/// <param name="element"> The element considered.</param>
/// <param name="event"> The event to pop.</param>
	public void popEventFor(IElement element, string evt) {
		StyleGroup group = getGroup(getElementGroup(element));

		if (group != null)
			group.popEventFor(element, evt);
	}

	/// <summary>
/// Specify the given element has dynamic style attribute values. This is normally done automatically by the graphic element.
/// </summary>
/// <param name="element"> The element to add to the dynamic subset.</param>
	public void pushElementAsDynamic(IElement element) {
		StyleGroup group = getGroup(getElementGroup(element));

		if (group != null)
			group.pushElementAsDynamic(element);
	}

	/// <summary>
/// Remove the given element from the subset of elements having dynamic style attribute values. This is normally done automatically by the graphic element.
/// </summary>
/// <param name="element"> The element to remove from the dynamic subset.</param>
	public void popElementAsDynamic(IElement element) {
		StyleGroup group = getGroup(getElementGroup(element));

		if (group != null)
			group.popElementAsDynamic(element);
	}

	/// <summary>
/// Add a listener for element style changes.
/// </summary>
/// <param name="listener"> The listener to add.</param>
	public void addListener(StyleGroupListener listener) {
		listeners.Add(listener);
	}

	/// <summary>
/// Remove a style change listener.
/// </summary>
/// <param name="listener"> The listener to remove.</param>
	public void removeListener(StyleGroupListener listener) {
		int index = listeners.LastIndexOf(listener);

		if (index >= 0) {
			listeners.Remove(index);
		}
	}

	// Listener -- What to do when a change occurs in the style sheet.

	public void styleAdded(Rule oldRule, Rule newRule) {
		// When a style change, we need to update groups.
		// Several cases :
		// 1. The style already exists
		// * Nothing to do in fact. All the elements are still in place.
		// No style rule (selectors) changed, and therefore we do not have
		// to change the groups since they are built using the selectors.
		// 2. The style is new
		// * we need to check all the groups concerning this kind of element (we
		// can
		// restrict our search to these groups, since other will not be
		// impacted),
		// and check all elements of these groups.

		if (oldRule == null)
			checkForNewStyle(newRule); // no need to check Z and shadow, done
										// when adding/changing group.
		else
			checkZIndexAndShadow(oldRule, newRule);
	}

	public void styleSheetCleared() {
		List<IElement> elements = new List<IElement>();

		foreach (IElement element in graphs())
			elements.Add(element);

		nodes().ToList().ForEach(elements::add);
		edges().ToList().ForEach(elements::add);
		sprites().ToList().ForEach(elements::add);

		clear();

		elements.ToList().ForEach(this::removeElement);
		elements.ToList().ForEach(this::addElement);
	}

	/// <summary>
/// Check each group that may have changed, for example to rebuild the Z index and the shadow set.
/// </summary>
/// <param name="oldRule"> The old rule that changed.</param>
/// <param name="newRule"> The new rule that participated in the change.</param>
	protected void checkZIndexAndShadow(Rule oldRule, Rule newRule) {
		if (oldRule != null) {
			if (oldRule.selector.getId() != null || oldRule.selector.getClazz() != null) {
				// We may accelerate things a bit when a class or id style is
				// modified,
				// since only the groups listed in the style are concerned (we
				// are at the
				// bottom of the inheritance tree).
				if (oldRule.getGroups() != null)
					foreach (string s in oldRule.getGroups()) {
						StyleGroup group = groups[s];
						if (group != null) {
							zIndex.groupChanged(group);
							shadow.groupChanged(group);
						}
					}
			} else {
				// For kind styles "NODE", "EDGE", "GRAPH", "SPRITE", we must
				// reset
				// the whole Z and shadows for the kind, since several styles
				// may
				// have changed.

				Selector.Type type = oldRule.selector.type;

				foreach (StyleGroup group in ((groups[])Enum.GetValues(typeof(groups)))) {
					if (group.getType() == type) {
						zIndex.groupChanged(group);
						shadow.groupChanged(group);
					}
				}
			}
		}
	}

	/// <summary>
/// We try to avoid at most to affect anew styles to elements and to recreate groups, which is time consuming. Two cases : <ol> <li>The style is an specific (id) style. In this case a new group may be added. <ul> <li>check an element matches the style and in this case create the group by adding the element.</li> <li>else do nothing.</li> </ul> </li> <li>The style is a kind or class style. <ul> <li>check all the groups in the kind of the style (graph, node, edge, sprite) and only in this kind (since other will never be affected).</li> <li>remove all groups of this kind.</li> <li>add all elements of this kind anew to recreate the group.</li> </ul> </li> </ol>
/// </summary>
	protected void checkForNewStyle(Rule newRule) {
		switch (newRule.selector.type) {
		case GRAPH:
			if (newRule.selector.getId() != null)
				checkForNewIdStyle(newRule, byGraphIdGroups);
			else
				checkForNewStyle(newRule, byGraphIdGroups);
			break;
		case NODE:
			if (newRule.selector.getId() != null)
				checkForNewIdStyle(newRule, byNodeIdGroups);
			else
				checkForNewStyle(newRule, byNodeIdGroups);
			break;
		case EDGE:
			if (newRule.selector.getId() != null)
				checkForNewIdStyle(newRule, byEdgeIdGroups);
			else
				checkForNewStyle(newRule, byEdgeIdGroups);
			break;
		case SPRITE:
			if (newRule.selector.getId() != null)
				checkForNewIdStyle(newRule, bySpriteIdGroups);
			else
				checkForNewStyle(newRule, bySpriteIdGroups);
			break;
		case ANY:
		default:
			throw new Exception("What ?");
		}
	}

	/// <summary>
/// Check for a new specific style (applies only to one element).
/// </summary>
/// <param name="newRule"> The new style rule.</param>
/// <param name="elt2grp"> The name space.</param>
	protected void checkForNewIdStyle(Rule newRule, Dictionary<string, string> elt2grp) {
		// There is only one element that matches the identifier.

		IElement element = getElement(newRule.selector.getId(), elt2grp);

		if (element != null) {
			checkElementStyleGroup(element);
			// removeElement( element ); // Remove the element from its old
			// group. Potentially delete a group.
			// addElement( element ); // Add the element to its new own group
			// (since this is an ID style).
		}
	}

	/// <summary>
/// Check for a new kind or class style in a given name space (node, edge, sprite, graph).
/// </summary>
/// <param name="newRule"> The new style rule.</param>
/// <param name="elt2grp"> The name space.</param>
	protected void checkForNewStyle(Rule newRule, Dictionary<string, string> elt2grp) {
		elt2grp.Keys.map(eltId => getElement(eltId, elt2grp)).ToList()
				.ToList().ForEach(this::checkElementStyleGroup);
	}

	// Utility

	
	public string toString() {
		System.Text.StringBuilder builder = new System.Text.StringBuilder();

		builder.Append(string.Format("Style groups ({0}) :\n", groups.Count));

		foreach (StyleGroup group in ((groups[])Enum.GetValues(typeof(groups)))) {
			builder.Append(group.toString(1));
			builder.Append(string.Format("\n"));
		}

		return builder.ToString();
	}

	// Inner classes

	/// <summary>
/// Set of events (meta-classes) actually active. <p> The event set contains the set of events actually occurring. This is used to select alternate styles. The events actually occurring are in precedence order. The last one is the most important. </p>
/// </summary>
	public class EventSet {
		public List<string> eventSet = new List<string>();

		public string events[] = new string[0];

		/// <summary>
/// Add an event to the set.
/// </summary>
/// <param name="event"> The event to add.</param>
		public void pushEvent(string evt) {
			eventSet.Add(evt);
			events = eventSet.toArray(events);
		}

		/// <summary>
/// Remove an event from the set.
/// </summary>
/// <param name="event"> The event to remove.</param>
		public void popEvent(string evt) {
			int index = eventSet.LastIndexOf(evt);

			if (index >= 0)
				eventSet.Remove(index);

			events = eventSet.toArray(events);
		}

		/// <summary>
/// The set of events in order, the most important at the end.
/// </summary>
/// <returns>The event set.</returns>
		public string[] getEvents() {
			return events;
		}
	}

	/// <summary>
/// All the style groups sorted by their Z index. <p> This structure is maintained by each time a group is added or removed, or when the style of a group changed. </p>
/// </summary>
	public class ZIndex : IEnumerable<HashSet<StyleGroup>> {
		/// <summary>
/// Ordered set of groups.
/// </summary>
		public List<HashSet<StyleGroup>> zIndex = new List<HashSet<StyleGroup>>();

		/// <summary>
/// Knowing a group, tell if its Z index.
/// </summary>
		public Dictionary<string, int> reverseZIndex = new Dictionary<string, int>();

		/// <summary>
/// New empty Z index.
/// </summary>
		public ZIndex() {
			initZIndex();
		}

		protected void initZIndex() {
			zIndex.ensureCapacity(256);

			for (int i = 0; i < 256; i++)
				zIndex.Add(null);
		}

		/// <summary>
/// Iterator on the set of Z index cells. Each item is a set of style groups that pertain to the same Z index.
/// </summary>
/// <returns>Iterator on the Z index.</returns>
		protected IEnumerator<HashSet<StyleGroup>> getIterator() {
			return new ZIndexIterator();
		}

		public IEnumerator<HashSet<StyleGroup>> iterator() {
			return getIterator();
		}

		/// <summary>
/// A new group appeared, put it in the z index.
/// </summary>
/// <param name="group"> The group to add.</param>
		protected void groupAdded(StyleGroup group) {
			int z = convertZ(group.getZIndex());

			if (zIndex[z] == null)
				zIndex.set(z, new HashSet<StyleGroup>());

			zIndex[z].Add(group);
			reverseZIndex[group.getId()] = z;
		}

		/// <summary>
/// A group eventually changed, check its location.
/// </summary>
/// <param name="group"> The group to check.</param>
		protected void groupChanged(StyleGroup group) {
			int oldZ = reverseZIndex[group.getId()];
			int newZ = convertZ(group.getZIndex());

			if (oldZ != newZ) {
				HashSet<StyleGroup> map = zIndex[oldZ];

				if (map != null) {
					map.Remove(group);
					reverseZIndex.Remove(group.getId());

					if (map.Length == 0)
						zIndex.set(oldZ, null);
				}

				groupAdded(group);
			}
		}

		/// <summary>
/// A group was removed, remove it from the Z index.
/// </summary>
/// <param name="group"> The group to remove.</param>
		protected void groupRemoved(StyleGroup group) {
			int z = convertZ(group.getZIndex());

			HashSet<StyleGroup> map = zIndex[z];

			if (map != null) {
				map.Remove(group);
				reverseZIndex.Remove(group.getId());

				if (map.Length == 0)
					zIndex.set(z, null);
			} else {
				throw new Exception("Inconsistency in Z-index");
			}
		}

		public void clear() {
			zIndex.Clear();
			reverseZIndex.Clear();
			initZIndex();
		}

		/// <summary>
/// Convert a [-127,127] value into a [0,255] value and check bounds.
/// </summary>
/// <param name="z"> The Z value to convert.</param>
/// <returns>The Z value converted and bounded to [0,255].</returns>
		protected int convertZ(int z) {
			z += 127;

			if (z < 0)
				z = 0;
			else if (z > 255)
				z = 255;

			return z;
		}

		
		public string toString() {
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.Append(string.Format("Z index :\n"));

			for (int i = 0; i < 256; i++) {
				if (zIndex[i] != null) {
					sb.Append(string.Format("    * {0} => ", i - 127));

					HashSet<StyleGroup> map = zIndex[i];

					foreach (StyleGroup g in map)
						sb.Append(string.Format("{0} ", g.getId()));

					sb.Append(string.Format("\n"));
				}
			}

			return sb.ToString();
		}

		public class ZIndexIterator : IEnumerator<HashSet<StyleGroup>> {
			public int index = 0;

			public ZIndexIterator() {
				zapUntilACell();
			}

			protected void zapUntilACell() {
				while (index < 256 && zIndex[index] == null)
					index++;
			}

			public bool hasNext() {
				return (index < 256);
			}

			public HashSet<StyleGroup> next() {
				if (hasNext()) {
					HashSet<StyleGroup> cell = zIndex[index];
					index++;
					zapUntilACell();
					return cell;
				}

				return null;
			}

			public void remove() {
				throw new Exception("This iterator does not support removal.");
			}
		}
	}

	/// <summary>
/// Set of groups that cast a shadow.
/// </summary>
	public class ShadowSet : IEnumerable<StyleGroup> {
		/// <summary>
/// The set of groups casting shadow.
/// </summary>
		protected HashSet<StyleGroup> shadowSet = new HashSet<StyleGroup>();

		/// <summary>
/// Iterator on the set of groups that cast a shadow.
/// </summary>
/// <returns>An iterator on the shadow style group set.</returns>
		protected IEnumerator<StyleGroup> getIterator() {
			return shadowSet.GetEnumerator();
		}

		public IEnumerator<StyleGroup> iterator() {
			return getIterator();
		}

		/// <summary>
/// A group appeared, check its shadow status.
/// </summary>
/// <param name="group"> The group added.</param>
		protected void groupAdded(StyleGroup group) {
			if (group.getShadowMode() != ShadowMode.NONE)
				shadowSet.Add(group);
		}

		/// <summary>
/// A group eventually changed, check its shadow status.
/// </summary>
/// <param name="group"> The group that changed.</param>
		protected void groupChanged(StyleGroup group) {
			if (group.getShadowMode() == ShadowMode.NONE)
				shadowSet.Remove(group);
			else
				shadowSet.Add(group);
		}

		/// <summary>
/// A group was removed, remove it from the shadow if needed.
/// </summary>
/// <param name="group"> The group removed.</param>
		protected void groupRemoved(StyleGroup group) {
			// Faster than to first test its existence or shadow status :

			shadowSet.Remove(group);
		}

		protected void clear() {
			shadowSet.Clear();
		}
	}

	/// <summary>
/// Iterator that allows to browse all graph elements of a given kind (nodes, edges, sprites, graphs) as if they where in a single set, whereas they are in style groups. The kind of graph element.
/// </summary>
	protected class ElementIterator<E> : IEnumerator<E> where E : IElement {
		protected Dictionary<string, string> elt2grp;

		protected IEnumerator<string> elts;

		public ElementIterator(Dictionary<string, string> elements2groups) {
			elt2grp = elements2groups;
			elts = elements2groups.Keys.GetEnumerator();
		}

		public bool hasNext() {
			return elts.MoveNext();
		}

		
		public E next() {
			string eid = elts.next();
			string gid = elt2grp[eid];
			StyleGroup grp = groups[gid];

			return (E) grp.getElement(eid);
		}

		public void remove() {
			throw new Exception("remove not implemented in this iterator");
		}
	}

	/// <summary>
/// Dummy set of nodes.
/// </summary>
	protected class NodeSet : IEnumerable<INode> {
		
		public IEnumerator<INode> iterator() {
			return (IEnumerator<INode>) getNodeIterator();
		}
	}

	/// <summary>
/// Dummy set of edges.
/// </summary>
	protected class EdgeSet : IEnumerable<IEdge> {
		
		public IEnumerator<IEdge> iterator() {
			return (IEnumerator<IEdge>) getEdgeIterator();
		}
	}

	/// <summary>
/// Dummy set of sprites.
/// </summary>
	protected class SpriteSet : IEnumerable<GraphicSprite> {
		
		public IEnumerator<GraphicSprite> iterator() {
			return (IEnumerator<GraphicSprite>) getSpriteIterator();
		}
	}

	protected class GraphSet : IEnumerable<GraphicGraph> {
		
		public IEnumerator<GraphicGraph> iterator() {
			return (IEnumerator<GraphicGraph>) getGraphIterator();
		}
	}

}
}
