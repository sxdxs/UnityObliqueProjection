#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

[ExecuteInEditMode]
public class GridDisplay : MonoBehaviour
{   
    public static void AddToActiveScene()
    {
        if( instance != null ) return;
        var go = new GameObject( typeof( GridDisplay ).Name );
        go.AddComponent<GridDisplay>();
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
    }

    public static GridDisplay instance;


    [Header("Grid size")]
    public uint width = 100;
    public uint height = 100;
    //public float scale = 1f;
    public bool pixelShift = false;
    [Range(-35f,35f)] public float angle = 0f;
    //[Range(0.25f,2f)] public float heightRatio = 1f;
    //public float skew = 0.45f;

    public Vector2Int SnapIndex( Vector3 p )
    {
        float w = this.cellWidth, h = this.cellHeight;
        int cell_index_y = Mathf.RoundToInt( p.y / h );
        var cell_offset_x = ( this.cellAngleStep * cell_index_y ) % this.cellWidth;
        int cell_index_x = Mathf.RoundToInt( ( p.x - cell_offset_x ) / w );
        return new Vector2Int( cell_index_x , cell_index_y );
    }

    public Vector3 SnapPosition( Vector3 p )
    {
        float w = this.cellWidth, h = this.cellHeight;
        int cell_index_y = Mathf.RoundToInt( p.y / h );
        var cell_offset_x = ( this.cellAngleStep * cell_index_y ) % this.cellWidth;
        int cell_index_x = Mathf.RoundToInt( ( p.x - cell_offset_x ) / w );
        p.x = this.cellOffsetX + cell_index_x * w;
        p.y = cell_index_y * h;
        p.x += cell_offset_x + ( ! this.pixelShift ? 0 : Mathf.Abs( cell_index_x ) * 0.01f );
        return p;
    }


    //[Header("Display")] public bool drawGrid = true;
    //[Range(1.1f,3f)] public float thickness = 1.1f;
    //public Color color = Color.red;

    //[Header("Snapping (WIP)")] public bool snapToGrid = true;
    //[Range(0f,1f)] public float accuracy = 0.5f;

    //[Header("Tiles configuration")]
    //public GridTileSettings tileSettings;

    public System.Action OnChange;

    [HideInInspector] public float cellAngleStep = 0f;
    [HideInInspector] public float cellWidth = 0f;
    [HideInInspector] public float cellHeight = 0f;
    [HideInInspector] public float cellOffsetX = 0f;

    public void OnValidate() 
    {
        var w = (float) width / 100f;
        var h = (float) height / 100f;

        cellAngleStep = Mathf.Sin( angle * Mathf.Deg2Rad ) * new Vector2( (float) width / 100f, (float) height / 100f ).magnitude;

        cellHeight = h;
        cellWidth = w - cellAngleStep;
        cellOffsetX = angle < 0 ? - cellAngleStep : 0;

        //foreach( var tile in Object.FindObjectsOfType<GridTile>() )
        //{
        //    tile.Ping();
        //}
    }

    void OnEnable()
    {
        //if( tileSettings == null ) 
        //{ 
        //    var scene_name = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        //    tileSettings = GridTileSettings.Create( scene_name );
        //}

        if( instance != null ) 
        {
            Debug.LogError("Do not use more then 1 Grid Display in a game");
            if( Application.isPlaying ) Destroy( this );
            else DestroyImmediate( this );
            return;
        }

        instance = this;

        #if UNITY_EDITOR
        GridDisplayEditor.Init( instance );
        #endif
    }
    void OnDestroy()
    {
        instance = null;

        #if UNITY_EDITOR
        GridDisplayEditor.Dispose();
        #endif
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(GridDisplay))]
public class GridDisplayEditor : Editor
{

    GUIStyle style_header;

    public override void OnInspectorGUI( )
    {
        if( style_header == null )
        {
            style_header = new GUIStyle( EditorStyles.boldLabel );
            style_header.fontSize = 20;
        }

        GUILayout.Space( 5 );
        GUILayout.Label( "Stage Settings", style_header );
        GUILayout.Label( "", GUI.skin.horizontalSlider );
        GUILayout.Space( 15 );

        base.OnInspectorGUI( );
    }

    static bool catched = false;
    static GridDisplay script;
    static SceneView view;

    public static void Init( GridDisplay target )
    {
        if( catched ) return; catched = true;

        script = target;

        SceneView.duringSceneGui += OnSceneGUI;
    }

