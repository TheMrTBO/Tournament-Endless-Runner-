using UnityEngine;
using System.Collections;
 
[ExecuteInEditMode]
public class CurveController : MonoBehaviour
{
 
    public Transform CurveOrigin;
 
    [Range(-500f, 500f)]
    [SerializeField]
    float x = 0f;
 
    [Range(-500f, 500f)]
    [SerializeField]
    float y = 0f;
 
    [Range(0f, 50f)]
    [SerializeField]
    float falloff = 0f;
 
    private Vector2 bendAmount = Vector2.zero;
 
    // Global shader property ids
    private int bendAmountId;
    private int bendOriginId;
    private int bendFalloffId;
 
    void Start ()
    {
        bendAmountId = Shader.PropertyToID("_BendAmount");
        bendOriginId = Shader.PropertyToID("_BendOrigin");
        bendFalloffId = Shader.PropertyToID("_BendFalloff");
    }
    
    void Update ()
    {
        bendAmount.x=x;
        bendAmount.y=y;
 
        Shader.SetGlobalVector(bendAmountId, bendAmount);
        Shader.SetGlobalVector(bendOriginId, CurveOrigin.position);
        Shader.SetGlobalFloat(bendFalloffId, falloff);
    }
        
}