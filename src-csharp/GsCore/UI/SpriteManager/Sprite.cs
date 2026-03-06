using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.SpriteManager
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
/// A gentle little sprite. <p> <p> Sprite objects allow to add data representations in a graphic display of a graph. A sprite is a graphical representation that can double anywhere in the graph drawing surface, or be "attached" to nodes or edges. When attached to an edge, a sprite can be positioned easily at any point along the edge, or perpendicular to it with one or two coordinates. When attached to a node, a sprite "orbits" around the node at any given radius and angle around it. </p> <p> <p> Sprites can have many shapes. Most of the CSS nodes shapes are available for sprites, but more are possible. Some shapes follow the form of the element (node or edge) they are attached to. </p> <p> <p> Sprites can be moved and animated easily along edges, around nodes, or anywhere on the graph surface. Their shape can change. Some sprites allows to draw pie charts or statistics, or images. </p> <p> <p> Sprites are not part of a graph so to speak. Furthermore they make sense only when a graph is displayed with a viewer that supports sprites. Therefore they are handled by a {@link SpriteManager} which is always associated to a graph and is in charge of handling the whole set of sprites, creating them, enumerating them, and destroying them. </p> <p> <p> Implementation note: sprites do not exist ! In fact the sprite class only handles a set of attributes that are stored in the graph (the one associated with the sprite manager that created the sprite). These special attributes are handled for you by the sprite class. This technique allows to pass sprites informations through the I/O system of GraphStream. Indeed sprites appearing in a graph can therefore be stored in files and retrieved if the graph file format supports attributes. If this is a dynamic graph format, like DGS, the whole sprite history is remembered: when it moved, when it changed, etc. </p> <p> <p> Second implementation node : often you will need to extend the sprite class. This is easily possible, but you must remember that you cannot create sprites yourself, you must use the {@link SpriteManager}. In order to create a sprite of a special kind, you can either use a {@link SpriteFactory} with the SpriteManager or the special {@link SpriteManager#addSprite(String, Type)} method of the SpriteManager. In both cases, the {@link #init(String, SpriteManager, Values)} method of the sprite will be called. Override this method to initialise your sprite. </p>
/// </summary>
public class Sprite : IElement {
	// Attribute

	/// <summary>
/// The sprite unique identifier.
/// </summary>
	protected string id;

	/// <summary>
/// The identifier prefixed by "ui.sprite.".
/// </summary>
	protected string completeId;

	/// <summary>
/// The boss.
/// </summary>
	protected SpriteManager manager;

	/// <summary>
/// Current sprite position.
/// </summary>
	protected Values position;

	/// <summary>
/// The element this sprite is attached to (or null).
/// </summary>
	protected IElement attachment;

	// Construction

	/// <summary>
/// For the use with {@link #init(String, SpriteManager, Values)}.
/// </summary>
	protected ISprite() {
	}

	/// <summary>
/// New sprite with a given identifier. <p> You cannot build sprites yourself, they are created by the sprite manager.
/// </summary>
	protected ISprite(string id, SpriteManager manager) : this(id, manager, null) {
	}

	/// <summary>
/// New sprite with a given identifier. <p> You cannot build sprites yourself, they are created by the sprite manager.
/// </summary>
	protected ISprite(string id, SpriteManager manager, Values position) {
		init(id, manager, position);
	}

	/// <summary>
/// New sprite with a given identifier. <p> You cannot build sprites yourself, they are created by the sprite managern. This method is used by the manager when creating instances of sprites that inherit this class. If you derive the sprite class you can override this method to initialise your sprite. It is always called when creating the sprite.
/// </summary>
	protected void init(string id, SpriteManager manager, Values position) {
		this.id = id;
		this.completeId = string.Format("ui.sprite.{0}", id);
		this.manager = manager;

		if (!manager.graph.hasAttribute(completeId)) {
			if (position != null) {
				manager.graph.setAttribute(completeId, position);
				this.position = position;
			} else {
				this.position = new Values(Style.Units.GU, 0f, 0f, 0f);
				manager.graph.setAttribute(completeId, this.position);
			}
		} else {
			if (position != null) {
				manager.graph.setAttribute(completeId, position);
				this.position = position;
			} else {
				this.position = SpriteManager.getPositionValue(manager.graph.getAttribute(completeId));
			}
		}
	}

