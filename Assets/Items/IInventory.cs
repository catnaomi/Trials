﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IInventory
{
    public List<Item> GetContents();
    public bool Contains(Item item);
    public bool Add(Item item);
    public bool Remove(Item item);
    public void Clear();
    public UnityEvent GetChangeEvent();

}