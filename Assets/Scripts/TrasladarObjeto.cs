using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrasladarObjeto : MonoBehaviour {

    public float metrosSeg = 1;
    void Update()
    {
        transform.Translate(0, 0, metrosSeg * Time.deltaTime);
    }
}
