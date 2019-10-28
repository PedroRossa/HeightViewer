using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    public GameObject Fire;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Contains("grab"))
        {
            ParticleSystem particles = Fire.GetComponent<ParticleSystem>();
            particles.Play();
            SetBillboardActive(other.gameObject);
            Destroy(other.gameObject, 0.5f);
            
        }
    }
    private void SetBillboardActive(GameObject obj) {
        BaseBillboard baseBill =  obj.GetComponent<BaseBillboard>();
        if (baseBill) {
            if (baseBill.billboard) {
                Billboard billb = baseBill.billboard.GetComponent<Billboard>();
                if (billb) {
                    billb.ActiveGrayEffetc();
                }
            }
        }


    }
}
