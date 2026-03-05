using System.Collections.Generic;
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
/// Style application rule. <p> A rule is made of a selector and values. The selector identifies the element(s) this rule applies to, and the values are styles to apply to the matched elements. </p>
/// </summary>
public class Rule {
	// Attributes

	/// <summary>
/// The match.
/// </summary>
	public Selector selector;

	/// <summary>
/// The style.
/// </summary>
	public Style style;

	/// <summary>
/// Optionally, the rule can store all the style groups it participates in.
/// </summary>
	public HashSet<string> groups;

	// Constructors

	protected Rule() {
	}

	/// <summary>
/// New rule with a matcher.
/// </summary>
/// <param name="selector"> The rule selector.</param>
	public Rule(Selector selector) {
		this.selector = selector;
	}

	public Rule(Selector selector, Rule parent) {
		this.selector = selector;
		this.style = new Style(parent);
	}

	/// <summary>
/// This rule style.
/// </summary>
/// <returns>The rule style.</returns>
	public Style getStyle() {
		return style;
	}

	/// <summary>
/// The group this rule participate in, maybe null if the rule does not participate in any group.
/// </summary>
/// <returns>The group set or null.</returns>
	public HashSet<string> getGroups() {
		return groups;
	}

	/// <summary>
/// True if this rule selector match the given identifier.
/// </summary>
/// <param name="identifier"> The identifier to test for the match.</param>
/// <returns>True if matching.</returns>
	public bool matchId(string identifier) {
		string ident = selector.getId();

		if (ident != null)
			return ident.Equals(identifier);

		return false;
	}

	/// <summary>
/// Change the style.
/// </summary>
/// <param name="style"> A style specification.</param>
	public void setStyle(Style style) {
		this.style = style;
	}

	/// <summary>
/// Specify that this rule participates in the given style group.
/// </summary>
/// <param name="groupId"> The group unique identifier.</param>
	public void addGroup(string groupId) {
		if (groups == null)
			groups = new HashSet<string>();
		groups.Add(groupId);
	}

	/// <summary>
/// Remove this rule from the style group.
/// </summary>
/// <param name="groupId"> The group unique identifier.</param>
	public void removeGroup(string groupId) {
		if (groups != null)
			groups.Remove(groupId);
	}

	
	public string toString() {
		return toString(-1);
	}

	public string toString(int level) {
		System.Text.StringBuilder builder = new System.Text.StringBuilder();
		string prefix = "";

		if (level > 0) {
			for (int i = 0; i < level; i++)
				prefix += "    ";
		}

		builder.Append(prefix);
		builder.Append(selector.ToString());
		builder.Append(style.toString(level + 1));

		return builder.ToString();
	}
}
}
