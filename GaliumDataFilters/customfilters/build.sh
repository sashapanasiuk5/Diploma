#!/bin/sh

# Build a Docker image that includes custom Java filters.

# Compile the Java filters into a jar file
mvn compiler:compile
mvn jar:jar
cp target/customfilters-1.0.jar docker

# Tar up the definition of the filters
cd metarepo
rm -f ../docker/repo.tar
tar -c -f ../docker/repo.tar .
cd ..

# Build a Docker image that includes the jar file and the definition files
cd docker
docker build -t gallium-dbguard:1.0 .
cd ..
