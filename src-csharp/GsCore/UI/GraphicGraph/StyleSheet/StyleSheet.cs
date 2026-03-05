using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;

namespace Org.GraphStream.UI.GraphicGraph.StyleSheet
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
/// Implementation of the style sheets that can be stored in the graphic graph.
/// </summary>
public class StyleSheet {
	// Attributes

	/// <summary>
/// The top-level default rule.
/// </summary>
	public Rule defaultRule;

	/// <summary>
/// The default, id and class rules for graphs.
/// </summary>
	public NameSpace graphRules = new NameSpace(Selector.Type.GRAPH);

	/// <summary>
/// The default, id and class rules for nodes.
/// </summary>
	public NameSpace nodeRules = new NameSpace(Selector.Type.NODE);

	/// <summary>
/// The default, id and class rules for edges.
/// </summary>
	public NameSpace edgeRules = new NameSpace(Selector.Type.EDGE);

	/// <summary>
/// The default, id and class rules for sprites.
/// </summary>
	public NameSpace spriteRules = new NameSpace(Selector.Type.SPRITE);

	/// <summary>
/// Set of listeners.
/// </summary>
	public List<StyleSheetListener> listeners = new List<StyleSheetListener>();

	// Constructors

	/// <summary>
/// New style sheet initialised to defaults.
/// </summary>
	public StyleSheet() {
		initRules();
	}

	// Access

	/// <summary>
/// The default rule for graphs.
/// </summary>
/// <returns>A rule.</returns>
	public Rule getDefaultGraphRule() {
		return graphRules.defaultRule;
	}

	/// <summary>
/// The default rule for nodes.
/// </summary>
/// <returns>A rule.</returns>
	public Rule getDefaultNodeRule() {
		return nodeRules.defaultRule;
	}

	/// <summary>
/// The default rule for edges.
/// </summary>
/// <returns>A rule.</returns>
	public Rule getDefaultEdgeRule() {
		return edgeRules.defaultRule;
	}

	/// <summary>
/// The default rule for sprites.
/// </summary>
/// <returns>A rule.</returns>
	public Rule getDefaultSpriteRule() {
		return spriteRules.defaultRule;
	}

	/// <summary>
/// The default style for graphs.
/// </summary>
/// <returns>A style.</returns>
	public Style getDefaultGraphStyle() {
		return getDefaultGraphRule().getStyle();
	}

	/// <summary>
/// The default style for nodes.
/// </summary>
/// <returns>A style.</returns>
	public Style getDefaultNodeStyle() {
		return getDefaultNodeRule().getStyle();
	}

	/// <summary>
/// The default style for edges.
/// </summary>
/// <returns>A style.</returns>
	public Style getDefaultEdgeStyle() {
		return getDefaultEdgeRule().getStyle();
	}

	/// <summary>
/// The default style for sprites.
/// </summary>
/// <returns>A style.</returns>
	public Style getDefaultSpriteStyle() {
		return getDefaultSpriteRule().getStyle();
	}

	/// <summary>
/// All the rules (default, specific and class) that apply to graphs.
/// </summary>
/// <returns>The set of rules for graphs.</returns>
	public NameSpace getGraphStyleNameSpace() {
		return graphRules;
	}

	/// <summary>
/// All the rules (default, specific and class) that apply to nodes.
/// </summary>
/// <returns>The set of rules for nodes.</returns>
	public NameSpace getNodeStyleNameSpace() {
		return nodeRules;
	}

	/// <summary>
/// All the rules (default, specific and class) that apply to edges.
/// </summary>
/// <returns>The set of rules for edges.</returns>
	public NameSpace getEdgeStyleNameSpace() {
		return edgeRules;
	}

	/// <summary>
/// All the rules (default, specific and class) that apply to sprites.
/// </summary>
/// <returns>The set of rules for sprites.</returns>
	public NameSpace getSpriteStyleNameSpace() {
		return spriteRules;
	}

	/// <summary>
/// Get the rules that match a given element. First a rule for the identifier of the element is looked for. It is looked for in its name space (nodes for Node element, etc.) If it is not found, the default rule for this kind of element is used. This rule is pushed at start of the returned array of rules. After a rule for the element is found, then the various classes the element pertains to are looked at and each class rule found is added in order in the returned array.
/// </summary>
/// <param name="element"> The element a rules are searched for.</param>
/// <returns>A set of rules matching the element, with the main rule at index 0.</returns>
	public List<Rule> getRulesFor(IElement element) {
		List<Rule> rules = null;

		if (element is IGraph) {
			rules = graphRules.getRulesFor(element);
		} else if (element is INode) {
			rules = nodeRules.getRulesFor(element);
		} else if (element is IEdge) {
			rules = edgeRules.getRulesFor(element);
		} else if (element is GraphicSprite) {
			rules = spriteRules.getRulesFor(element);
		} else {
			rules = new List<Rule>();
			rules.Add(defaultRule);
		}

		return rules;
	}

