# This scirpt should be run within blender python
# TODO: make it run without blender itself -> make it dependent of bpy module

import os
import bpy

extentions_ops = {"stl" : { "import" : bpy.ops.import_mesh.stl, "export" : bpy.ops.export_mesh.stl },
                  "obj" : { "import" : bpy.ops.import_scene.obj, "export" : bpy.ops.export_scene.obj } }

def file_iter(path, ext):
    for dirpath, dirnames, filenames in os.walk(path):
        for filename in filenames:
            ext = os.path.splitext(filename)[1]
            if ext.lower().endswith(ext):
                yield os.path.join(dirpath, filename)


def reset_blend():
    bpy.ops.wm.read_factory_settings(use_empty=True)

def convert_recursive(base_path, src_ext, dst_ext):
    for filepath_src in file_iter(base_path, "."+src_ext):
        filepath_dst = os.path.splitext(filepath_src)[0] + "." + dst_ext

        print("Converting %r -> %r" % (filepath_src, filepath_dst))

        reset_blend()

        extentions_ops[src_ext]["import"](filepath=filepath_src)
        extentions_ops[dst_ext]["export"](filepath=filepath_dst)

if __name__ == "__main__":
    CONVERT_DIR = "path/to/your/source/meshes/dir/here"
    source_extenstion = "stl"
    destination_extension = "obj"
    convert_recursive(CONVERT_DIR, source_extenstion, destination_extension)
