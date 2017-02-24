using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class QuadWarp : MonoBehaviour {

    public Material _mat;

    public Texture _tex;
    public Vector2[] _uvs = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
    public Vector2[] _vertixes = new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };

    Matrix4x4 CalcHomography(Vector2 topLeft, Vector2 topRight, Vector2 bottomRight, Vector2 bottomLeft)
    {
        var sx = (topLeft.x - topRight.x) + (bottomRight.x - bottomLeft.x);
        var sy = (topLeft.y - topRight.y) + (bottomRight.y - bottomLeft.y);

        var dx1 = topRight.x - bottomRight.x;
        var dx2 = bottomLeft.x - bottomRight.x;
        var dy1 = topRight.y - bottomRight.y;
        var dy2 = bottomLeft.y - bottomRight.y;

        var z = (dx1 * dy2) - (dy1 * dx2);
        var g = ((sx * dy2) - (sy * dx2)) / z;
        var h = ((sy * dx1) - (sx * dy1)) / z;

        var system = new[]{
            topRight.x - topLeft.x + g * topRight.x,
            bottomLeft.x - topLeft.x + h * bottomLeft.x,
            topLeft.x,
            topRight.y - topLeft.y + g * topRight.y,
            bottomLeft.y - topLeft.y + h * bottomLeft.y,
            topLeft.y,
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
        var homographyVtx = CalcHomography(_vertixes[0], _vertixes[3], _vertixes[2], _vertixes[1]);
        var homographyUV = CalcHomography(_uvs[0], _uvs[3], _uvs[2], _uvs[1]);
        var homography = homographyUV * homographyVtx.inverse;
        
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
