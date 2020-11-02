#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class GridTile : MonoBehaviour
{
    [HideInInspector] public int width = 100;
    [HideInInspector] public int height = 100;
    [HideInInspector] public Sprite sprite;
    [HideInInspector] public GridTileSettings.TileData data;
    
    [Range(-256,256)]public int offsetX = 0;
    [Range(-256,256)]public int offsetY = 0;

    [HideInInspector] public Vector3 offset;

    private void OnValidate( )
    {
        offset = new Vector3( offsetX, offsetY ) / 100f;

        data.offsetX = offsetX;
        data.offsetY = offsetY;

        //grid.tileSettings.Set( sprite, data );
        //grid.OnValidate();

        Snap();
    }


    public GridDisplay grid => GridDisplay.instance ?? Object.FindObjectOfType<GridDisplay>();

    void OnEnable( )
    {
        sprite = GetComponent<SpriteRenderer>()?.sprite ?? null;

        if( sprite == null )
        {
            this.enabled = false;
            return;
        }
        
        width = Mathf.RoundToInt( sprite.bounds.extents.x * 200f );
        height = Mathf.RoundToInt( sprite.bounds.extents.y * 200f );

        var _grid = grid;

        if( _grid == null ) 
        {
            this.enabled = false;
            return;
        }
        
        //if( _grid.tileSettings.Has( sprite ) )
        //{
        //    data = _grid.tileSettings.Get( sprite );

        //    offsetX = data.offsetX;
        //    offsetY = data.offsetY;
        //}
        //else
        //{
        //    data = new GridTileSettings.TileData { 
        //        width = width, 
        //        height = height,
        //        offsetX = offsetX,
        //        offsetY = offsetY
        //    };

        //    _grid.tileSettings.Add( sprite, data );
        //}
    }
    
    //public void Ping()
    //{
    //    // in case this object has been destroyed 
    //    if( transform == null ) return;

    //    grid.tileSettings.Get( sprite );

    //    offsetX = data.offsetX;
    //    offsetY = data.offsetY;
        
    //    offset = new Vector3( offsetX, offsetY ) / 100f;
        
    //    Snap();
    //}

    public Vector3 worldSnappedPos => grid.SnapPosition( transform.position + offset );

    public Vector2Int worldIndex => grid.SnapIndex( transform.position + offset );

    public void Snap() => transform.position = worldSnappedPos - offset;
}


#if UNITY_EDITOR

[CustomEditor(typeof(GridTile))]
public class GridTileEditor : Editor
{
    GUIStyle style_header;
    
    public override void OnInspectorGUI( )
    {
        var script = ( GridTile ) target; 

        if( style_header == null )
        {
            style_header = new GUIStyle( EditorStyles.boldLabel );
            style_header.fontSize = 20;
        }

        GUILayout.Space( 5 );
        GUILayout.Label( "Tile", style_header );
        GUILayout.Label( "", GUI.skin.horizontalSlider );
        GUILayout.Space( 15 );

        if( script.sprite == null )
        {
            EditorGUILayout.HelpBox("Missing sprite", MessageType.Error);
            return;
        }
        
        if( GridDisplay.instance == null )
        {
            EditorGUILayout.HelpBox("Missing " + typeof(GridDisplay).Name + " add it to the scene", MessageType.Error);
            
            GridDisplay.AddToActiveScene();

            return;
        }

        base.OnInspectorGUI( );

        GUILayout.Label("Size: " + script.width + " x " + script.height );
    }

    void OnSceneGUI()
    {
        var script = ( GridTile ) target; 

        if( Selection.activeGameObject !=  script.gameObject ) return;

        var grid = GridDisplay.instance ?? Object.FindObjectOfType<GridDisplay>();

        if( grid == null ) return;
        
        if( Event.current.type == EventType.MouseUp )
        {
            script.Snap();
        }
        else
        {
            Handles.color = Color.yellow;
            
            var p = script.worldSnappedPos;
            var w = grid.cellWidth;
            var h = grid.cellHeight;

            var S = new Vector3( w, h ) / 2f;

            //Handles.DrawWireCube( p, new Vector3( w, h, 0.1f ) );

            // note : pivote at center 

            var E = Vector3.right * grid.cellAngleStep / 2f;
            
            var A = p + new Vector3( - w/2f, - h/2f ) - E; // bottom left 
            var B = p + new Vector3(   w/2f, - h/2f ) - E; // bottom right 
            var C = p + new Vector3( - w/2f,   h/2f ) + E; // top left
            var D = p + new Vector3(   w/2f,   h/2f ) + E; // top right

            Handles.DrawAAPolyLine( 4f, A, B, D, C, A );
        }

        var text = "";

        var label_pos = script.transform.position + new Vector3( (1.1f) * ( (float) script.width / 100f ) / 2f , 0 );
        
        //text += "\nW: " + script.width;
        //text += "\nH: " + script.height;
        //text += "\nE: " + Event.current.type;
        text += "\nSnap: " + script.worldIndex;

        Handles.Label( label_pos , text );
    }

}

#endif