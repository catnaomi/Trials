using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class EventVisualizer : MonoBehaviour
{
    List<GameObject> targets;

    private void OnValidate()
    {
        Refresh();
    }

    public void Refresh()
    {
#if UNITY_EDITOR
        if (targets != null)
        {
            targets.Clear();
        }
        else
        {
            targets = new List<GameObject>();
        }
        Component[] monoBehaviours = this.GetComponents<Component>();

        foreach (Component mono in monoBehaviours)
        {
            if (mono == this) continue;
            if (mono is Behaviour behaviour && !behaviour.enabled) continue;
            CheckSpecialScripts(mono);

            List<UnityEvent> properties = mono.GetType()
                .GetFields()
                .Where(prop => prop.FieldType == typeof(UnityEvent))
                .Select(pi => (UnityEvent)pi.GetValue(mono))
                .ToList();

            foreach (UnityEvent e in properties)
            {
                int eventCount = e.GetPersistentEventCount();
                for (int i = 0; i < eventCount; i++)
                {
                    Object target = e.GetPersistentTarget(i);
                    if (target is Component targetMono && !targets.Contains(targetMono.gameObject))
                    {
                        targets.Add(targetMono.gameObject);
                    }
                }
            }
        }
#endif
    }

    public void CheckSpecialScripts(Component mono)
    {
        if (mono is ActivateAITrigger aiTrigger)
        {
            foreach (Actor actor in aiTrigger.actors)
            {
                if (actor == null) continue;
                targets.Add(actor.gameObject);
            }
        }
        else if (mono is IEventVisualizable visualizable)
        {
            targets.AddRange(visualizable.GetEventTargets());
        }
    }
    private void OnDrawGizmos()
    {
        if (targets != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
            foreach (GameObject target in targets)
            {
                if (target != null && target != this.gameObject && target.transform.position != this.transform.position)
                {
                    DrawArrow.ForGizmoTwoPoints(this.transform.position, target.transform.position);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (targets != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 1f);
            foreach (GameObject target in targets)
            {
                if (target != null && target != this.gameObject && target.transform.position != this.transform.position)
                {
                    DrawArrow.ForGizmoTwoPoints(this.transform.position, target.transform.position);
                }
            }
        }
    }
}
