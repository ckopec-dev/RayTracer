#!/bin/bash

dotnet run low render.ppm

echo "Converting to png format."
convert three_spheres.ppm three_spheres.png
convert balls_on_surface.ppm balls_on_surface.png
echo "Files saved."
