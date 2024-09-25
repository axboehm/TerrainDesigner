# Height Designer

Height Designer is a 3D application that lets you walk around a landscape and modify it interactively.
The terrain is stored as a heightmap, a texture with pixel values ranging from black to white representing the height of the terrain at each point.
Keeping the application running smoothly in real time was a main goal of the implementation. Working smoothly with detailed landscapes was a priority and the implementation is motivated by that goal.

# General Concept

The terrain in the application can be seen as a grid of points, coordinates in 3D space. These points are equidistant to each other along the width and depth of the terrain and each have a height. A mesh can be created using these points in a regular grid that represents the landscape.
If viewed directly from above, these points would appear in a regular pattern, like a 2D matrix, with each point having a different height value.
The height values can be expressed as a value between 0 and 1, when the distance between the lowest and the highest point is stored as well. A 2D texture with a size equal to the amount of points along the terrain's axes is used to represent this terrain, generally called a heightmap.
Heightmaps are the basis of terrain representation and modification in Height Designer.

Once the leap to presenting a terrain as a texture is made, new possibilities to modify the existing terrain can be considered.
A common procedure in image editing is to have multiple textures layered on top of each other that are blended in a specific way. For example, to create a vignetting effect on any image a secondary image with a gradient going from white at the center to black at the edges can be added on top of the original image. If each pixel of this layer gets multiplied with each pixel of the underlying one, the result would be an image that gets darker towards the edges.
This idea of blending different layers together can be used for terrain manipulation as well. Imagine a flat plane at a certain height as one texture and an undulating terrain with some points above and some points below the height of that plane as another texture. Those two textures could be blended to create a terrain that is flat where the undulating texture has a lower height than the plane and the undulating texture otherwise, in other words, the result is the maximum of these two textures.

To give the user a method of interacting with the landscape and create modifications, a way to interface with the terrain was required.
The solution was to use spheres that represent geometry. Each sphere sits on top of a truncated cone that is a preview of the texture that will be generated to modify the terrain.
Once the user has placed spheres and adjusted their properties they can be applied. Textures representing the changes are created and then blended with the original terrain. This step is not real-time but rather takes a few seconds, the result however is mathmatically exact and predictable.
Then the terrain that the user walks on gets updated to represent this new state and further modifications can be made.

# Technical Overview

- MainLoop
- Different components
- Performance through QuadTree and staggering of updates
