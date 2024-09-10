using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIElementConstructors
{

}


public class BattleUICharMarker {

    private VisualElement marker;

    public GameObject objectToMark;

    //private Camera mainCamera;

    //private Vector2 offset; // is a vec2 the best here? No, with the object we just get the transform and do WorldSpaceToScreen or whatev

    public BattleUICharMarker(VisualElement _marker, GameObject _objectToMark = null) {
        marker = _marker;
        objectToMark = _objectToMark;
        //mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    public void Update() {
        if(objectToMark == null) {
            marker.visible = false;
            return;
        } else {
            marker.visible = true;
        }

        Vector3 screen = Camera.main.WorldToScreenPoint(objectToMark.transform.position);
        

        // What is the problem here? Because of weird spaghetti, Screen dpi is twice that of the layout.
        // UI Base layout dimensions are half the size of the screen. So when going from Screen to Layout we need to do
        // screen/2 => layout
        // On top of this, when the marker instance is added it has a layout width of the entire screen, which i dont yet know how to change.

        int MN_MarkerImageWidth = (int)marker.layout.height;

        marker.style.left = screen.x - MN_MarkerImageWidth/2;
        marker.style.top = Screen.height - screen.y - 100;

        
        /*
        Debug.Log("BattleUICharMarker : Update()\n" + 
        "World to Screen Point is " + screen + "\n" + 
        "Screen is " + Screen.width + " " + Screen.height + "\n" +
        "layout is " + marker.layout.width + " " + marker.layout.height + "\n" +
        "style after change " + marker.style.left + " " + marker.style.top);
        */
    } 

}