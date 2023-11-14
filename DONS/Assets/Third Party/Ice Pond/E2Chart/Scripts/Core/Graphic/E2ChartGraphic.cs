using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace E2C.ChartGraphic
{
    public abstract class E2ChartGraphic : MaskableGraphic
    {
        protected const float STEP_SIZE = 10.0f;
        protected const float CURVATURE = 0.1f;
        protected const float CURVE_UNIT = 1.0f;
        protected static Vector2[] uvRect = new Vector2[] {
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f)
        };
        protected static Vector2[] uvUp = new Vector2[] {
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f)
        };
        protected static Vector2[] uvRight = new Vector2[] {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f)
        };

        [SerializeField] Texture m_Texture;

        [HideInInspector] public bool isDirty = true;
        [HideInInspector] public bool inited = false;

        protected List<UIVertex> vertices;
        protected List<int> indices;

        static Vector2[] m_cossin;

        public Vector2 rectSize { get => rectTransform.rect.size; }

        public static Vector2[] CosSin
        {
            get
            {
                if (m_cossin == null) m_cossin = E2ChartGraphicUtility.GetCosSin(128);
                return m_cossin;
            }
        }

        public override Texture mainTexture
        {
            get { return m_Texture == null ? s_WhiteTexture : m_Texture; }
        }

        public Texture texture
        {
            get
            {
                return m_Texture;
            }
            set
            {
                if (m_Texture == value) return;
                m_Texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (GetComponent<CanvasRenderer>() == null) gameObject.AddComponent<CanvasRenderer>();
            raycastTarget = false;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            isDirty = true;
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (isDirty) RefreshBuffer();
            if (!inited) return;
            vertices = new List<UIVertex>();
            indices = new List<int>();
            GenerateMesh();
            vh.AddUIVertexStream(vertices, indices);
            vertices = null;
            indices = null;
        }
        
        public virtual void RefreshBuffer() { isDirty = false; inited = true; }

        protected abstract void GenerateMesh();

        protected bool IsXInsideRect(float x)
        {
            return x >= 0.0f && x <= rectSize.x;
        }

        protected bool IsYInsideRect(float y)
        {
            return y >= 0.0f && y <= rectSize.y;
        }

        protected bool IsPointInsideRect(Vector2 point)
        {
            return point.x >= 0.0f && point.x <= rectSize.x && 
                   point.y >= 0.0f && point.y <= rectSize.y;
        }

        protected bool IsPointInsideRect(float x, float y)
        {
            return x >= 0.0f && x <= rectSize.x &&
                   y >= 0.0f && y <= rectSize.y;
        }

        protected void AddPolygonRect(Vector2[] points, Color color, Vector2[]uv = null)
        {
            if (uv == null) uv = uvRect;
            int index = vertices.Count;
            UIVertex[] v = new UIVertex[4];
            v[0].position = points[0];
            v[1].position = points[1];
            v[2].position = points[2];
            v[3].position = points[3];
            v[0].color = color;
            v[1].color = color;
            v[2].color = color;
            v[3].color = color;
            v[0].uv0 = uv[0];
            v[1].uv0 = uv[1];
            v[2].uv0 = uv[2];
            v[3].uv0 = uv[3];
            vertices.AddRange(v);

            int[] tri = new int[] { index, index + 1, index + 2, index + 2, index + 3, index };
            indices.AddRange(tri);
        }

        protected void AddPolygonRectUp(Vector2[] points, Color color, Vector2[] uv = null)
        {
            if (uv == null) uv = uvUp;
            int index = vertices.Count;
            UIVertex[] v = new UIVertex[4];
            v[0].position = points[0];
            v[1].position = points[1];
            v[2].position = points[2];
            v[3].position = points[3];
            v[0].color = color;
            v[1].color = color;
            v[2].color = color;
            v[3].color = color;
            v[0].uv0 = uv[0];
            v[1].uv0 = uv[1];
            v[2].uv0 = uv[1];
            v[3].uv0 = uv[0];
            vertices.AddRange(v);

            int[] tri = new int[] { index, index + 1, index + 2, index + 2, index + 3, index };
            indices.AddRange(tri);
        }

        protected void AddPolygonRectRight(Vector2[] points, Color color, Vector2[] uv = null)
        {
            if (uv == null) uv = uvRight;
            int index = vertices.Count;
            UIVertex[] v = new UIVertex[4];
            v[0].position = points[0];
            v[1].position = points[1];
            v[2].position = points[2];
            v[3].position = points[3];
            v[0].color = color;
            v[1].color = color;
            v[2].color = color;
            v[3].color = color;
            v[0].uv0 = uv[0];
            v[1].uv0 = uv[0];
            v[2].uv0 = uv[1];
            v[3].uv0 = uv[1];
            vertices.AddRange(v);

            int[] tri = new int[] { index, index + 1, index + 2, index + 2, index + 3, index };
            indices.AddRange(tri);
        }
        
        protected void AddPolygonRectRight(Vector2 pLast, Vector2[] points, Color color, Vector2[] uv = null)
        {
            if (uv == null) uv = uvRight;
            int index = vertices.Count;
            UIVertex[] v = new UIVertex[4];
            v[0].position = points[0] + pLast;
            v[1].position = points[1] + pLast;
            v[2].position = points[2] + pLast;
            v[3].position = points[3] + pLast;
            v[0].color = color;
            v[1].color = color;
            v[2].color = color;
            v[3].color = color;
            v[0].uv0 = uv[0];
            v[1].uv0 = uv[0];
            v[2].uv0 = uv[1];
            v[3].uv0 = uv[1];
            vertices.AddRange(v);

            int[] tri = new int[] { index, index + 1, index + 2, index + 2, index + 3, index };
            indices.AddRange(tri);
        }
        
        protected void AddPolygonRectIntersect(Vector2[] points, Color color, Vector2[] uv = null)
        {
            if (uv == null) uv = uvRect;
            int index = vertices.Count;
            if (Vector2.Dot(points[1] - points[0], points[2] - points[3]) >= 0.0f)
            {
                UIVertex[] v = new UIVertex[4];
                v[0].position = points[0];
                v[1].position = points[1];
                v[2].position = points[2];
                v[3].position = points[3];
                v[0].color = color;
                v[1].color = color;
                v[2].color = color;
                v[3].color = color;
                v[0].uv0 = uv[0];
                v[1].uv0 = uv[1];
                v[2].uv0 = uv[2];
                v[3].uv0 = uv[3];
                vertices.AddRange(v);

                int[] tri = new int[] { index, index + 1, index + 2, index + 2, index + 3, index };
                indices.AddRange(tri);
            }
            else
            {
                UIVertex[] v = new UIVertex[5];
                v[0].position = points[0];
                v[1].position = points[1];
                v[2].position = points[2];
                v[3].position = points[3];
                v[4].position = E2ChartGraphicUtility.LineIntersection(points);
                v[0].color = color;
                v[1].color = color;
                v[2].color = color;
                v[3].color = color;
                v[4].color = color;
                v[0].uv0 = uv[0];
                v[1].uv0 = uv[1];
                v[2].uv0 = uv[2];
                v[3].uv0 = uv[3];
                v[4].uv0 = Vector2.zero;
                vertices.AddRange(v);

                int[] tri = new int[] { index, index + 1, index + 4, index + 4, index + 3, index + 2 };
                indices.AddRange(tri);
            }
        }
    }
}