# Terrain Designer

Terrain Designer is a 3D application that lets you walk around a landscape and modify it interactively.<br />
It is made using the Godot open source game engine.<br />

![Terrain Designer](./readmeImages/readmeMain.jpg)


# General Concept

- Terrain is represented by a heightmap (single source of truth).
- Placeable spheres represent geometry that can modify the terrain.
- User can interact with spheres in real time.
- To apply modifications to the terrain, heightmaps representing different modifications get blended together.
- After modifications, the terrain samples an updated heightmap.

![General Concept](./readmeImages/readmeConcept.jpg)


# Technical Overview

- Application written in C# using Godot v.4.2.1 for rendering and collisions.
- Real time performance even with large terrains was a major consideration.
- Large meshes maintain good performance by using a quadtree to continually adjust the mesh fidelity around the user and efficiently reduce hardware load (mesh tiles closer to the user are smaller with higher resolution).
- Additional performance optimizations were: staggering updating of objects across multiple frames and pre-allocation of arrays with objects on startup that are continuosly reused.
- Single main loop that initializes objects and calls all other update functions to make the control flow obvious and prevent race conditions.
- Code is separated into components that deal with specific tasks, e.g. terrain generation, updating spheres and their geometry, updating the 3D character, etc.
- Representation of geometry using signed distance fields to get exact representations that are easy to store and modify.
- Terrain shader made to be adaptive to any type of geometry by using height based texture blending, triplanar UV mapping and texture bombing.
- Runtime Profiling implemented as main tool to assess performance of code and algorithms.

![Technical Overview](./readmeImages/readmeTechnical.jpg)


# Credits and Motivation
The code and assets were entirely created by Alexander Boehm.<br />
3D models were created using Blender, ZBrush and Marvelous Designer.<br />
Textures were made using Substance Designer, assets were textured using Substance Painter and Blender.<br />
Some textures, such as those for the terrain and UI are created in code at runtime of the application.<br />
<br />
I made this application mainly as a tool to help me learn how to implement complex, interrelated systems that consistently have to perform well to ensure a smooth user experience.<br />
The functionality that I implemented is limited in scope but still allowed me enough space to try out various implementations of the ideas I had. It also let me push the final product far enough to produce a polished result that shows a variety of skills.<br />

![Assets](./readmeImages/readmeAssets.jpg)
