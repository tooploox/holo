# VTK input file specification

In order to upload your model into the application, your data needs to follow specific folder and file structure listed below. Additionally, apart from the unicorn_heart model, there is a quick test example within this folder, containing the simplest configuration for you to ensure everything is working correctly.

## Data folder structure specification

All data describing a model should place in one root folder. The structure is as follows:

- `XXX` - Root folder.

	- `body`* - Folder with vtk meshes, which are animation frames for the model mesh. One such folder for each mesh layer can be present. Inside the folder, place each frame in a separate VTK file with a consecutive number.

	- `simulation`* - Folder with dataflow simulation vtk files - animation frames for data simulation. One such folder for each simulation layer can be present. Inside the folder, place each frame in a separate VTK file with a consecutive number.

	- `ModelInfo.json` - info file with the name of a model (caption to be displayed to user) and additional information about each layer.

You can choose any names for the root and layer folders. Simply make sure to list the appropriate layer directory names inside `ModelInfo.json`.

## Sample ModelInfo.json

```
{
    Caption: "Test Model",
    Layers: [
        {
            Caption: "Body",
            Simulation: false,
            Directory: "test_body"
        },
        {
            Caption: "Fibre Simulation",
            Simulation: true,
            Directory: "testfibre_simulation"
        },
        {
            Caption: "Flow Simulation",
            Simulation: true,
            Directory: "testflow_simulation"
        }
    ]
}
```

## Data files general specification

VTK is a versatile format, allowing user to save its model in various combinations. As such, we would like to standardise the data that are going to be used in our application. Below are general rules that apply to every file, regardless whether it's a body or simulation frame:

- All data should be made in accordance with VTK file format description. (https://www.vtk.org/VTK/img/file-formats.pdf).
- All data should be stored in ASCII format and of UNSTRUCTURED_GRID dataset type.
- One file is representing one frame in the animation.
- File concerning one mesh/one simulation should be stored in one folder separately.
- Each frame should have its number stated at the end, and the numbers should be zero-padded e.g.: XYZ_01.vtk, XYZ_02.vtk etc.
- Number of frames should be the same in every root subfolder.
- Every file starts with four lines listed below, followed by an empty line:

```
# vtk DataFile Version 4.0
<Name of the current body/simulation> vtk output
ASCII
DATASET UNSTRUCTURED_GRID
```


## Body data

Two fields are required; **POINTS** and **CELLS**.

-   **POINTS** are all the vertices that are defining the mesh in that particular frame. No additional vertices should be found there. A vertex is defined by 3 floats, corresponding to its x, y and z coordinates in that particular order. Additionally:

	-  The number after the POINTS flag indicates number of vertices in a mesh.
	- There should be only one vertex per line.
	- Number and order of vertices between frames is not supposed to change.

-   **CELLS** define mesh facets.  Currently we are supporting two types of topology - lines and triangles. A facet is defined first by a number of points which it consists of and then a list of indexes, indicating the vertices which belong to the facet. Furthermore:

	- The number after the CELLS flag indicates number of facets in a mesh.
	- There should be only one facet per line.
	- Number and order of facets between frames is not supposed to change.
	- In case of triangles, counter-clockwise winding order is required.

## Simulation data

Currently, there are two types of data that we can simulate: fibre orientation data and flow simulation data. Both types differ from each other in structure, which is described below. Similarly to the body data, you can find them in the test files within this folder.

### Fibre orientation data

Four fields are mandatory: **POINTS**; **SCALARS alpha float**, **SCALARS beta float** and **Vectors fn float** all three under **POINT_DATA** section.

- **POINTS** are vertices signifying initial vector point.
	-  The number after the POINTS flag indicates number of vertices in a mesh.
	- There should be only one vertex per line.
	- Number and order of vertices between frames is not supposed to change.

 - **POINT_DATA**
	- Number by the **CELL_DATA** flag indicates number of colours/vectors and should be consistent with number beside **POINTS** flag.
	-
	- **SCALARS alpha float** and **SCALARS beta float** are angles in radians, describing position of the vector orientation in space.
		- There should be only one angle per line and vertex.
		- Number and order of angles shouldn't change between the frames.
		- Each angle in both lists is corresponding to a vertex in **POINTS**, mapped by their order.
	- Each angle should be a float.

	- **Vectors fn float** is a list of vectors, signifying orientation of a particular fibre.
		- Similar to **POINTS**, Vectors are described by 3 (x,y,z) floats.
		- There should be only one vector per line and vertex.
		- Each vector is corresponding to a vertex in **POINTS**, mapped by their order.
		- Number and order of vectors between frames is not supposed to change.

### Flow data
Four fields are mandatory: **POINTS**; **COLOR_SCALARS** and **Vectors fn float** all two under **CELL_DATA** section.

- **POINTS** are vertices signifying initial vector point.
	-  The number after the POINTS flag indicates number of vertices in a mesh.
	- There should be only one vertex per line.
	- Number and order of vertices between frames is not supposed to change.

- **CELL_DATA**
	- Number by the **CELL_DATA** flag indicates number of colours/vectors and should be consistent with number beside **POINTS** flag

	- **COLOR_SCALARS** is a list of 3 dimensional vectors, describing colour of each simulation point in RGB system.
		- Similar to **POINTS**, Vectors are described by 3 (x,y,z) floats.
		- There should be only one RGB vector per line and vertex.
		- Each vector is corresponding to a vertex in **POINTS**, mapped by their order.
		- Number and order of colors between frames is not supposed to change.

	- **Vectors fn float** is a list of vectors, signifying orientation and speed of a particular point in the flow. We use this vector's length and color to visualise the speed of this point in the flow.
		- Similar to **POINTS**, Vectors are described by 3 (x,y,z) floats.
		- There should be only one vector per line and vertex.
		- Each vector is corresponding to a vertex in **POINTS**, mapped by their order.
		- Number and order of vectors between frames is not supposed to change.
