using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLines : MonoBehaviour
{

    bool enterPressed = false;
    public Camera perspectiveMainCamera;
    public Camera perspectiveTopCamera;
    public Camera orthographicTopCamera;
    Camera currentCam;
    bool drawLineMode = false;
    LineRenderer lr;

    Vector3 mousePos;
    Vector3 pos;
    Vector3 previousPos;
    List<Vector3> linePositions;
    float minimumDistance = 10f;
    public Material mat;
    public GameObject canvas;
    public GameObject[] prefabs;
    GameObject selectedPrefab;
    GameObject newObj;
    bool newObjInstantiated = false;
    Color c1 = new Color(1, 1, 1, 0.5f);
    Color c2 = new Color(1, 1, 1, 0.5f);

    // Start is called before the first frame update
    void Start()
    {
        perspectiveMainCamera.enabled = true;
        perspectiveTopCamera.enabled = false;
        orthographicTopCamera.enabled = false;
        currentCam = perspectiveMainCamera;
        canvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            perspectiveMainCamera.enabled = !perspectiveMainCamera.enabled;
            orthographicTopCamera.enabled = !orthographicTopCamera.enabled;
            perspectiveTopCamera.enabled = !perspectiveTopCamera.enabled;
            currentCam = (currentCam == perspectiveMainCamera) ? orthographicTopCamera : perspectiveMainCamera;
            perspectiveMainCamera.gameObject.tag = (perspectiveMainCamera.gameObject.tag == "MainCamera") ? "Untagged" : "MainCamera";
            perspectiveTopCamera.gameObject.tag = (perspectiveTopCamera.gameObject.tag == "MainCamera") ? "Untagged" : "MainCamera";
            Debug.Log(currentCam);
            enterPressed = !enterPressed;
            canvas.SetActive(!canvas.activeSelf);
            RenderSettings.fog = !RenderSettings.fog;

            if (!enterPressed)
            {
                drawLineMode = false;
                newObjInstantiated = false;
            }
        }

        if (drawLineMode)
        {
            Debug.Log("here");
            if (Input.GetMouseButtonDown(0))
            {
                InstantiateObject();
                mousePos = Input.mousePosition;
                mousePos.z = 10f;
                pos = currentCam.ScreenToWorldPoint(mousePos);
                pos.y = selectedPrefab.transform.position.y;

                previousPos = pos;
                linePositions.Add(pos);
            }
            else if (Input.GetMouseButton(0))
            {
                mousePos = Input.mousePosition;
                mousePos.z = 10f;
                pos = currentCam.ScreenToWorldPoint(mousePos);
                pos.y = selectedPrefab.transform.position.y;

                float distance = Vector3.Distance(pos, previousPos);
                if (distance >= minimumDistance)
                {
                    previousPos = pos;
                    linePositions.Add(pos);
                    lr.positionCount = linePositions.Count;
                    lr.SetPositions(linePositions.ToArray());
                }
            }

            if (Input.GetMouseButtonDown(1) && newObjInstantiated)
            {
                newObj.transform.position = lr.GetPosition(0);
                drawLineMode = false;
                newObjInstantiated = false;
                newObj.AddComponent<Navigation>();
                Navigation nav = newObj.GetComponent<Navigation>();
                Vector3[] linePositions = new Vector3[lr.positionCount];
                lr.GetPositions(linePositions);
                nav.wayPoints = linePositions;
            }

        }
    }

    void InstantiateObject()
    {
        newObj = Instantiate(selectedPrefab, selectedPrefab.transform.position, selectedPrefab.transform.rotation);
        newObj.transform.parent = this.transform;
        newObj.AddComponent<LineRenderer>();
        lr = newObj.GetComponent<LineRenderer>();
        lr.material = mat;
        lr.SetColors(c1, c2);
        lr.SetWidth(5f, 5f);
        linePositions = new List<Vector3>();
        lr.positionCount = linePositions.Count;
        lr.SetPositions(linePositions.ToArray());
        newObjInstantiated = true;
    }

    public void ButtonClick(int i)
    {
        selectedPrefab = prefabs[i];
        drawLineMode = true;
        Debug.Log(true);
    }


}
