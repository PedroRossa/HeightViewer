using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Material billboardMaterial;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ActiveGrayEffetc() {

        if (billboardMaterial) {
            billboardMaterial.SetFloat("_GrayEffectAmount", 0.9f);
        }
    }
}
