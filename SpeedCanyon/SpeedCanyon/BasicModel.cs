using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpeedCanyon
{
    class BasicModel
    {
        public Model Model { get; protected set; }
        protected Matrix _world = Matrix.Identity;

        public BasicModel(Model m)
        {
            Model = m;

        }

        public virtual void Update()
        {

        }

        public void Draw(Camera camera)
        {
            //Matrix[] transforms = new Matrix[_model.Bones.Count];
            //_model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.EnableDefaultLighting();
                    be.Projection = camera.Projection;
                    be.View = camera.View;
                    be.World = GetWorld() * mesh.ParentBone.Transform;
                }

                mesh.Draw();
            }
        }

        public virtual Matrix GetWorld()
        {
            return _world;
        }

        public bool CollidesWith(Model otherModel, Matrix otherWorld)
        {
            // Loop through each ModelMesh in both objects and compare
            // all bounding spheres for collisions
            foreach (ModelMesh myModelMeshes in Model.Meshes)
            {
                foreach (ModelMesh hisModelMeshes in otherModel.Meshes)
                {
                    if (myModelMeshes.BoundingSphere.Transform(
                        GetWorld()).Intersects(
                        hisModelMeshes.BoundingSphere.Transform(otherWorld)))
                        return true;
                }
            }
            return false;
        }
    }
}
