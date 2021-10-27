using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImNothing : MonoBehaviour
{
    private GameObject me;
    // Start is called before the first frame update
    void Start()
    {
        me = this.gameObject;
        me.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        me.SetActive(false);
    }
}
