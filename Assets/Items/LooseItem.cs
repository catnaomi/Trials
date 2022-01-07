using System.Collections;
using UnityEngine;

    public class LooseItem : Interactable
    {

        public Transform itemTransformParent;

        public Item item;
        public GameObject model;

        public GameObject defaultPrefab;
        // Use this for initialization

        void Start()
        {

            if (!interactionNode.isTrigger)
            {
                Debug.LogWarning("Warning! Interaction Nodes should be triggers!");
            }
            if (interactionNode.gameObject.layer != LayerMask.NameToLayer("InteractionNode"))
            {
                Debug.LogWarning("Warning! Interaction Nodes must be in the layer 'InteractionNode' !");
            }
            SetIconVisiblity(false);

            if (item != null)
            {
                if (item is IGeneratesModel itemWM)
                {
                    model = itemWM.GenerateModel();
                    
                }
                else
                {
                    model = GameObject.Instantiate(defaultPrefab);
                }
                model.transform.SetParent(itemTransformParent);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;

                UpdateBounds();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateBounds()
        {

            MeshFilter[] meshFilters = model.GetComponentsInChildren<MeshFilter>(false);
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            Matrix4x4 pTransform = model.transform.worldToLocalMatrix;

            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = pTransform * meshFilters[i].transform.localToWorldMatrix;
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine);

            MeshCollider meshCollider = this.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true;

            ((SphereCollider)interactionNode).radius = meshCollider.bounds.extents.magnitude + 0.5f;
        }

        public override void Interact(PlayerActor player)
        {
            player.inventory.Add(item);
            GameObject.Destroy(this.gameObject);
        }

        public static LooseItem CreateLooseItem(Item item)
        {
            GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/LooseItem"));

            LooseItem li = obj.GetComponent<LooseItem>();
            li.item = item;

            return li;
        }
    }