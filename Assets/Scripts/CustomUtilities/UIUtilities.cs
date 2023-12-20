using System.Collections;
using System.Reflection;
using UnityEngine;

namespace CustomUtilities
{
    public static class UIUtilities 
    {
        // use reflection to get the selection state of a selectable
        public static Color GetCurrentColor(this UnityEngine.UI.Selectable selectable)
        {
            var selectableStateInfo = typeof(UnityEngine.UI.Selectable).GetProperty("currentSelectionState", BindingFlags.NonPublic | BindingFlags.Instance);
            return selectable.colors.Evaluate((int)selectableStateInfo.GetValue(selectable));
        }

        public static Color Evaluate(this UnityEngine.UI.ColorBlock block, int state)
        {
            return block.Evaluate((SelectionState)state);
        }

        public static Color Evaluate(this UnityEngine.UI.ColorBlock block, SelectionState state)
        {
            switch (state)
            {
                default:
                case SelectionState.Normal:
                    return block.normalColor;
                case SelectionState.Highlighted:
                    return block.highlightedColor;
                case SelectionState.Pressed:
                    return block.pressedColor;
                case SelectionState.Selected:
                    return block.selectedColor;
                case SelectionState.Disabled:
                    return block.disabledColor;
            }
        }

        public enum SelectionState {
            Normal = 0,
            Highlighted = 1,
            Pressed = 2,
            Selected = 3,
            Disabled = 4
        }
    }
}