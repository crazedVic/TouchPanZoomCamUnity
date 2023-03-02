using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    private Vector3 dragOrigin; //Where are we moving?
    private Vector3 clickOrigin = Vector3.zero; //Where are we starting?
    private Vector3 basePos = Vector3.zero; //Where should the camera be initially?
    public float speed = 0.1F;
    private Vector3 worldStartPoint;

    private float distanceBetweenInitial;
    private float dragCooldown = 0;
    private bool dragStarted = false;

    void Update()
    {
        //MoveCameraViaMouse();
        MoveCameraViaTouch();
        PinchOrPoke();
        // make note that if we were to listen for touch events and hook them up to a method
        // it would make sense to run that method in a coroutine.

    }

    void PinchOrPoke()
    {
        if(Input.touchCount == 2){
            Debug.Log("Detect pinch/poke");
            Touch touchOne = Input.GetTouch(0);
            Touch touchTwo = Input.GetTouch(1);
            if(touchOne.phase == TouchPhase.Began || touchTwo.phase == TouchPhase.Began)
            {
                // set initial distance between them
                distanceBetweenInitial = Vector2.Distance(
                    Camera.main.ScreenToWorldPoint(touchOne.position), 
                    Camera.main.ScreenToWorldPoint(touchTwo.position)
                    );
            }
            if(touchOne.phase == TouchPhase.Moved || touchTwo.phase == TouchPhase.Moved)
            {
                float distance = Vector2.Distance(
                    Camera.main.ScreenToWorldPoint(touchOne.position),
                    Camera.main.ScreenToWorldPoint(touchTwo.position)
                    ) - distanceBetweenInitial;


                if (distance > 0.5)
                {
                    // poke or zoom in
                    Camera.main.orthographicSize -= 8f * Time.deltaTime;
                }
                else if (distance < -0.5)
                {
                    // pinch or zoom out
                    Camera.main.orthographicSize += 8f * Time.deltaTime;
                }
            }
        }
    }
    void MoveCameraViaTouch()
    {
        // need some sort of timer to prevent false positive drags
        // https://gamedev-resources.com/implementing-touch-with-input-systems-enhanced-touch-api/
        if (Input.touchCount == 1)
        {
            Touch currentTouch = Input.GetTouch(0);

            if (currentTouch.phase == TouchPhase.Began)
            {
                this.worldStartPoint = Camera.main.ScreenToWorldPoint(currentTouch.position);
                dragCooldown = 0;
                dragStarted = true;
            }

            else if (currentTouch.phase == TouchPhase.Moved && dragStarted)
            {
                dragCooldown += Time.deltaTime;

                if (dragCooldown > 0.1f)
                {
                    Vector2 worldDelta = Camera.main.ScreenToWorldPoint(currentTouch.position) - this.worldStartPoint;

                    Camera.main.transform.Translate(
                        -worldDelta.x,
                        -worldDelta.y,
                        0
                    );
                }
            }
            else if(currentTouch.phase == TouchPhase.Ended)
            {
                dragStarted = false;
            }


        }
    }

    // convert screen point to world point

    void MoveCameraViaMouse()
    {
        if (Input.GetMouseButton(0))
        {
            if (clickOrigin == Vector3.zero)
            {
                clickOrigin = Input.mousePosition;
                basePos = transform.position;
            }
            dragOrigin = Input.mousePosition;
        }

        if (!Input.GetMouseButton(0))
        {
            clickOrigin = Vector3.zero;
            return;
        }

        transform.position = new Vector3(basePos.x + ((clickOrigin.x - dragOrigin.x) * .01f), basePos.y + ((clickOrigin.y - dragOrigin.y) * .01f), -10);
    }
}