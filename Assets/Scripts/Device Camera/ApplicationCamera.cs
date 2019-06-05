using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EVISION.MobileCamera.API
{
    public class ApplicationElement : MonoBehaviour
    {
        // Gives access to the application and all instances.
        public ApplicationCamera _app { get { return FindObjectOfType<ApplicationCamera>(); } }
    }

    public class ApplicationCamera : MonoBehaviour
    {
        public CameraView camView;
        public CameraController camCtlr;
        public CameraModel camModel;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