	/// <summary>
/// Compute the name of the style group and element will pertain to knowing its styling rules.
/// </summary>
/// <param name="element"> The element.</param>
/// <param name="rules"> The styling rules.</param>
/// <returns>The unique identifier of the style group for the element.</returns>
	public string getStyleGroupIdFor(IElement element, List<Rule> rules) {
		System.Text.StringBuilder builder = new System.Text.StringBuilder();

		if (element is IGraph) {
			builder.Append("g");
		} else if (element is INode) {
			builder.Append("n");
		} else if (element is IEdge) {
			builder.Append("e");
		} else if (element is GraphicSprite) {
			builder.Append("s");
		} else {
			throw new Exception("What ?");
		}

		if (rules[0].selector.getId() != null) {
			builder.Append('_');
			builder.Append(rules[0].selector.getId());
		}

		int n = rules.Count;

		if (n > 1) {
			builder.Append('(');
			builder.Append(rules[1].selector.getClazz());
			for (int i = 2; i < n; i++) {
				builder.Append(',');
				builder.Append(rules[i].selector.getClazz());
			}
			builder.Append(')');
		}

		return builder.ToString();
	}

	// Commands

	/// <summary>
/// Create the default rules. This method is the place to set defaults for specific element types. This is here that the edge width is reset to one, since the default width is larger. The default z index that is different for every class of element is also set here.
/// </summary>
	protected void initRules() {
		defaultRule = new Rule(new Selector(Selector.Type.ANY), null);

		defaultRule.getStyle().setDefaults();

		graphRules.defaultRule = new Rule(new Selector(Selector.Type.GRAPH), defaultRule);
		nodeRules.defaultRule = new Rule(new Selector(Selector.Type.NODE), defaultRule);
		edgeRules.defaultRule = new Rule(new Selector(Selector.Type.EDGE), defaultRule);
		spriteRules.defaultRule = new Rule(new Selector(Selector.Type.SPRITE), defaultRule);

		graphRules.defaultRule.getStyle().setValue("padding", new Values(Style.Units.PX, 30));
		edgeRules.defaultRule.getStyle().setValue("shape", StyleConstants.Shape.LINE);
		edgeRules.defaultRule.getStyle().setValue("size", new Values(Style.Units.PX, 1));
		edgeRules.defaultRule.getStyle().setValue("z-index", new int(1));
		nodeRules.defaultRule.getStyle().setValue("z-index", new int(2));
		spriteRules.defaultRule.getStyle().setValue("z-index", new int(3));

		Colors colors = new Colors();
		colors.Add(Color.WHITE);

		graphRules.defaultRule.getStyle().setValue("fill-color", colors);
		graphRules.defaultRule.getStyle().setValue("stroke-mode", StrokeMode.NONE);

		foreach (StyleSheetListener listener in listeners) {
			listener.styleAdded(defaultRule, defaultRule);
			listener.styleAdded(graphRules.defaultRule, graphRules.defaultRule);
			listener.styleAdded(nodeRules.defaultRule, nodeRules.defaultRule);
			listener.styleAdded(edgeRules.defaultRule, edgeRules.defaultRule);
			listener.styleAdded(spriteRules.defaultRule, spriteRules.defaultRule);
		}

		// for( StyleSheetListener listener: listeners )
		// listener.styleAdded( defaultRule, defaultRule );
		// for( StyleSheetListener listener: listeners )
		// listener.styleAdded( graphRules.defaultRule, graphRules.defaultRule
		// );
		// for( StyleSheetListener listener: listeners )
		// listener.styleAdded( nodeRules.defaultRule, nodeRules.defaultRule );
		// for( StyleSheetListener listener: listeners )
		// listener.styleAdded( edgeRules.defaultRule, edgeRules.defaultRule );
		// for( StyleSheetListener listener: listeners )
		// listener.styleAdded( spriteRules.defaultRule, spriteRules.defaultRule
		// );
	}

	/// <summary>
/// Add a listener for style events. You never receive events for default rules and styles. You receive events only for the rules and styles that are added after this listener is registered.
/// </summary>
/// <param name="listener"> The new listener.</param>
	public void addListener(StyleSheetListener listener) {
		listeners.Add(listener);
	}

