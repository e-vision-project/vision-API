using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfomObjectReckognision : MonoBehaviour
{
    [SerializeField] TextAsset model;
    [SerializeField] TextAsset labels;
    Detector detector;

    private IList outputs;

    // Start is called before the first frame update
    void Start()
    {
        // Tiny YOLOv2
        detector = new Detector(model, labels, DetectionModels.YOLO,
        width: 480,
        height: 480,
        mean: 0,
        std: 255);

        // SSD MobileNet
        //detector = new Detector(model, labels,
        //                        input: "image_tensor");
    }

    public void ProcessImage(Texture2D tex)
    {
        Debug.Log("Object Detection started");

        outputs = detector.Detect(tex, angle: 90, threshold: 0.1f);
        for (int i = 0; i < outputs.Count; i++)
        {
            var output = outputs[i] as Dictionary<string, object>;
            Debug.Log(output["detectedClass"].ToString());
        }


    }

    public void OnGUI()
    {
        if (outputs != null)
        {
            // Object detection
            Utils.DrawOutput(outputs, new Vector2(20, 20), Color.yellow);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
