# Aether.Extras
MonoGame Content Importers, Shaders, etc


## Content Importers

* 'DDS Importer' - Import of DDS files (images, Cubemaps). Supports conversion from DTX to Color (ex. import DTX cubemaps for Android that doesn't support DXT compressed cubemaps).
* 'RawModelProcessor' - Import 3D Models with a raw copy of Vertex/Index data for platforms that don't support GetData().
* 'DynamicModel' - Base Processor to customize the build in Model. It allows to modify
VertexBuffer & IndexBuffers, make them Dynamic and WriteOnly.
