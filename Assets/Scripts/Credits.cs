using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{

    public void OpenLink(int ID)
    {
        switch(ID)
        {
            case 0: Application.OpenURL("https://fonts.google.com/specimen/Rye?selection.family=Rye");   break;
            case 1: Application.OpenURL("https://assetstore.unity.com/packages/2d/gui/icons/clean-vector-icons-132084");  break;
            case 2: Application.OpenURL("https://www.fontspace.com/dotline-font-f6023"); break;
            default: Application.OpenURL("https://www.google.com/"); break;

        }
    }
}
