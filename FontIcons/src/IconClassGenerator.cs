using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotNetXtensions;

namespace FontIcons
{

	/// <summary>
	/// Generates a C# code page of constant class names
	/// from an inputed string that has a list of class names
	/// with font hex values, separated by new lines. 
	/// </summary>
	public class IconClassGenerator
	{
		#region FIELDS / SETTINGS

		/// <summary>
		/// If true font icon names will generate capitalized code names 
		/// (note: not all-caps, just first words are capitalized). E.g. 
		/// "bookmark-empty" becomes "BookmarkEmpty" instead of "bookmarkempty"
		/// when false. Default is TRUE.
		/// </summary>
		public bool opt_CapitalizeCodeNames = true;

		/// <summary>
		/// If opt_UseConstNotStaticForCodeValues is set to false, this value
		/// will determine if readonly is added to each generated property font icon
		/// value as well (e.g. public static readonly string ...).  Default is TRUE.
		/// </summary>
		public bool opt_MarkReadonlyIfStatic = true;

		/// <summary>
		/// If true any underscores in the inputed font icon names will be 
		/// DELETED in the outputed code names (e.g. "bookmark_empty" would 
		/// generate the code name "BookmarkEmpty" instead of "Bookmark_Empty".
		///  Default is TRUE.
		/// </summary>
		public bool opt_DeleteUnderscoreInCodeNames = true;

		/// <summary>
		/// True to sort items after Generate is called before generating the
		/// final code values from them.  Default is TRUE.
		/// </summary>
		public bool opt_SortItems = true;

		/// <summary>
		/// If true, each name property will be a constant value
		/// instead of just static. (e.g. public const string ...).
		///  Default is TRUE.
		/// </summary>
		public bool opt_UseConstNotStaticForCodeValues = true;

		/// <summary>
		/// The prefix to add to the name of each *css* value for the final css 
		/// sheet selectors (if you are not rendering the .css page, this is never used).
		/// </summary>
		public string opt_CssNamePrefix;

		public string opt_CodeNamePrefix;

		/// <summary>
		/// The class name of the generated code.
		/// </summary>
		public string opt_NameOfGeneratedCodeClass;

		/// <summary>
		/// The namespace name of the generated code. By default uses the namespace (via `nameof`) of this class:
		/// <c><see cref="FontIcons"/></c>.
		/// </summary>
		public string opt_NameOfGeneratedCodeNamespace = nameof(FontIcons);

		/// <summary>
		/// Gets only lines that start with the given value, e.g. if reading the FontAwesome 
		/// _variables.scss file, "$fa-var-". 
		/// </summary>
		public string opt_StartsWith;

		/// <summary>
		/// The resultant code after Generate was run.
		/// </summary>
		public string GeneratedCode;

		/// <summary>
		/// The resultant css code after Generate was run.
		/// </summary>
		public string GeneratedCssCode;
		public Dictionary<string, string> GeneratedHexValuesDictionary;

		public List<(string key, string value)> DuplicatesThatWereIgnored = new List<(string key, string value)>();

		#endregion

