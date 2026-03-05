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
/// Set of sprites associated with a graph. <p> The sprite manager acts as a set of sprite elements that are associated with a graph. There can be only one sprite manager per graph. The sprite manager only role is to allow to create, destroy and enumerate sprites of a graph. </p> <p> See the {@link Sprite} class for an explanation of what are sprites and how to use them. </p> <p> In case you need to refine the Sprite class, you can change the {@link SpriteFactory} of this manager so that it creates specific instances of sprites instead of the default ones. This is mostly useful when all sprites will pertain to the same subclass. If you need to create several sprites of distinct subclasses, you can use the {@link #addSprite(String, Type)} and {@link #addSprite(String, Type, Values)} methods. </p>
/// </summary>
public class SpriteManager : IEnumerable<ISprite>, IAttributeSink {

	/// <summary>
/// class level logger
/// </summary>
	private static readonly object /* Logger */ logger = null /* Logger */;

	// Attribute

	/// <summary>
/// The graph to add sprites to.
/// </summary>
	protected IGraph graph;

	/// <summary>
/// The set of sprites.
/// </summary>
	protected Dictionary<string, ISprite> sprites = new Dictionary<string, ISprite>();

	/// <summary>
/// Factory to create new sprites.
/// </summary>
	protected SpriteFactory factory = new SpriteFactory();

	// Attributes

	/// <summary>
/// this acts as a lock when we are adding a sprite since we are also listener of the graph, and when we receive an "add" event, we automatically create a sprite. We can want to avoid listening at ourself.
/// </summary>
	bool attributeLock = false;

	// Construction

	/// <summary>
/// Create a new manager for sprite and bind it to the given graph. If the graph already contains attributes describing sprites, the manager is automatically filled with the existing sprites.
/// </summary>
/// <param name="graph"> The graph to associate with this manager;</param>
	public SpriteManager(IGraph graph){
		this.graph = graph;

		lookForExistingSprites();
		graph.addAttributeSink(this);
	}

	protected void lookForExistingSprites(){
		if (graph.getAttributeCount() > 0) {
			graph.attributeKeys().Where(key => key.StartsWith("ui.sprite.")).ToList().ForEach(key => {
				string id = key.Substring(10);

				if (id.IndexOf('.') < 0) {
					addSprite(id);
				} else {
					string sattr = id.Substring(id.IndexOf('.') + 1);
					id = id.Substring(0, id.IndexOf('.'));

					ISprite s = getSprite(id);

					if (s == null)
						s = addSprite(id);

					s.setAttribute(sattr, graph.getAttribute(key));
				}
			});
		}
	}

	// Access

	/// <summary>
/// Number of sprites in the manager.
/// </summary>
/// <returns>The sprite count.</returns>
	public int getSpriteCount() {
		return sprites.Count;
	}

	/// <summary>
/// True if the manager contains a sprite corresponding to the given identifier.
/// </summary>
/// <param name="identifier"> The sprite identifier to search for.</param>
	public bool hasSprite(string identifier) {
		return (sprites[identifier] != null);
	}

	/// <summary>
/// Sprite corresponding to the given identifier or null if no sprite is associated with the given identifier.
/// </summary>
/// <param name="identifier"> The sprite identifier.</param>
	public ISprite getSprite(string identifier) {
		return sprites[identifier];
	}

	/// <summary>
/// Iterable set of sprites in no particular order.
/// </summary>
/// <returns>The set of sprites.</returns>
	public IEnumerable<ISprite> sprites() {
		return ((sprites[])Enum.GetValues(typeof(sprites)));
	}

	/// <summary>
/// Iterator on the set of sprites.
/// </summary>
/// <returns>An iterator on sprites.</returns>
	public IEnumerator<ISprite> spriteIterator() {
		return ((sprites[])Enum.GetValues(typeof(sprites))).GetEnumerator();
	}

	/// <summary>
/// Iterator on the set of sprites.
/// </summary>
/// <returns>An iterator on sprites.</returns>
	public IEnumerator<ISprite> iterator() {
		return ((sprites[])Enum.GetValues(typeof(sprites))).GetEnumerator();
	}

	/// <summary>
/// The current sprite factory.
/// </summary>
/// <returns>A Sprite factory.</returns>
	public SpriteFactory getSpriteFactory() {
		return factory;
	}

