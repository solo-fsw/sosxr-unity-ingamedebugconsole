using UnityEngine;


namespace SOSXR.IngameDebugConsole
{
    /// <summary>
    /// Finds a <see cref="Camera"/> by tag at startup and assigns it as the world-space camera of the attached Canvas.
    /// Disables itself once a camera is found.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class CanvasTaggedWorldCam : MonoBehaviour
    {
        /// <summary>
        /// Tag of the GameObject that contains the target <see cref="Camera"/>.
        /// </summary>
        [SerializeField] private string m_camTag = "MainCamera";
        private Canvas _canvas;


        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }


        private void Start()
        {
            FindWorldCamera();
        }


        private void Update()
        {
            FindWorldCamera();
        }


        private void FindWorldCamera()
        {
            if (_canvas.worldCamera == null && GameObject.FindWithTag(m_camTag) != null)
            {
                _canvas.worldCamera = GameObject.FindWithTag(m_camTag).GetComponentInChildren<Camera>();

                return;
            }

            enabled = false; // Disable this component
        }
    }


    /// <summary>
    /// Finds a <see cref="Camera"/> by tag at startup and assigns it as the screen-space camera of the attached Canvas.
    /// Disables itself once a camera is found.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class CanvasTaggedScreenSpaceCam : MonoBehaviour
    {
        /// <summary>
        /// Tag of the GameObject that contains the target <see cref="Camera"/>.
        /// </summary>
        [SerializeField] private string m_camTag = "MainCamera";
        private Canvas _canvas;


        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }


        private void Start()
        {
            FindWorldCamera();
        }


        private void Update()
        {
            FindWorldCamera();
        }


        private void FindWorldCamera()
        {
            if (_canvas.worldCamera == null && GameObject.FindWithTag(m_camTag) != null)
            {
                _canvas.worldCamera = GameObject.FindWithTag(m_camTag).GetComponentInChildren<Camera>();

                return;
            }

            enabled = false; // Disable this component
        }
    }
}
