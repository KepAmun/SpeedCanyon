using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpeedCanyon
{
    public class BoundingSphereRenderer
    {
        VertexBuffer _vb;
        BasicEffect _effect;

        const int SLICES = 10;
        const int STACKS = 20;

        public BoundingSphereRenderer(GraphicsDevice graphicsDevice)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[SLICES * STACKS];

            int i = 0;
            for (float theta = 0; theta < Math.PI; theta += (float)Math.PI / SLICES)
            {
                for (float phi = -(float)MathHelper.PiOver2; phi < MathHelper.TwoPi - MathHelper.PiOver2; phi += (float)MathHelper.TwoPi / STACKS)
                {
                    vertices[i].Position.X = (float)(Math.Cos(phi) * Math.Cos(theta));
                    vertices[i].Position.Y = (float)(Math.Sin(phi));
                    vertices[i].Position.Z = (float)(Math.Cos(phi) * Math.Sin(theta));
                    vertices[i].Color = Color.Green;
                    i++;
                }
            }

            _vb = new VertexBuffer(graphicsDevice, VertexPositionColor.VertexDeclaration, SLICES * STACKS, BufferUsage.None);
            _vb.SetData<VertexPositionColor>(vertices);

            _effect = new BasicEffect(graphicsDevice);
            _effect.VertexColorEnabled = true;
        }


        public void Render(BoundingSphere boundingSphere, Matrix world, Matrix view, Matrix projection)
        {
            _effect.GraphicsDevice.SetVertexBuffer(_vb);

            _effect.World = Matrix.CreateScale(boundingSphere.Radius) * Matrix.CreateTranslation(boundingSphere.Center) * world;
            _effect.View = view;
            _effect.Projection = projection;

            _effect.CurrentTechnique.Passes[0].Apply();

            _effect.GraphicsDevice.DrawPrimitives(PrimitiveType.LineStrip, 0, SLICES * STACKS - 1);

        }

    }
}
