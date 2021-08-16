namespace MetaSheets{
	using UnityEngine;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	public class Copy{
		public static CopyTypes.SheetCopy copy = new CopyTypes.SheetCopy();
		static Copy(){
			copy.Init(); 
		}
	}
	namespace CopyTypes{
		public class Copy:DMetaSheetsCopy{
			public string id;
			public Copy(){}
			public Copy(string id, string title, string copy){
				this.id = id;
				this.title = title;
				this.copy = copy;
			}
		}
		public class SheetCopy: IEnumerable{
			public System.DateTime updated = new System.DateTime(2017,7,7,8,16,49);
			public readonly string[] labels = new string[]{"id","title","copy"};
			private Copy[] _rows = new Copy[36];
			public void Init() {
				_rows = new Copy[]{
						new Copy("popup_newSettingsFile","MetaSheets Settings","The Settings file '{0}' has been created."),
						new Copy("popup_newDataFile","Created '{0}.cs'","Meta Sheets created '{0}.cs'. Feel free to move this file to any folder in the project."),
						new Copy("popup_cantBuildWhileRunning","Cannot build at runtime","Can't generate the data sheet when the Unity Editor is playing. Stop the playback and try generating again."),
						new Copy("hint_specifyClassName","","Please specify a valid class name, for example 'Data'."),
						new Copy("hint_badClassName","","The class name is not ideal. Consider using '{0}' instead."),
						new Copy("hint_specifyDocumentKey","","Please specify a Google sheets document key. You can also paste the URL of the document in the Google Document Key field."),
						new Copy("hint_proxyUrlIncorrect","","Make sure that your proxy URL starts with a 'http'."),
						new Copy("hint_webTargetNoProxyURL","","Unity's Web Player has WWW restrictions which also apply to the Editor. Switch to a different build target like standalone to use Meta Sheets without a proxy url. \n\nTo use the current platform provide a proxy URL that is hosted on a crossdomain.xml enabled domain."),
						new Copy("hint_webTargetCantBuild","","Unity's Web Player has WWW restrictions which also apply to the Editor. Switch to a different build target like standalone to use Meta Sheets."),
						new Copy("tooltip_help","","Open the Meta Sheets Help."),
						new Copy("tooltip_settings","","Lists all {0}x settings in this Unity project. "),
						new Copy("tooltip_getPro","","Get the professional version of Meta Sheets."),
						new Copy("tooltip_openSheet","","Open this document inside Google Sheets."),
						new Copy("tooltip_createSheet","","Create a new document inside Google Sheets."),
						new Copy("tooltip_addReloadCode","","Add additional reload code that allows you to reload the data at runtime using {0}.Reload() or individually by {0}.{sheet}.Reload();"),
						new Copy("tooltip_proxyURL","","You can specify an optional proxy url that routes the reload URL to Unity. This is sometimes necessary when building Unity in a plugin environment or any platform that uses a crossdomain security sandbox."),
						new Copy("tooltip_generateCode","","Download and generate the '{0}.cs' code."),
						new Copy("notification_codeGenerated","","MetaSheets generated '{0}.cs'."),
						new Copy("notification_keyExtracted","","Extracted key from URL"),
						new Copy("help_overviewIntroduction","","The website www.metasheets.com provides an in depth manual with examples."),
						new Copy("help_overviewDataTypes","","The Data Types reference contains a list of all supported data types and how you use them. \n\nYou can reach out to support with questions or suggestions and visit the blog for some inspirations."),
						new Copy("help_timedOutIndex","Timed out","Downloading the document index took more than {0} seconds. Please try again or check your Internet connection."),
						new Copy("help_timedOutSheet","Timed out","Downloading sheet '{0}' took more than {1} seconds. Please try again or check your Internet connection."),
						new Copy("help_400BadRequest","400 Bad Request","Loading the specified document returned \"{0}\". Did you specify a correct Google Document Id?\n\nTip: You can also copy the url the URL from your document inside your browser and paste it into the Google Document Key field."),
						new Copy("help_notAccessable","Not Accessible","Did you specify a correct Google Document Id? Make sure you published the document. From Google Sheets you can publish via File > 'Publish to the web...'\n\nIf you are using 'Google Apps for Work' there is a chance that publishing the document is restricted for security reasons. In this case make sure that the creator of the sheet document uses a regular Google Account so that it can be published unlisted without the need to sign in order to view it."),
						new Copy("help_wrongSheetSetup","Wrong Sheet Setup","The {0}. row at the sheet \"{1}\" cannot be empty in its first column. Please enter a value in the cell."),
						new Copy("help_notAllParsedFree","Not all data has been parsed","Some columns and rows of your Spreadsheet were not parsed, upgrade to Meta Sheets Pro for bigger data support."),
						new Copy("help_notAllParsedPro","Not all data has been parsed","Some columns and rows of your Spreadsheet were not parsed because you exceeded either the maximum of supported {0} columns or the maximum set of rows that Meta Sheets can read."),
						new Copy("help_noContentCells","No data","You do not have any cells with content in sheet '{0}'. You need at least one row for the variables, and a second row of values. Aborting generating code."),
						new Copy("help_noContentRows","No rows of values","You need at least one row of values in sheet '{0}' starting at row number 2. Aborting generating code."),
						new Copy("help_refError","#REF Error","A cell in the sheet '{0}' has a '#REF!' error. Resolve this issue in Google Sheets first before generating data in Meta Sheets.\""),
						new Copy("help_wrongClassItem","Wrong Class item","A cell in the sheet '{0}' contains a wrong Class value of '{1}.{2}' which does not match the expected format of e.g. '{3}'."),
						new Copy("help_wrongClassArrayItem","Wrong Class Array item","A cell in the sheet '{0}' contains a wrong Class-Array value of '{1}' which does not match the expected Array format of e.g. '{2}'."),
						new Copy("settings_infoBox","","This scriptable object stores the settings for a MetaSheets document."),
						new Copy("settings_buttonOpenEditor","","Open MetaSheets Window"),
						new Copy("settings_buttonGenerate","","Generate '{0}'.cs")
					};
			}
			public IEnumerator GetEnumerator(){
				return new SheetEnumerator(this);
			}
			private class SheetEnumerator : IEnumerator{
				private int idx = -1;
				private SheetCopy t;
				public SheetEnumerator(SheetCopy t){
					this.t = t;
				}
				public bool MoveNext(){
					if (idx < t._rows.Length - 1){
						idx++;
						return true;
					}else{
						return false;
					}
				}
				public void Reset(){
					idx = -1;
				}
				public object Current{
					get{
						return t._rows[idx];
					}
				}
			}
			public int Length{ get{ return _rows.Length; } }
			public Copy this[int index]{
				get{
					return _rows[index];
				}
			}
			public Copy this[string id]{
				get{
					for (int i = 0; i < _rows.Length; i++) {
						if( _rows[i].id == id){ return _rows[i]; }
					}
					return null;
				}
			}
			public bool ContainsKey(string key){
				for (int i = 0; i < _rows.Length; i++) {
					if( _rows[i].id == key){ return true; }
				}
				return false;
			}
			public Copy[] ToArray(){
				return _rows;
			}
			public Copy Random() {
				return _rows[ UnityEngine.Random.Range(0, _rows.Length) ];
			}
			public Copy popup_newSettingsFile{	get{ return _rows[0]; } }
			public Copy popup_newDataFile{	get{ return _rows[1]; } }
			public Copy popup_cantBuildWhileRunning{	get{ return _rows[2]; } }
			public Copy hint_specifyClassName{	get{ return _rows[3]; } }
			public Copy hint_badClassName{	get{ return _rows[4]; } }
			public Copy hint_specifyDocumentKey{	get{ return _rows[5]; } }
			public Copy hint_proxyUrlIncorrect{	get{ return _rows[6]; } }
			public Copy hint_webTargetNoProxyURL{	get{ return _rows[7]; } }
			public Copy hint_webTargetCantBuild{	get{ return _rows[8]; } }
			public Copy tooltip_help{	get{ return _rows[9]; } }
			public Copy tooltip_settings{	get{ return _rows[10]; } }
			public Copy tooltip_getPro{	get{ return _rows[11]; } }
			public Copy tooltip_openSheet{	get{ return _rows[12]; } }
			public Copy tooltip_createSheet{	get{ return _rows[13]; } }
			public Copy tooltip_addReloadCode{	get{ return _rows[14]; } }
			public Copy tooltip_proxyURL{	get{ return _rows[15]; } }
			public Copy tooltip_generateCode{	get{ return _rows[16]; } }
			public Copy notification_codeGenerated{	get{ return _rows[17]; } }
			public Copy notification_keyExtracted{	get{ return _rows[18]; } }
			public Copy help_overviewIntroduction{	get{ return _rows[19]; } }
			public Copy help_overviewDataTypes{	get{ return _rows[20]; } }
			public Copy help_timedOutIndex{	get{ return _rows[21]; } }
			public Copy help_timedOutSheet{	get{ return _rows[22]; } }
			public Copy help_400BadRequest{	get{ return _rows[23]; } }
			public Copy help_notAccessable{	get{ return _rows[24]; } }
			public Copy help_wrongSheetSetup{	get{ return _rows[25]; } }
			public Copy help_notAllParsedFree{	get{ return _rows[26]; } }
			public Copy help_notAllParsedPro{	get{ return _rows[27]; } }
			public Copy help_noContentCells{	get{ return _rows[28]; } }
			public Copy help_noContentRows{	get{ return _rows[29]; } }
			public Copy help_refError{	get{ return _rows[30]; } }
			public Copy help_wrongClassItem{	get{ return _rows[31]; } }
			public Copy help_wrongClassArrayItem{	get{ return _rows[32]; } }
			public Copy settings_infoBox{	get{ return _rows[33]; } }
			public Copy settings_buttonOpenEditor{	get{ return _rows[34]; } }
			public Copy settings_buttonGenerate{	get{ return _rows[35]; } }
		}
	}
}
