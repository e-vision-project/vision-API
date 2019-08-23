using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IModelPrediction
{
    // Gets the output of the model
    T FetchOutput<T, U>(U param) where T : class, IList
                                 where U : class;
}
