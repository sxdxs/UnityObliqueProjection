
#if UNITY_EDITOR 
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
[RequireComponent(typeof(Grid))]
public class GridHax : MonoBehaviour
{
    public enum CalcMod { ReadTransform, Skew, Matrix, Angles, E11Scale }

    [Header("Common")]
    public CalcMod calculation = CalcMod.Skew;
    [Range(0.01f,1f)] public float scale = 1f;

    [Header("Mod : Skew")]
    public float skew = 45f;
    
    [Header("Mod : Matrix")]
    public Matrix4x4 matrix = Matrix4x4.identity;

    [Header("Mod : Angles")]
    public float angle1 = 45f;
    public float angle2 = 50f;

    bool needsUpdate = false;

    void OnValidate() => needsUpdate = true;

    private void Update( )
    {
        if( Application.isPlaying ) return;

        if( CalcMod.ReadTransform == calculation )
        {
            matrix = transform.localToWorldMatrix;
            ApplyMatrix();
        }

        if( ! needsUpdate ) return; needsUpdate = false;

        switch(calculation)
        { 
            case CalcMod.Skew: CalcSkew(); break;
            case CalcMod.Angles: CalcAngles(); break;
        }

        if( calculation != CalcMod.ReadTransform ) ApplyMatrix();
    }

    void CalcSkew()
    {
        var O = Vector3.zero;
        var R = Quaternion.identity;
        var S = Vector3.one * scale;
        var M = Matrix4x4.TRS( O, R, S );
        M.m01 = skew / 100f;
        matrix = M;
    }

    void CalcAngles()
    {
        var A = Mathf.Cos( angle2 * Mathf.Deg2Rad );
        var M = new Matrix4x4(
            new Vector4( - ( angle1 / 100f ), 0 ) ,
            new Vector4( A, scale, 0, 0 ),
            new Vector4( 0, 0, 1, 0 ),
            new Vector4( 0, 0, 0, 1 )
        );

        matrix = M;

        var v = M * Vector3.zero;
    }
    
    void ApplyMatrix( )
    {
        var grid = GetComponent<Grid>();
        
        var M = matrix;

        if( calculation == CalcMod.E11Scale )
        {
            M.m00 *= scale;
            M.m01 *= scale;
            M.m10 *= scale;
            M.m11 *= scale;
        }

        grid.cellSize = Vector3.one;
        
        transform.localScale = M.lossyScale;
        transform.rotation = M.rotation;
        
        var inv_mat = transform.localToWorldMatrix.inverse;
        
        foreach( var tm in grid.GetComponentsInChildren<Tilemap>( true ) )
        {
            tm.orientationMatrix = inv_mat;

            //// this replaces an inversetransformdirection call
            //Matrix4x4 rotMatrix = Matrix4x4.identity;
            //rotMatrix.SetTRS(Vector3.zero, Quaternion.Inverse(rotation), Vector3.one);
            //float3 directionFactor = rotMatrix.MultiplyVector(velocity).normalized;
        }
    }
}

#if UNITY_EDITOR

[CustomEditor( typeof( GridHax ) )]
public class GridHaxEditor : Editor
{
    public override void OnInspectorGUI( )
    {
        base.OnInspectorGUI( );

        var script = ( GridHax ) target;

        GUILayout.Label( "Matrix 4x4" );
        
        using(new GUILayout.HorizontalScope( ))
        {
            GUILayout.Label( "m0" );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m00.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m01.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m02.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m03.ToString("N2") );
            GUILayout.FlexibleSpace();
        }
        
        using(new GUILayout.HorizontalScope( ))
        {
            GUILayout.Label( "m1" );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m10.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m11.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m12.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m13.ToString("N2") );
            GUILayout.FlexibleSpace();
        }
        
        using(new GUILayout.HorizontalScope( ))
        {
            GUILayout.Label( "m2" );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m20.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m21.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m22.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m23.ToString("N2") );
            GUILayout.FlexibleSpace();
        }
        
        using(new GUILayout.HorizontalScope( ))
        {
            GUILayout.Label( "m3" );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m30.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m31.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m32.ToString("N2") );
            GUILayout.FlexibleSpace();
            GUILayout.Label( script.matrix.m33.ToString("N2") );
            GUILayout.FlexibleSpace();
        }
        


        //using(new GUILayout.HorizontalScope( ))
        //{
        //    script.matrix.m00 = EditorGUILayout.FloatField( "E00", script.matrix.m00, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m01 = EditorGUILayout.FloatField( "E01", script.matrix.m01, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m02 = EditorGUILayout.FloatField( "E02", script.matrix.m02, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m03 = EditorGUILayout.FloatField( "E03", script.matrix.m03, GUILayout.MaxWidth( 100f ) );
        //}
        //using(new GUILayout.HorizontalScope( ))
        //{
        //    script.matrix.m10 = EditorGUILayout.FloatField( "E10", script.matrix.m10, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m11 = EditorGUILayout.FloatField( "E11", script.matrix.m11, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m12 = EditorGUILayout.FloatField( "E12", script.matrix.m12, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m13 = EditorGUILayout.FloatField( "E13", script.matrix.m13, GUILayout.MaxWidth( 100f ) );
        //}
        //using(new GUILayout.HorizontalScope( ))
        //{
        //    script.matrix.m20 = EditorGUILayout.FloatField( "E00", script.matrix.m20, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m21 = EditorGUILayout.FloatField( "E00", script.matrix.m21, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m22 = EditorGUILayout.FloatField( "E00", script.matrix.m22, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m23 = EditorGUILayout.FloatField( "E00", script.matrix.m23, GUILayout.MaxWidth( 100f ) );
        //}
        //using(new GUILayout.HorizontalScope( ))
        //{
        //    script.matrix.m30 = EditorGUILayout.FloatField( "E00", script.matrix.m30, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m31 = EditorGUILayout.FloatField( "E00", script.matrix.m31, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m32 = EditorGUILayout.FloatField( "E00", script.matrix.m32, GUILayout.MaxWidth( 100f ) );
        //    script.matrix.m33 = EditorGUILayout.FloatField( "E00", script.matrix.m33, GUILayout.MaxWidth( 100f ) );
        //}   
    }
}


#endif