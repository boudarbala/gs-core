using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Util.Time
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
/// Defines components of {@link ISODateIO}.
/// </summary>
abstract class ISODateComponent {

	/// <summary>
/// Directives shortcut of the component. This property can not be changed.
/// </summary>
	protected string directive;
	/// <summary>
/// Replacement of the directive. Could be a regular expression. The value catch will be sent to the component with <i>set(catched_value,Calendar)</i>. This property can not be changed.
/// </summary>
	protected string replace;

	/// <summary>
/// Build a new component composed of a directive name ("%.") and a replacement value.
/// </summary>
/// <param name="directive"> directive name, should start with a leading '%'.</param>
/// <param name="replace"> replace the directive with the value given here.</param>
	public ISODateComponent(string directive, string replace) {
		this.directive = directive;
		this.replace = replace;
	}

	/// <summary>
/// Access to the directive name of the component.
/// </summary>
/// <returns>directive of the component.</returns>
	public string getDirective() {
		return directive;
	}

	/// <summary>
/// Return true if this component is an alias. An alias can contain other directive name and its replacement should be parse again.
/// </summary>
/// <returns>true if component is an alias.</returns>
	public bool isAlias() {
		return false;
	}

	/// <summary>
/// Get the replacement value of this component.
/// </summary>
/// <returns>replacement value</returns>
	public string getReplacement() {
		return replace;
	}

	/// <summary>
/// Handle the value catched with the replacement value.
/// </summary>
/// <param name="value"> value matching the replacement string</param>
/// <param name="calendar"> calendar we are working on</param>
	public abstract void set(string value, Calendar calendar);

	/// <summary>
/// Get a string representation of this component for a given calendar.
/// </summary>
/// <param name="calendar"> the calendar</param>
/// <returns>string representation of this component.</returns>
	public abstract string get(Calendar calendar);

	/// <summary>
/// Defines an alias component. Such component does nothing else that replace them directive by another string.
/// </summary>
	class AliasComponent : ISODateComponent {

		public AliasComponent(string shortcut, string replace) : base(shortcut, replace) {
		}

		public bool isAlias() {
			return true;
		}

		public void set(string value, Calendar calendar) {
			// Nothing to do
		}

		public string get(Calendar calendar) {
			return "";
		}
	}

	/// <summary>
/// Defines a text component. Such component does nothing else that append text to the resulting regular expression.
/// </summary>
	class TextComponent : ISODateComponent {
		string unquoted;

		public TextComponent(string value) : base(null, System.Text.RegularExpressions.Regex.quote(value)) {
			unquoted = value;
		}

		public void set(string value, Calendar calendar) {
			// Nothing to do
		}

		public string get(Calendar calendar) {
			return unquoted;
		}
	}

	/// <summary>
/// Defines a component associated with a field of a calendar. When a value is handled, component will try to set the associated field of the calendar.
/// </summary>
	class FieldComponent : ISODateComponent {
		protected int field;
		protected int offset;
		protected string format;

		public FieldComponent(string shortcut, string replace, int field, string format) : this(shortcut, replace, field, 0, format) {
		}

		public FieldComponent(string shortcut, string replace, int field, int offset, string format) : base(shortcut, replace) {
			this.field = field;
			this.offset = offset;
			this.format = format;
		}

		public void set(string value, Calendar calendar) {
			while (value[0] == '0' && value.Length > 1)
				value = value.Substring(1);
			int val = int.Parse(value);
			calendar.set(field, val + offset);
		}

		public string get(Calendar calendar) {
			return string.Format(format, calendar[field]);
		}
	}

	/// <summary>
/// Base for locale-dependent component.
/// </summary>
	abstract class LocaleDependentComponent : ISODateComponent {
		protected Locale locale;
		protected DateFormatSymbols symbols;

		public LocaleDependentComponent(string shortcut, string replace) : this(shortcut, replace, System.Globalization.CultureInfo.InvariantCulture()) {
		}

		public LocaleDependentComponent(string shortcut, string replace, Locale locale) : base(shortcut, replace) {
			this.locale = locale;
			this.symbols = DateFormatSymbols.getInstance(locale);
		}
	}

