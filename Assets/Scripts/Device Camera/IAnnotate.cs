using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnnotate
{
    IEnumerator PerformAnnotation(Texture2D snap);
    string GetAnnotationText();
}
