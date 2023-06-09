using System.Collections;
using UnityEngine;
using Yarn.Unity;

namespace CustomUtilities
{
    public static class YarnUtilities
    {
        public static void StartDialogueWhenAble(this DialogueRunner runner, string startNode)
        {
            runner.StartCoroutine(StartDialogueRoutine(runner, startNode));
        }

        static IEnumerator StartDialogueRoutine(DialogueRunner runner, string node)
        {
            yield return new WaitWhile(() => runner.IsDialogueRunning);
            yield return null;
            runner.StartDialogue(node);
        }
    }
}