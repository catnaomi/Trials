namespace MetaSheets {
	///		Meta Sheets Free
	///		Copyright © 2017 renderhjs
	///		Version 2.00.0
	///		Website www.metasheets.com
	using System.Collections;
	using System.Collections.Generic;
	using System;
	using UnityEngine;
	using UnityEditor;
	using System.Linq;
	[Serializable]
	public class Configuration {
#region Properties
		public string className         = "Data";//Default class name
		public string documentKey       = "";
		public string documentTitle     = "";
#endregion
		public Configuration(string className, string documentKey) {
			this.className		= className;
			this.documentKey	= documentKey;
		}
		public void Apply(Configuration configuration) {
			this.className			= configuration.className;
			this.documentKey		= configuration.documentKey;
			this.documentTitle		= configuration.documentTitle;
		}
		public string DocumentURL {
			get {
				return string.Format(@"https://spreadsheets.google.com/feeds/worksheets/{0}/public/basic?alt=json-in-script",documentKey);
			}
		}
#region Cache
		private static bool cacheLoaded = false;
		private static string cacheString = "";
		public static Configuration Cache {
			get {
				if (cacheString.Length == 0 || !cacheLoaded) {
					cacheString = EditorPrefs.GetString("MetaSheetsCache"); 
					cacheLoaded = true;
				}
				Dictionary<string,string> d = new Dictionary<string, string>();
				if (cacheString.Contains("=")) {
					d = cacheString.Split(';').Select(p => p.Trim().Split('=')).ToDictionary(p => p[0], p => p[1]);
				}
				Configuration c = new Configuration(
					d.ContainsKey("className") ? d["className"] : "",
					d.ContainsKey("documentKey") ? d["documentKey"] : ""
				);
				return c;
			}
			set {
				Dictionary<string,string> d = new Dictionary<string,string>();
				d.Add("className", value.className);
				d.Add("documentKey", value.documentKey);
				cacheString = string.Join(";", d.Select(x => x.Key + "=" + x.Value).ToArray());
				EditorPrefs.SetString("MetaSheetsCache", cacheString);
			}
		}
#endregion
	}
}
