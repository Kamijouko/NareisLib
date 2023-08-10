using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class NareisLib_GraphicMeshSet
    {
        public NareisLib_GraphicMeshSet(float size)
        {
            this.meshes[0] = (this.meshes[2] = MeshMakerPlanes.NewPlaneMesh(size, false, true));
            this.meshes[1] = MeshMakerPlanes.NewPlaneMesh(size, false, true);
            this.meshes[3] = MeshMakerPlanes.NewPlaneMesh(size, true, true);

            this.meshes_flip[0] = (this.meshes_flip[2] = MeshMakerPlanes.NewPlaneMesh(size, true, true));
            this.meshes_flip[1] = MeshMakerPlanes.NewPlaneMesh(size, false, true);
            this.meshes_flip[3] = MeshMakerPlanes.NewPlaneMesh(size, true, true);
        }

        public NareisLib_GraphicMeshSet(float width, float height)
        {
            Vector2 size = new Vector2(width, height);
            this.meshes[0] = (this.meshes[2] = MeshMakerPlanes.NewPlaneMesh(size, false, true, false));
            this.meshes[1] = MeshMakerPlanes.NewPlaneMesh(size, false, true, false);
            this.meshes[3] = MeshMakerPlanes.NewPlaneMesh(size, true, true, false);

            this.meshes_flip[0] = (this.meshes_flip[2] = MeshMakerPlanes.NewPlaneMesh(size, true, true, false));
            this.meshes_flip[1] = MeshMakerPlanes.NewPlaneMesh(size, false, true, false);
            this.meshes_flip[3] = MeshMakerPlanes.NewPlaneMesh(size, true, true, false);
        }

        public Mesh MeshAt(Rot4 rot, bool flipped = false)
        {
            return flipped ? meshes_flip[rot.AsInt] : meshes[rot.AsInt];
        }

        private Mesh[] meshes = new Mesh[4];
        private Mesh[] meshes_flip = new Mesh[4];
    }
}
