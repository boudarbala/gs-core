using System.Collections.Generic;
using System.IO;
using System.Linq;
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
/// Representation of a set of parameters. <p> The environment class mimics the environment variables available in any shell using a hash map of keys/values, the key being the variables names, excepted here they are called parameters. </p> <p> In addition, this class provides facilities to: <ul> <li>Read a parameter file and set the parameters from this file;</li> <li>Write a parameter file from the parameter of this environment;</li> <li>Parse the command line and get parameters from it;</li> <li>Take a class as argument and set all its fields having the same name as parameters in this class;</li> </ul> </p> <p> As in any shell, most of the time, the environment is global and accessible from any part of the system. Here a singleton instance of this class is created and accessible from anywhere in the JVM using the {@link #getGlobalEnvironment()} method (indeed the singleton instance is created at its first access). However, it is still possible to create a private instance of this class for use in a specific part of a program. </p> <p> To read a file of parameters, simply call the {@link #readParameterFile(String)} method. In the same way, to write a set of parameters to a file, call the {@link #writeParameterFile(String)} method. The format of the parameter file is given in the description of these methods. </p> <p> To read parameters from he command line, call the {@link #readCommandLine(String[])} or {@link #readCommandLine(String[], Collection)} methods. These methods expect a format for the command line that is described in there respective documentations. </p> <p> It is also possible to setup automatically the fields of an arbitrary object, provided these fields have name that match parameters in this environment. To do this call the {@link #initializeFieldsOf(object)} method passing the object to initialise as argument. The object to setup must provide methods of the form "setThing(Type)" where "Thing" or "thing" is the name of the field to set and "Type" is one of "int", "long", "float", "double", "String" and "boolean". For the boolean type, the accepted values meaning true are "true", "on", "1", and "yes", all other value are considered as false. </p> TODO: how (or when) does the default configuration file is read? TODO: how to handle parameters that cannot be setup in the {@link #initializeFieldsOf(object)}?
/// </summary>
public class Environment : ICloneable {
	private static readonly object /* Logger */ logger = null /* Logger */;

	// ---------- Attributes -----------

	/// <summary>
/// Name of the configuration file. Default is "config"
/// </summary>
	protected string configFileName = "config";

	/// <summary>
/// Has the configuration file been read yet?.
/// </summary>
	protected bool configFileRead = false;

	/// <summary>
/// Set of parameters. This is a hash table and not a hashmap since several thread may access this class at once.
/// </summary>
	protected Hashtable<string, string> parameters = new Hashtable<string, string>();

	/// <summary>
/// When locked the environment parameters value still can be changed but it is no more possible to add new parameters.
/// </summary>
	protected bool locked;

	// --------- Static attributes ---------

	/// <summary>
/// Global environment for the whole JVM. This global environment is available <b>and editable</b> from everywhere. It is create as soon as the {@link #getGlobalEnvironment()} static method is called if this field was not yet initialized by any other mean.
/// </summary>
	public static Environment GLOBAL_ENV;

	// --------- Static methods -----------

	/// <summary>
/// Access to the global shared environment for the whole JVM. This method allows to access a shared environment, that can be read and written from anywhere.
/// </summary>
/// <returns>A singleton instance of the global environment.</returns>
	public static Environment getGlobalEnvironment() {
		if (GLOBAL_ENV == null)
			GLOBAL_ENV = new Environment();

		return GLOBAL_ENV;
	}

	// --------- Methods -------------

	/// <summary>
/// Is the environment locked?.
/// </summary>
/// <returns>True if the environment is locked.</returns>
	public bool isLocked() {
		return locked;
	}

	/// <summary>
/// Access to a parameter in the environment.
/// </summary>
/// <param name="parameter"> The parameter name.</param>
/// <returns>The parameter value (empty string if not set).</returns>
	public string getParameter(string parameter) {
		string p = parameters[parameter];

		return (p == null) ? "" : p;
	}

	/// <summary>
/// True if the given paramter exist.
/// </summary>
/// <param name="parameter"> The parameter name.</param>
/// <returns>True if the given paramter name points to a value.</returns>
	public bool hasParameter(string parameter) {
		return (parameters[parameter] != null);
	}

	/// <summary>
/// Check a parameter expected to be of boolean type. This method returns "true" if the parameter exists and has a value that is "1", "true", "on" or "yes" (with any possible combination of upper or lower-case letters). For any other values of the parameter or if the parameter does not exist in the environment, "false" is returned.
/// </summary>
/// <param name="parameter"> The parameter name.</param>
/// <returns>True if the parameter value means "true", false for any other value or if the parameter does not exist.</returns>
	public bool getBooleanParameter(string parameter) {
		int val = getBooleanParameteri(parameter);

		return (val == 1);
	}

