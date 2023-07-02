using UnityEngine;
using System;
using System.Collections.Generic;

namespace QueuedGizmos
{
    static class DrawQueue
    {
        /// <summary>
        /// A list that contains queued drawing commands, is does the draw only once, it will need to be reinserted before the next frame to be drawn again.
        /// </summary>
        static List<GizmoQueueDrawable> delayedDraws = new List<GizmoQueueDrawable>();
        /// <summary>
        /// Contains permanet draws these will last for the entire program unless manually deleted.
        /// </summary>
        static List<GizmoQueueDrawable> permanentDraws = new List<GizmoQueueDrawable>();

        /// <summary>
        /// Adds an IQueueDraw to the Queued Drawing System, useful if need to draw in a scope which can't have batched rendering enabled.
        /// </summary>
        public static void Add(GizmoQueueDrawable draw, bool permanent = false)
        {
            if (permanent)
            {
                permanentDraws.Add(draw);
            }
            else
            {
                delayedDraws.Add(draw);
            }
        }

        /// <summary>
        /// Begins the drawing of the queued drawings. (requires batch rendering to be active)
        /// </summary>
        public static void DrawAll()
        {
            var tmp = Gizmos.color;
            foreach (var item in delayedDraws)
            {
                Gizmos.color = item.color;
                item.Draw();
            }

            foreach (var item in permanentDraws)
            {
                Gizmos.color = item.color;
                item.Draw();
            }

            Gizmos.color = tmp;
            delayedDraws.Clear();
        }
        public static void Clear()
        {
            permanentDraws.Clear();
        }
    }

    abstract class GizmoQueueDrawable
    {
        public Color color;

        public abstract void Draw();
    }

    class DrawRect : GizmoQueueDrawable
    {
        Vector3 position;
        float width, height, depth;

        public static DrawRect Create(Vector3 position, float width, float depth, float height, Color color)
        {
            return new DrawRect() { position = position, width = width, height = height, depth = depth, color = color };
        }

        public override void Draw()
        {
            var flt = new Vector3(width, height, depth);
            var flb = new Vector3(width, -height, depth);

            var frt = new Vector3(-width, height, depth);
            var frb = new Vector3(-width, -height, depth);

            var blt = new Vector3(width, height, -depth);
            var blb = new Vector3(width, -height, -depth);

            var brt = new Vector3(-width, height, -depth);
            var brb = new Vector3(-width, -height, -depth);
            // For vertical pieces
            Gizmos.DrawLine(flt + position, flb + position);
            Gizmos.DrawLine(frt + position, frb + position);
            Gizmos.DrawLine(blt + position, blb + position);
            Gizmos.DrawLine(brt + position, brb + position);

            // Top
            Gizmos.DrawLine(flt + position, frt + position);
            Gizmos.DrawLine(frt + position, brt + position);
            Gizmos.DrawLine(brt + position, blt + position);
            Gizmos.DrawLine(blt + position, flt + position);

            // Bottom
            Gizmos.DrawLine(flb + position, frb + position);
            Gizmos.DrawLine(frb + position, brb + position);
            Gizmos.DrawLine(brb + position, blb + position);
            Gizmos.DrawLine(blb + position, flb + position);
        }
    }

    class DrawCube : GizmoQueueDrawable
    {
        Vector3 position;
        Vector3 size;

        public static DrawCube Create(Vector3 position, Vector3 size, Color color)
        {
            return new DrawCube() { position = position, size = size, color = color };
        }

        public override void Draw()
        {
            Gizmos.DrawCube(position, size);
        }
    }

    class DrawSphere : GizmoQueueDrawable
    {
        Vector3 position;
        float radius;

        public static DrawSphere Create(Vector3 position, float radius, Color color)
        {
            return new DrawSphere() { position = position, radius = radius, color = color };
        }

        public override void Draw()
        {
            Gizmos.DrawSphere(position, radius);
        }
    }

    class DrawLine : GizmoQueueDrawable
    {
        Vector3 from;
        Vector3 to;

        public static DrawLine Create(Vector3 from, Vector3 to, Color color)
        {
            return new DrawLine() { from = from, to = to, color = color };
        }

        public override void Draw()
        {
            Gizmos.DrawLine(from, to);
        }
    }

    class DrawArrow : GizmoQueueDrawable
    {
        Vector3 from;
        Vector3 to;

        public static DrawArrow Create(Vector3 from, Vector3 to, Color color)
        {
            return new DrawArrow() { from = from, to = to, color = color };
        }

        public override void Draw()
        {
            Vector3 dir = from - to;
            Vector3 dirn = dir.normalized;

            Vector3 paralel1 = Vector3.Cross(dirn, Vector3.forward).normalized * dir.magnitude;
            Vector3 paralel2 = Vector3.Cross(dirn, Vector3.back).normalized * dir.magnitude;

            Gizmos.DrawLine(from, to);
            Gizmos.DrawLine(to + dir / 2f, to + dir / 2f + (paralel1 + dir) * 0.1f);
            Gizmos.DrawLine(to + dir / 2f, to + dir / 2f + (paralel2 + dir) * 0.1f);
        }
    }


    class DrawRay : GizmoQueueDrawable
    {
        Vector3 center;
        Quaternion direction;

        public static DrawRay Create(Vector3 center, Quaternion direction, Color color)
        {
            return new DrawRay() { center = center, direction = direction, color = color };
        }

        public override void Draw()
        {
            Vector3 fwd = direction * Vector3.forward;
            Debug.DrawRay(center, fwd, color, 0f, true);
        }
    }
}
