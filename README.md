# Height Designer

Height Designer is a 3D application made using the game engine Godot, that lets you walk around a landscape and modify it interactively.

![Test Image Hover Description](./readmeImages/test.png)


# General Concept

- Terrain as heightmap
- spheres represent geometry
- user can interact with spheres
- apply terrain -> blend heightmaps of spheres with terrain and update


# Technical Overview

- using godot v.4.2.1 using C#
- performance a main goal
- Performance through QuadTree, staggering of updates and re-use of objects
- MainLoop
- Different components
- representation of sphere geometry using signed distance fields
- terrain shader made to be adaptive to any type of geometry using: height based blending, triplanar UV mapping, texture bombing
