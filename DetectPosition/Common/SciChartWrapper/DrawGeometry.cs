using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using SciChart.Charting3D;
using SciChart.Charting3D.Interop;
using SciChart.Charting3D.Primitives;
using SciChart.Charting3D.RenderableSeries;

namespace SciChartWrapper
{
    /// <summary>
    /// A class to demonstrate a 3D Geometry added to the SciChart3D Scene. Created using our BaseSceneEntity and Mesh APIs
    /// </summary>
    public class DrawGeometry : BaseSceneEntity
    {
        public Vector3 firstPos;
        public Vector3 secondPos;
        public Vector3 ThirdPos; // For Circle 3P
        public List<Vector3> circlePos; // For Draw Circle

        public Color faceColor;
        public Color lineColor;

        public ECheckShape checkShape;

        public DrawGeometry(Vector3 firstPos, Vector3 secondPos, Color faceColor, ECheckShape checkShape)
        {
            // Shady : Setting the position of scene entities will be used back when sorting them from camera perspective back to front
            using (TSRVector3 centerPosition = new TSRVector3(
                    0.5f * (firstPos.x + secondPos.x),
                    0.5f * (firstPos.y + secondPos.y),
                    0.5f * (firstPos.z + secondPos.z)))
            {
                SetPosition(centerPosition);
            }

            this.firstPos = firstPos;
            this.secondPos = secondPos;
            this.faceColor = faceColor;
            lineColor = Color.FromArgb(255, faceColor.R, faceColor.G, faceColor.B);
            this.checkShape = checkShape;
            circlePos = new List<Vector3>();
        }

        /// <summary>
        /// Determines a kind of the entity. If SCRT_SCENE_ENTITY_KIND_TRANSPARENT then the 3D Engine must make some internal adjustments to allow order independent transparency
        /// </summary>
        public override eSCRTSceneEntityKind GetKind()
        {
            return faceColor.A == 255 ? eSCRTSceneEntityKind.SCRT_SCENE_ENTITY_KIND_OPAQUE : eSCRTSceneEntityKind.SCRT_SCENE_ENTITY_KIND_TRANSPARENT;
        }

        /// <summary>
        ///     Called when the 3D Engine wishes to update the geometry in this element. This is where we need to cache geometry
        ///     before draw.
        /// </summary>
        /// <param name="e">The <see cref="IRenderPassInfo3D" /> containing parameters for the current render pass.</param>
        public override void UpdateScene(IRenderPassInfo3D e)
        {
        }

