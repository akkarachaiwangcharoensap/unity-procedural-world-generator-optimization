using UnityEngine;

public class ProceduralMapGenerator : MonoBehaviour
{
    /**
     * <summary>
     * Seed
     * </summary>
     */
    [SerializeField]
    public float seed = 0;

    /**
     * <summary>
     * Size    
     * </summary>
     */
    [SerializeField]
    private Vector2 size = new Vector2(0, 0);

    /**
     * <summary>
     * Frequency
     * </summary>
     */
    public float frequency = 1;

    /**
     * <summary>
     * Mesh filter
     * </summary>
     */
    private MeshFilter meshFilter;

    /**
     * <summary>
     * UVs
     * </summary>
     */
    [SerializeField]
    private Vector2[] UVs;

    /**
     * <summary>
     * Vertex triangles
     * </summary>
     */
    private int[] triangles;

    /**
     * <summary>
     * Vertices
     * </summary>
     */
    private Vector3[] vertices;

    /**
     * <summary>
     * Normals
     * </summary>
     */
    private Vector3[] normals;

    /**
     * <summary>
     * Run before start() is called.
     * </summary>
     * 
     * <returns>
     * void
     * </returns>
     */
    private void Awake() {}

    /**
     * <summary>
     * Use this for initialization
     * </summary>
     * 
     * <returns>
     * void
     * </returns>
     */
    private void Start()
    {
        this.meshFilter = this.GetComponent<MeshFilter>();

        this.Generate();
    }

    /**
     * <summary>
     * Update is called once per frame    
     * </summary>
     * 
     * <returns>
     * void
     * </returns>
     */
    private void Update() {}

    /**
     * <summary>
     * Generate a new world based on the properties    
     * </summary>
     * 
     * <returns>
     * GameObject
     * </returns>
     */
    public void Generate()
    {
        this.meshFilter = this.GetComponent<MeshFilter>();

        int numberOfTiles = (int)(this.size.x * this.size.y); // x * y
        int numberOfTriangles = numberOfTiles * 6; //
        int numberOfVertices = numberOfTiles * 4; // Vertices or the node

        this.UVs = new Vector2[numberOfVertices];

        // Generate vertices
        this.vertices = this.GenerateVertices();

        // Generate UVs
        Vector2 position = new Vector2(this.transform.position.x, this.transform.position.z);
        this.UVs = this.GenerateAndMapUV(position);

        // Generate Triangles
        this.triangles = this.GenerateTriangles();

        // Generate normals
        this.normals = this.GenerateNormals();

        Mesh mesh = new Mesh();

        // Set the maximum rendering meshes
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.name = "Optimized Procedural Mesh";
        mesh.vertices = this.vertices;
        mesh.triangles = this.triangles;
        mesh.normals = this.normals;
        mesh.uv = this.UVs;

        this.meshFilter.mesh = mesh;
        this.meshFilter.mesh.RecalculateTangents();
        this.meshFilter.mesh.RecalculateNormals();

        this.gameObject.AddComponent<BoxCollider>();
    }

    /**
     * <summary>
     * Generate vertices
     * </summary>
     *
     * <returns>
     * void
     * </returns>
     */
    private Vector3[] GenerateVertices()
    {
        int numberOfTiles = (int)(this.size.x * this.size.y); // x * y
        int numberOfVertices = numberOfTiles * 4;

        int vertice = 0;

        this.vertices = new Vector3[numberOfVertices];

        for (int x = 0; x < (int)this.size.x; x++)
        {
            for (int y = 0; y < (int)this.size.y; y++)
            {
                this.vertices[vertice + 0] = new Vector3(x, 0, y);
                this.vertices[vertice + 1] = new Vector3(x + 1, 0, y);
                this.vertices[vertice + 2] = new Vector3(x + 1, 0, y + 1);
                this.vertices[vertice + 3] = new Vector3(x, 0, y + 1);

                vertice = vertice + 4;
            }
        }

        return this.vertices;
    }