    public static void Dispose()
    {
        if( ! catched ) return; catched = false;
        
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    static void ScreenLine( float x1, float y1, float x2, float y2, float thickness = 2f )

        => ScreenLine( new Vector2( x1, y1 ) , new Vector2( x2, y2 ), thickness );

    static void ScreenLine( Vector2 A, Vector2 B, float thickness = 2f )
    {
        var a = view.camera.ScreenToWorldPoint( A ); a.z = 0f;
        var b = view.camera.ScreenToWorldPoint( B ); b.z = 0f;

        Handles.DrawAAPolyLine( thickness, a, b ); // PolyLine hates Z in 2D view mode
    }

    static Tool prevTool = Tool.None;

    static void AutoTool()
    {
        if( Selection.activeObject == null ) Selection.activeGameObject = GridDisplay.instance.gameObject; 

        bool isStageSeleection = Selection.activeGameObject == GridDisplay.instance.gameObject;

        bool isGameObjSelected = Selection.activeGameObject != null;

        if( ! isStageSeleection && isGameObjSelected && Tools.current != Tool.None )
        {
            prevTool = Tools.current;
        }

        if( isStageSeleection && Tools.current != Tool.None )
        {
            prevTool = Tools.current;
            Tools.current = Tool.None;
        }
        
        if( ! isStageSeleection && prevTool != Tool.None )
        {
            Tools.current = prevTool;
            prevTool = Tool.None;
        }
        
        if( isGameObjSelected && ! isStageSeleection && Tools.current == Tool.None )
        {
            Tools.current = prevTool = Tool.Rect;
        }
    }

    static void OnSceneGUI( SceneView view )
    {
        // debug
        var line = ""; void PrintLine( params object[] s ) => line += string.Join(" ", s) + "\n";

        GridDisplayEditor.view = view;

        AutoTool();

        if( ! view.in2DMode ) return; // || ! script.drawGrid ) return;

        if( view.camera.activeTexture == null ) return; 

        var distance = view.cameraDistance;
        
        PrintLine("Distance:", distance);

        view.showGrid = false;

        var world_origin_2_screen = view.camera.WorldToScreenPoint( Vector3.zero );

        //var offset = - ( view.camera.WorldToScreenPoint( view.camera.transform.position ) ) ;//  - world_origin_2_screen );
        
        PrintLine( "World Origin 2 Screen:", world_origin_2_screen );

        var worldunit_in_screenspace = view.camera.WorldToScreenPoint( Vector3.one ) - world_origin_2_screen;

        PrintLine( "World Unit in Screen:", worldunit_in_screenspace );
        
        var screen_unit = Mathf.Min( worldunit_in_screenspace.x, worldunit_in_screenspace.y );

        PrintLine( "Screen Unit:", screen_unit );

        var w = screen_unit * script.width / 100f;
        var h = screen_unit * script.height / 100f;

        PrintLine( "Size:", w.ToString("N2"), "x" , h.ToString("N2") );



        //var screen_unit = Mathf.Min( worldunit_in_screenspace.x, worldunit_in_screenspace.y ) * script.scale;

        //var sizeX = screen_unit;
        //var sizeY = screen_unit * script.heightRatio;

        //PrintLine( "Size:", sizeX, sizeY );

        //if( sizeX < 15f || sizeY < 15f ) return;

        //float line_angle = script.angle;

        //float screen_w = view.camera.activeTexture.width + sizeX * 2;
        //float screen_h = view.camera.activeTexture.height + sizeY * 2;

        //var screen_size = new Vector3( screen_w, screen_h, 0 );

        //var angle_offset_x = Mathf.Sin( line_angle * Mathf.Deg2Rad ) * screen_size.magnitude;

        //var cell_step_offset = sizeX * ( angle_offset_x / screen_h ) * script.heightRatio;

        //var offset_relative = new Vector2
        //(
        //    world_origin_2_screen.x % sizeX,
        //    world_origin_2_screen.y % sizeY
        //);

        //PrintLine( "Offset Relative:", offset_relative );

        //float x0 = ( world_origin_2_screen.x % sizeX ) - sizeX;
        //float y0 = ( world_origin_2_screen.y % sizeY ) - sizeY;

        //PrintLine("Snap:", world_origin_2_screen.y / sizeY );

        //PrintLine("Step:" , y0 / sizeY );

        //// cool effect : shift Y axis to the angle 
        ////x0 += cell_step_offset * y0 / sizeY;

        ////x0 -= Mathf.Floor( y0 / sizeY ) * cell_step_offset;

        //x0 -= cell_step_offset * ( Mathf.Floor( world_origin_2_screen.y / sizeY ) ) % sizeX;

        //PrintLine( "Start:", x0, y0 );

        //var normalized_offset_y = offset_relative.y / sizeY;

        ////offset_relative.x -= normalized_offset_y * cell_step_offset;

        //var countX = screen_w / sizeX;
        //var countY = screen_h / sizeY;

        //PrintLine( "Count:", countX, countY );

        //// int outsideX = 0; // count outside indecies 

        //Handles.color = script.color;

        //Vector2 A = Vector2.zero;
        //Vector2 B = Vector2.zero;

        //for(var x = 0; x < countX + 1; ++x)
        //{
        //    var px = x * sizeX;

        //    var p2 = px + angle_offset_x;

        //    A.x = x0 + px;
        //    A.y = y0;

        //    B.x = x0 + px + angle_offset_x;
        //    B.y = y0 + screen_h;

        //    //A += offset_relative;
        //    //B += offset_relative;

        //    ScreenLine( A, B, script.thickness );
        //}

        //var countExtra = countX * ( ( screen_w + angle_offset_x ) / screen_w - 1 );

        //if( countExtra > 0 )
        //{
        //    var shift_x = 1 - countExtra % 1; 

        //    for(var x = 0; x < countExtra; ++x)
        //    {
        //        var px = x0 + ( x - shift_x ) * sizeX;

        //        A.x = px;
        //        A.y = y0 + screen_h;

        //        B.x = px - angle_offset_x;
        //        B.y = y0;

        //        //A += offset_relative;
        //        //B += offset_relative;

        //        ScreenLine( A, B, script.thickness );
        //        //ScreenLine( px, y0 + screen_h, px - angle_offset_x, y0, script.thickness );
        //    }
        //}
        //else if( countExtra < 0 )
        //{
        //    //Debug.Log( (screen_w % countX)  + " " + (screen_w * sizeX) );

        //    var step_x = screen_w % sizeX;

        //    var shift_x = countExtra % 1;

        //    for(var x = 1; x < - countExtra + 1; ++x)
        //    {
        //        var px = screen_w + x * sizeX - step_x;

        //        A.x = x0 + px;
        //        A.y = y0;

        //        B.x = x0 + px + angle_offset_x;
        //        B.y = screen_h;

        //        //A += offset_relative;
        //        //B += offset_relative;

        //        ScreenLine( A, B, script.thickness );

        //        //ScreenLine( px, 0, px + angle_offset_x, screen_h, script.thickness );
        //    }
        //}

        //for( var y = 0 ; y < countY; ++y )
        //{
        //    var py = y * sizeY;

        //    A.x = x0;
        //    A.y = y0 + py;

        //    B.x = x0 + screen_w;
        //    B.y = y0 + py;

        //    //A += offset_relative;
        //    //B += offset_relative;

        //    ScreenLine( A, B, script.thickness );

        //    //ScreenLine( 0, py, screen_w, py, script.thickness );
        //}

        //Debug.Log( Event.current.type );

        //if( Event.current.type != EventType.Repaint 
        //    || Event.current.type != EventType.Layout 
        //    || Event.current.type != EventType.Used ) return;

        // debug 
        Handles.BeginGUI();
        GUILayout.Label( line );
        Handles.EndGUI();
    }

    //void SkewTest()
    //{
    //    Vector2 A = Vector2.zero;
    //    Vector2 B = Vector2.zero;

    //    Vector2 Orig = view.camera.ViewportToScreenPoint( new Vector3( 0.5f, 0.5f ) );
        
    //    Vector2 C = Vector2.zero;
    //    Vector2 D = Vector2.zero;


    //    A = - Vector2.one * 20;
    //    C = Vector2.one * 20;
    //    B = new Vector2( -20, 20 );
    //    D = new Vector2( 20, -20 );
        
    //    //var m = Matrix4x4.TRS( Orig, Quaternion.identity, Vector4.one ); // TRS-position doens't work ... why  ?
    //    var m = Matrix4x4.identity;
        
    //    var skew = 0.45f;

    //    m.m01 = skew;
        
    //    A = m * A;
    //    B = m * B;
    //    C = m * C;
    //    D = m * D;
        
    //    // manual position

    //    ScreenLine( Orig + A, Orig + B, script.thickness );
    //    ScreenLine( Orig + B, Orig + C, script.thickness );
    //    ScreenLine( Orig + C, Orig + D, script.thickness );
    //    ScreenLine( Orig + D, Orig + A, script.thickness );
    //}
}
#endif