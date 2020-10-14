using CustomUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ForgeUI : MonoBehaviour
{
    public CraftableWeapon emptyWeapon;
    CraftableWeapon weapon;
    List<WeaponComponent> componentInventory;
    public Inventory targetInventory;

    public GameObject uiParent;
    public Camera uiCamera;
    public bool uiActive = true;
    public Button attachBlade;
    public Button[] attachBladeInsets = new Button[3];
    public Button attachHilt;
    public Button[] attachHiltInsets = new Button[3];
    public Button attachAdornment;
    public Button unattach;
    public Button hollow;

    public Button addToInventory;
    public GameObject componentSlotHolder;
    public GameObject componentSlotPrefab;
    public Vector2 componentSlotOffset = new Vector2(90, -90);
    List<Button> componentSlots;

    public Dictionary<WeaponComponent, Button> slotLookup;

    public Text componentText;
    public Text weaponText;

    public GameObject parentObject;
    GameObject model;

    public Inset hollowComponent;

    WeaponComponent selectedComponent;

    bool unattachSelected;
    bool hollowSelected;

    UnityEvent OnInteract;
    // Start is called before the first frame update
    void Start()
    {
        weapon = Instantiate(emptyWeapon);
        componentInventory = new List<WeaponComponent>();
        OnInteract = new UnityEvent();
        componentSlots = new List<Button>();
        //componentSlots.AddRange(componentSlotHolder.GetComponentsInChildren<Button>());
        slotLookup = new Dictionary<WeaponComponent, Button>();
        ResetUI();
    }
    void ResetUI()
    {
        UpdateFromInventory();

        int buttonsToMake = componentInventory.Count - componentSlots.Count;
        for (int i = 0; i < buttonsToMake; i++)
        {
            int col = i % 4;
            int row = (int)(i / 4);
            Vector3 position = componentSlotHolder.transform.position + new Vector3(componentSlotOffset.x * col, componentSlotOffset.y * row, 0) + new Vector3(20, -20, 0);
            var buttonObj = GameObject.Instantiate(componentSlotPrefab, position, componentSlotHolder.transform.rotation, componentSlotHolder.transform);
            componentSlots.Add(buttonObj.GetComponentInChildren<Button>());
        }
        foreach (Button button in componentSlots)
        {
            button.interactable = false;
            button.GetComponentInChildren<Text>().text = "";
        }
        for (int i = 0; i < componentInventory.Count; i++)
        {
            //WeaponComponent component = Instantiate(componentInventory[i]);
            WeaponComponent component = componentInventory[i];
            componentSlots[i].interactable = true;
            componentSlots[i].GetComponentInChildren<Text>().text = component.itemName;
            componentSlots[i].onClick.AddListener(() =>
            {
                selectedComponent = component;
                OnInteract.Invoke();
            });
            slotLookup[component] = componentSlots[i];
        }
        OnInteract.AddListener(InteractUpdate);
        attachBlade.onClick.RemoveAllListeners();
        attachBlade.onClick.AddListener(() => {
            if (unattachSelected || hollowSelected)
            {
                if (weapon.blade != null)
                {
                    weapon.blade.attached = false;
                }
                weapon.blade = null;
            }
            else if (selectedComponent is Blade)
            {
                if (weapon.blade != null)
                {
                    weapon.blade.attached = false;
                }
                weapon.blade = (Blade)selectedComponent;
                weapon.blade.attached = true;

            }
            OnInteract.Invoke();
        });
        attachHilt.onClick.RemoveAllListeners();
        attachHilt.onClick.AddListener(() => {
            if (unattachSelected || hollowSelected)
            {
                if (weapon.hilt != null)
                {
                    weapon.hilt.attached = false;
                }
                weapon.hilt = null;
            }
            else if (selectedComponent is Hilt)
            {
                if (weapon.hilt != null)
                {
                    weapon.hilt.attached = false;
                }
                weapon.hilt = (Hilt)selectedComponent;
                weapon.hilt.attached = true;
            }
            OnInteract.Invoke();
        });
        attachAdornment.onClick.RemoveAllListeners();
        attachAdornment.onClick.AddListener(() => {
            if (unattachSelected || hollowSelected)
            {
                if (weapon.adornment != null)
                {
                    weapon.adornment.attached = false;
                }
                weapon.adornment = null;
            }
            else if (selectedComponent is Adornment)
            {
                if (weapon.adornment != null)
                {
                    weapon.adornment.attached = false;
                }
                weapon.adornment = (Adornment)selectedComponent;
                weapon.adornment.attached = true;
            }
            OnInteract.Invoke();
        });
        for(int i = 0; i < attachBladeInsets.Length; i++) 
        {
            int ii = i;
            attachBladeInsets[ii].onClick.RemoveAllListeners();
            attachBladeInsets[ii].onClick.AddListener(() => {
                if (weapon.blade != null)
                {
                    if (unattachSelected)
                    {
                        weapon.blade.UnattachInset(ii);
                    }
                    else if (hollowSelected)
                    {
                        weapon.blade.AttachInset(Instantiate(hollowComponent), ii);
                    }
                    else
                    {
                        weapon.blade.AttachInset((Inset)selectedComponent, ii);
                    }
                }
                OnInteract.Invoke();
            });
        }
        for (int j = 0; j < attachHiltInsets.Length; j++)
        {
            int ji = j;
            attachHiltInsets[ji].onClick.RemoveAllListeners();
            attachHiltInsets[ji].onClick.AddListener(() => {
                if (weapon.hilt != null)
                {
                    if (unattachSelected)
                    {
                        weapon.blade.UnattachInset(ji);
                    }
                    else if (hollowSelected)
                    {
                        weapon.hilt.AttachInset(Instantiate(hollowComponent), ji);
                    }
                    else
                    {
                        weapon.hilt.AttachInset((Inset)selectedComponent, ji);
                    }
                }
                OnInteract.Invoke();
            });
        }

        unattach.onClick.RemoveAllListeners();
        unattach.onClick.AddListener(() => {
            selectedComponent = null;
            HighlightAttachedSlots();
            unattachSelected = true;
        });

        hollow.onClick.RemoveAllListeners();
        hollow.onClick.AddListener(() =>
        {
            selectedComponent = null;
            HighlightHollowableInsets();
            hollowSelected = true;
        });
        InteractUpdate();

        addToInventory.onClick.RemoveAllListeners();
        addToInventory.onClick.AddListener(TryFinishAndAttach);
        
        uiParent.SetActive(uiActive);
        uiCamera.gameObject.SetActive(uiActive);
    }

    public void HighlightAttachedSlots()
    {
        attachHilt.interactable = (weapon.hilt != null);
        attachBlade.interactable = (weapon.blade != null);
        attachAdornment.interactable = (weapon.adornment != null);
        for (int i = 0; i < attachBladeInsets.Length; i++)
        {
            if (weapon.blade == null)
            {
                attachBladeInsets[i].interactable = false;
                continue;
            }
            if (i >= weapon.blade.slots)
            {
                attachBladeInsets[i].interactable = false;
                continue;
            }
            if (weapon.blade.insets[i] == null)
            {
                attachBladeInsets[i].interactable = false;
                continue;
            }
            attachBladeInsets[i].interactable = true;
        }
        for (int j = 0; j < attachHiltInsets.Length; j++)
        {
            if (weapon.hilt == null)
            {
                attachHiltInsets[j].interactable = false;
                continue;
            }
            if (j >= weapon.hilt.slots)
            {
                attachHiltInsets[j].interactable = false;
                continue;
            }
            if (weapon.hilt.insets[j] == null)
            {
                attachHiltInsets[j].interactable = false;
                continue;
            }
            attachHiltInsets[j].interactable = true;
        }
    }

    public void HighlightHollowableInsets()
    {
        attachHilt.interactable = false;
        attachBlade.interactable = false;
        attachAdornment.interactable = false;
        for (int i = 0; i < attachBladeInsets.Length; i++)
        {
            if (weapon.blade == null)
            {
                attachBladeInsets[i].interactable = false;
                continue;
            }
            if (i >= weapon.blade.slots)
            {
                attachBladeInsets[i].interactable = false;
                continue;
            }
            attachBladeInsets[i].interactable = true;
        }
        for (int j = 0; j < attachHiltInsets.Length; j++)
        {
            if (weapon.hilt == null)
            {
                attachHiltInsets[j].interactable = false;
                continue;
            }
            if (j >= weapon.hilt.slots)
            {
                attachHiltInsets[j].interactable = false;
                continue;
            }
            attachHiltInsets[j].interactable = true;
        }
    }
    
    void InteractUpdate()
    {
        weapon.itemName = BladeNameGenerator.GenerateName(weapon);
        weapon.SetProperties();
        if (selectedComponent == null)
        {
            attachBlade.interactable = false;
            attachHilt.interactable = false;
            attachAdornment.interactable = false;
            foreach (Button attachInsetBlade in attachBladeInsets)
            {
                attachInsetBlade.interactable = false;
            }
            foreach (Button attachInsetHilt in attachHiltInsets)
            {
                attachInsetHilt.interactable = false;
            }
            componentText.text = "none selected";
        }
        else if (selectedComponent is Blade)
        {
            attachBlade.interactable = true;
            attachHilt.interactable = false;
            attachAdornment.interactable = false;
            foreach (Button attachInsetBlade in attachBladeInsets)
            {
                attachInsetBlade.interactable = false;
            }
            foreach (Button attachInsetHilt in attachHiltInsets)
            {
                attachInsetHilt.interactable = false;
            }
        }
        else if (selectedComponent is Hilt)
        {
            attachBlade.interactable = false;
            attachHilt.interactable = true;
            attachAdornment.interactable = false;
            foreach (Button attachInsetBlade in attachBladeInsets)
            {
                attachInsetBlade.interactable = false;
            }
            foreach (Button attachInsetHilt in attachHiltInsets)
            {
                attachInsetHilt.interactable = false;
            }
        }
        else if (selectedComponent is Adornment)
        {
            attachBlade.interactable = false;
            attachHilt.interactable = false;
            attachAdornment.interactable = true;

            foreach (Button attachInsetBlade in attachBladeInsets)
            {
                attachInsetBlade.interactable = false;
            }

            foreach (Button attachInsetHilt in attachHiltInsets)
            {
                attachInsetHilt.interactable = false;
            }
        }
        else if (selectedComponent is Inset)
        {
            attachBlade.interactable = false;
            attachHilt.interactable = false;
            attachAdornment.interactable = false;
            if (weapon.hilt != null)
            {
                for (int h = 0; h < attachHiltInsets.Length; h++)
                {
                    attachHiltInsets[h].interactable = (h < weapon.hilt.slots);
                }
            }
            if (weapon.blade != null)
            {
                for (int b = 0; b < attachBladeInsets.Length; b++)
                {
                    attachBladeInsets[b].interactable = (b < weapon.blade.slots);
                }
            }
        }

        if (selectedComponent != null)
        {
            componentText.text = selectedComponent.ToString();
        }

        weaponText.text = weapon.ToString();

        foreach (WeaponComponent component in slotLookup.Keys)
        {
            slotLookup[component].interactable = !component.attached;
        }

        unattachSelected = false;
        hollowSelected = false;

        GenerateModel();
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            uiActive = !uiActive;
            uiParent.SetActive(uiActive);
            uiCamera.gameObject.SetActive(uiActive);

            ResetUI();
        }
    }
    public void GenerateModel()
    {
        model = weapon.GenerateModel();
        if (model != null)
        {
            model.transform.SetParent(parentObject.transform, false);
        }
        InterfaceUtilities.SetLayerRecursively(parentObject, "UI-3D");
    }

    public void UpdateFromInventory()
    {
        componentInventory.Clear();
        foreach (Item item in targetInventory.contents)
        {
            if (item is WeaponComponent comp)
            {
                componentInventory.Add(comp);
            }
        }
    }
    public void TryFinishAndAttach()
    {
        if (targetInventory != null && weapon.hilt != null && weapon.blade != null)
        {
            foreach (WeaponComponent comp in weapon.GetAllComponents())
            {
                targetInventory.RemoveItem(comp);
            }
            targetInventory.AddItem(weapon);

            weapon = Instantiate(emptyWeapon);

            foreach (Button compSlot in componentSlots)
            {
                Destroy(compSlot.gameObject);
            }
            componentSlots.Clear();
            ResetUI();
        }
    }
}