        /// <summary>
        ///     Called when the 3D Engine wishes to render this element. This is where geometry must be drawn to the 3D scene
        /// </summary>
        /// <param name="e">The <see cref="IRenderPassInfo3D" /> containing parameters for the current render pass.</param>
        public override void RenderScene(IRenderPassInfo3D e)
        {
            // y          2--------0
            // |         /|       /|
            // |       3--------1  |
            // |       |  |     |  |
            // |       |  |     |  |
            // |       |  6--------4
            // |  z    | /      |/    
            // | /     7--------5        
            // |/
            // ----------- X

            Vector3[] points;
            //Vector3[] normals;

            Vector3[] normals = {
                new Vector3(+0.0f, +0.0f, -1.0f), //front
                new Vector3(+0.0f, +0.0f, +1.0f), //back
                new Vector3(+1.0f, +0.0f, +0.0f), //right 
                new Vector3(-1.0f, +0.0f, +0.0f), //left                               
                new Vector3(+0.0f, +1.0f, +0.0f), //top
                new Vector3(+0.0f, -1.0f, +0.0f), //bottom
                //new Vector3(+0.0f, +0.0f, +0.0f) //center
            };

            switch (checkShape)
            {
                case ECheckShape.Line:

                    if(firstPos.Y > secondPos.Y)
                    {
                        points = new Vector3[] {
                            new Vector3(firstPos.X, firstPos.Y, firstPos.Z),
                            new Vector3(secondPos.X, firstPos.Y, secondPos.Z),
                            new Vector3(firstPos.X, 0, firstPos.Z),
                            new Vector3(secondPos.X, 0, secondPos.Z)
                        };
                    }
                    else
                    {
                        points = new Vector3[] {
                            new Vector3(firstPos.X, secondPos.Y, firstPos.Z),
                            new Vector3(secondPos.X, secondPos.Y, secondPos.Z),
                            new Vector3(firstPos.X, 0, firstPos.Z),
                            new Vector3(secondPos.X, 0, secondPos.Z)
                        };
                    }

                    using (var rectContext = base.BeginLitMesh(TSRRenderMode.TRIANGLES))
                    {
                        SCRTImmediateDraw.PushRasterizerState(RasterizerStates.CullBackFacesState.TSRRasterizerState);
                        rectContext.SetVertexColor(lineColor);

                        SetNormal(rectContext, normals[0]); // Front
                        SetVertex(rectContext, points[0]);
                        SetVertex(rectContext, points[2]);
                        SetVertex(rectContext, points[1]);
                        SetVertex(rectContext, points[1]);
                        SetVertex(rectContext, points[2]);
                        SetVertex(rectContext, points[3]);

                        SetNormal(rectContext, normals[1]); // Back
                        SetVertex(rectContext, points[0]);
                        SetVertex(rectContext, points[3]);
                        SetVertex(rectContext, points[2]);
                        SetVertex(rectContext, points[3]);
                        SetVertex(rectContext, points[0]);
                        SetVertex(rectContext, points[1]);
                    }
                    SCRTImmediateDraw.PopRasterizerState();

                    SCRTImmediateDraw.PushRasterizerState(RasterizerStates.WireframeState.TSRRasterizerState);
                    DrawSquareLine(new Vector3[] { points[0], points[1], points[3], points[2] });
                    SCRTImmediateDraw.PopRasterizerState();
                    break;
                case ECheckShape.Rect:
                    points = new Vector3[] {
                        new Vector3(firstPos.X, firstPos.Y, firstPos.Z), // Top
                        new Vector3(secondPos.X, firstPos.Y, firstPos.Z),
                        new Vector3(firstPos.X, secondPos.Y, secondPos.Z),
                        new Vector3(secondPos.X, secondPos.Y, secondPos.Z),
                        new Vector3(firstPos.X, 0, firstPos.Z), // Bottom
                        new Vector3(secondPos.X, 0, firstPos.Z),
                        new Vector3(firstPos.X, 0, secondPos.Z),
                        new Vector3(secondPos.X, 0, secondPos.Z)
                    };

                    using (var cubeContext = base.BeginLitMesh(TSRRenderMode.TRIANGLES))
                    {
                        SCRTImmediateDraw.PushRasterizerState(RasterizerStates.CullBackFacesState.TSRRasterizerState);
                        cubeContext.SetVertexColor(faceColor);
                        
                        // y          2--------0
                        // |         /|       /|
                        // |       3--------1  |
                        // |       |  |     |  |
                        // |       |  |     |  |
                        // |       |  6--------4
                        // |  z    | /      |/    
                        // | /     7--------5        
                        // |/
                        // ----------- X

                        SetNormal(cubeContext, normals[5]); // Front
                        SetVertex(cubeContext, points[0]);
                        SetVertex(cubeContext, points[6]);
                        SetVertex(cubeContext, points[2]);
                        SetVertex(cubeContext, points[6]);
                        SetVertex(cubeContext, points[0]);
                        SetVertex(cubeContext, points[4]);
                        
                        SetNormal(cubeContext, normals[1]); // Back
                        SetVertex(cubeContext, points[5]);
                        SetVertex(cubeContext, points[3]);
                        SetVertex(cubeContext, points[7]);
                        SetVertex(cubeContext, points[5]);
                        SetVertex(cubeContext, points[1]);
                        SetVertex(cubeContext, points[3]);
                        
                        SetNormal(cubeContext, normals[2]); // Left
                        SetVertex(cubeContext, points[4]);
                        SetVertex(cubeContext, points[0]);
                        SetVertex(cubeContext, points[1]);
                        SetVertex(cubeContext, points[4]);
                        SetVertex(cubeContext, points[1]);
                        SetVertex(cubeContext, points[5]);
                        
                        SetNormal(cubeContext, normals[3]); // right
                        SetVertex(cubeContext, points[2]);
                        SetVertex(cubeContext, points[6]);
                        SetVertex(cubeContext, points[7]);
                        SetVertex(cubeContext, points[2]);
                        SetVertex(cubeContext, points[7]);
                        SetVertex(cubeContext, points[3]);
                        
                        SetNormal(cubeContext, normals[4]); // Top
                        SetVertex(cubeContext, points[6]);
                        SetVertex(cubeContext, points[5]);
                        SetVertex(cubeContext, points[7]);
                        SetVertex(cubeContext, points[5]);
                        SetVertex(cubeContext, points[6]);
                        SetVertex(cubeContext, points[4]);
                        
                        SetNormal(cubeContext, normals[0]); // Bottom
                        SetVertex(cubeContext, points[0]);
                        SetVertex(cubeContext, points[2]);
                        SetVertex(cubeContext, points[3]);
                        SetVertex(cubeContext, points[0]);
                        SetVertex(cubeContext, points[3]);
                        SetVertex(cubeContext, points[1]);
                    }

                    SCRTImmediateDraw.PopRasterizerState();
                    SCRTImmediateDraw.PushRasterizerState(RasterizerStates.WireframeState.TSRRasterizerState);

                    DrawSquareLine(new Vector3[] { points[0], points[1], points[3], points[2] });
                    DrawSquareLine(new Vector3[] { points[0], points[1], points[5], points[4] });
                    DrawSquareLine(new Vector3[] { points[0], points[2], points[6], points[4] });
                    DrawSquareLine(new Vector3[] { points[2], points[3], points[7], points[6] });
                    DrawSquareLine(new Vector3[] { points[1], points[3], points[7], points[5] });
                    DrawSquareLine(new Vector3[] { points[4], points[6], points[7], points[5] });

                    SCRTImmediateDraw.PopRasterizerState();
                    break;
                case ECheckShape.Circle_2P:
                    points = new Vector3[] {
                        new Vector3(firstPos.X, firstPos.Y, firstPos.Z),
                        new Vector3(secondPos.X, secondPos.Y, secondPos.Z)
                    };

                    circlePos.Add(points[0]);

                    using (var circleContext = base.BeginLitMesh(TSRRenderMode.TRIANGLEFAN))
                    {
                        SCRTImmediateDraw.PushRasterizerState(RasterizerStates.CullBackFacesState.TSRRasterizerState);
                        circleContext.SetVertexColor(faceColor);

                        SetNormal(circleContext, normals[4]);
                        Vector3 prev = points[1];
                        Vector3 post = points[1];

                        circlePos.Add(prev);
                        circlePos.Add(post);

                        for (int i = 0; i < 180; i++)
                        {
                            SetVertex(circleContext, points[0]);
                            SetVertex(circleContext, prev);
                            SetVertex(circleContext, post);
                            prev = post;

                            post.x += 1.536f;
                            post.z += 0.2865f;

                            circlePos.Add(prev);
                            circlePos.Add(post);
                        }

                        SCRTImmediateDraw.PopRasterizerState();
                        SCRTImmediateDraw.PushRasterizerState(RasterizerStates.WireframeState.TSRRasterizerState);

                        using (var lineContext = base.BeginLineStrips(2.0f, true))
                        {
                            lineContext.SetVertexColor(faceColor);

                            foreach (var v in points)
                            {
                                SetVertex(lineContext, v);
                            }
                            SetVertex(lineContext, points.First());
                            lineContext.Freeze();
                            lineContext.Draw();
                        }

                        SCRTImmediateDraw.PopRasterizerState();
                    }
                    break;
                case ECheckShape.Circle_3P:
                    break;
            }
        }

