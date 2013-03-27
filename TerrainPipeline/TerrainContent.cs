
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.IO;


namespace TerrainPipeline
{
    public class TerrainContent
    {
        public byte[] height;
        public Vector3[] position;
        public Vector3[] normal;
        public float cellWidth, cellHeight;

        // hard coded values to match height map pixel and world dimensions
        public int NUM_ROWS = 257;
        public int NUM_COLS = 257;
        public float worldWidth = 16.0f;
        public float worldHeight = 16.0f;
        public float heightScale = 0.0104f;

        // constructor for raw data - used during bulk data import
        public TerrainContent(byte[] bytes)
        {
            height = bytes;
            setCellDimensions();
            generatePositions();
            generateNormals();
        }

        // sets height and width of cells made from pixels in .raw file
        public void setCellDimensions()
        {
            cellWidth = 2.0f * worldWidth / (NUM_COLS - 1);
            cellHeight = 2.0f * worldHeight / (NUM_ROWS - 1);
        }

        // generate X, Y, and Z position data where Y is the height.
        private void generatePositions()
        {
            position = new Vector3[NUM_ROWS * NUM_COLS];
            for (int row = 0; row < NUM_ROWS; row++)
            {
                for (int col = 0; col < NUM_COLS; col++)
                {
                    float X = -worldWidth + col * cellWidth;
                    float Y = height[row * NUM_COLS + col] * heightScale;
                    float Z = -worldHeight + row * cellHeight;
                    position[col + row * NUM_COLS] = new Vector3(X, Y, Z);
                }
            }
        }

        // generate normal vector for each cell in height map
        private void generateNormals()
        {
            Vector3 tail, right, down, cross;
            normal = new Vector3[NUM_ROWS * NUM_COLS];

            // normal is cross product of two vectors joined at tail
            for (int row = 0; row < NUM_ROWS - 1; row++)
            {
                for (int col = 0; col < NUM_COLS - 1; col++)
                {
                    tail = position[col + row * NUM_COLS];
                    right = position[col + 1 + row * NUM_COLS] - tail;
                    down = position[col + (row + 1) * (NUM_COLS)] - tail;
                    cross = Vector3.Cross(down, right);
                    cross.Normalize();
                    normal[col + row * NUM_COLS] = cross;
                }
            }
        }
    }

    // all processors must derive from this class
    [ContentProcessor]
    public class TerrainProcessor : ContentProcessor<TerrainContent,
    TerrainContent>
    {
        public override TerrainContent Process(TerrainContent input,
        ContentProcessorContext context)
        {
            return new TerrainContent(input.height);
        }
    }

    // stores information about importer, file extension, and caching
    [ContentImporter(".raw", DefaultProcessor = "TerrainProcessor")]

    // ContentImporter reads original data from original media file
    public class TerrainPipeline : ContentImporter<TerrainContent>
    {
        // reads original data from binary or text based files
        public override TerrainContent Import(String filename,
                                              ContentImporterContext context)
        {
            byte[] bytes = File.ReadAllBytes(filename);
            TerrainContent terrain = new TerrainContent(bytes);
            return terrain; // returns compiled data object
        }
    }

    // write compiled data to *.xnb file
    [ContentTypeWriter]
    public class TerrWriter : ContentTypeWriter<TerrainContent>
    {
        protected override void Write(ContentWriter cw,
                                      TerrainContent terrain)
        {
            cw.Write(terrain.NUM_ROWS);
            cw.Write(terrain.NUM_COLS);
            cw.Write(terrain.worldWidth);
            cw.Write(terrain.worldHeight);
            cw.Write(terrain.heightScale);
            cw.Write(terrain.cellWidth);
            cw.Write(terrain.cellHeight);

            for (int row = 0; row < terrain.NUM_ROWS; row++)
            {
                for (int col = 0; col < terrain.NUM_COLS; col++)
                {
                    cw.Write(terrain.position[col + row * terrain.NUM_COLS]);
                    cw.Write(terrain.normal[col + row * terrain.NUM_COLS]);
                }
            }
        }

        // Sets the CLR data type to be loaded at runtime.
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "TerrainRuntime.Terrain, TerrainRuntime, Version=1.0.0.0, Culture=neutral";
        }

        // Tells the content pipeline about reader used to load .xnb data
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "TerrainRuntime.TerrainReader, TerrainRuntime, Version=1.0.0.0, Culture=neutral";
        }
    }
}