	/// <summary>
/// Called by the manager when the sprite is removed.
/// </summary>
	protected void removed() {
		manager.graph.removeAttribute(completeId);

		string start = string.Format("{0}.", completeId);

		if (attached())
			detach();

		List<string> keys = new List<string>();

		manager.graph.attributeKeys().ToList().ForEach(key => {
			if (key.StartsWith(start))
				keys.Add(key);
		});

		foreach (string key in keys)
			manager.graph.removeAttribute(key);
	}

	// Access

	/// <summary>
/// The element the sprite is attached to or null if the sprite is not attached.
/// </summary>
/// <returns>An element the sprite is attached to or null.</returns>
	public IElement getAttachment() {
		return attachment;
	}

	/// <summary>
/// True if attached to an edge or node.
/// </summary>
/// <returns>False if not attached.</returns>
	public bool attached() {
		return (attachment != null);
	}

	/// <summary>
/// X position.
/// </summary>
/// <returns>The position in abscissa.</returns>
	public double getX() {
		if (position.values.Count > 0)
			return position.values[0];

		return 0;
	}

	/// <summary>
/// Y position.
/// </summary>
/// <returns>The position in ordinate.</returns>
	public double getY() {
		if (position.values.Count > 1)
			return position.values[1];

		return 0;
	}

	/// <summary>
/// Z position.
/// </summary>
/// <returns>The position in depth.</returns>
	public double getZ() {
		if (position.values.Count > 2)
			return position.values[2];

		return 0;
	}

	public Style.Units getUnits() {
		return position.units;
	}

	// Command

	/// <summary>
/// Attach the sprite to a node with the given identifier. If needed the sprite is first detached. If the given node identifier does not exist, the sprite stays in detached state.
/// </summary>
/// <param name="id"> Identifier of the node to attach to.</param>
	public void attachToNode(string id) {
		if (attachment != null)
			detach();

		attachment = manager.graph.getNode(id);

		if (attachment != null)
			attachment.setAttribute(completeId);
	}

	/// <summary>
/// Attach the sprite to an edge with the given identifier. If needed the sprite is first detached. If the given edge identifier does not exist, the sprite stays in detached state.
/// </summary>
/// <param name="id"> Identifier of the edge to attach to.</param>
	public void attachToEdge(string id) {
		if (attachment != null)
			detach();

		attachment = manager.graph.getEdge(id);

		if (attachment != null)
			attachment.setAttribute(completeId);
	}

	/// <summary>
/// Detach the sprite from the element it is attached to (if any).
/// </summary>
	public void detach() {
		if (attachment != null) {
			attachment.removeAttribute(completeId);
			attachment = null;
		}
	}

	public void setPosition(double percent) {
		setPosition(position.units, percent, 0, 0);
	}

	public void setPosition(double x, double y, double z) {
		setPosition(position.units, x, y, z);
	}

	public void setPosition(Style.Units units, double x, double y, double z) {
		bool changed = false;

		if (position[0] != x) {
			changed = true;
			position.setValue(0, x);
		}
		if (position[1] != y) {
			changed = true;
			position.setValue(1, y);
		}
		if (position[2] != z) {
			changed = true;
			position.setValue(2, z);
		}
		if (position.units != units) {
			changed = true;
			position.setUnits(units);
		}

		if (changed)
			manager.graph.setAttribute(completeId, new Values(position));
	}

	protected void setPosition(Values values) {
		if (values != null) {
			int n = values.values.Count;

			if (n > 2) {
				setPosition(values.units, values[0], values[1], values[2]);
			} else if (n > 0) {
				setPosition(values[0]);
			}
		}
	}

	// Access (Element)

