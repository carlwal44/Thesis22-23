using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleCanvas : MonoBehaviour
{
    public Canvas canvas;
    public Button button;

    // Start is called before the first frame update
    void Start()
    {
        canvas.enabled = true;

        Button btn = button.GetComponent<Button>();
        btn.onClick.AddListener(switchUI);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void switchUI()
    {
        canvas.enabled = !canvas.enabled;

        /*var sprite = Resources.Load<Sprite>("Icons/Iconclosebutton");

        if(canvas.enabled == false)
        {
            button.GetComponent<Image>().sprite = sprite;
        }*/
    }
}
