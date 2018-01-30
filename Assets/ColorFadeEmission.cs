using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColorFadeEmission : MonoBehaviour {
    Material m;
	void Start () {
        m = GetComponent<MeshRenderer>().sharedMaterial;
        m.SetColor("_EmissionColor", Color.red);

    }
	
	// Update is called once per frame
	void OnGUI () {
        transform.position += Vector3.zero;
        Color col = m.GetColor("_EmissionColor");

        float h, s, v;
        Color.RGBToHSV(col, out h, out s, out v);
        h += 0.01f;
        col = Color.HSVToRGB(h, s, v);
        m.SetColor("_EmissionColor", col);
	}
}