	// Command

	/// <summary>
/// Detach this manager from its graph. This manager will no more be usable to create or remove sprites. However sprites not yet removed are still present as attributes in the graph and binding another sprite manager to this graph will retrieve all sprites.
/// </summary>
	public void detach() {
		graph.removeAttributeSink(this);
		sprites.Clear();

		graph = null;
	}

	/// <summary>
/// Specify the sprite factory to use. This allows to use specific sprite classes (descendants of Sprite).
/// </summary>
/// <param name="factory"> The new factory to use.</param>
	public void setSpriteFactory(SpriteFactory factory) {
		this.factory = factory;
	}

	/// <summary>
/// Reset the sprite factory to defaults.
/// </summary>
	public void resetSpriteFactory() {
		factory = new SpriteFactory();
	}

	/// <summary>
/// Add a sprite with the given identifier. If the sprite already exists, nothing is done. The sprite identifier cannot actually contain dots. This character use is reserved by the sprite mechanism. If the given identifier contains a dot.
/// </summary>
/// <param name="identifier"> The identifier of the new sprite to add.</param>
/// <returns>The created sprite.</returns>
	public ISprite addSprite(string identifier){
		return addSprite(identifier, (Values) null);
	}

	/// <summary>
/// Add a sprite with the given identifier and position. If the sprite already exists, nothing is done, excepted if the position is not null in which case it is repositioned. If the sprite does not exists, it is added and if position is not null, it is used as the initial position of the sprite. The sprite identifier cannot actually contain dots. This character use is reserved by the sprite mechanism. If the given identifier contains a dot.
/// </summary>
/// <param name="identifier"> The sprite identifier.</param>
/// <param name="position"> The sprite position (or null for (0,0,0)).</param>
/// <returns>The created sprite.</returns>
	protected ISprite addSprite(string identifier, Values position){
		if (identifier.IndexOf('.') >= 0)
			throw new InvalidSpriteIDException("ISprite identifiers cannot contain dots.");

		ISprite sprite = sprites[identifier];

		if (sprite == null) {
			attributeLock = true;
			sprite = factory.newSprite(identifier, this, position);
			sprites[identifier] = sprite;
			attributeLock = false;
		} else {
			if (position != null)
				sprite.setPosition(position);
		}

		return sprite;
	}

	/// <summary>
/// Add a sprite of a given subclass of Sprite with the given identifier. If the sprite already exists, nothing is done. This method allows to add a sprite of a chosen subclass of Sprite, without using a {@link SpriteFactory}. Most often you use a sprite factory when all sprites will pertain to the same subclass. If some sprites pertain to distinct subclasses, you can use this method.
/// </summary>
/// <param name="identifier"> The identifier of the new sprite to add.</param>
/// <param name="spriteClass"> The class of the new sprite to add.</param>
/// <returns>The created sprite.</returns>
	public T addSprite<T>(string identifier, Type spriteClass) where T : ISprite {
		return addSprite(identifier, spriteClass, null);
	}

	/// <summary>
/// Same as {@link #addSprite(String, Type)} but also allows to specify an initial position.
/// </summary>
/// <param name="identifier"> The identifier of the new sprite to add.</param>
/// <param name="spriteClass"> The class of the new sprite to add.</param>
/// <param name="position"> The sprite position, or null for position (0, 0, 0).</param>
/// <returns>The created sprite.</returns>
	public T addSprite<T>(string identifier, Type spriteClass, Values position) where T : ISprite {
		try {
			T sprite = spriteClass.newInstance();
			sprite.init(identifier, this, position);
			return sprite;
		} catch (Exception e) {
			Console.Error.WriteLine(string.Format("Error while trying to instantiate class {0}.", spriteClass.Name), e);
		}
		return null;
	}

	/// <summary>
/// Remove a sprite knowing its identifier. If no such sprite exists, this fails silently.
/// </summary>
/// <param name="identifier"> The identifier of the sprite to remove.</param>
	public void removeSprite(string identifier) {
		ISprite sprite = sprites[identifier];

		if (sprite != null) {
			attributeLock = true;
			sprites.Remove(identifier);
			sprite.removed();
			attributeLock = false;
		}
	}

	// Utility

