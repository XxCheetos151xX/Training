using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MPBT
{
    public class FindMyPlatform : MonoBehaviour
    {
        public Text txt;
        public void Find()
        {
            txt.text = "My Platform Is: " + Application.platform.ToString();
        }
    }
}
