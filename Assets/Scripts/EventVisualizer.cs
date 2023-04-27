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
#if UNITY_EDITOR
        if (targets != null)
        {
            targets.Clear();
        }
        else
        {
            targets = new List<GameObject>();
        }
        MonoBehaviour[] monoBehaviours = this.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour mono in monoBehaviours)
        {
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
                    if (target is MonoBehaviour targetMono && !targets.Contains(targetMono.gameObject))
                    {
                        targets.Add(targetMono.gameObject);
                    }
                }
            }
        }
#endif
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
