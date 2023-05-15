Rendering
===============

This package offers URP renderers finetuned to be compatible with the Netherlands3D package rendering requirements, and offers scripts that add coordinate based methods and properties to graphic components like Projectors.
Usage

## How to use

Set the UniversalRenderPipelineAsset from the Renderers folder as the 'Render Pipeline Asset' in your quality options under 'Project Settings/Quality' 

Next, set it in 'Project Settings/Graphics' as the Scriptable Render Pipeline Settings.

## Renderer features

The renderer contains the following render features:

- Terrain - Render feature to render your terrain layer inc. rendering to stencil mask

- Buildings - Render feature to render your buildings layer inc. rendering to stencil mask

- Projector - Render feature for the RendererProjectorPrefab

- Decal - Render feature for the DecalProjectorPrefab with large decal rendering distance, and set to Screen Space to be supported by WebGL builds
  
  
  
  
  
  