		/// <summary>
		/// Generates code, a static class, from an inputted
		/// new line separated group of: 
		/// <para/>
		/// <code>"class-name:hexnum", example: "beach-umbrella:E014"</code>
		/// <para />
		/// Example: The downloaded FontAwesome package has a scss file called _variables.scss,
		/// which has a list of the font names with their font hex number type as follows:
		/// 
		/// <code><![CDATA[
		/// ...
		/// $fa-var-beer: "\f0fc";
		/// $fa-var-behance: "\f1b4";
		/// $fa-var-behance-square: "\f1b5";
		/// $fa-var-bell: "\f0f3";
		/// $fa-var-bell-o: "\f0a2";
		/// $fa-var-bell-slash: "\f1f6";
		/// $fa-var-bell-slash-o: "\f1f7";
		/// $fa-var-bicycle: "\f206";
		/// $fa-var-binoculars: "\f1e5";
		/// $fa-var-birthday-cake: "\f1fd";
		/// $fa-var-bitbucket: "\f171";
		/// ...
		/// ]]></code>
		/// 
		/// To make this work, in a text document get all of these lines copied for 
		/// the fonts you want, then remove the '$fa-var-' prefix from them all in a find-replace,
		/// leaving you with a list like this which this code can work with:
		/// 
		/// <code><![CDATA[
		/// ...
		/// beer: "\f0fc";
		/// behance: "\f1b4";
		/// behance-square: "\f1b5";
		/// bell: "\f0f3";
		/// bell-o: "\f0a2";
		/// bell-slash: "\f1f6";
		/// bell-slash-o: "\f1f7";
		/// bicycle: "\f206";
		/// binoculars: "\f1e5";
		/// birthday-cake: "\f1fd";
		/// bitbucket: "\f171";
		/// ...
		/// ]]></code>
		/// 
		/// Pass in that file or text as a string into this method, from which 
		/// a list is generated containing: 
		/// 1) a class of constants for all of these font name values, with 
		/// 2) their code names optionally capitalized and with dashes removed 
		/// and so forth, depending on the settings, with 
		/// 3) a HexValuesDictionary containing the name of each font value as it's key,
		/// and the hex value as its value. 
		/// </summary>
		public string Generate(string kvLines)
		{
			if (kvLines.IsNulle()) throw new ArgumentNullException();
			if (opt_NameOfGeneratedCodeClass.IsNulle()) throw new ArgumentNullException(nameof(opt_NameOfGeneratedCodeClass));
			if (opt_NameOfGeneratedCodeNamespace.IsNulle()) throw new ArgumentNullException(nameof(opt_NameOfGeneratedCodeNamespace));

			var enumbl = kvLines.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(l => l.Trim())
				.Where(l => l.NotNulle());

			if (opt_StartsWith.NotNulle()) {
				enumbl = enumbl
					.Where(l => l.StartsWith(opt_StartsWith))
					.Select(l => l.Substring(opt_StartsWith.Length)?.Trim())
					.Where(l => l.NotNulle());
			}

			string[] lines = enumbl.ToArray();

			if (lines.IsNulle())
				return null;

			bool hasHex = lines[0].IndexOf(':') > 0;

			if (!hasHex)
				throw new NotImplementedException("Haven't tested this yet, it may work though.");
			else
				GeneratedHexValuesDictionary = new Dictionary<string, string>(lines.Length, StringComparer.OrdinalIgnoreCase);

			string cPreTeSt = "\t\tpublic " + (opt_UseConstNotStaticForCodeValues ? "const " : "static " + (opt_MarkReadonlyIfStatic ? "readonly " : null)) + "string ";

			string cPre = new StringBuilder()
				.Append($@"		public {(opt_UseConstNotStaticForCodeValues ? "const" : "static")}")
				.AppendIf(opt_MarkReadonlyIfStatic && !opt_UseConstNotStaticForCodeValues, " readonly")
				.Append(" string ")
				.ToString();

			var items = new KeyValuePair<string, string>[lines.Length];
			var colon = new char[] { ':' };

			for (int i = 0; i < lines.Length; i++) {

				string line = lines[i];

				string[] kv = line.Split(colon, StringSplitOptions.RemoveEmptyEntries);
				if (kv == null || kv.Length != 2)
					throw new Exception("Invalid items, entry was: " + line);

				string name = (hasHex ? kv[0] : line).TrimIfNeeded();
				string hex = hasHex ? kv[1].TrimIfNeeded() : null;

				if (name.IsNulle() || hex.IsNulle())
					continue;

				string codeName = GetCodeName(name);
				if (codeName.IsNulle())
					continue;

				if (hasHex) {
					if (hex.Last() == ';')
						hex = hex.Substring(0, hex.Length - 1);

					if (hex[0] == '"' && hex.Last() == '"')
						hex = hex.Substring(1, hex.Length - 2);

					if (hex[0] == '\\')
						hex = hex.Substring(1, hex.Length - 1);

					if (GeneratedHexValuesDictionary.ContainsKey(name)) {
						DuplicatesThatWereIgnored.Add((name, hex)); //throw new Exception("Duplicate key names were found");
						continue;
					}

					GeneratedHexValuesDictionary[name] = hex;
				}

				string codeLine = $@"{cPre}{codeName} = ""{opt_CodeNamePrefix}{opt_CssNamePrefix}{name}"";";
				//string codeLine = string.Concat(cPre, codeName, " = \"", opt_ClassNamePreNameValue, name, "\";");

				items[i] = new KeyValuePair<string, string>(codeName, codeLine);
			}

			if (opt_SortItems)
				items = items.OrderBy(kv => kv.Key).ToArray();

			string autoGeneratedNote = $"--- auto-generated by {nameof(FontIcons)}.{nameof(IconClassGenerator)}.{nameof(Generate)} ({DateTime.UtcNow} UTC) ---";

			var sb = new StringBuilder(40000)
				.Append($@"// {autoGeneratedNote}

using System;
using System.Collections.Generic;

namespace {opt_NameOfGeneratedCodeNamespace} 
{{
	public static class {opt_NameOfGeneratedCodeClass} 
	{{	
");

			foreach (var kv in items)
				sb.AppendLine(kv.Value);

			if (hasHex) {

				var d = new Dictionary<string, string>(items.Length);

				foreach (var kv in GeneratedHexValuesDictionary.OrderBy(kv => kv.Key).ToArray())
					d.Add(kv.Key, kv.Value);

				GeneratedHexValuesDictionary = d;

				sb.Append(@"
		public static readonly Dictionary<string, string> HexValuesDictionary = new Dictionary<string, string>() 
		{
");

				int i = 0;
				int last = items.Length - 1;
				foreach (var kv in GeneratedHexValuesDictionary) {
					sb.AppendLine($@"			{{ ""{kv.Key}"", ""{kv.Value}"" }}{(i != last ? "," : null)}");
					i++;
				}

				sb.AppendLine("		};");

				// sets the CssToHexValuesDictionary 
				string staticConstructor = $@"

		public static readonly Dictionary<string, string> CssToHexValuesDictionary;

		static {opt_NameOfGeneratedCodeClass}()
		{{
			CssToHexValuesDictionary = new Dictionary<string, string>(HexValuesDictionary.Count);
			foreach (var kv in HexValuesDictionary)
				CssToHexValuesDictionary[kv.Value] = kv.Key; //.Add(kv.Value, kv.Key);
		}}

";
				sb.AppendLine(staticConstructor);

				StringBuilder css = new StringBuilder(GeneratedHexValuesDictionary.Count * 50);

				// do these separate because we don't want the css file itself to be commented out...
				css.AppendLine($@"/* ======= CSS ======= 
{autoGeneratedNote} */

");
				sb.AppendLine("/* ======= CSS ======= ");

				foreach (var kv in GeneratedHexValuesDictionary) {
					string line = $@".{opt_CssNamePrefix}{kv.Key}:before {{ content: ""\{kv.Value}""; }}";
					css.AppendLine(line);
					sb.AppendLine(line);
				}

				sb.AppendLine("*/");

				GeneratedCssCode = css.ToString();
			}

			sb.AppendLine(
@"	}
}
");

			GeneratedCode = sb.ToString();

			return GeneratedCode;
		}

		public string GetCodeName(string className)
		{
			if (opt_DeleteUnderscoreInCodeNames)
				className = className.Replace('_', '-');

			char[] val = className.ToCharArray();

			if (opt_CapitalizeCodeNames) {
				val[0] = char.ToUpper(val[0]);

				int len = val.Length;
				for (int i = 1; i < len; i++) {
					if (val[i - 1] == '-' || val[i - 1] == '_')
						val[i] = char.ToUpper(val[i]);
				}
			}

			string name = new string(val.Where(c => c != '-').ToArray());

			if (name[0].IsNumber())
				name = '_' + name;

			return name;
		}

		public static string Generate_FA_FontAwesome_CodeFile(
			string faVariablesPath, 
			string faCsFilePathDirectory)
		{
			if (faCsFilePathDirectory.IsNulle() || !Directory.Exists(faCsFilePathDirectory))
				throw new DriveNotFoundException();

			string scssVariablesFileText = File.ReadAllText(faVariablesPath);

			var gen = new IconClassGenerator() {
				opt_CapitalizeCodeNames = true,
				opt_CssNamePrefix = "fa-",
				opt_CodeNamePrefix = "fa ",
				opt_NameOfGeneratedCodeClass = "FA",
				opt_StartsWith = "$fa-var-", //".fa-"
			};

			string generatedCode = gen.Generate(scssVariablesFileText);

			string faCsFilePath = Path.Combine(faCsFilePathDirectory, "FA.cs");
			saveFile_CommentOutOld(faCsFilePath, generatedCode);

			string faCssFilePath = Path.Combine(faCsFilePathDirectory, "font-awesome.css");
			saveFile_CommentOutOld(faCssFilePath, gen.GeneratedCssCode);

			return generatedCode;
		}

		static void saveFile_CommentOutOld(string filePath, string content)
		{
			if (File.Exists(filePath)) {

				string ext = Path.GetExtension(filePath);

				string[] lines = File.ReadAllText(filePath)
					.SplitLines();

				StringBuilder sb = new StringBuilder(lines.Sum(ln => ln?.Length ?? 0) + lines.Length * 2);

				foreach (string line in lines) {
					sb.Append("// ");
					sb.AppendLine(line);
				}

				string commentedOutFile = sb.ToString();

				string removedDir = Path.GetDirectoryName(filePath) + "/removed/";
				if (!Directory.Exists(removedDir))
					Directory.CreateDirectory(removedDir);

				string dt = DateTime.UtcNow.ToString();
				string newFilePath = $"{removedDir}{Path.GetFileName(filePath)}-removed-{dt}.txt";
				File.WriteAllText(newFilePath, commentedOutFile);
			}

			File.WriteAllText(filePath, content);
		}
	}
}