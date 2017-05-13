# Aether.Extras
MonoGame Content Importers, Shaders, etc


## Content Importers

* 'Animation' - Import animations from a Model.
* 'GPU AnimatedModel' - Import an animated Model.
* 'CPU AnimatedModel' - Import an animated Model to be animated by the CPU. Based on DynamicModelProcessor, the imported asset is of type Microsoft.Xna.Framework.Graphics.Model where the VertexBuffer is replaced by a CpuAnimatedVertexBuffer. CpuAnimatedVertexBuffer inherits from DynamicVertexBuffer.
* 'DDS Importer' - Import of DDS files (images, Cubemaps). Supports conversion from DTX to Color (ex. import DTX cubemaps for Android that doesn't support DXT compressed cubemaps).
* 'RawModelProcessor' - Import 3D Models with a raw copy of Vertex/Index data for platforms that don't support GetData().
* 'DynamicModel' - Base Processor to customize the build in Model. It allows to modify
VertexBuffer & IndexBuffers, make them Dynamic and WriteOnly.
* 'AtlasImporter' - Import sprite atlas. Supports .tmx files. Mipmaps are generated individually for each sprite, no color-leak.

## tainicom.Aether.Animation

Play animated 3D models and support for CPU animation.
CPU animation is optimized using unsafe code, writing directly to mapped VertexBuffer memory using reflection (DirectX) and unmanaged/C++ code (WP8.0). 


## tainicom.Aether.Shaders

* 'FXAA' - MonoGame port of NVIDIA's FXAA 3.11 shader.
* 'Deferred' - Deferred rendering.