        private void DrawSquareLine(Vector3[] vertices)
        {
            using (var lineContext = base.BeginLineStrips(2.0f, true))
            {
                lineContext.SetVertexColor(lineColor);

                foreach (var v in vertices)
                {
                    SetVertex(lineContext, v);
                }
                SetVertex(lineContext, vertices.First());
                lineContext.Freeze();
                lineContext.Draw();
            }
        }

        private void SetVertex(IImmediateLitMeshContext meshContext, Vector3 vector3)
        {
            meshContext.SetVertex3(vector3.X, vector3.Y, vector3.Z);
        }

        private void SetVertex(ILinesMesh linesContext, Vector3 vector3)
        {
            linesContext.SetVertex3(vector3.X, vector3.Y, vector3.Z);
        }        

        private void SetNormal(IImmediateLitMeshContext meshContext, Vector3 vector3)
        {
            meshContext.Normal3(vector3.X, vector3.Y, vector3.Z);
        }

        private void SetNormal(ILinesMesh linesContext, Vector3 vector3)
        {
            linesContext.SetVertex3(vector3.X, vector3.Y, vector3.Z);
        }

        /// <summary>
        ///     Performs selection on this entity, setting the IsSelected flag to True or False on the specified
        ///     <see cref="VertexId">Vertex Ids</see>
        /// </summary>
        /// <param name="isSelected">if set to <c>true</c> the vertices become .</param>
        /// <param name="vertexIds">The vertex ids.</param>
        public override void PerformSelection(bool isSelected, List<VertexId> vertexIds)
        {
            // Do nothing
        }
    }
}