using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    // http://clipart-library.com/clip-art/transparent-photo-frame-24.htm
    // https://www.youtube.com/watch?v=Q-G-W93jhYc&t=1s&ab_channel=Tarodev
    // https://www.youtube.com/watch?v=YSPT7wvBJJo&ab_channel=SteveSutcliffe
    // https://docs.unity3d.com/2019.1/Documentation/ScriptReference/UI.GraphicRaycaster.Raycast.html

    [SerializeField] Image[] buttons;
    [SerializeField] float max_zoom;
    [SerializeField] float min_zoom;
    [SerializeField] float zoom_speed;

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    ScrollRect currentRect;

    private List<RawImage> imagesBeingTouched;
    private bool zooming = false;
    private bool drag_started = false;
    private float drag_cooldown = 0f;
    private Vector2 dragStartPoint;

    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas) (must add this component)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();
        imagesBeingTouched = new List<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        // lets see how many touches we have
        for (int x = 1; x < buttons.Length; x++)
        {
            if (x <= Input.touchCount)
            {
                buttons[x - 1].color = Color.red;
            }
            else
            {
                buttons[x - 1].color = Color.white;
            }
        }

        // this catalogs all touches in a list, which is cleared when the touches are removed.
        // we use this list to determine if the first 2 touches are on the same gameobject
        if (Input.touchCount > 0)
        {
            for (int y = 0; y < Input.touchCount; y++)
            {

                if (Input.touches[y].phase == TouchPhase.Began)
                {
                    //Set up the new Pointer Event
                    m_PointerEventData = new PointerEventData(m_EventSystem);
                    //Set the Pointer Event Position to that of the mouse position
                    m_PointerEventData.position = Input.touches[y].position;

                    //Create a list of Raycast Results
                    List<RaycastResult> results = new List<RaycastResult>();

                    //Raycast using the Graphics Raycaster and mouse click position
                    m_Raycaster.Raycast(m_PointerEventData, results);

                    //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
                    foreach (RaycastResult result in results)
                    {
                        if (result.gameObject.GetComponent<RawImage>() != null)
                        {
                            imagesBeingTouched.Add(result.gameObject.GetComponent<RawImage>());
                        }
                    }
                }
            }
        }
        else
        {
            imagesBeingTouched.Clear();
        }

        // Are we attempting to pinch and poke?
        if (Input.touchCount == 2)
        {
            Touch firstTouch = Input.touches[0];
            Touch secondTouch = Input.touches[1];

            // the touch of the second finger implies zoom attempt
            if (secondTouch.phase == TouchPhase.Began || zooming == true)
            {
                zooming = true;
                // now make sure both are on the same object
                if (imagesBeingTouched[0] == imagesBeingTouched[1])
                {
                    currentRect = imagesBeingTouched[0].GetComponentInParent<ScrollRect>();

                    Vector2 firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
                    Vector2 secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition;

                    float prevTouchDeltaMag = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
                    float touchDeltaMag = (firstTouch.position - secondTouch.position).magnitude;

                    float deltaMagDiff = prevTouchDeltaMag - touchDeltaMag;
                    // if the distance is greater (negative value) then the fingers are moving apart (zoom in)
                    // if the distance is smaller (positive value) then the fingers are moving closer together

                    Debug.Log($"Zooming: {deltaMagDiff}");
                    // normally you would adjust the size of the ortho camera but we are going to scale the image being touched
                    if (deltaMagDiff < 0 && imagesBeingTouched[0].rectTransform.localScale.x < max_zoom)
                        imagesBeingTouched[0].rectTransform.localScale =
                            new Vector3(imagesBeingTouched[0].rectTransform.localScale.x + zoom_speed / 100f,
                                imagesBeingTouched[0].rectTransform.localScale.y + zoom_speed / 100f, 1f);

                    if (deltaMagDiff > 0 && imagesBeingTouched[0].rectTransform.localScale.x > min_zoom)
                        imagesBeingTouched[0].rectTransform.localScale =
                            new Vector3(imagesBeingTouched[0].rectTransform.localScale.x - zoom_speed / 100f,
                                imagesBeingTouched[0].rectTransform.localScale.y - zoom_speed / 100f, 1f);
                }
                else
                {
                    zooming = false;
                  /*  currentRect.horizontal = true;
                    currentRect.vertical = true;*/
                }
            }
        }
        else
        {
            zooming = false;
            /*if (currentRect != null)
            {
                currentRect.horizontal = true;
                currentRect.vertical = true;
            }*/
        }

        if (zooming)
        {
           /* if (currentRect != null)
            {

                currentRect.horizontal = false;
                currentRect.vertical = false;
            }*/
        }


        // Are we attempting to just drag the image
        // We need to know the max y and x it can be dragged
        if (Input.touchCount == 1 && imagesBeingTouched.Count > 0)
            {
            //image must be anchored to bottom left corner
            //values should be adjusted to account for the surrounding border.
            RawImage image = imagesBeingTouched[0];
            if (image != null)
            {
                Touch touch = Input.touches[0];
                float max_x = image.texture.width * -1;
                float max_y = image.texture.height * -1;

                if (touch.phase == TouchPhase.Began)
                {
                    drag_started = true;
                }
                else if (touch.phase == TouchPhase.Moved && drag_started)
                {
                    drag_cooldown += Time.deltaTime;

                    if (drag_cooldown > 0.1f)
                    {
                        //  Debug.Log(image.transform.localPosition);
                        float nextLocationX = image.transform.localPosition.x + touch.deltaPosition.x * 0.25f;
                        float nextLocationY = image.transform.localPosition.y + touch.deltaPosition.y * 0.25f;
                        if (nextLocationX > -850f && nextLocationX < 850f
                            && nextLocationY > -500f && nextLocationY < 500f)
                            image.transform.Translate(
                                    touch.deltaPosition.x * 0.25f,
                                    touch.deltaPosition.y * 0.25f,
                                    0
                                );

                    }
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    drag_started = false;
                    drag_cooldown = 0f;
                }
            }
        }
    }
}
   