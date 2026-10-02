using UnityEngine;
using System.Collections.Generic;

public class Repeal : MonoBehaviour
{
    public InventoryUI ui;
    public List<RectTransform> items;
    public float repelForce = 100000f;
    public float repelRadius = 1000;
    public RectTransform este;

    void Start()
    {
        ui = GameObject.FindWithTag("UI").GetComponent<InventoryUI>();
    }
    void Update()
    {
        for (int i = 0; i < ui.existingItems.Count; i++)
        {
            for (int j = i + 1; j < ui.existingItems.Count; j++)
            {
                if (!ui.existingItems[j].GetComponent<DraggableItemUI>().equiped)
                {
                    Vector2 diff = (ui.existingItems[i].anchoredPosition - ui.existingItems[j].anchoredPosition);
                    float dist = diff.magnitude;

                    if (dist < repelRadius && dist > 0.01f)
                    {
                        Vector2 dir = diff.normalized;
                        float force = (repelRadius - dist) / repelRadius * repelForce;
                        ui.existingItems[i].anchoredPosition += dir * force * Time.deltaTime;
                        ui.existingItems[j].anchoredPosition -= dir * force * Time.deltaTime;
                    }
                }
            }
        }
    }
}