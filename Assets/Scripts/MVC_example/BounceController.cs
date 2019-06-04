using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceController : BounceElement
{
    // Handles the ball hit event
   public void OnBallGroundHit()
    {
        app.model.bounces++;
        Debug.Log("Bounce" + app.model.bounces);
        if (app.model.bounces >= app.model.winCondition)
        {
            app.view.ball.enabled = false;
            app.view.ball.GetComponent<Rigidbody>().isKinematic = true; // stops the ball
            OnGameComplete();
        }
    }

    // Handles the win condition
    public void OnGameComplete() { Debug.Log("Victory!!"); }
}