	/// <summary>
/// Component handling AM/PM.
/// </summary>
	class AMPMComponent : LocaleDependentComponent {
		public AMPMComponent() : base("%p", "AM|PM|am|pm") {
		}

		public void set(string value, Calendar calendar) {
			if (value.Equals(symbols.getAmPmStrings()[Calendar.AM]))
				calendar.set(Calendar.AM_PM, Calendar.AM);
			else if (value.Equals(symbols.getAmPmStrings()[Calendar.PM]))
				calendar.set(Calendar.AM_PM, Calendar.PM);
		}

		public string get(Calendar calendar) {
			return symbols.getAmPmStrings()[calendar[Calendar.AM_PM]];
		}
	}

	/// <summary>
/// Component handling utc offset (+/- 0000).
/// </summary>
	class UTCOffsetComponent : ISODateComponent {
		public UTCOffsetComponent() : base("%z", "(?:[-+]\\d{4}|Z)") {
		}

		public void set(string value, Calendar calendar) {
			if (value.Equals("Z")) {
				calendar.getTimeZone().setRawOffset(0);
			} else {
				string hs = value.Substring(1, 3);
				string ms = value.Substring(3, 5);
				if (hs[0] == '0')
					hs = hs.Substring(1);
				if (ms[0] == '0')
					ms = ms.Substring(1);

				int i = value[0] == '+' ? 1 : -1;
				int h = int.Parse(hs);
				int m = int.Parse(ms);

				calendar.getTimeZone().setRawOffset(i * (h * 60 + m) * 60000);
			}
		}

		public string get(Calendar calendar) {
			int offset = calendar.getTimeZone().getRawOffset();
			string sign = "+";

			if (offset < 0) {
				sign = "-";
				offset = -offset;
			}

			offset /= 60000;

			int h = offset / 60;
			int m = offset % 60;

			return string.Format("{0}{1}{2}", sign, h, m);
		}
	}

	/// <summary>
/// Component handling a number of milliseconds since the epoch (january, 1st 1970).
/// </summary>
	class EpochComponent : ISODateComponent {
		public EpochComponent() : base("%K", "\\d+") {
		}

		public void set(string value, Calendar calendar) {
			long e = long.Parse(value);
			calendar.setTimeInMillis(e);
		}

		public string get(Calendar calendar) {
			return string.Format("{0}", calendar.getTimeInMillis());
		}
	}

	/// <summary>
/// Defines a not implemented component. Such components throw an Error if used.
/// </summary>
	class NotImplementedComponent : ISODateComponent {
		public NotImplementedComponent(string shortcut, string replace) : base(shortcut, replace) {
		}

		public void set(string value, Calendar cal) {
			throw new Error("not implemented component");
		}

		public string get(Calendar calendar) {
			throw new Error("not implemented component");
		}
	}

