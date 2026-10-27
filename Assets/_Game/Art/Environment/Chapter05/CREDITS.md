# Chapter 5 forest — third-party art

All files below come from Poly Haven (https://polyhaven.com) and are released under **CC0** (public domain,
https://polyhaven.com/license). Credit is not required; it is given anyway. Downloaded on 2026-10-26 from
`dl.polyhaven.org`, the maps at 2K (the models' geometry is the same file at every resolution). Asset info was checked
through `api.polyhaven.com/info/<id>` at that time.

| Asset | Page | Author(s) | Files used | Used for |
|---|---|---|---|---|
| Brown Mud 02 | https://polyhaven.com/a/brown_mud_02 | Rob Tuytel | diff, nor_gl, rough, ao (2K) | the forest floor and the way (`C5F_Floor`, shader `ThuyKieu/Ch5ForestGround`); rough + ao packed into `C5F_GroundARM.png` (AO r, roughness g) |
| Fern 02 | https://polyhaven.com/a/fern_02 | Rob Tuytel (scanning), Rico Cilliers (modelling) | fern_02.fbx (4 clumps, 6,232 tris in all), diff, alpha, nor_gl, rough, ao (2K) | the ferns by the way (`BambooGrove`); diff + alpha packed into `C5F_FernAlbedo.png`, ao + roughness into `C5F_FernMask.png` |
| Rock Moss Set 01 | https://polyhaven.com/a/rock_moss_set_01 | Kless Gyzen | rock_moss_set_01.fbx (6 rocks, 63,127 tris in all), diff, nor_gl, rough, ao (2K) | rocks in the banks and on the forest floor (`Chapter05Forest.Rocks`), with Mesh LODs generated on import; ao + roughness packed into `C5F_RockMask.png` |

Not used: the preview images, the 4K/8K maps, displacement, the .blend/.gltf files (the .gltf was read only to check the
node and mesh sizes). The bamboo itself (culms, leaves, canopy, hedge tangle, skyline) is still drawn by code in
`Chapter05Forest.Art.cs`; it is not a scan.