	/// <summary>
/// Remove a previously registered listener.
/// </summary>
/// <param name="listener"> The listener to remove.</param>
	public void removeListener(StyleSheetListener listener) {
		int index = listeners.IndexOf(listener);

		if (index >= 0)
			listeners.Remove(index);
	}

	/// <summary>
/// Clear all specific rules and initialise the default rules. The listeners are not changed.
/// </summary>
	public void clear() {
		graphRules.Clear();
		nodeRules.Clear();
		edgeRules.Clear();
		spriteRules.Clear();
		initRules();

		foreach (StyleSheetListener listener in listeners)
			listener.styleSheetCleared();
	}

	/// <summary>
/// Parse a style sheet from a file. The style sheet will complete the previously parsed style sheets. For any kind of I/O error or parse error.
/// </summary>
/// <param name="fileName"> Name of the file containing the style sheet.</param>
	public void parseFromFile(string fileName){
		parse(new System.IO.StreamReader(new BufferedInputStream(new FileInputStream(fileName))));
	}

	/// <summary>
/// Parse a style sheet from an URL. The style sheet will complete the previously parsed style sheets. First, this method will search the URL as SystemRessource, then as a file and if there is no match, just try to create an URL object giving the URL as constructor's parameter. For any kind of I/O error or parse error.
/// </summary>
/// <param name="url"> Name of the file containing the style sheet.</param>
	public void parseFromURL(string url){
		System.Uri u = typeof(StyleSheet).getClassLoader().getResource(url);
		if (u == null) {
			string fileUrl = url.Replace("file://", "");
			System.IO.FileInfo f = new System.IO.FileInfo(fileUrl);

			if (f.Exists)
				u = f.toURI().toURL();
			else
				u = new System.Uri(url);
		}

		parse(new System.IO.StreamReader(u /* .openStream() */));
	}

	/// <summary>
/// Parse a style sheet from a string. The style sheet will complete the previously parsed style sheets. For any kind of I/O error or parse error.
/// </summary>
/// <param name="styleSheet"> The string containing the whole style sheet.</param>
	public void parseFromString(string styleSheet){
		parse(new StringReader(styleSheet));
	}

	/// <summary>
/// Parse only one style, create a rule with the given selector, and add this rule.
/// </summary>
/// <param name="select"> The elements for which this style must apply.</param>
/// <param name="styleString"> The style string to parse.</param>
	public void parseStyleFromString(Selector select, string styleString){
		StyleSheetParser parser = new StyleSheetParser(this, new StringReader(styleString));

		Style style = new Style();

		try {
			parser.stylesStart(style);
		} catch (ParseException e) {
			throw new System.IO.IOException(e.getMessage());
		}

		Rule rule = new Rule(select);

		rule.setStyle(style);
		addRule(rule);
	}

	/// <summary>
/// Load a style sheet from an attribute value, the value can either be the contents of the whole style sheet, or begin by "url". If it starts with "url", it must then contain between parenthesis the string of the URL to load. For example: <pre> url('file:///some/path/on/the/file/system') </pre> Or <pre> url('http://some/web/url') </pre> The loaded style sheet will be merged with the styles already present in the style sheet. If the loading or parsing of the style sheet failed.
/// </summary>
/// <param name="styleSheetValue"> The style sheet name of content.</param>
	public void load(string styleSheetValue){
		if (styleSheetValue.StartsWith("url")) {
			// Extract the part between '(' and ')'.

			int beg = styleSheetValue.IndexOf('(');
			int end = styleSheetValue.LastIndexOf(')');

			if (beg >= 0 && end > beg)
				styleSheetValue = styleSheetValue.Substring(beg + 1, end);

			styleSheetValue = styleSheetValue.Trim();

			// Remove the quotes (') or (").

			if (styleSheetValue.StartsWith("'")) {
				beg = 0;
				end = styleSheetValue.LastIndexOf('\'');

				if (beg >= 0 && end > beg)
					styleSheetValue = styleSheetValue.Substring(beg + 1, end);
			}

			styleSheetValue = styleSheetValue.Trim();

			if (styleSheetValue.StartsWith("\"")) {
				beg = 0;
				end = styleSheetValue.LastIndexOf('"');

				if (beg >= 0 && end > beg)
					styleSheetValue = styleSheetValue.Substring(beg + 1, end);
			}

			// That's it.

			parseFromURL(styleSheetValue);
		} else // Parse from string, the value is considered to be the style
				// sheet contents.
		{
			parseFromString(styleSheetValue);
		}
	}

