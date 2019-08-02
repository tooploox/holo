# VTK input file specification

This file describes how folders and data files should be arranged to upload then into our application. Anatomical data (meshes) and simulation data (i.e data to be visualised on the anatomy) should be provided as separate layers. As a reference, we provide two examples showing the basic working structure -- see the [Test Model](https://github.com/MicroscopeIT/holo/tree/master/Input%20documentation/Test%20Model) and [UnicornHeart](https://github.com/MicroscopeIT/holo/tree/master/UnicornHeart) subdirectories.

## Data folder structure specification

All data describing a model should place in one root folder. The structure is as follows:

- `XXX` - Root folder.

    - `body`* - Folder with VTK meshes, describing the anatomy. One such folder for each mesh layer can be present. Inside the folder, place each frame in a separate VTK file with a consecutive number.

    - `simulation`* - Folder with simulation VTK files. One such folder for each simulation layer can be present. Inside the folder, place each frame in a separate VTK file with a consecutive number.

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

VTK is a versatile format, allowing user to save its model in various combinations. As such, we would like to standardise the data that are going to be used in our application. Below are general rules that apply to every layer (regardless whether it's anatomy or simulation):

- All data should be made in accordance with [VTK file format description](https://www.vtk.org/VTK/img/file-formats.pdf).
- All data should be stored in ASCII format and of UNSTRUCTURED_GRID dataset type.
- One file is representing one frame in the animation.
- Each frame should have its number stated at the end, and the numbers should be zero-padded e.g.: XYZ_01.vtk, XYZ_02.vtk etc.
- Number of frames should be the same in every root subfolder.
- Every file starts with four lines listed below, followed by an empty line:

```
# vtk DataFile Version 4.0
<Name of the current body/simulation> vtk output
ASCII
DATASET [POLYDATA/UNSTRUCTURED_GRID]
```
Type of the dataset depends on the data inside.

## Anatomical (mesh) data
Dataset type **UNSTRUCTURED_GRID**

Two fields are required; **POINTS** and **CELLS**.

- **POINTS** define vertices positions the mesh in one particular frame. No additional vertices should be found there. A vertex is defined by 3 floats, corresponding to its x, y and z coordinates in that particular order. Additionally:

    - The integer number after the POINTS flag indicates number of vertices in a mesh.
    - There should be only one vertex per line.
    - Number and order of vertices between frames is not supposed to change.

- **CELLS** defines mesh facets. Currently we support two types of topology -- lines and triangles. A facet is defined first by the number of connected vertices followed by the vertices' indexes. Vertex index is interpreted according to the entry order specified in POINTS. Furthermore:

    - The integer number after the CELLS flag indicates the number of facets in a mesh (first number) and the total number of integers in the CELL section (second number).
    - There should be only one facet per line.
    - Number and order of facets between frames is not supposed to change.
    - In case of all polygons, consistent winding order is required. We advise using counter-clockwise winding order (as seen from the outside of the mesh), although the clockwise ordering will also work, as long as it is consistent.
    - Only one **CELLS** section in the file is supported.

- The VTK format also requires **CELL_TYPES**, so you should add it to the VTK file. Although it is ignored by our application: in our application, the first column within each CELLS row indicates whether it's a triangle, quad etc.

## Simulation data
Dataset type: **POLYDATA**

With simulation data we refer to the vector field that we want to visualise on the anatomy described by the mesh in the _Anatomical (mesh) data_ section. Currently,we can only visualize vector fields: fibre orientation and flow data. Both types differ from each other in structure, which is described below. As in the case of the anatomy data, you can find the examples of files and directory structures in this folder.

### Fibre orientation data

Two sections are mandatory: **POINTS** and **POINT_DATA**. Moreover within the **POINT_DATA**, place subsections **SCALARS alpha float**, **SCALARS beta float** and **Vectors fn float**.

- **POINTS** are vertices specifying vector locations.
    - The integer number after the POINTS flag indicates number of vertices in a mesh.
    - There should be only one vertex per line.
    - Number and order of vertices between frames is not supposed to change.

 - **POINT_DATA**
    - Number by the **POINT_DATA** flag indicates number of colours/vectors and should be consistent with number of points specified in the **POINTS** section.

    - **SCALARS alpha float** and **SCALARS beta float** are angles in radians, describing position of the vector orientation in space.
        - Each angle should be a float.
        - These values are used for the colormap visualisation of the vector.
        - There should be only one angle (scalar) per line and vertex.
        - Number and order of angles shouldn't change between the frames.
        - Each angle (scalar) corresponds to a vertex in **POINTS**, mapped by their order.

    - **Vectors fn float** is a list of vectors, defining the orientation of a particular fibre.
        - Similar to **POINTS**, Vectors are described by 3 (x,y,z) floats.
        - There should be only one vector per line and vertex.
        - Each vector is corresponding to a vertex in **POINTS**, mapped by their order.
        - Number and order of vectors between frames is not supposed to change.

- Note that we ignore the **CELLS** information for the simulation. We don't need this information, as we assume that the simulation data is composed only from points.

### Flow data

Two sections are mandatory: **POINTS** and **CELL_DATA**. Moreover within the **CELL_DATA**, place subsections **COLOR_SCALARS** and **Vectors fn float**.

- **POINTS** are vertices that define the initial vector point.
    - The number after the POINTS flag indicates number of vertices in a mesh.
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
