using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LoadLOGO : MonoBehaviour
{
    public string logoUrl = "https://myapps.rmit.edu.au/nidp/images/pool/[NAM30IND41BGA3IGtrCFVMAVpdLHZ3QCUkbjQ1HWx0PzViRWZTA1MwUVES/[NAM30IND41BGA3IGtrCFVMAVpdLHZ3QCUkbjQ1HWx0PzViRWZTA1MwUVES.png";
    public RawImage rawImage;

    IEnumerator Start()
    {
        using (WWW www = new WWW(logoUrl))
        {
            yield return www;

            if(www.error != null) {
                Debug.Log(www.error);
            }

            Renderer renderer = GetComponent<Renderer>();
            rawImage.texture = www.texture;
        }
    }

}
