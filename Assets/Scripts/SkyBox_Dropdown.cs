using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkyBox_Dropdown : MonoBehaviour
{
    public Material skybox1;
    public Material skybox2;
    public Material skybox3;
    public Material skybox4;
    public Material skybox5;
    public Material skybox6;

    private void Update()
    {
        DropdownSkybox();
    }

    public void DropdownSkybox()
    {
        switch (gameObject.GetComponent<TMP_Dropdown>().value)
        {
            case 0: RenderSettings.skybox = skybox1; break;
            case 1: RenderSettings.skybox = skybox2; break;
            case 2: RenderSettings.skybox = skybox3; break;
            case 3: RenderSettings.skybox = skybox4; break;
            case 4: RenderSettings.skybox = skybox5; break;
            case 5: RenderSettings.skybox = skybox6; break;
        }
    }
}
