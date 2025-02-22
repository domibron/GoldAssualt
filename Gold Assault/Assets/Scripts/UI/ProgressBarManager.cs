using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode()]
public class ProgressBarManager : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Linear progress bar (float)")]
    public static void AddLinearProgressBar()
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("UI/Linear progress bar"));
        obj.transform.SetParent(Selection.activeGameObject.transform, false);
    }
#endif

    [Header("Values")]
    public float minimum;
    public float maximum;
    public float current;

    [Header("Important values for the progress bar to work")]
    public Image mask;
    public Image fill;
    public Color color;
    public Image backFill;
    public Color backColor;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetCurrentFill();
    }

    void GetCurrentFill()
    {
        float currentOffset = current - minimum;
        float maximumOffset = maximum - minimum;
        float fillAmount = currentOffset / maximumOffset;
        mask.fillAmount = fillAmount;

        fill.color = color;
        backFill.color = backColor;
    }
}
