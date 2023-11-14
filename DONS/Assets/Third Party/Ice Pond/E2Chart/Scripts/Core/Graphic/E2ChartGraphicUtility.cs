using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public static class E2ChartGraphicUtility
    {
        public static Vector2[] GetCosSin(float side, float offset = 0.0f, float total = 360.0f, bool last = true)
        {
            Vector2[] cs = last ? new Vector2[Mathf.CeilToInt(side) + 1] : new Vector2[Mathf.CeilToInt(side)];
            float unit = (total / side) * Mathf.Deg2Rad;
            offset *= Mathf.Deg2Rad;
            for (int i = 0; i < cs.Length; ++i)
            {
                float angle = offset + unit * i;
                cs[i].x = Mathf.Cos(angle);
                cs[i].y = Mathf.Sin(angle);
            }
            return cs;
        }

        public static float GetAngle(Vector2 angleVector)
        {
            float angle;
            angle = -Vector2.SignedAngle(new Vector2(0.0f, 1.0f), angleVector);
            angle = Mathf.Repeat(angle, 360.0f);
            return angle;
        }

        public static Vector2 GetAngleVector(float angle)
        {
            Vector2 cosSin = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            return RotateCW(Vector2.up, cosSin);
        }

        public static int GetAngleIndex(float angle, float start, float width)
        {
            if (width < 0.1f) return -1;
            float index = (angle - start) / width;
            index = Mathf.Repeat(index, 360.0f / width);
            return Mathf.FloorToInt(index);
        }

        public static int GetSector(Vector2 vec)
        {
            int sec = 0;
            if (vec.x >= 0.0f && vec.y > 0.0f) sec = 0;
            else if (vec.y <= 0.0f && vec.x > 0.0f) sec = 1;
            else if (vec.x <= 0.0f && vec.y < 0.0f) sec = 2;
            else sec = 3;
            return sec;
        }

        public static int GetSector(float angle)
        {
            int sec = 0;
            angle = Mathf.Repeat(angle, 360.0f);
            if (angle < 90.0f) sec = 0;
            else if (angle < 180.0f) sec = 1;
            else if (angle < 270.0f) sec = 2;
            else sec = 3;
            return sec;
        }

        public static Vector2 RotateCW(Vector2 p, Vector2 cs)
        {
            Vector2 pp = new Vector2();
            pp.x = p.x * cs.x + p.y * cs.y;
            pp.y = -p.x * cs.y + p.y * cs.x;
            return pp;
        }

        public static Vector2 RotateCCW(Vector2 p, Vector2 cs)
        {
            Vector2 pp = new Vector2();
            pp.x = p.x * cs.x - p.y * cs.y;
            pp.y = p.x * cs.y + p.y * cs.x;
            return pp;
        }

        public static Vector2[] CreateRect(Vector2 dir, float dis)
        {
            Vector2[] points = new Vector2[4];

            Vector2 v = Vector3.Cross(dir, Vector3.forward);
            v.Normalize();

            points[0] = v * dis;
            points[1] = v * dis + dir;
            points[2] = -v * dis + dir;
            points[3] = -v * dis;

            return points;
        }

        public static Vector2 BerzierCurve(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return Mathf.Pow(1 - t, 3) * p0 + 3 * Mathf.Pow(1 - t, 2) * t * p1 + 3 * (1 - t) * t * t * p2 + Mathf.Pow(t, 3) * p3;
        }

        public static Vector2 LineIntersection(Vector2[] points) //p0-p2, p1-p3
        {
            float t = (points[1].x - points[0].x) * (points[0].y - points[3].y) - (points[0].x - points[3].x) * (points[1].y - points[0].y);
            t /= (points[1].x - points[2].x) * (points[0].y - points[3].y) - (points[0].x - points[3].x) * (points[1].y - points[2].y);
            Vector2 pIntersect = points[1] + (points[2] - points[1]) * t;
            return pIntersect;
        }
    }
}