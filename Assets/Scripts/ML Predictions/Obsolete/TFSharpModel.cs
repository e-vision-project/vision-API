using System.Collections;
using System.Collections.Generic;
using TensorFlow;
using UnityEngine;

public abstract class TFSharpModel
{
    protected string inputName;
    protected string outputName;
    protected int inputHeight;
    protected int inputWidth;
    protected float inputMean;
    protected float inputStd;
    protected TextAsset modelFile;
    protected TextAsset labelFile;
    
    protected TFGraph graph;
    protected TFSession session;

    public abstract IList FetchOutput(Texture2D tex);
    public abstract List<float> FetchIntermidiateOutput(Texture2D tex);
}