	/// <summary>
/// Check a parameter expected to be of boolean type. This method returns the value 1 if the parameter has value "1", "true", "on", "yes" (the case does not matter). Else it returns 0. To account the case of non-existing parameters, this method returns -1 if the given parameter does not exist.
/// </summary>
/// <param name="parameter"> The parameter name.</param>
/// <returns>1 if the parameter value means "true", 0 if it has any other value, or -1 if it does not exist.</returns>
	public int getBooleanParameteri(string parameter) {
		string p = parameters[parameter];

		if (p != null) {
			p = p.ToLower();

			if (p.Equals("1"))
				return 1;
			if (p.Equals("true"))
				return 1;
			if (p.Equals("on"))
				return 1;
			if (p.Equals("yes"))
				return 1;

			return 0;
		}

		return -1;
	}

	/// <summary>
/// Get the value of a parameter that is expected to be a number. If the parameter does not exist or is not a number, 0 is returned.
/// </summary>
/// <param name="parameter"> The parameter name.</param>
/// <returns>The numeric value of the parameter. 0 if the parameter does not exist or is not a number.</returns>
	public double getNumberParameter(string parameter) {
		string p = parameters[parameter];

		if (p != null) {
			try {
				return double.Parse(p);
			} catch (FormatException e) {
				return 0;
			}
		}

		return 0;
	}

	/// <summary>
/// Returns the number of parameters found in the configuration file.
/// </summary>
/// <returns>The number of parameters found in the configuration file.</returns>
	public int getParameterCount() {
		return parameters.Count;
	}

	/// <summary>
/// Set of all parameter names.
/// </summary>
/// <returns>A set of all the names identifying parameters in this environment.</returns>
	public HashSet<string> getParametersKeySet() {
		return parameters.Keys;
	}

	/// <summary>
/// Generate a new Environment object with a deep copy of the elements this object.
/// </summary>
/// <returns>An Environment object identical to this one</returns>
	
	public Environment clone() {
		Environment e = new Environment();
		e.configFileName = configFileName;
		e.configFileRead = configFileRead;
		e.locked = locked;
		foreach (string key in parameters.Keys) {
			e.parameters[key] = parameters[key];
		}
		return e;
	}

	/// <summary>
/// Set the value of a parameter. If the parameter already exists its old value is overwritten. This works only if the environment is not locked.
/// </summary>
/// <param name="parameter"> The parameter name.</param>
/// <param name="value"> The new parameter value.</param>
	public void setParameter(string parameter, string value) {
		if (!locked) {
			parameters[parameter] = value;
		} else {
			if (parameters[parameter] != null)
				parameters[parameter] = value;
		}
	}

	/// <summary>
/// Disallow the addition of new parameters. The already declared parameters are still modifiable, but no new parameter can be added.
/// </summary>
/// <param name="on"> If true the environment is locked.</param>
	public void lockEnvironment(bool on) {
		locked = on;
	}

	/// <summary>
/// Initialize all the fields of the given object whose name correspond to parameters of this environment. This works only if the object to initialize provides methods that begins by "set". For example if the object provides a method named "setThing(int value)", and if there is a parameter named "thing" in this environment and its value is convertible to an integer, then the method "setThing()" will be invoked on the object with the correct value.
/// </summary>
/// <param name="object"> The object to initialize.</param>
	public void initializeFieldsOf(object obj) {
		Method[] methods = obj.GetType().GetMethods();

		foreach (Method method in methods) {
			if (method.Name.StartsWith("set")) {
				Type[] types = method.getParameterTypes();

				if (types.Length == 1) {
					string name = method.Name.Substring(3, 4).ToLower() + method.Name.Substring(4);
					string value = parameters[name];

					if (value != null) {
						invokeSetMethod(obj, method, types, name, value);
					}
				}
			}
		}
	}

	/// <summary>
/// Initialize all the fields of the given object that both appear in the given field list and whose name correspond to parameters of this environment. See the {@link #initializeFieldsOf(object)} method description.
/// </summary>
/// <param name="object"> The object to initialize.</param>
/// <param name="fieldList"> The name of the fields to initialize in the object.</param>
	public void initializeFieldsOf(object obj, params string[] fieldList) {
		Method[] methods = obj.GetType().GetMethods();
		HashSet<string> names = new HashSet<string>();

		foreach (string s in fieldList)
			names.Add(s);

		foreach (Method method in methods) {
			if (method.Name.StartsWith("set")) {
				Type[] types = method.getParameterTypes();

				if (types.Length == 1) {
					string name = method.Name.Substring(3, 4).ToLower() + method.Name.Substring(4);

					if (names.Contains(name)) {
						string value = parameters[name];

						if (value != null) {
							invokeSetMethod(obj, method, types, name, value);
						}
					}
				}
			}
		}
	}

