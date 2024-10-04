# Terrain Designer

Terrain Designer is a 3D application that lets you walk around a landscape and modify it interactively.<br />
The application is made entirely by Alexander Boehm and uses the Godot open source game engine for rendering and collisions. No external libraries were used.<br />

![Terrain Designer](./readmeImages/readmeMain.jpg)


# General Concept

- Terrain is represented by a heightmap.
- Placeable spheres represent geometry that can modify the terrain.
- Interact with spheres in real-time.
- To apply modifications to the terrain, heightmaps representing different modifications get blended together.
- After modifications, the terrain samples an updated heightmap.

![General Concept](./readmeImages/readmeConcept.jpg)


# Technical Overview

- Application written in C# using Godot v.4.2.1 for rendering and collisions; repository contains only the custom code and not the engine code.
- Real-Time performance even with large terrains was a main goal.
- Large meshes split up using a custom quadtree implementation to continuously adjust the mesh resolution based on distance to reduce hardware load (distant mesh tiles are larger with a lower resolution than close ones).
- Staggered resampling of mesh tiles across multiple frames to avoid lag spikes caused by multiple mesh re-assignments per frame
- Minimizing of object creation at run-time by pre-allocation of arrays with objects on startup that are continuosly re-used (frontloads performance impact).
- Single main loop that initializes objects and calls all other update functions to make the control flow obvious and prevent race conditions.
- Code is separated into components that deal with specific tasks, e.g. terrain generation, updating spheres and their geometry, updating the 3D character, etc.
- Representation of 3D geometry using 2D signed distance fields to get exact representations that are easy to store, modify and visualize.
- Adaptive terrain shader that works with any type of geometry by using height based texture blending, triplanar UV mapping and texture bombing.
- Runtime Profiling implemented to assess performance of code and algorithms, this was fundamental for assessing the effectiveness of my optimizations based on actual data.

![Technical Overview](./readmeImages/readmeTechnical.jpg)


# Assets and Motivation
The code (~8000 lines) and assets were entirely created by Alexander Boehm.<br />
3D models were created using Blender, ZBrush and Marvelous Designer.<br />
Textures were made using Substance Designer, assets were textured using Substance Painter and Blender.<br />
Some textures, such as those for the terrain and UI are created in code at runtime of the application.<br />
<br />
I made this application to help me learn how to implement complex, interrelated systems that consistently have to perform well to ensure a smooth user experience.<br />
The functionality that I implemented is limited in scope but still allowed me enough space to try out various implementations of the ideas I had. It also let me push the final product far enough to produce a polished result that shows a variety of skills.<br />

![Assets](./readmeImages/readmeAssets.jpg)
