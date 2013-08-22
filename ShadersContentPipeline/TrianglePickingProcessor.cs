using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace ShadersContentPipeline
{
	[ContentProcessor(DisplayName = "Triangle Picking Processor")]
	public class TrianglePickingProcessor : ModelProcessor
	{
		List<Vector3> vertices = new List<Vector3>();

		public override ModelContent Process(NodeContent input, ContentProcessorContext context)
		{
			ModelContent model = base.Process(input, context);

			FindVertices(input);

			model.Tag = vertices;
            
			return model;
		}


		/// <summary>
		/// Helper for extracting a list of all the vertex positions in a model.
		/// </summary>
		void FindVertices(NodeContent node)
		{
			// Is this node a mesh?
			MeshContent mesh = node as MeshContent;

			if (mesh != null)
			{
				// Look up the absolute transform of the mesh.
				//Matrix absoluteTransform = mesh.AbsoluteTransform;

				// Loop over all the pieces of geometry in the mesh.
				foreach (GeometryContent geometry in mesh.Geometry)
				{
					// Loop over all the indices in this piece of geometry.
					// Every group of three indices represents one triangle.
					foreach (int index in geometry.Indices)
					{
						// Look up the position of this vertex.
						Vector3 vertex = geometry.Vertices.Positions[index];

						// Transform from local into world space.
						//vertex = Vector3.Transform(vertex, absoluteTransform);

						// Store this vertex.
						vertices.Add(vertex);
					}
				}
			}

			// Recursively scan over the children of this node.
			foreach (NodeContent child in node.Children)
			{
				FindVertices(child);
			}
		}
	}
}