	public static readonly ISODateComponent ABBREVIATED_WEEKDAY_NAME = new NotImplementedComponent("%a", "\\w+[.]");
	public static readonly ISODateComponent FULL_WEEKDAY_NAME = new NotImplementedComponent("%A", "\\w+");
	public static readonly ISODateComponent ABBREVIATED_MONTH_NAME = new NotImplementedComponent("%b", "\\w+[.]");
	public static readonly ISODateComponent FULL_MONTH_NAME = new NotImplementedComponent("%B", "\\w+");
	public static readonly ISODateComponent LOCALE_DATE_AND_TIME = new NotImplementedComponent("%c", null);
	public static readonly ISODateComponent CENTURY = new NotImplementedComponent("%C", "\\d\\d");
	public static readonly ISODateComponent DAY_OF_MONTH_2_DIGITS = new FieldComponent("%d", "[012]\\d|3[01]",
			Calendar.DAY_OF_MONTH, "%02d");
	public static readonly ISODateComponent DATE = new AliasComponent("%D", "%m/%d/%y");
	public static readonly ISODateComponent DAY_OF_MONTH = new FieldComponent("%e", "\\d|[12]\\d|3[01]",
			Calendar.DAY_OF_MONTH, "%2d");
	public static readonly ISODateComponent DATE_ISO8601 = new AliasComponent("%F", "%Y-%m-%d");
	public static readonly ISODateComponent WEEK_BASED_YEAR_2_DIGITS = new FieldComponent("%g", "\\d\\d", Calendar.YEAR,
			"%02d");
	public static readonly ISODateComponent WEEK_BASED_YEAR_4_DIGITS = new FieldComponent("%G", "\\d{4}", Calendar.YEAR,
			"%04d");
	public static readonly ISODateComponent ABBREVIATED_MONTH_NAME_ALIAS = new AliasComponent("%h", "%b");
	public static readonly ISODateComponent HOUR_OF_DAY = new FieldComponent("%H", "[01]\\d|2[0123]", Calendar.HOUR_OF_DAY,
			"%02d");
	public static readonly ISODateComponent HOUR = new FieldComponent("%I", "0\\d|1[012]", Calendar.HOUR, "%02d");
	public static readonly ISODateComponent DAY_OF_YEAR = new FieldComponent("%j", "[012]\\d\\d|3[0-5]\\d|36[0-6]",
			Calendar.DAY_OF_YEAR, "%03d");
	public static readonly ISODateComponent MILLISECOND = new FieldComponent("%k", "\\d{3}", Calendar.MILLISECOND, "%03d");
	public static readonly ISODateComponent EPOCH = new EpochComponent();
	public static readonly ISODateComponent MONTH = new FieldComponent("%m", "0[1-9]|1[012]", Calendar.MONTH, -1, "%02d");
	public static readonly ISODateComponent MINUTE = new FieldComponent("%M", "[0-5]\\d", Calendar.MINUTE, "%02d");
	public static readonly ISODateComponent NEW_LINE = new AliasComponent("%n", "\n");
	public static readonly ISODateComponent AM_PM = new AMPMComponent();
	public static readonly ISODateComponent LOCALE_CLOCK_TIME_12_HOUR = new NotImplementedComponent("%r", "");
	public static readonly ISODateComponent HOUR_AND_MINUTE = new AliasComponent("%R", "%H:%M");
	public static readonly ISODateComponent SECOND = new FieldComponent("%S", "[0-5]\\d|60", Calendar.SECOND, "%02d");
	public static readonly ISODateComponent TABULATION = new AliasComponent("%t", "\t");
	public static readonly ISODateComponent TIME_ISO8601 = new AliasComponent("%T", "%H:%M:%S");
	public static readonly ISODateComponent DAY_OF_WEEK_1_7 = new FieldComponent("%u", "[1-7]", Calendar.DAY_OF_WEEK, -1,
			"%1d");
	public static readonly ISODateComponent WEEK_OF_YEAR_FROM_SUNDAY = new FieldComponent("%U", "[0-4]\\d|5[0123]",
			Calendar.WEEK_OF_YEAR, 1, "%2d");
	public static readonly ISODateComponent WEEK_NUMBER_ISO8601 = new NotImplementedComponent("%V",
			"0[1-9]|[2-4]\\d|5[0123]");
	public static readonly ISODateComponent DAY_OF_WEEK_0_6 = new FieldComponent("%w", "[0-6]", Calendar.DAY_OF_WEEK,
			"%01d");
	public static readonly ISODateComponent WEEK_OF_YEAR_FROM_MONDAY = new FieldComponent("%W", "[0-4]\\d|5[0123]",
			Calendar.WEEK_OF_YEAR, "%02d");
	public static readonly ISODateComponent LOCALE_DATE_REPRESENTATION = new NotImplementedComponent("%x", "");
	public static readonly ISODateComponent LOCALE_TIME_REPRESENTATION = new NotImplementedComponent("%X", "");
	public static readonly ISODateComponent YEAR_2_DIGITS = new FieldComponent("%y", "\\d\\d", Calendar.YEAR, "%02d");
	public static readonly ISODateComponent YEAR_4_DIGITS = new FieldComponent("%Y", "\\d{4}", Calendar.YEAR, "%04d");
	public static readonly ISODateComponent UTC_OFFSET = new UTCOffsetComponent();
	public static readonly ISODateComponent LOCALE_TIME_ZONE_NAME = new NotImplementedComponent("%Z", "\\w*");
	public static readonly ISODateComponent PERCENT = new AliasComponent("%%", "%");
}

}
