# VTK input file specification

This file describes how folders and data files should be arranged to upload then into our application. Anatomical data (meshes) and simulation data (i.e data to be visualised on the anatomy) should be provided as separate layers. As a reference, we provide two examples showing the basic working structure -- see the [Test Model](https://github.com/MicroscopeIT/holo/tree/master/Input%20documentation/Test%20Model) and [UnicornHeart](https://github.com/MicroscopeIT/holo/tree/master/UnicornHeart) subdirectories.

## Data folder structure specification

All data describing a model should place in one root folder. The structure is as follows:

- `XXX` - Root folder.

    - `anatomy` - Folder with VTK meshes, describing the anatomy. One such folder for each mesh layer can be present. Inside the folder, place each frame in a separate VTK file with a consecutive number.

    - `simulation` - Folder with simulation VTK files. One such folder for each simulation layer can be present. Inside the folder, place each frame in a separate VTK file with a consecutive number.

    - `ModelInfo.json` - info file with the name of a model (caption to be displayed to user) and additional information about each layer.

You can choose any names for the root and layer folders. Simply make sure to list the appropriate layer directory names inside `ModelInfo.json`.

## Sample ModelInfo.json

```
{
    Caption: "Test Model",
    Layers: [
        {
            "Caption": "Body",
            "DataType": "anatomy",
            Directory: "test_body"
        },
        {
            Caption: "Fibre Simulation",
            "DataType": "fibre",
            "Directory": "testfibre_simulation"
        },
        {
            "Caption": "Flow Simulation",
            "DataType": "flow",
            "Directory": "testflow_simulation"
        }
    ]
}
```

Note that the field DataType can accept only three values: "anatomy", "fibre" "flow".

## Data files general specification

VTK is a versatile format, allowing user to save its model in various combinations. As such, we would like to standardise the data that are going to be used in our application. Below are general rules that apply to every layer (regardless whether it's anatomy or simulation):

- All data should be made in accordance with [VTK file format description](https://www.vtk.org/VTK/img/file-formats.pdf).
- Dataset type should be either **POLYDATA** or **UNSTRUCTURED_GRID**
- One file is representing one frame in the animation.
- Each frame should have its number stated at the end, and the numbers should be zero-padded e.g.: XYZ_01.vtk, XYZ_02.vtk etc.
- Number of frames should be the same in every root subfolder.

## Anatomical (mesh) data

Data used: **POINTS** and **CELLS**/**LINES**/**POLYGONS**.

- **POINTS** define vertices positions the mesh in one particular frame. No additional vertices should be found there. A vertex is defined by 3 floats, corresponding to its x, y and z coordinates in that particular order. Additionally:

    - The integer number after the POINTS flag indicates number of vertices in a mesh.
    - Number and order of vertices between frames is not supposed to change.

- **CELLS**/**LINES**/**POLYGONS** defines mesh facets. Currently we support two types of topology -- **lines** and **triangles**. A facet is defined first by the number of connected vertices followed by the vertices' indexes. Vertex index is interpreted according to the entry order specified in POINTS. Furthermore:

    - Number and order of facets between frames is not supposed to change.
    - In case of all polygons, consistent winding order is required. We advise using counter-clockwise winding order (as seen from the outside of the mesh), although the clockwise ordering will also work, as long as it is consistent.

## Simulation data

With simulation data we refer to the vector field that we want to visualise on the anatomy described by the mesh in the _Anatomical (mesh) data_ section. Currently, we can only visualize vector fields: fibre orientation and flow data. Both types differ from each other in structure, which is described below. As with the anatomy data, you can find the examples of files and directory structures in this folder.

### Fibre orientation data

Two sections are mandatory: **POINTS** and **POINT_DATA**. Moreover within the **POINT_DATA**, place subsections **SCALARS alpha float**, **SCALARS beta float** and **Vectors fn float**.

- **POINTS** are vertices specifying vector locations.
    - The integer number after the POINTS flag indicates number of vertices in a mesh.
    - Number and order of vertices between frames is not supposed to change.

 - **POINT_DATA**
    - Number by the **POINT_DATA** flag indicates number of colours/vectors and should be consistent with number of points specified in the **POINTS** section.

    - **SCALARS alpha float** and **SCALARS beta float** are scalars describing desired fibre characteristics, expressed with colorcoding.
        - Each scalar should be a float.
        - These values are used for the colormap visualisation of the vector.
        - Number and order of scalars shouldn't change between the frames.
        - Each scalar corresponds to a vertex in **POINTS**, mapped by their order.

    - **Vectors fn float** is a list of vectors, defining the orientation of a particular fibre.
        - Similar to **POINTS**, Vectors are described by 3 (x,y,z) floats.
        - Each vector is corresponding to a vertex in **POINTS**, mapped by their order.
        - Number and order of vectors between frames is not supposed to change.

### Flow data

Two sections are mandatory: **POINTS**, **CELLS**/**LINES** and **CELL_DATA**. Moreover, within the **CELL_DATA**, place a subsection **COLOR_SCALARS**.

- **POINTS** are vertices that define the initial vector point.
    - The number after the POINTS flag indicates number of vertices in a mesh.
    - Number and order of vertices between frames is not supposed to change.

- **CELLS**/**LINES** describe which vertices form a line in the flow data.
    - Only two vertices per line are allowed.

- **CELL_DATA**
    - Number by the **CELL_DATA** flag indicates number of colours/vectors and should be consistent with number beside **CELLS**/**LINES** flag

    - **COLOR_SCALARS** is a list of 3 dimensional vectors, describing colour of each simulation point in RGB system.
        - Similar to **POINTS**, Vectors are described by 3 (x,y,z) floats.
        - There should be only one RGB vector per line.a
        - Each vector is corresponding to a line in **CELLS** or **LINES**, mapped by their order.
        - Number of scalars and order of coloring between frames is not supposed to change.