	protected static Values getPositionValue(object value) {
		if (value is object[]) {
			object[] values = (object[]) value;

			if (values.Length == 4) {
				if (values[0] is IConvertible && values[1] is IConvertible && values[2] is IConvertible
						&& values[3] is Style.Units) {
					return new Values((Style.Units) values[3], ((IConvertible) values[0]),
							((IConvertible) values[1]), ((IConvertible) values[2]));
				} else {
					Console.Error.WriteLine("Cannot parse values[4] for sprite position.");
				}
			} else if (values.Length == 3) {
				if (values[0] is IConvertible && values[1] is IConvertible && values[2] is IConvertible) {
					return new Values(Units.GU, ((IConvertible) values[0]), ((IConvertible) values[1]),
							((IConvertible) values[2]));
				} else {
					Console.Error.WriteLine("Cannot parse values[3] for sprite position.");
				}
			} else if (values.Length == 1) {
				if (values[0] is IConvertible) {
					return new Values(Units.GU, ((IConvertible) values[0]));
				} else {
					Console.Error.WriteLine(string.Format("ISprite position percent is not a number."));
				}
			} else {
				Console.Error.WriteLine(string.Format("Cannot transform value '{0}' (length={1}) into a position.",
						"[values]"(values), values.Length));
			}
		} else if (value is IConvertible) {
			return new Values(Units.GU, ((IConvertible) value));
		} else if (value is Value) {
			return new Values((Value) value);
		} else if (value is Values) {
			return new Values((Values) value);
		} else {
			System.err.printf("GraphicGraph : cannot place sprite with posiiton '{0}' (instance of {1})\n", value,
					value.GetType().Name);
		}

		return null;
	}

	// GraphAttributesListener

	public void graphAttributeAdded(string graphId, long time, string attribute, object value) {
		if (attributeLock)
			return; // We want to avoid listening at ourselves.

		if (attribute.StartsWith("ui.sprite.")) {
			string spriteId = attribute.Substring(10);

			if (spriteId.IndexOf('.') < 0) {
				if (getSprite(spriteId) == null) {
					// A sprite has been created by another entity.
					// Synchronise this manager.

					Values position = null;

					if (value != null)
						position = getPositionValue(value);

					try {
						addSprite(spriteId, position);
					} catch (InvalidSpriteIDException e) {
						Console.Error.WriteLine(e);
						throw new Exception(e);
						// Ho !! Dirty !!
					}
				}
			}
		}
	}

	public void graphAttributeChanged(string graphId, long time, string attribute, object oldValue, object newValue) {
		if (attributeLock)
			return; // We want to avoid listening at ourselves.

		if (attribute.StartsWith("ui.sprite.")) {
			string spriteId = attribute.Substring(10);

			if (spriteId.IndexOf('.') < 0) {
				ISprite s = getSprite(spriteId);

				if (s != null) {
					// The sprite has been moved by another entity.
					// Update its position.

					if (newValue != null) {
						Values position = getPositionValue(newValue);
						s.setPosition(position);
					} else {
						Console.Error.WriteLine(
								string.Format("{0} changed but newValue == null ! (old={1}).", spriteId, oldValue));
					}
				} else {
					throw new InvalidOperationException("ISprite changed, but not added.");
				}
			}
		}
	}

	public void graphAttributeRemoved(string graphId, long time, string attribute) {
		if (attributeLock)
			return; // We want to avoid listening at ourselves.

		if (attribute.StartsWith("ui.sprite.")) {
			string spriteId = attribute.Substring(10);

			if (spriteId.IndexOf('.') < 0) {
				if (getSprite(spriteId) != null) {
					// A sprite has been removed by another entity.
					// Synchronise this manager.

					removeSprite(spriteId);
				}
			}
		}
	}

	// Unused.

	public void edgeAttributeAdded(string graphId, long time, string edgeId, string attribute, object value) {
	}

	public void edgeAttributeChanged(string graphId, long time, string edgeId, string attribute, object oldValue,
			object newValue) {
	}

	public void edgeAttributeRemoved(string graphId, long time, string edgeId, string attribute) {
	}

	public void nodeAttributeAdded(string graphId, long time, string nodeId, string attribute, object value) {
	}

	public void nodeAttributeChanged(string graphId, long time, string nodeId, string attribute, object oldValue,
			object newValue) {
	}

	public void nodeAttributeRemoved(string graphId, long time, string nodeId, string attribute) {
	}
}
}
