# VTK input file specification

In order to upload your model into the application, your data needs to follow specific folder and file structure listed below. Additionally, apart from the unicorn_heart model, there is a quick test example within this folder, containing the simplest configuration for you to ensure everything is working correctly.

## Data folder structure specification

All data describing a model should place in one root folder. The structure is as follows:

-   [Root_folder_name]* - Root folder. Its actual name can be specified by the user.

	-  [body_#]* - Folder with vtk meshes - animation frames for the model mesh, where # is its number.
    
	- [simulation_#]* - Folder with dataflow simulation vtk files - animation frames for data simulation,, where # is its number.
    
	- [ModelInfo.txt]* - info file with the name of a model, and names of optional thumbnails,, where # is its number.

*[name of the folder/file]


## Data files specification

VTK is a versatile format, allowing user to save its model in various combinations. As such, we would like to standardise the data that are going to be used in our application. Below are general rules that apply to every file, regardless whether it's a body or simulation frame:

- All data should be made in accordance with VTK file format description. (https://www.vtk.org/VTK/img/file-formats.pdf). 
- All data should be stored in ASCII format and of UNSTRUCTURED_GRID dataset type. 
- One file is representing one frame in the animation. 
- File concerning one mesh/one simulation should be stored in one folder separately. 
- Each frame should have its number stated at the end, and the numbers should be zero-padded e.g.: lv_01.vtk, lv_02.vtk etc.


### Body data

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

### Simulation data

Currently, there are two types of data that we can simulate: fibre orientation data and flow simulation data. Both types differ from each other in structure, which is described below. Similarly to the body data, you can find them in the test files within this folder.

#### Fibre orientation data

Four fields are mandatory: **POINTS**; **alpha float**, **beta float** and **Vectors fn float** all three under **POINTS_DATA** section.

- **POINTS** are identical to the body data signifying initial vector point. 
 
- **alpha** and **beta** are angles in radians, describing position of the vector orientation in space. 
- **Vectors fn float** field, described by (x,y,z) floats.

#### Flow data
Four fields are mandatory: **POINTS**; **COLOR_TABLE** and **Vectors fn float** all two under **CELL_DATA** section.

- **POINTS** are identical to the body data signifying initial vector point. 
 
- **alpha** and **beta** are angles in radians, describing position of the vector orientation in space. 
- **Vectors fn float** field, described by (x,y,z) floats.



