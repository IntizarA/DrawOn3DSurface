using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brush : ICloneable
{

    [SerializeField] private Texture brushTexture;
    [SerializeField] private Texture brushNormalTexture;
    [SerializeField] private Texture brushHeightTexture;
    
    [SerializeField, Range(0,1)] 
    private float brushScale=0.1f;

    private ICloneable _cloneableImplementation;

    public Texture BrushTexture
    {
        get { return brushTexture; }
        set { brushTexture = value; }
    }
    
    public Texture BrushNormalTexture
    {
        get { return brushNormalTexture; }
        set { brushNormalTexture = value; }
    }
    
    public Texture BrushHeightTexture
    {
        get { return brushHeightTexture; }
        set { brushHeightTexture = value; }
    }

    public object Clone ()
    {
        return new ();
    }
}