	/// <summary>
/// Parse the style sheet from the given reader. For any kind of I/O error or parse error.
/// </summary>
/// <param name="reader"> The reader pointing at the style sheet.</param>
	protected void parse(System.IO.TextReader reader){
		StyleSheetParser parser = new StyleSheetParser(this, reader);

		try {
			parser.Start();
		} catch (ParseException e) {
			throw new System.IO.IOException(e.getMessage());
		}
	}

	/// <summary>
/// Add a new rule with its style. If the rule selector is just GRAPH, NODE, EDGE or SPRITE, the default corresponding rules make a copy (or augmentation) of its style. Else if an id or class is specified the rules are added (or changed/augmented if the id or class was already set) and their parent is set to the default graph, node, edge or sprite rules. If this is an event rule (or meta-class rule), its sibling rule (the same rule without the meta-class) is searched and created if not found and the event rule is added as an alternative to it.
/// </summary>
/// <param name="newRule"> The new rule.</param>
	public void addRule(Rule newRule) {
		Rule oldRule = null;

		switch (newRule.selector.getType()) {
		case ANY:
			throw new Exception("The ANY selector should never be used, it is created automatically.");
		case GRAPH:
			oldRule = graphRules.addRule(newRule);
			break;
		case NODE:
			oldRule = nodeRules.addRule(newRule);
			break;
		case EDGE:
			oldRule = edgeRules.addRule(newRule);
			break;
		case SPRITE:
			oldRule = spriteRules.addRule(newRule);
			break;
		default:
			throw new Exception("Ho ho ho ?");
		}

		foreach (StyleSheetListener listener in listeners)
			listener.styleAdded(oldRule, newRule);
	}

	
	public string toString() {
		System.Text.StringBuilder builder = new System.Text.StringBuilder();

		builder.Append("StyleSheet  {\n");
		builder.Append("  default styles:\n");
		builder.Append(defaultRule.toString(1));
		builder.Append(graphRules.toString(1));
		builder.Append(nodeRules.toString(1));
		builder.Append(edgeRules.toString(1));
		builder.Append(spriteRules.toString(1));

		return builder.ToString();
	}

	// Nested classes

	/// <summary>
/// A name space is a tuple (default rule, id rule set, class rule set). <p> The name space defines a default rule for a kind of elements, a set of rules for this kind of elements with a given identifier, and a set or rules for this kind of elements with a given class. </p>
/// </summary>
	public class NameSpace {
		// Attribute

		/// <summary>
/// The kind of elements in this name space.
/// </summary>
		public Selector.Type type;

		/// <summary>
/// The default rule for this kind of elements.
/// </summary>
		public Rule defaultRule;

		/// <summary>
/// The set of rules for this kind of elements with a given identifier.
/// </summary>
		public Dictionary<string, Rule> byId = new Dictionary<string, Rule>();

		/// <summary>
/// The set of rules for this kind of elements with a given class.
/// </summary>
		public Dictionary<string, Rule> byClass = new Dictionary<string, Rule>();

		// Constructor

		public NameSpace(Selector.Type type) {
			this.type = type;
		}

		// Access

		/// <summary>
/// The kind of elements this name space applies rules to.
/// </summary>
/// <returns>A type of element (node, edge, sprite, graph).</returns>
		public Selector.Type getGraphElementType() {
			return type;
		}

		/// <summary>
/// Number of specific (id) rules.
/// </summary>
/// <returns>The number of rules that apply to elements by their identifiers.</returns>
		public int getIdRulesCount() {
			return byId.Count;
		}

		/// <summary>
/// Number of specific (class) rules.
/// </summary>
/// <returns>The number of rules that apply to elements by their classes.</returns>
		public int getClassRulesCount() {
			return byClass.Count;
		}

		/// <summary>
/// Get the rules that match a given element. The rules are returned in a given order. The array always contain the "main" rule that matches the element. This rule is either a default rule for the kind of element given or the rule that matches its identifier if there is one. Then class rules the element has can be appended to this array in order.
/// </summary>
/// <returns>an array of rules that match the element, with the main rule at index 0.</returns>
		protected List<Rule> getRulesFor(IElement element) {
			Rule rule = byId[element.getId()];
			List<Rule> rules = new List<Rule>();

			if (rule != null)
				rules.Add(rule);
			else
				rules.Add(defaultRule);

			getClassRules(element, rules);

			if (rules.Length == 0)
				rules.Add(defaultRule);

			return rules;
		}

