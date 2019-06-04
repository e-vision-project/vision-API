using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallView : BounceElement
{
    // Reference to the ball
    public BallView ball;

    void OnCollisionEnter() { app.controller.OnBallGroundHit(); }
}