    /**
     * <summary>
     * Generate UV
     * </summary>
     *
     * <returns>
     * void
     * </returns>
     */
    private Vector2[] GenerateAndMapUV(Vector2 initPosition)
    {
        int vertice = 0;

        for (int x = (int)initPosition.x; x < (int)initPosition.x + (int)this.size.x; x++)
        {
            for (int z = (int)initPosition.y; z < (int)initPosition.y + (int)this.size.y; z++)
            {
                double nx = this.seed + (double)x / this.size.x - 0.5f;
                double nz = this.seed + (double)z / this.size.y - 0.5f;

                float elevationNoise = Mathf.PerlinNoise(frequency * (float)nx, frequency * (float)nz);

                this.MapUVTile(
                    elevationNoise,
                    vertice
                );

                vertice = vertice + 4;
            }
        }

        return this.UVs;
    }

    /**
     * <summary>
     * Map Tile UV
     * </summary>
     *
     * <param name="e">float elevation</param>
     * <param name="vertice">int vertice</param>
     *
     * <returns>
     * void
     * </returns>
     */
    private void MapUVTile(float e, int vertice)
    {
        // Water
        if (e < 0.2f)
        {
            Vector2[] waterUVs = TileUVMap.Water();

            this.UVs[vertice + 0] = waterUVs[0];
            this.UVs[vertice + 1] = waterUVs[1];
            this.UVs[vertice + 2] = waterUVs[2];
            this.UVs[vertice + 3] = waterUVs[3];
        }

        // Sand
        else if (e < 0.3f)
        {
            Vector2[] sandUVs = TileUVMap.Sand();

            this.UVs[vertice + 0] = sandUVs[0];
            this.UVs[vertice + 1] = sandUVs[1];
            this.UVs[vertice + 2] = sandUVs[2];
            this.UVs[vertice + 3] = sandUVs[3];
        }

        // Dirt
        else if (e > 0.6f)
        {
            Vector2[] dirtUVs = TileUVMap.Dirt();

            this.UVs[vertice + 0] = dirtUVs[0];
            this.UVs[vertice + 1] = dirtUVs[1];
            this.UVs[vertice + 2] = dirtUVs[2];
            this.UVs[vertice + 3] = dirtUVs[3];
        }

        // Grass
        else
        {
            Vector2[] grassUVs = TileUVMap.Grass();

            this.UVs[vertice + 0] = grassUVs[0];
            this.UVs[vertice + 1] = grassUVs[1];
            this.UVs[vertice + 2] = grassUVs[2];
            this.UVs[vertice + 3] = grassUVs[3];
        }
    }

    /**
     * <summary>
     * Generate triangles
     * </summary>
     *
     * <returns>
     * void
     * </returns>
     */
    private int[] GenerateTriangles()
    {
        int numberOfTiles = (int)(size.x * size.y); // x * y
        int numberOfTriangles = numberOfTiles * 6; //

        int[] triangles = new int[numberOfTriangles];

        int index = 0;
        int vertice = 0;

        for (int i = 0; i < numberOfTiles; i++)
        {
            triangles[index + 0] += (vertice + 2);
            triangles[index + 1] += (vertice + 1);
            triangles[index + 2] += (vertice + 0);
            triangles[index + 3] += (vertice + 3);
            triangles[index + 4] += (vertice + 2);
            triangles[index + 5] += (vertice + 0);

            vertice = vertice + 4;
            index = index + 6;
        }

        return triangles;
    }

    /**
     * <summary>
     * Generate normals
     * </summary>
     *
     * <returns>
     * void
     * </returns>
     */
    private Vector3[] GenerateNormals()
    {
        int numberOfTiles = (int)(this.size.x * this.size.y); // x * y
        int numberOfVertices = numberOfTiles * 4;

        this.normals = new Vector3[numberOfVertices];

        // Setting normals
        for (int i = 0; i < numberOfVertices; i++)
        {
            this.normals[i] = new Vector3(0, 1, 0);
        }

        return this.normals;
    }
}
