using System;
using System.Collections.Generic;
using UnityEngine;


class MyPoint
{
    public GameObject PointGameObject;
}

public class BezierCurve : MonoBehaviour
{
    public LineRenderer BaseLine;
    public GameObject PointPrefab;
    public GameObject LineRendererPrefab;
    
    private bool _checkClick;
    private bool _isInit;

    private List<MyPoint> _pointsList = new List<MyPoint>();
    private List<LineRenderer> _renderers = new List<LineRenderer>();

    private List<Vector3> _resultList = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        _checkClick = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1) && _checkClick)
        {
            Debug.Log("Mouse is down");
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo)) 
            {
                Debug.Log("Hit " + hitInfo.transform.gameObject.name);
                if (hitInfo.transform.gameObject.CompareTag("Background"))
                {
                    var newPoint = new MyPoint();
                    newPoint.PointGameObject =
                        Instantiate(PointPrefab, hitInfo.point, Quaternion.identity);
                    _pointsList.Add(newPoint);
                    UpdateBaseLine();
                }
            }
        } 
    }

    Vector3[] GetBasePoints()
    {
        Vector3[] result = new Vector3[_pointsList.Count];
        for (int index = 0; index < _pointsList.Count; index++)
        {
            result[index] = _pointsList[index].PointGameObject.transform.position;
        }

        return result;
    }

    void UpdateBaseLine()
    {
        BaseLine.positionCount = _pointsList.Count;
        BaseLine.SetPositions(GetBasePoints());
    }

    public float Step = 0.0f;
    public void DoDeCasteljauBezier()
    {
        _checkClick = false;
        for (int index = 0; !_isInit && index < _pointsList.Count - 1; index++)
        {
            _renderers.Add(Instantiate(LineRendererPrefab).GetComponent<LineRenderer>());
        }
        
        _renderers[^1].startColor = Color.green;
        _renderers[^1].endColor = Color.green;

        _isInit = true;

        Step = Math.Min(1.0f, Step + 0.01f);
        
        UpdateBezierByDeCasteljau(0, GetBasePoints(), Step);
    }

    public void DoBernsteinBezier()
    {
        _checkClick = false;
        if (!_isInit)
        {
            _renderers.Add(Instantiate(LineRendererPrefab).GetComponent<LineRenderer>());
            _renderers.Add(Instantiate(LineRendererPrefab).GetComponent<LineRenderer>());
            _renderers[^1].startColor = Color.green;
            _renderers[^1].endColor = Color.green;
            _isInit = true;
        }
        Step = Math.Min(1.0f, Step + 0.01f);
        Vector3[] tempPoint = new Vector3[_pointsList.Count + 1];
        Vector3[] points = GetBasePoints();
        Vector3 endPoint = Vector3.zero;
        for (int index = 0; index < _pointsList.Count; index++)
        {
            tempPoint[index+1] = BernsteinNum((uint)index, (uint)_pointsList.Count-1, Step) * points[index];
            endPoint += tempPoint[index+1];
        }
        _resultList.Add(endPoint);
        tempPoint[0] = Vector3.zero;
        tempPoint[_pointsList.Count] = endPoint;
        _renderers[0].positionCount = tempPoint.Length;
        _renderers[0].SetPositions(tempPoint);

        _renderers[1].positionCount = _resultList.Count;
        _renderers[1].SetPositions(_resultList.ToArray());
    }

    ulong Factorial(uint n)
    {
        if (n == 0) return 1;
        ulong result = n;
        for (n--; n > 0; n--)
            result *= n;
        return result;
    }
    
    float BernsteinNum(uint i, uint n, float t)
    {
        return CombinatorialNum(i, n) * Mathf.Pow(t, i) * Mathf.Pow(1.0f - t, n - i);
    }

    ulong CombinatorialNum(uint i, uint n)
    {
        if (i > n) return 0;
        return Factorial(n) / (Factorial(i) * Factorial(n - i));
    }
    
    void UpdateBezierByDeCasteljau(int levelIndex, Vector3[] points, float currentStep)
    {
        Vector3[] result = new Vector3[points.Length - 1];
        for (int index = 0; index < points.Length - 1; index++)
        {
            result[index] = points[index] + (points[index + 1] - points[index]) * currentStep;
        }
        if (result.Length > 1)
        {
            _renderers[levelIndex].positionCount = result.Length;
            _renderers[levelIndex].SetPositions(result);
            UpdateBezierByDeCasteljau(levelIndex + 1, result, currentStep);
        }
        else
        {
            _resultList.Add(result[0]);
            _renderers[levelIndex].positionCount = _resultList.Count;
            _renderers[levelIndex].SetPositions(_resultList.ToArray());
        }
        
    }
    
}
