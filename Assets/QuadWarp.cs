using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class QuadWarp : MonoBehaviour {

    public Material _mat;

    public Texture _tex;
    public Vector2[] _vertixes = new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };

    Matrix4x4 CalcHomography(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        var sx = p0.x - p1.x + p2.x - p3.x;
        var sy = p0.y - p1.y + p2.y - p3.y;

        var dx1 = p1.x - p2.x;
        var dx2 = p3.x - p2.x;
        var dy1 = p1.y - p2.y;
        var dy2 = p3.y - p2.y;

        var z = (dy1 * dx2) - (dx1 * dy2);
        var g = ((sx * dy1) - (sy * dx1)) / z;
        var h = ((sy * dx2) - (sx * dy2)) / z;

        var system = new[]{
            p3.x * g - p0.x + p3.x,
            p1.x * h - p0.x + p1.x,
            p0.x,
            p3.y * g - p0.y + p3.y,
            p1.y * h - p0.y + p1.y,
            p0.y,
            g,
            h,
        };

        var mtx = Matrix4x4.identity;
        mtx.m00 = system[0]; mtx.m01 = system[1]; mtx.m02 = system[2];
        mtx.m10 = system[3]; mtx.m11 = system[4]; mtx.m12 = system[5];
        mtx.m20 = system[6]; mtx.m21 = system[7]; mtx.m22 = 1f;

		return mtx;
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var homography = CalcHomography(_vertixes[0], _vertixes[1], _vertixes[2], _vertixes[3]).inverse;
        
        Graphics.SetRenderTarget(destination);
        GL.Clear(true, true, Color.clear);

        GL.PushMatrix();
        GL.LoadOrtho();

        _mat.mainTexture = _tex;
        _mat.SetMatrix("_Homography", homography);
        _mat.SetPass(0);

        var rectPixel = new Rect(0f, 0f , Screen.width, Screen.height);
        GL.Viewport(rectPixel);

        GL.Begin(GL.QUADS);

        for (var i = 0; i < 4; ++i)
        {
            GL.Vertex(_vertixes[i]);
        }

        GL.End();


        GL.PopMatrix();
    }
}