	/// <summary>
/// Initialize all the fields of the given object that both appear in the given field list and whose name correspond to parameters of this environment. See the {@link #initializeFieldsOf(object)} method description.
/// </summary>
/// <param name="object"> The object to initialize.</param>
/// <param name="fieldList"> The name of the fields to initialize in the object.</param>
	protected void initializeFieldsOf(object obj, ICollection<string> fieldList) {
		Method[] methods = obj.GetType().GetMethods();

		foreach (Method method in methods) {
			if (method.Name.StartsWith("set")) {
				Type[] types = method.getParameterTypes();

				if (types.Length == 1) {
					string name = method.Name.Substring(3).ToLower();

					if (fieldList.Contains(name)) {
						string value = parameters[name];

						if (value != null) {
							invokeSetMethod(obj, method, types, name, value);
						}
					}
				}
			}
		}
	}

	protected void invokeSetMethod(object obj, Method method, Type[] types, string name, string value) {
		try {
			// XXX a way to avoid this overlong and repetitive
			// list of setters ?

			if (types[0] == typeof(long)) {
				try {
					long val = long.Parse(value);
					method.Invoke(obj, val);
				} catch (FormatException e) {
					Console.Error.WriteLine(string.Format("cannot set '{0}' to the value '{1}', values is not a long\n",
							method.ToString(), value));
				}
			} else if (types[0] == typeof(int)) {
				try {
					int val = (int) double.Parse(value);
					method.Invoke(obj, val);
				} catch (FormatException e) {
					Console.Error.WriteLine(string.Format("cannot set '{0}' to the value '{1}', values is not a int\n",
							method.ToString(), value));
				}
			} else if (types[0] == typeof(double)) {
				try {
					double val = double.Parse(value);
					method.Invoke(obj, val);
				} catch (FormatException e) {
					Console.Error.WriteLine(string.Format("cannot set '{0}' to the value '{1}', values is not a double\n",
							method.ToString(), value));
				}
			} else if (types[0] == typeof(float)) {
				try {
					float val = float.Parse(value);
					method.Invoke(obj, val);
				} catch (FormatException e) {
					Console.Error.WriteLine(string.Format("cannot set '{0}' to the value '{1}', values is not a float\n",
							method.ToString(), value));
				}
			} else if (types[0] == typeof(bool)) {
				try {
					bool val = false;
					value = value.ToLower();

					if (value.Equals("1") || value.Equals("true") || value.Equals("yes") || value.Equals("on"))
						val = true;

					method.Invoke(obj, val);
				} catch (FormatException e) {
					Console.Error.WriteLine(string.Format("cannot set '{0}' to the value '{1}', values is not a bool\n",
							method.ToString(), value));
				}
			} else if (types[0] == typeof(string)) {
				method.Invoke(obj, value);
			} else {
				Console.Error.WriteLine(
						string.Format("cannot match parameter '{0}' and the method '{1}'\n", value, method.ToString()));
			}
		} catch (TargetInvocationException ite) {
			Console.Error.WriteLine(string.Format("cannot invoke method '{0}' : invocation targer error  {1}\n",
					method.ToString(), ite.Message));
		} catch (MemberAccessException iae) {
			Console.Error.WriteLine(string.Format("cannot invoke method '{0}' : illegal access error  {1}\n", method.ToString(),
					iae.Message));
		}
	}

	/// <summary>
/// Print all parameters to the given stream.
/// </summary>
/// <param name="out"> The output stream to use.</param>
	public void printParameters(System.IO.TextWriter out) {
		output.println(toString());
	}

	/// <summary>
/// Print all parameters the stdout.
/// </summary>
	public void printParameters() {
		printParameters(Console.Out);
	}

	
	public string toString() {
		return parameters.ToString();
	}

	/// <summary>
/// Read the parameters from the given command line array. See the more complete {@link #readCommandLine(String[], Collection)} method.
/// </summary>
/// <param name="args"> The command line.</param>
	public void readCommandLine(string[] args) {
		readCommandLine(args, null);
	}

