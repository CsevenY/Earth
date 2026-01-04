using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class starbt : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void btstarclicked()
    {
        GameObject.Find("Gameui").SetActive(false);
    }
}
