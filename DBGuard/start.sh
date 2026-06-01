#!/bin/bash

# Start ML server using venv python
/opt/venv/bin/python /ml-model/server.py &

# Start ASP.NET app
dotnet DBGuard.AdminApp.dll