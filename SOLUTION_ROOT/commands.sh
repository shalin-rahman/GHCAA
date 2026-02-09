# clean then restore (separate steps)
dotnet clean
dotnet restore --force
dotnet build