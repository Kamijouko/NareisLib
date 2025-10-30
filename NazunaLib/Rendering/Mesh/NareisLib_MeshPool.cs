using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace NareisLib
{
    [StaticConstructorOnStartup]
    public static class NareisLib_MeshPool
    {
        public static Mesh FlippedMeshAt(Graphic graph, Rot4 rot)
        {
            Vector2 vector = graph.drawSize;
            if (rot.IsHorizontal && !graph.ShouldDrawRotated)
                vector = vector.Rotated();

            if ((rot == Rot4.West && graph.WestFlipped) || (rot == Rot4.East && graph.EastFlipped))
                return MeshPool.GridPlaneFlip(vector);

            if (rot == Rot4.East)
                return MeshPool.GridPlane(vector);

            return MeshPool.GridPlaneFlip(vector);
        }



        public static NareisLib_GraphicMeshSet GetMeshSetForWidth(float width)
        {
            return GetMeshSetForSize(width, width);
        }

        public static NareisLib_GraphicMeshSet GetMeshSetForSize(float width, float height)
        {
            Vector2 key = new Vector2(width, height);
            if (!humanlikeMeshSet_Custom.ContainsKey(key))
                humanlikeMeshSet_Custom[key] = new NareisLib_GraphicMeshSet(width, height);
            return humanlikeMeshSet_Custom[key];
        }

        public static readonly NareisLib_GraphicMeshSet humanlikeBodySet = new NareisLib_GraphicMeshSet(1.5f);

        public static readonly NareisLib_GraphicMeshSet humanlikeHeadSet = new NareisLib_GraphicMeshSet(1.5f);

        private static readonly Dictionary<Vector2, NareisLib_GraphicMeshSet> humanlikeMeshSet_Custom = new Dictionary<Vector2, NareisLib_GraphicMeshSet>();

        private static Dictionary<Vector2, Mesh> planes = new Dictionary<Vector2, Mesh>(FastVector2Comparer.Instance);

        private static Dictionary<Vector2, Mesh> planesFlip = new Dictionary<Vector2, Mesh>(FastVector2Comparer.Instance);
    }
}
