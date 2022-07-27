// by OnatKorucu
// http://answers.unity.com/answers/1874177/view.html

 using System;
 using System.ComponentModel;
 using UnityEngine;
     
 public partial class SROptions
 {
     public static event Action OnCopyAllLogsButtonPressed;
     public static event Action OnCopyAllDistinctLogsButtonPressed;
     
     [Category("Utilities")]
     public void CopyAllLogs()
     {
         OnCopyAllLogsButtonPressed();
     }   
     
     [Category("Utilities")]
     public void CopyAllDistinctLogs()
     {
         OnCopyAllDistinctLogsButtonPressed();
     }
     
     [Category("Utilities")]
     public void ClearPlayerPrefs() {
         Debug.Log("Clearing PlayerPrefs"); 
         PlayerPrefs.DeleteAll();
     }
     
     [Category("Utilities")]
     public void CopyDeviceUniqueID() { 
         GUIUtility.systemCopyBuffer = SystemInfo.deviceUniqueIdentifier;
     }
 }