		/// <summary>
/// Search if the given element has classes attributes and fill the given array with the set of rules that match these classes.
/// </summary>
/// <param name="element"> The element for which classes must be found.</param>
/// <param name="rules"> The rule array to fill.</param>
		protected void getClassRules(IElement element, List<Rule> rules) {
			object o = element.getAttribute("typeof(ui)");

			if (o != null) {
				if (o is object[]) {
					foreach (object s in (object[]) o) {
						if (s is string) {
							Rule rule = byClass[(string) s];

							if (rule != null)
								rules.Add(rule);
						}
					}
				} else if (o is string) {
					string classList = ((string) o).ToString().Trim();
					string[] classes = classList.Split("\\s*,\\s*");

					foreach (string c in classes) {
						Rule rule = byClass[c];

						if (rule != null)
							rules.Add(rule);
					}
				} else {
					throw new Exception("Oups ! class attribute is of type " + o.GetType().Name);
				}
			}
		}

		// Command

		/// <summary>
/// Remove all styles.
/// </summary>
		protected void clear() {
			defaultRule = null;
			byId.Clear();
			byClass.Clear();
		}

		/// <summary>
/// Add a new rule. <p> Several cases can occur : </p> <ul> <li>The rule to add has an ID or class and the rule does not yet exists and is not an event rule : add it directly.</li> <li>If the rule has an ID or class but the rule already exists, augment to already existing rule.</li> <li>If the rule has no ID or class and is not an event, augment the default style.</li> <li>If the rule is an event, the corresponding normal rule is searched, if it does not exists, it is created then or else, the event is added to the found rule.</li> </ul>
/// </summary>
/// <param name="newRule"> The rule to add or copy.</param>
/// <returns>It the rule added augments an existing rule, this existing rule is returned, else null is returned.</returns>
		protected Rule addRule(Rule newRule) {
			Rule oldRule = null;

			if (newRule.selector.getPseudoClass() != null) {
				oldRule = addEventRule(newRule);
			} else if (newRule.selector.getId() != null) {
				oldRule = byId[newRule.selector.getId()];

				if (oldRule != null) {
					oldRule.getStyle().augment(newRule.getStyle());
				} else {
					byId[newRule.selector.getId()] = newRule;
					newRule.getStyle().reparent(defaultRule);
				}
			} else if (newRule.selector.getClazz() != null) {
				oldRule = byClass[newRule.selector.getClazz()];

				if (oldRule != null) {
					oldRule.getStyle().augment(newRule.getStyle());
				} else {
					byClass[newRule.selector.getClazz()] = newRule;
					newRule.getStyle().reparent(defaultRule);
				}
			} else {
				oldRule = defaultRule;
				defaultRule.getStyle().augment(newRule.getStyle());
				newRule = defaultRule;
			}

			// That's it.

			return oldRule;
		}

		protected Rule addEventRule(Rule newRule) {
			Rule parentRule = null;

			if (newRule.selector.getId() != null) {
				parentRule = byId[newRule.selector.getId()];

				if (parentRule == null) {
					parentRule = addRule(new Rule(new Selector(newRule.selector.getType(), newRule.selector.getId(),
							newRule.selector.getClazz())));
				}
			} else if (newRule.selector.getClazz() != null) {
				parentRule = byClass[newRule.selector.getClazz()];

				if (parentRule == null) {
					parentRule = addRule(new Rule(new Selector(newRule.selector.getType(), newRule.selector.getId(),
							newRule.selector.getClazz())));
				}
			} else {
				parentRule = defaultRule;
			}

			newRule.getStyle().reparent(parentRule);
			parentRule.getStyle().addAlternateStyle(newRule.selector.getPseudoClass(), newRule);

			return parentRule;
		}

		
		public string toString() {
			return toString(-1);
		}

		public string toString(int level) {
			string prefix = "";

			if (level > 0) {
				for (int i = 0; i < level; i++)
					prefix += "    ";
			}

			System.Text.StringBuilder builder = new System.Text.StringBuilder();

			builder.Append(string.Format("{0}{1} default style :\n", prefix, type));
			builder.Append(defaultRule.toString(level + 1));
			toStringRules(level, builder, byId, string.Format("{0}{1} id styles", prefix, type));
			toStringRules(level, builder, byClass, string.Format("{0}{1} class styles", prefix, type));

			return builder.ToString();
		}

		protected void toStringRules(int level, System.Text.StringBuilder builder, Dictionary<string, Rule> rules, string title) {
			builder.Append(title);
			builder.Append(string.Format(" :\n"));

			foreach (Rule rule in ((rules[])Enum.GetValues(typeof(rules))))
				builder.Append(rule.toString(level + 1));
		}
	}
}
}
