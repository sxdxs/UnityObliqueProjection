#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;

public class GridTileSettings : ScriptableObject
{
    [System.Serializable] 
    public struct TileData
    {
        public bool isNull;
        public int width;
        public int height;
        public int offsetX;
        public int offsetY;
    }
    
    public static readonly TileData NullTile = new TileData{ isNull = true };

    public List<Object> references;
    public List<TileData> tiles;

    public bool Has( Object reference ) => references.Contains( reference );
    public bool Has( TileData data ) => tiles.Contains( data );
    
    public void Add( Object reference, TileData data )
    {
        references.Add( reference );
        tiles.Add( data );
    }
    public void Set( Object reference, TileData data )
    {
        for( var i = 0; i < references.Count; ++i )
            if( references[ i ] == reference )
                tiles[ i ] = data;
    }
    
    public TileData Get( Object reference )
    {
        for( var i = 0; i < references.Count; ++i )
            if( references[ i ] == reference )
                return tiles[ i ];

        return NullTile;
    }


    public static GridTileSettings Create( string prefix )
    {
        var data = ScriptableObject.CreateInstance<GridTileSettings>();

        var path = "Assets/" + prefix + "_" + (typeof(GridTileSettings).Name.Replace(" ", "") + ".asset");

        AssetDatabase.CreateAsset( data, path );
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        // Selection.activeObject = data;

        return data;
    }

}
