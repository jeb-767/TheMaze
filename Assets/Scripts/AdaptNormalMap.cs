using UnityEngine;

public class AdaptNormalMap : MonoBehaviour
{
    public Transform entrance;
    public Material material;
    private Renderer rend;
    [SerializeField] private MaterialPropertyBlock block;
    public float normalIntensity = 1f;
    public bool brokable;
    public bool movile;
    public float grietasIntensity;

    void Awake()
    {
        grietasIntensity = 1.1f;
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }

    void Start()
    {
        entrance = GameObject.FindWithTag("Entrance").transform;
        normalIntensity = (entrance.position - this.transform.position).magnitude / 10f;
        UpdateNormalIntensity();
    }

    public void UpdateNormalIntensity()
    {
        rend.GetPropertyBlock(block);
        if (brokable)
        {
            Debug.Log("Brokable material detected");
            block.SetFloat("_GrietasIntensity", grietasIntensity);
            block.SetFloat("_BaseIntensity", 0.7f);
            block.SetFloat("_NormalIntensity", 1);
        }
        else if (movile)
        {
            Debug.Log("Brokable Movile detected");
            block.SetFloat("_BaseIntensity", 0.7f);
            block.SetFloat("_NormalIntensity", 1);
        }
        else
        {
            block.SetFloat("_NormalIntensity", normalIntensity);
            block.SetFloat("_GrietasIntensity", 0);

        }
        rend.SetPropertyBlock(block);
    }
    public void GrietasChange()
    {
        grietasIntensity += 1f;
        block.SetFloat("_GrietasIntensity", grietasIntensity);
    }
}