	/// <summary>
/// Read the parameters from the given command line array. The expected format of this array is the following: <ul> <li>a word beginning by a "-" is the parameter name (for example "-param");</li> <li>if this word is immediately followed by a "=" and another word, this word is considered as its string value (for example "-param=aValue");</li> <li>If the parameter name is not followed by "=", it is considered a boolean option and its value is set to the string "true" (to set this to false simply give the string "-param=false");</li> <li>If a word is found on the command line without any preceding "-" but is followed by a "=" and by another word, then it is considered as a key,value brace</li> <li>If a word is found on the command line without any preceding "-" and is not followed by any "=", the it is considered to be a filename for a configuration file. The method will try to open this file for reading. A configuration file is composed of lines. Each line is composed of a brace key/value separated by a "=". If a line starts with a "#", then it is considered as a comment. Finally if no format is recognized the line is inserted to the <code>trashcan</code>.</li> </ul>
/// </summary>
/// <param name="args"> The command line.</param>
/// <param name="trashcan"> Will be filled by the set of unparsed strings (can be null if these strings can be ignored).</param>
	public void readCommandLine(string[] args, ICollection<string> trashcan) {
		foreach (string arg in args) {
			bool startsWithMinus = arg.StartsWith("-");
			int equalPos = arg.IndexOf('=');
			string value = "true";
			if (equalPos >= 0) {
				value = arg.Substring(equalPos + 1);
				if (startsWithMinus) {
					arg = arg.Substring(1, equalPos);
				} else {
					arg = arg.Substring(0, equalPos);
				}
				parameters[arg] = value;
			} else {
				if (startsWithMinus) {
					arg = arg.Substring(1);
					parameters[arg] = value;
				} else {
					readConfigFile(arg, trashcan);
				}
			}
		}
	}

	/// <summary>
/// Internal method that reads a configuration file.
/// </summary>
	protected void readConfigFile(string filename, ICollection<string> trashcan) {
		System.IO.StreamReader br;
		int count = 0;
		try {
			br = new System.IO.StreamReader(new System.IO.StreamReader(filename));
			string str;
			while ((str = br.readLine()) != null) {
				count++;
				if (str.Length > 0 && !str.Substring(0, 1).Equals("#")) {
					string[] val = str.Split("=");
					if (val.Length != 2) {
						if (val.Length == 1) {
							parameters[val[0].Trim()] = "true";
						} else {
							Console.Error.WriteLine(string.Format(
									"Something is wrong with the configuration file \"{0}\"near line %d :\n %s",
									filename, count, str));
							if (trashcan != null) {
								trashcan.Add(str);
							}
						}
					} else {
						string s0 = val[0].Trim();
						string s1 = val[1].Trim();
						parameters[s0] = s1;
					}
				}
			}

		} catch (FileNotFoundException fnfe) {
			System.err.printf("Tried to open \"{0}\" as a config file: file not found.%n", filename);
			if (trashcan != null) {
				trashcan.Add(filename);
			}
		} catch (System.IO.IOException ioe) {
			Console.Error.WriteLine(ioe);
			System.Environment.Exit(0);
		}
	}

	/// <summary>
/// Save the curent parameters to a file. For any output error on the given file name.
/// </summary>
/// <param name="fileName"> Name of the file to save the config in.</param>
	public void writeParameterFile(string fileName){
		BufferedWriter bw = new System.IO.StreamWriter(new System.IO.StreamWriter(fileName));
		HashSet<string> ks = parameters.Keys;

		foreach (string key in ks) {
			bw.Write(key + " = " + parameters[key]);
			bw.newLine();
		}

		bw.Close();
	}

	/// <summary>
/// Read the default configuration file. Once this file has been correctly parsed, the {@link #configFileRead} boolean is set to true.
/// </summary>
	protected void readConfigurationFile() {
		try {
			readParameterFile(configFileName);
			configFileRead = true;
		} catch (System.IO.IOException ioe) {
			Console.Error.WriteLine(string.Format("{0}  {1}  {2}\n", "Warning", "Environment",
					"Something wrong while reading the configuration file."), ioe);
		}
	}

	/// <summary>
/// Read a parameter file. The format of this file is as follows: <ul> <li>Each line contains a parameter setting or a comment;</li> <li>Lines beginning by a "#" are considered comments (be careful, a "#" in the middle of a line <b>is not</b> a comment);</li> <li>parameters settings are of the form "name=value", spaces are allowed, but space before and after the parameter name of value will be stripped.</li> </ul> For any error with the given parameter file name.
/// </summary>
/// <param name="fileName"> Name of the parameter file to read.</param>
	public void readParameterFile(string fileName){
		System.IO.StreamReader br;
		int count = 0;

		br = new System.IO.StreamReader(new System.IO.StreamReader(fileName));

		string str;

		while ((str = br.readLine()) != null) {
			count++;

			if (str.Length > 0 && !str.StartsWith("#")) {
				string[] val = str.Split("=");

				if (val.Length != 2) {
					Console.Error.WriteLine(string.Format("{0}  {1}  {2}\n", "Warn", "Environment",
							"Something is wrong in your configuration file near line " + count + " : \n"
									+ "[values]"(val)));
				} else {
					string s0 = val[0].Trim();
					string s1 = val[1].Trim();

					setParameter(s0, s1);
				}
			}
		}

		br.Close();
	}
}
}
