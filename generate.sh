#!/bin/bash

dotnet run 4k render.ppm

echo "Converting to png format."
convert render.ppm render.png
echo "File saved to render.png."