	public string getId() {
		return id;
	}

	public string getLabel(string key) {
		return manager.graph.getLabel(string.Format("{0}.{1}", completeId, key));
	}

	public object getAttribute(string key) {
		return manager.graph.getAttribute(string.Format("{0}.{1}", completeId, key));
	}

	public T getAttribute<T>(string key, Type clazz) {
		return manager.graph.getAttribute(string.Format("{0}.{1}", completeId, key), clazz);
	}

	/// <summary>
/// Quite expensive operation !.
/// </summary>
	public int getAttributeCount() {
		string start = string.Format("{0}.", completeId);

		return (int) manager.graph.attributeKeys().Where(key => key.StartsWith(start)).Count();
	}

	
	public IEnumerable<string> attributeKeys() {
		throw new Exception("not implemented");
	}

	public Dictionary<string, object> getAttributeMap() {
		throw new Exception("not implemented");
	}

	public object getFirstAttributeOf(params string[] keys) {
		string[] completeKeys = new string[keys.Length];
		int i = 0;

		foreach (string key in keys) {
			completeKeys[i] = string.Format("{0}.{1}", completeId, key);
			i++;
		}

		return manager.graph.getFirstAttributeOf(completeKeys);
	}

	public T getFirstAttributeOf<T>(Type clazz, params string[] keys) {
		string[] completeKeys = new string[keys.Length];
		int i = 0;

		foreach (string key in keys) {
			completeKeys[i] = string.Format("{0}.{1}", completeId, key);
			i++;
		}

		return manager.graph.getFirstAttributeOf(clazz, completeKeys);
	}

	public object[] getArray(string key) {
		return manager.graph.getArray(string.Format("{0}.{1}", completeId, key));
	}

	public Dictionary<object, object> getMap(string key) {
		return manager.graph.getMap(string.Format("{0}.{1}", completeId, key));
	}

	public double getNumber(string key) {
		return manager.graph.getNumber(string.Format("{0}.{1}", completeId, key));
	}

	public List<IConvertible> getVector(string key) {
		return manager.graph.getVector(string.Format("{0}.{1}", completeId, key));
	}

	public bool hasAttribute(string key) {
		return manager.graph.hasAttribute(string.Format("{0}.{1}", completeId, key));
	}

	public bool hasArray(string key) {
		return manager.graph.hasArray(string.Format("{0}.{1}", completeId, key));
	}

	public bool hasAttribute(string key, Type clazz) {
		return manager.graph.hasAttribute(string.Format("{0}.{1}", completeId, key), clazz);
	}

	public bool hasMap(string key) {
		return manager.graph.hasMap(string.Format("{0}.{1}", completeId, key));
	}

	public bool hasLabel(string key) {
		return manager.graph.hasLabel(string.Format("{0}.{1}", completeId, key));
	}

	public bool hasNumber(string key) {
		return manager.graph.hasNumber(string.Format("{0}.{1}", completeId, key));
	}

	public bool hasVector(string key) {
		return manager.graph.hasVector(string.Format("{0}.{1}", completeId, key));
	}

	// Commands (Element)

	public void setAttribute(string attribute, params object[] values) {
		manager.graph.setAttribute(string.Format("{0}.{1}", completeId, attribute), values);
	}

	public void setAttributes(Dictionary<string, object> attributes) {
		foreach (string key in attributes.Keys)
			manager.graph.setAttribute(string.Format("{0}.{1}", completeId, key), attributes[key]);
	}

	public void clearAttributes() {
		string start = string.Format("{0}.", completeId);

		manager.graph.attributeKeys().Where(key => key.StartsWith(start)).ToList()
				.ToList().ForEach(key => manager.graph.removeAttribute(key));
	}

	public void removeAttribute(string attribute) {
		manager.graph.removeAttribute(string.Format("{0}.{1}", completeId, attribute));
	}

	// XXX => UGLY FIX
	// Sprites do not have unique index but is this useful?
	public int getIndex() {
		return 0;
	}
}
}
