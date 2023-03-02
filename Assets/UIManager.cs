using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    // http://clipart-library.com/clip-art/transparent-photo-frame-24.htm
    // https://www.youtube.com/watch?v=Q-G-W93jhYc&t=1s&ab_channel=Tarodev
    // https://www.youtube.com/watch?v=YSPT7wvBJJo&ab_channel=SteveSutcliffe

    [SerializeField] Image[] buttons;

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    private List<RawImage> imagesBeingTouched;

    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
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
            if(x <= Input.touchCount)
            {
                buttons[x-1].color = Color.red;
            }
            else
            {
                buttons[x-1].color = Color.white;
            }
        }

        // lets see what each touch is touching
        if (Input.touchCount > 0 )
        {
            for (int y =0; y < Input.touchCount; y++)
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
    }
}

//Attach this script to your Canvas GameObject.
//Also attach a GraphicsRaycaster component to your canvas by clicking the Add Component button in the Inspector window.
//Also make sure you have an EventSystem in your hierarchy.

