using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Version : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TMP_Text versionText = gameObject.GetComponent(typeof(TMP_Text)) as TMP_Text;
        if(versionText)
        {
            versionText.text = Application.version;
        }      
    }

}
