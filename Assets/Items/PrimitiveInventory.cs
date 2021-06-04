using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Items
{
    public class PrimitiveInventory : MonoBehaviour, IInventory
    {
        public List<Item> startingContents;
        public List<Item> contents;
        public UnityEvent OnChange;

        void Awake()
        {
            foreach(Item item in startingContents)
            {
                contents.Add(ScriptableObject.Instantiate(item));
            }
            startingContents.Clear();
        }
        public List<Item> GetContents()
        {
            return contents;
        }

        public bool Contains(Item item)
        {
            return contents.Contains(item);
        }

        public bool Add(Item item)
        {
            contents.Add(item);
            return true;
        }

        public bool Remove(Item item)
        {
            return contents.Remove(item);
        }

        public void Clear()
        {
            contents.Clear();
        }

        public UnityEvent GetChangeEvent()
        {
            return OnChange;
        }
    }
}